// Best Pins to Use – ESP8266 -- https://randomnerdtutorials.com/esp8266-pinout-reference-gpios/
// Label  GPIO    Input         Output                  Notes
// D0	    GPIO16  no interrupt  no PWM or I2C support   HIGH at boot used to wake up from deep sleep
// D1	    GPIO5	  OK	          OK	                    often used as SCL (I2C)
// D2	    GPIO4	  OK	          OK	                    often used as SDA (I2C)
// D3	    GPIO0	  pulled up	    OK	                    connected to FLASH button, boot fails if pulled LOW
// D4	    GPIO2	  pulled up	    OK	                    HIGH at boot connected to on-board LED, boot fails if pulled LOW
// D5	    GPIO14	OK	          OK	                    SPI (SCLK)
// D6	    GPIO12	OK	          OK	                    SPI (MISO)
// D7	    GPIO13	OK	          OK	                    SPI (MOSI)
// D8	    GPIO15	pulled to GND	OK	                    SPI (CS) Boot fails if pulled HIGH
// RX	    GPIO3	  OK	          RX pin	                HIGH at boot
// TX	    GPIO1	  TX pin	      OK	                    HIGH at boot debug output at boot, boot fails if pulled LOW
// A0	    ADC0	  Analog Input	X	

#include <ArduinoJson.h>

// C99 libraries
#include <string.h>
#include <stdbool.h>
#include <time.h>
#include <cstdlib>

// Libraries for MQTT client, WiFi connection and SAS-token generation.
#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <WiFiClientSecure.h>
#include <base64.h>
#include <bearssl/bearssl.h>
#include <bearssl/bearssl_hmac.h>
#include <libb64/cdecode.h>

// Azure IoT SDK for C includes
#include <az_core.h>
#include <az_iot.h>
#include <azure_ca.h>

// Additional sample headers 
#include "iot_configs.h"

// Temp sensor
#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <DHT_U.h>

// Fastled
#include <FastLED.h>

#define LED_PIN     2 // D2 - GPIO4 -- Fast led uses the D number, so connect this to D2. 
#define NUM_LEDS    14
#define LED_TYPE    WS2812B
#define COLOR_ORDER GRB

CRGB leds[NUM_LEDS];

// Setup temp sensor DHT11
#define DHT_TYPE     DHT11
#define DHT_PIN      5 // D1 - GPIO5 -- DHT library uses GPIO pin number, so coonnect this to D1, which equals to GPIO 5

DHT_Unified dht(DHT_PIN, DHT_TYPE);


// When developing for your own Arduino-based platform,
// please follow the format '(ard;<platform>)'. 
#define AZURE_SDK_CLIENT_USER_AGENT "c/" AZ_SDK_VERSION_STRING "(ard;esp8266)"

// Utility macros and defines
#define sizeofarray(a) (sizeof(a) / sizeof(a[0]))
#define ONE_HOUR_IN_SECS 3600
#define NTP_SERVERS "pool.ntp.org", "time.nist.gov"
#define MQTT_PACKET_SIZE 1024

// Translate iot_configs.h defines into variables used by the sample
static const char* ssid = IOT_CONFIG_WIFI_SSID;
static const char* password = IOT_CONFIG_WIFI_PASSWORD;
static const char* host = IOT_CONFIG_IOTHUB_FQDN;
static const char* device_id = IOT_CONFIG_DEVICE_ID;
static const char* device_key = IOT_CONFIG_DEVICE_KEY;
static const int port = 8883;

// Memory allocated for the sample's variables and structures.
static WiFiClientSecure wifi_client;
static X509List cert((const char*)ca_pem);
static PubSubClient mqtt_client(wifi_client);
static az_iot_hub_client client;
static char sas_token[200];
static uint8_t signature[512];
static unsigned char encrypted_signature[32];
static char base64_decoded_device_key[32];
static unsigned long next_telemetry_send_time_ms = 0;
static char telemetry_topic[128];


static void initializeTime()
{
  Serial.print("Setting time using SNTP");

  configTime(-5 * 3600, 0, NTP_SERVERS);
  time_t now = time(NULL);
  while (now < 1510592825)
  {
    delay(500);
    Serial.print(".");
    now = time(NULL);
  }
  Serial.println("done!");
}

