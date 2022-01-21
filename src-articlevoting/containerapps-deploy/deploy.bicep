module cosmos 'deploy-cosmos.bicep' = {
  name: 'jjcosmos'
  params:{
    cosmosName: 'jjcosmos'
  }
}

module st 'deploy-storage.bicep' = {
  name: 'jjstoragedapr'
  params:{
    stName: 'jjstoragedapr'
  }
}

module sb 'deploy-sb.bicep' = {
  name: 'jjsbus'
  params:{
    sbName: 'jjsbus'
  }
}

module app 'deploy-app.bicep' = {
  name: 'jjapp'
  params: {
    appName: 'jjarticlevoting'
    imageRegistryName: 'jjakscontainers'
    imageArticles: 'api-articles:v1'
    imageVotes: 'api-votes:v1'
    cosmosAccountName: cosmos.outputs.cosmosAccountName
    sbNamespaceName: sb.outputs.sbNamespaceName
    stAccountName: st.outputs.stAccountName
    location: 'northeurope' // location must be harcoded for now because of preview
  }
}
