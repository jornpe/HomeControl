@description('Specifies the name of the Azure Storage account.')
param storageAccountName string
@description('Name of the app config resource')
param configStoreName string
@description('Specifies the location in which the Azure Storage resources should be deployed.')
param location string

resource appconfigStore 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: configStoreName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  properties:{
    allowBlobPublicAccess: false
  }
  kind: 'StorageV2'
  identity: {
    type: 'SystemAssigned'
  }

  resource tableService 'tableServices' = {
    name: 'default'
  }
}

resource keyValueConfigPairs 'Microsoft.AppConfiguration/configurationStores/keyValues@2022-05-01' = {
  name: 'StorageAccountName'
  parent: appconfigStore
  properties: {
    value: storageAccount.name
  }
}
