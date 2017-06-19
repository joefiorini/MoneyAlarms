open Expecto
open MoneyAlarms
open Firebase

let simpleTest =
    testCase "makeAccount creates an account" <| fun () ->
        let firebaseUserId = "USER_ID"
        let plaidAccountId = "ACCOUNT_ID"
        let itemAccessToken = "ACCESS_TOKEN"
        let expected =
            { UserId = firebaseUserId
              AccountId = plaidAccountId
              ItemAccessToken = itemAccesToken
            }
        Expect.equal expected.FirebaseUserId firebaseUserId
        Expect.equal expected.PlaidAccountId plaidAccountId
        Expect.equal expected.ItemAccessToken itemAccessToken

runTests defaultConfig simpleTest
