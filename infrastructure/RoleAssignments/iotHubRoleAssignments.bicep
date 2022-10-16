param principalId string
param iotHubName string

resource iotHub 'Microsoft.Devices/IotHubs@2022-04-30-preview' existing = {
  name: iotHubName
}

// Role name: "IoT Hub Data Contributor" - Allows for full access to IoT Hub data plane operations. https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#iot-hub-data-contributor
var iotHubDataContributorId = '4fc6c259-987e-4a07-842e-c321cc9d413f'
resource iotHubDataContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: iotHubDataContributorId
}

resource iotHubDataContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(iotHubDataContributor.id, principalId, iotHub.id)
  scope: iotHub
  properties: {
    principalId: principalId
    roleDefinitionId: iotHubDataContributor.id
    principalType: 'ServicePrincipal'
  }
}

// Role name: "IoT registry Contributor" - Allows for full access to IoT Hub device registry. https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#iot-hub-registry-contributor
var iotHubRegistryContributorId = '4ea46cd5-c1b2-4a8e-910b-273211f9ce47'
resource iotHubRegistryContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: iotHubRegistryContributorId
}

resource iotHubRegistryContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(iotHubRegistryContributor.id, principalId, iotHub.id)
  scope: iotHub
  properties: {
    principalId: principalId
    roleDefinitionId: iotHubRegistryContributor.id
    principalType: 'ServicePrincipal'
  }
}

// Role name: "IoT Hub Twin Contributor" - Allows for read and write access to all IoT Hub device and module twins. https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#iot-hub-twin-contributor
var iotHubTwinContributorId = '494bdba2-168f-4f31-a0a1-191d2f7c028c'
resource iotHubTwinContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: iotHubTwinContributorId
}

resource iotHubTwinContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(iotHubTwinContributor.id, principalId, iotHub.id)
  scope: iotHub
  properties: {
    principalId: principalId
    roleDefinitionId: iotHubTwinContributor.id
    principalType: 'ServicePrincipal'
  }
}
