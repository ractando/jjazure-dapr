param stName string = 'jjstoragedaprca'

param location string = resourceGroup().location

resource storage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: stName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

output stAccountName string = storage.name
