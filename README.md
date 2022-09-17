# HomeProject

Home Project is a system for controlling IOT devices in my home.
It will be using azure as the cloud provider and the necessary resource groups, resources and apps should eb deployed automatically using github actions.

Please feel free to comment, add PR's and fork are try it on your own. I will try to make this as easy as possible to setup with everything being automated and manual steps being documented.

## Motivations

My motivation for creating this project is 2 part, first I want to learn how to build the infrastructure in azure in a best practice and secure way. Second, I really like working with IOT devices, and I need to be able to open my garage from inside my car 😀

## Architecture overview

It will have a backend system for connecting to the IOT devices using azure IOT hub. The sensor data received to the IOT hub should be routed to a database. Then a static web app will be used as a frontend with azure functions to retrieve the data from the database.
Inside my home, the devices will also connect to apple HomeKit so that sensor data is available in the HomeKit system.

The devices are mainly being built up on the ESP8266 and ESP32 modules. I will be using platform IO with Arduino framework on these devices. Some of the devices I have plans to build are:

- Temperature and humidity sensors.
- Garage opener.
- Automatic watering of plants based on humidity in the soil they are planted in.
- ....and whatever more I come up with :)

## Deployment

See [github actions setup](./doc/GitHub.md) for more info about deployment using github actions.
