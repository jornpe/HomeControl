param domainNames array
param staticSiteName string

resource parent 'Microsoft.Web/staticSites@2022-03-01' existing =  {
  name: staticSiteName
}

resource domains 'Microsoft.Web/staticSites/customDomains@2022-03-01' = [for domainName in domainNames: {  
  name: domainName.domain
  parent: parent
  properties: {
    validationMethod: domainName.validationMethod
  }
}]
