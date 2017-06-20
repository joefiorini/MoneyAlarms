module ScotchSetup

open System
open System.IO
open Scotch

let shouldRecord =
    Environment.GetEnvironmentVariable "SCOTCH_MODE_RECORDING" = "true"

let scotchMode =
    if shouldRecord then
        printfn "Scotch set to Record"
        ScotchMode.Recording
    else
        printfn "Scotch set to Playback"
        ScotchMode.Replaying

let cassettePath = AppDomain.CurrentDomain.BaseDirectory + "../MoneyAlarms.Test/cassettes"

let httpClientForCassette cassette =
    HttpClients.NewHttpClient(Path.Combine(cassettePath, cassette), scotchMode)
