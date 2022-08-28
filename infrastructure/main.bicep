targetScope = 'subscription'

param application string = 'HomeProject'
param location string = 'norwayeast'
param rgName string = 'rg-${application}-${location}-001'
param appConfigName string = 'appc-${application}-${location}-001'

@description('Date and time in this format: ')
param dateTime string = dateTimeAdd(utcNow('F'), 'PT2H') // Could not find a solution to get correct time zone so added 2 hours. 

@description('Tags to add the all resources that is being deployed')
param tags object = {
  CreationDate: dateTime
  Application: application
}

resource environmentRg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
  tags: tags
}



