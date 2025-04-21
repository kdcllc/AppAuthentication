# Azure Container Apps MSI Authentication

The example includes a Python script that retrieves an access token from the Microsoft Entra identity endpoint.

Each Microsoft Azure SDK implementation is different for .NET set `AZURE_CLIENT_ID` to MSI principal ID. For .NET, set `AZURE_CLIENT_ID` to the service principal ID.

[stackoverflow-code base](https://stackoverflow.com/questions/62455511/error-getting-managed-identity-access-token-from-azure-function)

[User Managed Identity unable to get tokens - Unable to load the proper Managed Identity](https://github.com/microsoft/azure-container-apps/issues/442)
[Azure.Identity issue with Container Apps](https://github.com/microsoft/azure-container-apps/issues/325#issuecomment-1265380377)