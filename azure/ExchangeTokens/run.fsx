#I __SOURCE_DIRECTORY__
#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "FSharp.Data.dll"
#r "FSharp.Data.DesignTime.dll"
#r "MoneyAlarms.Core.dll"

open System
open System.Net
open System.Net.Http
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open MoneyAlarms.Core
open FSharp.Data
open FSharp.Extensions

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info("Reading data from request")
        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask

        // if (String.IsNullOrEmpty(data)) then
        //     return req.CreateResponse(HttpStatusCode.BadRequest, "Need a body")
        // else
        log.Info(sprintf "Got data: %A" data)
        let httpClient = new HttpClient()
        let contractResolver = new DefaultContractResolver()
        contractResolver.NamingStrategy <- new SnakeCaseNamingStrategy(true, false);

        let settings = new JsonSerializerSettings()
        settings.ContractResolver <- contractResolver

        let dto = JsonConvert.DeserializeObject<TokenExchangeDto>(data, settings)

        ServicePointManager.SecurityProtocol <- SecurityProtocolType.Tls12

        log.Info(sprintf "Made DTO: %A" dto)
        let plaidConfig = Plaid.ServiceConfig.fromEnvironment httpClient
        log.Info(sprintf "Got plaid config: %A" plaidConfig)
        let plaidExchangeToken = Plaid.plaidExchangeToken plaidConfig
        let plaidGetAccounts = Plaid.Accounts.get plaidConfig
        let plaidGetInstitutionName = Plaid.Institutions.getName plaidConfig

        let firebaseToken = Firebase.AccessToken.generate()
        log.Info(sprintf "Got firebaseToken: %A" firebaseToken)
        let firebaseConfig =
                Firebase.ServiceConfig.fromEnvironment httpClient firebaseToken

        log.Info(sprintf "Got firebase config: %A" firebaseConfig)
        let firebaseCreateAccount =
              Firebase.SaveAccount.run firebaseConfig
        let firebaseAddItem = Firebase.AddItem.run firebaseConfig

        let resultFnA = Commands.CreateAccount.run log plaidExchangeToken
        let resultFnB = resultFnA plaidGetAccounts
        let resultFnC = resultFnB plaidGetInstitutionName
        let resultFnD = resultFnC firebaseCreateAccount
        let resultFnE = resultFnD firebaseAddItem

        let exchangeResult = resultFnE dto

        log.Info(sprintf "Got exchangeResult: %A" exchangeResult)
        // match exchangeResult with
        // | Ok _ ->
        //   return req.CreateResponse(HttpStatusCode.OK, "Good");
        // | Error e ->
        //   return req.CreateResponse(HttpStatusCode.InternalServerError, sprintf "Error: %A" e)
    } |> Async.StartAsTask
