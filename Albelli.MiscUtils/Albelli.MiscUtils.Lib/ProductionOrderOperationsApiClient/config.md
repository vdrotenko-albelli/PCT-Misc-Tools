> see https://aka.ms/autorest
## Configuration 
The following are the settings for this using this API with AutoRest.
``` yaml
input-file: docs.json
directive:
  - from: source-file-csharp
    where: $
    transform: >
     return $.replace( /(\s+)BaseUri = new System.Uri.*/g, "");
csharp:
  namespace: ProductionOrderOperationsApi
  output-folder: .
  output-file: ProductionOrderOperationsApiClient.cs
  add-credentials: true
  override-client-name: ProductionOrderOperationsApiClient
  sync-methods: none
  license-header: "Copyright © Albelli BV 2023. Please use .\\generateClient.ps1 to regenerate this client"
  use-datetimeoffset: true
```