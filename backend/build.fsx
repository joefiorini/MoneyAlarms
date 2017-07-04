// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"
#r "./packages/testing/DotEnvFile/lib/net452/DotEnvFile.dll"

open Fake
open System
open System.IO
open DotEnvFile

// Directories
let buildDir  = "./Debug"
let releaseDir = "./Release"
let deployDir = "./deploy"
let dockerDir = "./docker"

let inline toMap kvps =
    kvps
    |> Seq.map (|KeyValue|)
    |> Map.ofSeq

let dotEnvPath = "../.env"
let dotEnvVars = DotEnvFile.LoadFile(dotEnvPath) |> toMap

let actions =
    [ "TestError"
      "TestSuccess"
      "TestNotFound"
      "ExchangeTokens"
      "PersistTransactions"
    ]

// Filesets
let appReferences  = !! "MoneyAlarms.Dispatch/*.fsproj"
    // !! "/libraries/**/*.csproj"
    // ++ "/libraries/**/*.fsproj"

let releaseReferences = appReferences -- "libraries/MoneyAlarms.Test/*"
// version info
let version = "0.1"  // or retrieve from CI server

type ActionName = string
type DockerImage = string
type IsWeb =
    | True
    | False
    | Raw

type WskParams =
    Map<string,string>

type WskActionMutation = ActionName * DockerImage * IsWeb * WskParams

type WskActionCommand =
    | Create of WskActionMutation
    | Update of WskActionMutation
    | Delete of ActionName

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

let makeDockerImage = sprintf "joefiorini/money-alarms:Dispatcher"

Target "BuildDockerImage" (fun _ ->
    Copy "docker/stage/" (!! "Debug/*")
    let result =
      ExecProcess (fun info ->
        info.FileName <- "docker"
        info.WorkingDirectory <- dockerDir
        info.Arguments <- (sprintf "build --tag %s ." makeDockerImage)
      ) (TimeSpan.FromMinutes 2.0)

    if result <> 0 then
      failwithf "Unable to build docker image: %s; returned: %i" makeDockerImage result
    else
      ignore result
)

Target "DeployDockerImage" (fun _ ->
      let result =
        ExecProcess (fun info ->
          info.FileName <- "docker"
          info.WorkingDirectory <- dockerDir
          info.Arguments <- sprintf "push %s" makeDockerImage
        ) (TimeSpan.FromMinutes 2.0)

      if result <> 0 then
        failwithf "Unable to push docker image: %s; returned: %i" makeDockerImage result
      else
        ignore result
)

Target "DeleteDockerImages"  (fun _ ->
    let result =
      ExecProcess (fun info ->
        info.FileName <- "docker"
        info.WorkingDirectory <- dockerDir
        info.Arguments <- sprintf "rmi -f %s" makeDockerImage
      ) (TimeSpan.FromMinutes 2.0)

    if result <> 0 then
      failwithf "Unable to delete docker image: %s; returned: %i" makeDockerImage result
    else
      ignore result
)

let msprintf fn s = sprintf "%s" <| fn s

let makeParamArg s key value =
    sprintf "%s --param %s %s" s key value

let buildWskMutation =
    fun (dockerImage, actionName, isWeb, paramValues) ->
        match isWeb with
          | True -> "--web yes"
          | Raw -> "--web raw"
          | _ -> ""
        |> sprintf
              "%s --docker %s %s %s"
              actionName
              dockerImage
              <| Map.fold makeParamArg "" paramValues

type RunWskAction = WskActionCommand -> Result<unit, int>
let runWskAction: RunWskAction =
    fun action ->
        let wskArgs =
          match action with
            | Create args ->
                let argStr = buildWskMutation args
                sprintf "create %s" argStr
            | Update args ->
                let argStr = buildWskMutation args
                sprintf "update %s" argStr
            | Delete actionName ->
                sprintf "delete %s" actionName

        let result =
            ExecProcess (fun info ->
              info.FileName <- "wsk"
              info.Arguments <- sprintf "action %s" wskArgs
            ) (TimeSpan.FromMinutes 2.0)

        if result <> 0 then
          Result.Error result
        else
          Ok ()

let paramsForAction actionName =
    Map.add "action_name"  actionName dotEnvVars

let runActionMutation t actions =
  List.iter (fun actionName ->
    let result =
      runWskAction <|
        t (makeDockerImage, actionName, True, paramsForAction actionName)

    match result with
      | Ok () -> printfn "Created action %s" actionName
      | Result.Error code -> printfn "Unable to mutate action: %s; command returned %i" actionName code
  ) actions

Target "CreateActions" (fun _ ->
    runActionMutation Create actions
)

Target "UpdateActions" (fun _ ->
    runActionMutation Update actions
)

Target "DeleteActions" (fun _ ->
    List.iter (fun actionName ->
      let result =
        runWskAction <|
          Delete actionName

      match result with
        | Ok () -> printfn "Deleted action %s" actionName
        | Result.Error code -> printfn "Unable to delete action: %s; command returned %i" actionName code
    ) actions
)

// Build order
"Clean"
  ==> "Build"
  ==> "Deploy"

"Clean"
  ==> "Release"

"Build"
  ==> "BuildDockerImage"
  ==> "DeployDockerImage"

// start build
RunTargetOrDefault "Build"
