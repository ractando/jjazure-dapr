param sbName string = 'jjsbusca'

param location string = resourceGroup().location

resource sbNamespace 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: sbName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource sbTopic 'Microsoft.ServiceBus/namespaces/topics@2021-06-01-preview' = {
  parent: sbNamespace
  name: 'likeprocess'
}

output sbNamespaceName string = sbNamespace.name
