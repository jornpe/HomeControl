@description('Name of the app configuration resource')
param storageAccountName string

@description('Pricipal IDs to resources that recuire read access')
param principalId string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: storageAccountName
}

// Role name: "Storage Table Data Contributor" - Allows read access to App Configuration data. https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#app-configuration-data-reader
var StorageTableDataContributorId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
resource StorageTableDataContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: StorageTableDataContributorId
}

resource appConfigurationDataReaderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(StorageTableDataContributor.id, principalId, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: principalId
    roleDefinitionId: StorageTableDataContributor.id
    principalType: 'ServicePrincipal'
  }
}