static char* getCurrentLocalTimeString()
{
  time_t now = time(NULL);
  return ctime(&now);
}

static void printCurrentTime()
{
  Serial.print("Current time: ");
  Serial.print(getCurrentLocalTimeString());
}


static uint32_t getSecondsSinceEpoch()
{
  return (uint32_t)time(NULL);
}

void sendTelemetryPayload()
{
  char payload[256];
  DynamicJsonDocument doc(256);
  sensors_event_t tEvent;
  sensors_event_t hEvent;

  dht.temperature().getEvent(&tEvent);
  dht.humidity().getEvent(&hEvent);

  // Return if the temperature or humidity reading returned "nan"
  if (isnan(tEvent.temperature) || isnan(hEvent.relative_humidity)) {
    Serial.println(F("Error reading temperature or humidity!"));
    return;
  }

  doc["device_id"] = device_id;
  doc["time"] = getSecondsSinceEpoch();
  doc["temp"] = tEvent.temperature;
  doc["humidity"] = hEvent.relative_humidity;
  serializeJson(doc, payload);

  Serial.println(payload);

  Serial.print(" ESP8266 Sending telemetry . . . ");
  if (az_result_failed(az_iot_hub_client_telemetry_get_publish_topic(
          &client, NULL, telemetry_topic, sizeof(telemetry_topic), NULL)))
  {
    Serial.println("Failed az_iot_hub_client_telemetry_get_publish_topic");
    return;
  }

  mqtt_client.publish(telemetry_topic, payload, false);
  Serial.println("OK");
  delay(100);
    
}

void setLedStripColor(uint8_t red, uint8_t green, uint8_t blue)
{
  for( int i = 0; i < NUM_LEDS; ++i)
  {
    leds[i].setRGB( red, green, blue );
  }
  FastLED.show();
}


void receivedCallback(char* topic, byte* payload, unsigned int length)
{
  Serial.print("Received [");
  Serial.print(topic);
  Serial.print("]: ");
  for (int i = 0; i < length; i++)
  {
    Serial.print((char)payload[i]);
  }

  if (strstr(topic, "devicebound") != NULL)
  {
    DynamicJsonDocument doc(1024);
    deserializeJson(doc, payload);

    uint8_t red = doc["Red"];
    uint8_t green = doc["Green"];
    uint8_t blue = doc["Blue"];

    setLedStripColor(red, green, blue);
  }

}

// Auxiliary functions

static void connectToWiFi()
{
  Serial.begin(115200);
  Serial.println();
  Serial.print("Connecting to WIFI SSID ");
  Serial.println(ssid);

  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  Serial.print("WiFi connected, IP address: ");
  Serial.println(WiFi.localIP());
}

static void initializeClients()
{
  az_iot_hub_client_options options = az_iot_hub_client_options_default();
  options.user_agent = AZ_SPAN_FROM_STR(AZURE_SDK_CLIENT_USER_AGENT);

  wifi_client.setTrustAnchors(&cert);
  if (az_result_failed(az_iot_hub_client_init(
          &client,
          az_span_create((uint8_t*)host, strlen(host)),
          az_span_create((uint8_t*)device_id, strlen(device_id)),
          &options)))
  {
    Serial.println("Failed initializing Azure IoT Hub client");
    return;
  }

  mqtt_client.setServer(host, port);
  mqtt_client.setCallback(receivedCallback);
}


