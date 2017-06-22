#r "System.Net.Http"
#r "Newtonsoft.Json"
#r "MoneyAlarms.dll"

open System.Net
open System.Net.Http
open Newtonsoft.Json

type Named = {
    name: string
}

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        let blah = Ok "Doo"
        match blah with
        | Ok s -> log.Info s
        | Error s -> log.Error s
        log.Info(sprintf
            "F# HTTP trigger function processed a request.")

        // Set name to query string
        let name =
            req.GetQueryNameValuePairs()
            |> Seq.tryFind (fun q -> q.Key = "name")

        match name with
        | Some x ->
            return req.CreateResponse(HttpStatusCode.OK, "Hello World");
        | None ->
            let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask

            if not (String.IsNullOrEmpty(data)) then
                let named = JsonConvert.DeserializeObject<Named>(data)
                return req.CreateResponse(HttpStatusCode.OK, "Hello " + named.name);
            else
                return req.CreateResponse(HttpStatusCode.BadRequest, "Specify a Name value");
    } |> Async.RunSynchronously
