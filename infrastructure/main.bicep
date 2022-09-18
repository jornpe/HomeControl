targetScope = 'subscription'
@description('URL to the github repository where all the source code is located')
param repositoryUrl string = 'https://github.com/jornpe/HomeProject'
@description('Name of the project, used in most resource names')
param application string = 'homeproject'
@description('Location to use for the resources') // Default westeurope as static websites are not supported in norwayeast as of 18-09-22
param location string = 'westeurope'
@description('Name of the resource group to deploy all resources to')
param rgName string = 'rg-${application}-${location}-001'

@description('Date and time in this format: ')
param dateTime string = dateTimeAdd(utcNow('F'), 'PT2H') // Could not find a solution to get correct time zone so added 2 hours. 

@description('Tags to add the all resources that is being deployed')
param tags object = {
  CreationDate: dateTime
  Application: application
}

resource homeRg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
  tags: tags
}

module HomeProject 'Home.bicep' = {
  scope: homeRg
  name: 'HomeReourceDeployment'
  params: {
    repositoryUrl: repositoryUrl
    tags: tags
    location: location
    application: application
  }
}
