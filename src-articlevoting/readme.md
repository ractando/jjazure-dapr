# JJ Article Voting application

This application records likes for article. Number of article votes calculates async.

Services

- API Like - saves like into Cosmos and publish event
- API LikeProcess - process like and counts number of votes

## Create project for API Like

```dotnetcli
dotnet new webapi -n api-like
```

Open VS Code in new folder api-like.

- Add defautl dotnet core launch task
- Run Dapr scaffold task and add Dapr ID api-like

Using CosmosDB binding - https://github.com/dapr/docs/blob/master/reference/specs/bindings/cosmosdb.md
