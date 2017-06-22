module Expecto.Expect

open Expecto

/// Expects the value to be a Result.Ok value.
let isOk x message =
    match x with
    | Ok _ -> ()
    | Error x ->
        Tests.failtestf "%s. Expected Ok, was Error(%A)." message x

/// Expects the value to be a Result.Error value.
let isError x message =
    match x with
    | Ok x ->
        Tests.failtestf "%s. Expected Error _, was Ok(%A)." message x
    | Error _ -> ()
