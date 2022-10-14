param appInsightName string
param exceptionAlertName string = 'Exceptions'
param location string
param tags object

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightName
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'web'
  }
  tags: tags
}

resource exceptionAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: exceptionAlertName
  location: 'global'
  properties: {
    description: 'Exceptions from web api or web app'
    severity: 0
    enabled: true
    scopes: [
      appInsights.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'Metric1'
          metricName: 'exceptions/count'
          operator: 'GreaterThan'
          threshold: 1
          timeAggregation: 'Count'
          criterionType: 'StaticThresholdCriterion'
          metricNamespace: 'microsoft.insights/components'
        }
      ]
    }
  }
}

output InstrumentationKey string = appInsights.properties.InstrumentationKey
