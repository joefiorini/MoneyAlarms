module FirebaseTests

open Expecto
open System
open Firebase
open Firebase.SaveAccount
open ScotchSetup

let accessToken = "ya29.EltwBLZDfJmt66Uji6ckPEBfcmXrkEE7JXlyTzt8Bt_gqPqHEtCxQbj2KGa36x2DeCF2ODJ_Y17aiICSGkMFrLXfWE2f8zp_a7-UgRCnk56NVeoNOcWqJjtibQfp"

[<Tests>]
let tests =
  testList "Firebase Client"
    [ testCase "returns the created account" <| fun _ ->
        let httpClient = httpClientForCassette "firebase.successes.json"
        let serviceConfig = ServiceConfig.fromEnvironment httpClient accessToken
        let testAccount =
          { UserId = "5Hu5khcyxmgmOTMMm1AzlcyDClC2"
            ItemAccessToken = "token"
            AccountId = "12346"
          }

        let result = firebaseCreateAccount serviceConfig testAccount

        match result with
          | Ok v ->
             Expect.equal v testAccount "Account is correct"
          | Error e ->
             printfn "Unexpected error: %A" e

        Expect.isOk result "Ok result"
    ]
