module MoneyAlarms.Test

open System
open DotEnvFile
open Expecto

let dotEnvPath = AppDomain.CurrentDomain.BaseDirectory + "../../.env"
let vars = DotEnvFile.LoadFile(dotEnvPath)
DotEnvFile.InjectIntoEnvironment(vars)

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