static int generateSasToken(char* sas_token, size_t size)
{
  az_span signature_span = az_span_create((uint8_t*)signature, sizeofarray(signature));
  az_span out_signature_span;
  az_span encrypted_signature_span
      = az_span_create((uint8_t*)encrypted_signature, sizeofarray(encrypted_signature));

  uint32_t expiration = getSecondsSinceEpoch() + ONE_HOUR_IN_SECS;

  // Get signature
  if (az_result_failed(az_iot_hub_client_sas_get_signature(
          &client, expiration, signature_span, &out_signature_span)))
  {
    Serial.println("Failed getting SAS signature");
    return 1;
  }

  // Base64-decode device key
  int base64_decoded_device_key_length
      = base64_decode_chars(device_key, strlen(device_key), base64_decoded_device_key);

  if (base64_decoded_device_key_length == 0)
  {
    Serial.println("Failed base64 decoding device key");
    return 1;
  }

  // SHA-256 encrypt
  br_hmac_key_context kc;
  br_hmac_key_init(
      &kc, &br_sha256_vtable, base64_decoded_device_key, base64_decoded_device_key_length);

  br_hmac_context hmac_ctx;
  br_hmac_init(&hmac_ctx, &kc, 32);
  br_hmac_update(&hmac_ctx, az_span_ptr(out_signature_span), az_span_size(out_signature_span));
  br_hmac_out(&hmac_ctx, encrypted_signature);

  // Base64 encode encrypted signature
  String b64enc_hmacsha256_signature = base64::encode(encrypted_signature, br_hmac_size(&hmac_ctx));

  az_span b64enc_hmacsha256_signature_span = az_span_create(
      (uint8_t*)b64enc_hmacsha256_signature.c_str(), b64enc_hmacsha256_signature.length());

  // URl-encode base64 encoded encrypted signature
  if (az_result_failed(az_iot_hub_client_sas_get_password(
          &client,
          expiration,
          b64enc_hmacsha256_signature_span,
          AZ_SPAN_EMPTY,
          sas_token,
          size,
          NULL)))
  {
    Serial.println("Failed getting SAS token");
    return 1;
  }

  return 0;
}

static int connectToAzureIoTHub()
{
  size_t client_id_length;
  char mqtt_client_id[128];
  if (az_result_failed(az_iot_hub_client_get_client_id(
          &client, mqtt_client_id, sizeof(mqtt_client_id) - 1, &client_id_length)))
  {
    Serial.println("Failed getting client id");
    return 1;
  }

  mqtt_client_id[client_id_length] = '\0';

  char mqtt_username[128];
  // Get the MQTT user name used to connect to IoT Hub
  if (az_result_failed(az_iot_hub_client_get_user_name(
          &client, mqtt_username, sizeofarray(mqtt_username), NULL)))
  {
    printf("Failed to get MQTT clientId, return code\n");
    return 1;
  }

  Serial.print("Client ID: ");
  Serial.println(mqtt_client_id);

  Serial.print("Username: ");
  Serial.println(mqtt_username);

  mqtt_client.setBufferSize(MQTT_PACKET_SIZE);

  while (!mqtt_client.connected())
  {
    time_t now = time(NULL);

    Serial.print("MQTT connecting ... ");

    if (mqtt_client.connect(mqtt_client_id, mqtt_username, sas_token))
    {
      Serial.println("connected.");
    }
    else
    {
      Serial.print("failed, status code =");
      Serial.print(mqtt_client.state());
      Serial.println(". Trying again in 5 seconds.");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }

  mqtt_client.subscribe(AZ_IOT_HUB_CLIENT_C2D_SUBSCRIBE_TOPIC);

  return 0;
}

static void establishConnection() 
{
  connectToWiFi();
  initializeTime();
  printCurrentTime();
  initializeClients();

  // The SAS token is valid for 1 hour by default in this sample.
  // After one hour the sample must be restarted, or the client won't be able
  // to connect/stay connected to the Azure IoT Hub.
  if (generateSasToken(sas_token, sizeofarray(sas_token)) != 0)
  {
    Serial.println("Failed generating MQTT password");
  }
  else
  {
    connectToAzureIoTHub();
  }
}


// Arduino setup and loop main functions.

void setup()
{
  Serial.begin(115200);

  // Setup fastled
  FastLED.addLeds<LED_TYPE, LED_PIN, COLOR_ORDER>(leds, NUM_LEDS).setCorrection( TypicalLEDStrip );
  FastLED.setBrightness(255);

  // Set it ro red as default color
  setLedStripColor(255,0,0);

  // Initialize temp sensor
  dht.begin();

  establishConnection();

  Serial.print("Epoch time in seconds: ");
  Serial.println(getSecondsSinceEpoch());
}

void loop()
{

  if (millis() > next_telemetry_send_time_ms)
  {
    // Check if connected, reconnect if needed.
    if(!mqtt_client.connected())
    {
      establishConnection();
    }

    sendTelemetryPayload();
    next_telemetry_send_time_ms = millis() + TELEMETRY_FREQUENCY_MILLISECS;
  }

  // MQTT loop must be called to process Device-to-Cloud and Cloud-to-Device.
  mqtt_client.loop();
  delay(100);
}


