module MoneyAlarms.Dispatch.ServiceConfigHelpers

[<RequireQualifiedAccess>]
module Plaid =
    open Chiron
    open Chiron.Operators

    let fromPayload (payload: Json) =
        Plaid.ServiceConfig.create
            <!> Json.read "PLAID_CLIENT_ID"
            <*> Json.read "PLAID_SECRET"
            <*> Json.read "PLAID_HOST"
            <*> Json.read "PLAID_PUBLIC_KEY"
            <| payload

// [<RequireQualifiedAccess>]
// module Firebase =
//     open Chiron
//     open Chiron.Operators

//     let fromPayload (payload: Json) httpClient accessToken  =
//         Firebase.ServiceConfig.createConfig
//             <!> Json.read "FIREBASE_API_KEY"
//             <*> Json.read "FIREBASE_AUTH_DOMAIN"
//             <*> Json.read "FIREBASE_DATABASE_URL"
//             <*> Json.read "FIREBASE_PROJECT_ID"
//             <*> Json.read "FIREBASE_STORAGE_BUCKET"
//             <*> Json.read "FIREBASE_MESSAGE_SENDER_ID"
//             <| accessToken
//             <| httpClient
