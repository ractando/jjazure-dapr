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
dapr run --app-id app-votes --port 5020 --app-port 5000 dotnet run --components-path ./components
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

## Create project for API Articles

Follow same steps as for API Votes, configure state store for CosmosDB - articles container

Dapr Publish/Subscribe

- https://github.com/dapr/docs/tree/master/howto/publish-topic
- https://github.com/dapr/quickstarts/tree/master/pub-sub/react-form

Run from commandline or from VS Code Launch task Dapr-Debug (changed default dotnet kestrel port to 5005)

```powershell
cd api-articles
dapr run --app-id app-articles --port 5030 --app-port 5005 dotnet run --components-path ./components
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
