namespace HttpUtils

open System
open System.Net.Http

module GetJson =
    type GetJsonSync = HttpClient -> string -> HttpResponseMessage * string
    let sync: GetJsonSync =
        fun httpClient url ->
            async {
              let request = new HttpRequestMessage()
              request.RequestUri <- new Uri(url)
              request.Method <- System.Net.Http.HttpMethod.Get

              let! response = httpClient.SendAsync(request) |> Async.AwaitTask

              let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

              return (response, content)
            } |> Async.RunSynchronously

module PostJson =
  type PostJsonSync = HttpClient -> string -> string -> HttpResponseMessage * string
  let sync: PostJsonSync =
      fun httpClient url body ->
        async {
          let request = new HttpRequestMessage()
          request.RequestUri <- Uri(url)
          request.Method <- System.Net.Http.HttpMethod.Post

          let reqContent = new StringContent(body)
          reqContent.Headers.ContentType <- new Headers.MediaTypeHeaderValue("application/json")
          request.Content <- reqContent

          let! response = httpClient.SendAsync(request) |> Async.AwaitTask

          let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

          return (response, content)
        } |> Async.RunSynchronously
