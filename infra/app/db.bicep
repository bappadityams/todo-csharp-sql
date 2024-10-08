param name string
param location string = resourceGroup().location
param tags object = {}

param databaseName string = ''
param keyVaultName string

param apiAppName string
param userAssignedManagedIdentityId string
param userassignedmanagedidentityName string
param userAssignedManagedIdentityClientId string
@secure()
param sqlAdminPassword string

// Because databaseName is optional in main.bicep, we make sure the database name is set here.
var defaultDatabaseName = 'Todo'
var actualDatabaseName = !empty(databaseName) ? databaseName : defaultDatabaseName

module sqlServer '../core/database/sqlserver/sqlserver.bicep' = {
  name: 'sqlserver'
  params: {
    name: name
    location: location
    tags: tags
    databaseName: actualDatabaseName
    keyVaultName: keyVaultName
    sqlAdminPassword: sqlAdminPassword
    apiAppName: apiAppName
    userassignedmanagedidentityName: userassignedmanagedidentityName
    userAssignedManagedIdentityId: userAssignedManagedIdentityId
    userAssignedManagedIdentityClientId: userAssignedManagedIdentityClientId
  }
}

output connectionStringKey string = sqlServer.outputs.connectionStringKey
output databaseName string = sqlServer.outputs.databaseName
output sqlServerFQDN string = sqlServer.outputs.sqlServerFQDN
