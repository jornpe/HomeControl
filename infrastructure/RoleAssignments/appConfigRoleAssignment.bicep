@description('Name of the app configuration resource')
param appConfigStoreName string

@description('Pricipal IDs to resources that recuire read access')
param principalId string

resource appconfigStore 'Microsoft.AppConfiguration/configurationStores@2022-05-01' existing = {
  name: appConfigStoreName
}

// Role name: "App Configuration Data Reader" - Allows read access to App Configuration data. https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#app-configuration-data-reader
var appConfigurationDataReaderId = '516239f1-63e1-4d78-a4de-a74fb236a071'
resource appConfigurationDataReader 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: appConfigurationDataReaderId
}

resource appConfigurationDataReaderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(appConfigurationDataReader.id, principalId, appconfigStore.id)
  scope: appconfigStore
  properties: {
    principalId: principalId
    roleDefinitionId: appConfigurationDataReader.id
    principalType: 'ServicePrincipal'
  }
}
