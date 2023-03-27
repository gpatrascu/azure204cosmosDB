az group create --location westeurope --name az204-cosmos-rg

az cosmosdb create --name az204-study-orders --resource-group az204-cosmos-rg

az cosmosdb keys list --name az204-study-orders --resource-group az204-cosmos-rg