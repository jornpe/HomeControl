param repositoryUrl string
@description('Name of the application to be deployed')
param application string
@description('Tags to add the all resources that is being deployed')
param tags object
@description('Location to use for the resources')
param location string

var websiteName = 'HomeWebsite'

var appInsightName = 'appi-${application}'
var functionAppName = 'fnapp-${application}'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightName
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'web'
  }
  tags: tags
}

module functionApp 'Modules/Function.bicep' = {
  name: functionAppName
  params: {
    appInsightInstrumantionKey: appInsights.properties.InstrumentationKey
    appName: application
    location: location
    tags: tags
  }
}

module website 'Modules/StaticWebApp.bicep' = {
  name: 'webSite'
  params: {
    tags: tags
    websiteName: websiteName
    repositoryUrl: repositoryUrl
    location: location
    functionAppEndpoint: functionApp.outputs.functionEndpoint
    appInsightInstrumantionKey: appInsights.properties.InstrumentationKey
  }
}



