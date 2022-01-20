param appName string = 'jjarticlevoting'
param envName string = '${appName}-env'
param imageRegistryName string
param imageArticles string
param imageVotes string

param cosmosAccountName string
param sbNamespaceName string

param logName string = 'jjdev-analytics'
param logResourceGroupName string = 'jjdevmanagement'

param location string = resourceGroup().location

// Reference existing resources:
//    - Log Analytics workspace
//    - Container Registry
//    - Cosmos DB account
//    - ServiceBus namespace with topic
resource log 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logName
  scope: resourceGroup(logResourceGroupName)
}

resource acr 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' existing = {
  name: imageRegistryName
}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2021-10-15' existing = {
  name: cosmosAccountName
} 

resource sbNamespace 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' existing = {
  name: sbNamespaceName
}
resource sbAuthorization 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2021-06-01-preview' = {
  name: 'RootManageSharedAccessKey'
  parent: sbNamespace
  properties: {
    rights: [
      'Listen'
      'Send'
      'Manage'
    ]
  }
}

// Create Container App Environment
resource env 'Microsoft.Web/kubeEnvironments@2021-02-01' = {
  name: envName
  location: location
  properties: {
    type: 'managed'
    internalLoadBalancerEnabled: false
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: log.properties.customerId
        sharedKey: log.listKeys().primarySharedKey
      }
    }
  }
}

// Create Container App: Articles
resource appArticles 'Microsoft.Web/containerapps@2021-03-01' = {
  name: '${appName}-articles'
  kind: 'containerapp'
  location: location
  properties: {
    kubeEnvironmentId: env.id
    configuration: {
      ingress: {
        external: true
        targetPort: 5005
      }
      secrets: [
        {
          name: 'registry-pwd'          
          value: acr.listCredentials().passwords[0].value
        }
        {
          name: 'cosmos-key'
          value: cosmosAccount.listKeys().primaryMasterKey
        } 
        {
          name: 'sb-conn'
          value: sbAuthorization.listKeys().primaryConnectionString          
        }       
      ]
      registries: [
        {
          server: acr.properties.loginServer
          username: acr.listCredentials().username
          passwordSecretRef: 'registry-pwd'
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${acr.properties.loginServer}/${imageArticles}'
          name: 'api-articles'
          resources: {
            cpu: '.25'
            memory: '.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      dapr: {
        enabled: true
        appPort: 5005
        appId: 'api-articles'
        components: [
          {
            name: 'jjstate-articles'
            type: 'state.azure.cosmosdb'
            version: 'v1'
            metadata: [
              {
                name: 'url'
                value: cosmosAccount.properties.documentEndpoint
              }
              {
                name: 'masterKey '
                secretRef: 'cosmos-key'
              }
              {
                name: 'database'
                value: 'jjdb'
              }
              {
                name: 'collection'
                value: 'articles'
              }
            ]
          }
          {
            name: 'pubsub'
            type: 'pubsub.azure.servicebus'
            version: 'v1'
            metadata: [
              {
                name: 'connectionString'
                secretRef: 'sb-conn'
              }
            ]
          }
        ]
      }
    }
  }
}

// TODO: doplnit api-votes s imageVotes
