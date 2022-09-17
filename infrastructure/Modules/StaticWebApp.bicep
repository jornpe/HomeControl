param repositoryUrl string
param websiteName string
param tags object
param appInsightInstrumantionKey string
param location string
param branch string = 'main'
param provider string = 'GitHub'


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
    name: 'Free'
    tier: 'Free'
  }
}

resource symbolicname 'Microsoft.Web/staticSites/config@2022-03-01' = {
  name: 'appsettings'
  kind: 'string'
  parent: website
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsightInstrumantionKey
  }
}
