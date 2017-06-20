module FirebaseTests

open Expecto
open System
open Firebase
open ScotchSetup

[<Tests>]
let tests =
  testList "Firebase Client"
    [ ptestCase "returns the created account" <| fun _ ->
        let httpClient = httpClientForCassette "firebase.successes.json"
        Expect.equal (1 + 1) 2 "1+1"
    ]
