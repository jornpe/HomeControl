@description('URL to the github repository where the project source is located')
param repositoryUrl string
@description('Name fo the website resource to create')
param websiteName string
@description('Instrumentation key for the ap insigts resource to send app logs to')
param appInsightInstrumantionKey string
@description('Location to use for the resources')
param location string
@description('Tags to tag the resources with')
param tags object

var branch = 'main'
var provider = 'GitHub'
var tier = 'Free'


resource website 'Microsoft.Web/staticSites@2022-03-01' = {
  name: websiteName
  location: location
  tags: tags
  properties: {
    provider: provider
    repositoryUrl: repositoryUrl
    branch: branch
  }
  sku: {
    name: tier
    tier: tier
  }

  resource siteConfig 'config@2022-03-01' = {
    name: 'appsettings'
    kind: 'string'
    properties: {
      APPINSIGHTS_INSTRUMENTATIONKEY: appInsightInstrumantionKey
    }
  }
}

output staticSiteName string = website.name
output defaultHostname string = website.properties.defaultHostname
