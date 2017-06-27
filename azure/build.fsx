// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open System.IO

// Directories
let buildDir  = "./build/"
let releaseDir = "./bin/"
let deployDir = "./deploy/"

let azureFunctions = !! "*/function.json"

// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

let releaseReferences = appReferences -- "MoneyAlarms.Test/*"
// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir; releaseDir]
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "Release" (fun _ ->
    MSBuildRelease releaseDir "Build" releaseReferences
    |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
    -- "*.zip"
    |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

Target "PrepareFunctionsBinsDebug" (fun _ ->
    let files = seq { for file in (!! (buildDir + "/*.dll")) do yield file }
    printfn "Copying %A" files
    for func in azureFunctions do
        Copy ((DirectoryName func) + "/bin/") files
)

Target "PrepareFunctionsBins" (fun _ ->
    let files = seq { for file in (!! (releaseDir + "/*.dll") ++ (releaseDir + "/*.dll.mdb")) do yield file }
    printfn "Copying %A" files
    for func in azureFunctions do
        Copy ((DirectoryName func) + "/bin/") files
)
// Build order
"Clean"
  ==> "Build"
  ==> "Deploy"

"Clean"
  ==> "Release"
  ==> "PrepareFunctionsBins"

// start build
RunTargetOrDefault "Build"
