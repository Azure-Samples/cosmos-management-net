---
page_type: sample
languages:
- csharp
products:
- dotnet
description: "Azure Management Libraries for Azure Cosmos DB"
urlFragment: "cosmos-management-net"
---

# Azure Management Libraries for .NET for Azure Cosmos DB

The Azure Management Libraries for .NET for Azure Cosmos DB is a object-oriented API for managing Azure Cosmos DB resources via it's Resource Provider.
There are two projects and two versions of this API. A project of samples built on our auto-generated libraries built against the Swagger spec on the REST API
for the Cosmos DB Resource Provider, and another fluent-style API that is built upon the auto-generated libary. 

Please note that the fluent interfaces are not fully complete and both the generated and fluent libraries are themselves Preview releases.

## Contents

Outline the file contents of the repository. It helps users navigate the codebase, build configuration and any related assets.

| File/folder                  | Description                                |
|------------------------------|--------------------------------------------|
| `cosmos-management-fluent`   | Cosmos DB fluent management samples.       |
| `cosmos-management-generated`| Cosmos DB generated management samples.    |


## Prerequisites

None

## Setup

To get started you will need to generate a service principal in Azure. Add the required information needed in appSettings.json or authenticate using MSI with the 
proper RBAC on the Cosmos resources to manage and modify the authentication in the samples.

## Running the sample

Set the start up project, ensure you have your service principal credentials in appSettings.json. 
To learn more see, [Authentication in Azure Management Libraries for .NET](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md)

## Key concepts

These samples demonstrate how to manage Cosmos DB resources through it's Control Plane (Resource Provider). You cannot do any database CRUD operations using these samples

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
