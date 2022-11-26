param zoneName string
param defaultHostname string
param staticSiteName string

resource staticWebSite 'Microsoft.Web/staticSites@2022-03-01' existing =  {
  name: staticSiteName
}

resource zone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: zoneName
  location: 'global'

  resource wwwRecord 'CNAME@2018-05-01' = {
    name: 'www'
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: defaultHostname
      }
    }
  }

  resource aRecord 'A@2018-05-01' = {
    name: '@'
    properties: {
      targetResource: {
        id: staticWebSite.id
      }
    }
  }
}
