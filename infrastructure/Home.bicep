param repositoryUrl string
@description('Name of the application to be deployed')
param application string
@description('Tags to add the all resources that is being deployed')
param tags object
@description('Location to use for the resources')
param location string

var websiteName = 'HomeWebsite'

var appInsightName = 'appi-${application}'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightName
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'web'
  }
  tags: tags
}

module website 'Modules/StaticWebApp.bicep' = {
  name: 'webSite'
  params: {
    tags: tags
    websiteName: websiteName
    repositoryUrl: repositoryUrl
    location: 'westeurope'
    appInsightInstrumantionKey: appInsights.properties.InstrumentationKey
  }
}



