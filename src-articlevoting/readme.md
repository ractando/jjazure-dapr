# JJ Article Voting application

This application records likes for article. Number of article votes calculates async. It's using Dapr.

Services

- API Votes - saves like into Cosmos and publish event
- API Articles - process like and counts number of votes

How to use Dapr https://github.com/dapr/docs/tree/master/howto

## Create Azure resources

Create Azure CosmosDB and configure connection string and credentials in dapr components folder.

You can use [arm-deploy](/arm-deploy) scripts.

## Create project for API Votes

```dotnetcli
dotnet new webapi -n api-votes
```

Open VS Code in new folder api-votes.

- Add defautl dotnet core launch task
- Run Dapr scaffold task and add Dapr ID api-votes

Modify code based on this sample

- https://dev.to/mkokabi/learning-dapr-simple-dotnet-core-hello-world-b0k
- https://github.com/dapr/dotnet-sdk/tree/master/samples/AspNetCore/ControllerSample

Configure state store for CosmosDB - votes container

- https://github.com/dapr/docs/blob/master/howto/setup-state-store/setup-azure-cosmosdb.md

Run from commandline or from VS Code Launch task Dapr-Debug

```powershell
cd api-votes
dapr run --app-id app-votes --dapr-http-port 5020 --app-port 5000 dotnet run --components-path ./components
```

Test Hello call

```powershell
curl http://localhost:5020/v1.0/invoke/app-votes/method/hello
```

Test like

```powershell
curl -X POST http://localhost:5000/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
curl -X POST http://localhost:5020/v1.0/invoke/app-votes/method/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
curl -X POST http://localhost:5020/v1.0/invoke/app-votes/method/like -H "Content-Type: application/json" -d '{ \"articleid\": \"2\", \"userid\": \"jj\" }'
```

```powershell
$i = 0
while ($i -ne 30) {
    curl -X POST http://localhost:5000/like -H "Content-Type: application/json" -d ('{ \"articleid\": \"1\", \"userid\": \"jj' + $i + '\" }')
    $i++
    #Start-Sleep -Seconds 1
}
```

## Create project for API Articles

Follow same steps as for API Votes, configure state store for CosmosDB - articles container

Configure state store for CosmosDB - articles container

- https://github.com/dapr/docs/blob/master/howto/setup-state-store/setup-azure-cosmosdb.md
- https://docs.dapr.io/developing-applications/building-blocks/state-management/state-management-overview/#concurrency

Dapr Publish/Subscribe

- https://github.com/dapr/docs/tree/master/howto/publish-topic
- https://github.com/dapr/quickstarts/tree/master/pub-sub/react-form

Run from commandline or from VS Code Launch task Dapr-Debug (changed default dotnet kestrel port to 5005)

```powershell
cd api-articles
dapr run --app-id app-articles --dapr-http-port 5030 --app-port 5005 dotnet run --components-path ./components
```

Test Hello call

```powershell
curl http://localhost:5030/v1.0/invoke/app-articles/method/hello
```

Create article and publish message with new vote

```powershell

curl -X POST http://localhost:5000/create -H "Content-Type: application/json" -d '{ \"articleid\": \"1\" }'
curl -X POST http://localhost:5030/v1.0/invoke/app-articles/method/create -H "Content-Type: application/json" -d '{ \"articleid\": \"1\" }'

curl -X POST http://localhost:5030/v1.0/publish/pubsub/likeprocess -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
dapr publish --topic likeprocess --pubsub pubsub --data '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
```

## Deploy into Azure Kubernetes Service (AKS)

We will use this Azure components

- Azure Azure Kubernetes Service (AKS)
- Azure Container Service
- Azure CosmosDB - name jjcosmos
- Azure ServiceBus - name jjsbus tier Standard with Topic likeprocess

Install Dapr into AKS https://docs.dapr.io/getting-started/install-dapr-kubernetes/

Configuration prepared by this sample https://github.com/dapr/quickstarts/tree/release-0.11/hello-kubernetes/deploy

Configuration for pubsub Azure Service Bus Topics https://docs.dapr.io/operations/components/setup-pubsub/supported-pubsub/setup-azure-servicebus/

```powershell
dapr init -k

kubectl get pods -n dapr-system
dapr dashboard -k
```

Modify security keys and create state stores with pubsubs.

```powershell
kubectl apply -f ./aks-deploy/jjstate-votes.yaml
kubectl apply -f ./aks-deploy/jjstate-articles.yaml
kubectl apply -f ./aks-deploy/pubsub.yaml
```

Build and deploy API Votes 

```powershell
docker build -t api-votes ./api-votes
docker tag api-votes jjakscontainers.azurecr.io/api-votes:v1
docker push jjakscontainers.azurecr.io/api-votes:v1
kubectl apply -f ./aks-deploy/api-votes.yaml
```

Check API Votes is running http://<your_ip>/hello

Build and deploy API Articles

```powershell
docker build -t api-articles ./api-articles
docker tag api-articles jjakscontainers.azurecr.io/api-articles:v1
docker push jjakscontainers.azurecr.io/api-articles:v1
kubectl apply -f ./aks-deploy/api-articles.yaml
```

Send test data and check Cosmos DB for results

```powershell
curl -X POST http://<YOUR_IP>/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
```

## Deploy into Azure Container Apps

Check this documentation https://docs.microsoft.com/en-us/azure/container-apps/microservices-dapr-azure-resource-manager?tabs=bash&pivots=container-apps-bicep

Bicep deployment using 2 modules
- CosmosDb module for data persistency
- ServiceBus module for pubsub functionality
- App module for Container App

Prerequisite is pushed image into Azure Container Registry

```powershell
cd containerapps-deploy
.\deploy.ps1
```

Test Hello call

```powershell
curl https://<CONTAINERAPP_URL>/hello
```

Test like

```powershell
curl -X POST http://<CONTAINERAPP_URL>/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
```

curl -X POST https://jjarticlevoting-votes.calmbush-5cf2c954.northeurope.azurecontainerapps.io/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'

Monitoring - run following query

```kusto
ContainerAppConsoleLogs_CL | where ContainerAppName_s == 'jjarticlevoting-articles' | project ContainerAppName_s, Log_s, TimeGenerated | order by TimeGenerated
ContainerAppConsoleLogs_CL | where ContainerAppName_s == 'jjarticlevoting-votes' | project ContainerAppName_s, Log_s, TimeGenerated | order by TimeGenerated
```

I found problem with CosmosDB - missing MSI - error message: level=fatal msg="process component jjstate-articles error: the MSI endpoint is not available. Failed HTTP request to MSI endpoint: Get \"http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01\": context deadline exceeded" app_id=api-articles instance=jjarticlevoting-articles--2cfumef-6c67c9b97c-5qj4x scope=dapr.runtime type=log ver=edge

Submitted GitHub Issue https://github.com/MicrosoftDocs/azure-docs/issues/86703

