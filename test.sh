#!/usr/bin/env bash

set -e

cd azure

try_package_dir() {
    group="$3"
    version=$2
    package="$1"
    attempt="packages/$group/$package/lib/$version"
    if [[ -d "$attempt" ]]; then
        echo "$attempt"
    else
        echo ""
    fi
}

build_refs_in_group() {
    group="$1"
    ref_args=""

    for package in $(ls packages/$group); do
        VERSIONS=(net45 net40 net461)
        for version in ${VERSIONS[@]}; do
            dir="$(try_package_dir "$package" "$version" "$group")"
            if [[ ! -z $dir ]]; then
                package_dir="$dir"
            else
                continue
            fi
        done

        # package_dir="$(try_package_dir "$package" net45)" || "$(try_package_dir "$package" net40)" || "$(try_package_dir "$package" net461)"
        if [[ -z "$package_dir" ]]; then
            echo "Could not find dir for package: $package"
            exit 1;
        fi
        ref_args+="--lib:$package_dir "
    done
    echo $ref_args
}

echo "here it is:"
libs="$(build_refs_in_group "testing")"
echo $libs
fsharpc \
    --target:exe \
    --out:bin/MoneyAlarms.Test.exe \
    --standalone \
    $libs \
    --lib:"$(pwd)/bin" \
    -r Expecto \
    -r MoneyAlarms \
    test/*.fs

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
