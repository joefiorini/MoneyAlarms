#!/usr/bin/env bash

fsharpc \
    --target:library \
    --out:azure/bin/MoneyAlarms.dll \
    --standalone \
    --debug:pdbonly \
    azure/Utils.fs

# fsharpc \
#     --out:azure/bin/MoneyAlarms.dll \
#     --standalone \
#     --target:library \
#     --lib:packages/Newtonsoft.Json/lib/net45 \
#     --lib:packages/WindowsAzure.ServiceBus/lib/net45 \
#     --lib:packages/Microsoft.Azure.WebJobs/lib/net45 \
#     --lib:packages/Microsoft.Azure.WebJobs.Core/lib/net45 \
#     --lib:packages/WindowsAzure.Storage/lib/net45 \
#     --lib:packages/Microsoft.Data.Services.Client/lib/net40 \
#     --lib:packages/Microsoft.Data.OData/lib/net40 \
#     --lib:packages/Microsoft.Data.Edm/lib/net40 \
#     --lib:packages/Microsoft.Azure.KeyVault.Core/lib/net45 \
#     --lib:packages/System.Spatial/lib/net40 \
#     --warnaserror \
#     --debug:pdbonly \
#     --optimize \
#     -r System \
#     -r System.Core \
#     -r System.Net.dll \
#     -r System.Net.Http.dll \
#     -r System.Net.Http.Formatting.dll \
#     -r System.Web.dll \
#     -r System.Web.Http.dll \
#     -r System.Web.Extensions \
#     -r System.Web.Services \
#     -r Microsoft.ServiceBus.dll \
#     -r Newtonsoft.Json.dll \
#     -r Microsoft.Azure.WebJobs.Host.dll \
#     -r Microsoft.Azure.WebJobs.dll \
#     -r Microsoft.WindowsAzure.Storage.dll \
#     -r Microsoft.Data.Services.Client.dll \
#     -r Microsoft.Data.OData \
#     -r Microsoft.Data.Edm \
#     -r Microsoft.Azure.KeyVault.Core \
#     -r System.Spatial.dll \
#     azure/*.fs
