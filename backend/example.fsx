#load "./result.fsx"

type ReportFormat = string
type AccessToken = string

type Settings =
    { ReportFormat: ReportFormat
    }

type Query =
    { ReportFormat: ReportFormat
      AccessToken: AccessToken
      StartIndex: int
      EndIndex: int
    }
    static member Create startIndex endIndex format accessToken =
        { ReportFormat = format
          StartIndex = startIndex
          EndIndex = endIndex
          AccessToken = accessToken
        }

type QueryResult =
    { Value: string
    }

let getSettings: Result<Settings,int> = Ok { ReportFormat = "json" }

let getAccessToken: Result<AccessToken,int> = Ok "blah"

type QueryForResult = Query -> Result<QueryResult,string>
let queryForResult: QueryForResult =
    fun query ->
        printfn "format=%A" query.ReportFormat
        printfn "accessToken=%A" query.AccessToken
        Ok { Value = "blah" }

let settings = getSettings
let accessToken = getAccessToken |> Result.mapError string
let startIndex = 0
let endIndex = 10

printfn "*** USING BIND"

let (<!>) r fn = Result.bind fn r
let (<*>) r fn = Result.map fn r
let (<*!>) r fn = Result.mapError fn r

let reportFormat = getSettings <!> (fun s -> Ok s.ReportFormat)

reportFormat
|> Result.mapError string
<!> (fun format ->
        Result.mapError string accessToken
        <*> Query.Create startIndex endIndex format)
<!> queryForResult

printfn "*** USING EXPRESSION"

let result = Result.ResultBuilder()
let errorToString r = Result.mapError string r

let final =
  result {
    let! settings = getSettings |> errorToString
    let! accessToken = getAccessToken |> errorToString

    return!
      Query.Create startIndex endIndex settings.ReportFormat accessToken
        |> queryForResult
  }

printfn "Final query result: %A" final
