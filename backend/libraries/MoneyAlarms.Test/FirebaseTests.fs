module FirebaseTests

open Expecto
open System
open Firebase
open Firebase.SaveAccount
open ScotchSetup

let accessToken = "ya29.Elt2BNYAMen4A1Rp-aC5A4gZSL-guGoXHrUst2XXc4u9x-_JnOpkTe81tg0XMy27vPxFSzXanqEEi7I_7dqPD82Vg9pTGuUfI1qI2rUJDpdvyw0PWR4OmuRb2zKr"

[<Tests>]
let tests =
  testList "Firebase Client"
    [ testCase "returns the created account" <| fun _ ->
        let httpClient = httpClientForCassette "firebase.successes.json"
        let serviceConfig = ServiceConfig.fromEnvironment httpClient accessToken
        let testAccount = AccountDto.GetSample()

        let result = run serviceConfig testAccount

        Expect.isOk result "Ok result"

      testCase "gets the account when it exists" <| fun _ ->
        // let firebaseToken = Firebase.AccessToken.generate()
        // printfn "%A" firebaseToken
        let httpClient = httpClientForCassette "firebase.successes.json"
        let serviceConfig = ServiceConfig.fromEnvironment httpClient accessToken
        let testAccount = AccountDto.GetSample()
        let getAccount = getAccountByAccountId serviceConfig
        let accountId = "-KnblGYOWOtYDRq0WR1x"

        match getAccount testAccount.FirebaseUserId accountId with
          | Ok o -> Expect.isTrue (Option.isSome o) "Account exists"
          | Error e -> Tests.failtestf "Expected account to exist, got error %A" e
    ]
