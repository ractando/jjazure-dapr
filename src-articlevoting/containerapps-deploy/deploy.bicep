module cosmos 'deploy-cosmos.bicep' = {
  name: 'jjcosmosca'
  params:{
    cosmosName: 'jjcosmosca'
  }
}

module st 'deploy-storage.bicep' = {
  name: 'jjstoragedaprca'
  params:{
    stName: 'jjstoragedaprca'
  }
}

module sb 'deploy-sb.bicep' = {
  name: 'jjsbusca'
  params:{
    sbName: 'jjsbusca'
  }
}

module app 'deploy-app.bicep' = {
  name: 'jjappca'
  params: {
    appName: 'jjarticlevotingca'
    imageRegistryName: 'jjakscontainersne'
    imageArticles: 'api-articles:v1'
    imageVotes: 'api-votes:v1'
    cosmosAccountName: cosmos.outputs.cosmosAccountName
    sbNamespaceName: sb.outputs.sbNamespaceName
    stAccountName: st.outputs.stAccountName
    location: 'northeurope' // location must be harcoded for now because of preview
  }
}
