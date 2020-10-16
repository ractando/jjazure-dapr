# JJ Article Voting application

This application records likes for article. Number of article votes calculates async. It's using Dapr.

Services

- API Like - saves like into Cosmos and publish event
- API LikeProcess - process like and counts number of votes

How to use Dapr https://github.com/dapr/docs/tree/master/howto

## Create project for API Like

```dotnetcli
dotnet new webapi -n api-like
```

Open VS Code in new folder api-like.

- Add defautl dotnet core launch task
- Run Dapr scaffold task and add Dapr ID api-like

Modify code based on this sample

- https://dev.to/mkokabi/learning-dapr-simple-dotnet-core-hello-world-b0k
- https://github.com/dapr/dotnet-sdk/tree/master/samples/AspNetCore/ControllerSample

Configure state store for CosmosDB

- https://github.com/dapr/docs/blob/master/howto/setup-state-store/setup-azure-cosmosdb.md

Run from commandline or from VS Code Launch task Dapr-Debug

```powershell
cd api-like
dapr run --app-id app-like --port 5020 --app-port 5000 dotnet run --components-path ./components
```

Test Hello call

```powershell
curl http://localhost:5020/v1.0/invoke/app-like/method/hello
```

Test like

```powershell
curl -X POST http://localhost:5000/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
curl -X POST http://localhost:5020/v1.0/invoke/app-like/method/like -H "Content-Type: application/json" -d '{ \"articleid\": \"1\", \"userid\": \"jj\" }'
```

## Create service for like processing

Dapr Publish/Subscribe sample - https://github.com/dapr/quickstarts/tree/master/pub-sub/react-form