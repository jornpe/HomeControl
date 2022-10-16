@description('Name of the Iot hub resource to create')
param iotHubName string
@description('Name of the app config resource')
param configStoreName string
@description('Location to use for the resources')
param location string
@description('Tags to tag the resources with')
param tags object

resource appconfigStore 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: configStoreName
}

resource iotHub 'Microsoft.Devices/IotHubs@2022-04-30-preview' = {
  name: iotHubName
  location: location
  sku: {
    name: 'F1'
    capacity: 1
  }
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
}

resource iotHubHostName 'Microsoft.AppConfiguration/configurationStores/keyValues@2022-05-01' = {
  name: 'iotHubHostName'
  parent: appconfigStore
  properties: {
    value: iotHub.properties.hostName
  }
}
