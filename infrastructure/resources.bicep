param repositoryUrl string
@description('Name of the application to be deployed')
param application string
@description('Tags to add the all resources that is being deployed')
param tags object
@description('Location to use for the resources')
param location string

var appInsightName = 'appi-${application}'
var websiteName = 'site-${application}'
var appServicePlanName = 'plan-${application}'
var functionAppName = 'fnapp-${application}'
var iotHubName = 'iot-${application}'
var appConfigName = 'appc-${application}'
var storageAccountName = '${toLower(application)}${uniqueString(resourceGroup().id)}'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightName
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'web'
  }
  tags: tags
}

module appConfig 'Modules/appConfig.bicep' = {
  name: 'AppConfigStore'
  params: {
    configStoreName: appConfigName
    location: location
    tags: tags
  }
}

module iotHub 'Modules/IotHub.bicep' = {
  name: 'IotHub'
  params: {
    configStoreName: appConfigName
    iotHubName: iotHubName
    location: location
    tags: tags
  }
  dependsOn:[
    appConfig
  ]
}

module website 'Modules/StaticWebApp.bicep' = {
  name: 'webSite'
  params: {
    tags: tags
    repositoryUrl: repositoryUrl
    location: location
    appInsightInstrumantionKey: appInsights.properties.InstrumentationKey
    websiteName: websiteName
  }
}

module functionApp 'Modules/Function.bicep' = {
  name: functionAppName
  params: {
    appInsightInstrumantionKey: appInsights.properties.InstrumentationKey
    functionAppName: functionAppName
    hostingPlanName: appServicePlanName
    storageAccountName: storageAccountName
    appConfigStoreEndpoint: appConfig.outputs.appConfigEndpoint
    allowedOrigins: [ website.outputs.uri ]
    location: location
    tags: tags
  }
}

module iotHubRoleAssignment 'RoleAssignments/iotHubRoleAssignments.bicep' = {
  name: 'IotHubRoleAssignment'
  params: {
    iotHubName: iotHubName
    principalId: functionApp.outputs.procipleId
  }
  dependsOn: [
    iotHub
    functionApp
  ]
}

module appConfigRoleAssignment 'RoleAssignments/appConfigRoleAssignment.bicep' = {
  name: 'AppConfigRoleAssignment'
  params: {
    appConfigStoreName: appConfigName
    principalId: functionApp.outputs.procipleId
  }
  dependsOn: [
    appConfig
    functionApp
  ]
}
