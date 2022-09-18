@description('Name of the app configuration resource')
param configStoreName string
@description('Location to use for the resources')
param location string
@description('Tags to tag the resources with')
param tags object

resource appconfigStore 'Microsoft.AppConfiguration/configurationStores@2022-05-01' = {
  name: configStoreName
  location: location
  sku: {
    name: 'free'
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: tags
}

output appConfigEndpoint string = appconfigStore.properties.endpoint
