namespace MoneyAlarms.Core.Commands

open Plaid
open Firebase
open Microsoft.FSharp.Reflection
open FSharp.Extensions
open MoneyAlarms.Core

  module DomainTypes =
    let toDomainType<'T> externalType: 'T =
      let found = FSharpType.GetUnionCases(typeof<'T>) |> Array.find (fun t -> t.Name = externalType.GetType().Name)
      FSharpValue.MakeUnion(found, [| externalType |]) :?> 'T

    let mapError externalResult: Result<_,MoneyAlarmsError> =
        Result.mapError toDomainType<MoneyAlarmsError> externalResult

    type BindDomainError<'T,'U,'Error> = ('T -> Result<'U,'Error>) -> Result<'T,MoneyAlarmsError> -> Result<'U,MoneyAlarmsError>
    let bindError: BindDomainError<'T,'U,'Error> =
        fun fn ->
          Result.bind (fun t -> mapError <| fn t)

module CreateAccount =
  type ResultTupleMap<'a,'b,'c,'d> = Result<'a*'b,'c> -> ('a -> 'b -> 'd) -> Result<'d,'c>
  let (|*>): ResultTupleMap<'a,'b,'c,'d> =
      fun r f -> Result.map (fun v -> v ||> f) r

  type ResultTupleBind<'T,'U,'V,'Error> = ('T -> 'U -> Result<'V,'Error>) -> Result<'T*'U,'Error> -> Result<'V,'Error>
  let resultTupleBind: ResultTupleBind<'T,'U,'V,'Error> =
      fun f r -> Result.bind (fun (v1,v2) -> f v1 v2) r

  type MakeAccount = string -> Plaid.Accounts.AccountDto -> Account
  let private makeAccount: MakeAccount =
      fun institutionName accountDto ->
           {  AccountId = accountDto.AccountId
              Name = accountDto.Name
              OfficialName = accountDto.OfficialName
              Mask = accountDto.Mask
              Type = accountDto.Type
              SubType = accountDto.Subtype
              InstitutionName = institutionName
            }

  type MakeItem = Plaid.Accounts.ItemDto -> PlaidItem
  let private makeItem: MakeItem =
      fun itemDto ->
        {
          ItemId = itemDto.ItemId
          Webhook = itemDto.Webhook
          InstitutionId = itemDto.InstitutionId
        }

  let extractAccountsAndItem (accountsDto: Plaid.Accounts.AccountsDto) (institutionName: string): Account [] * PlaidItem =
      let accounts =
          Array.map <| makeAccount institutionName <| accountsDto.Accounts
      let item = makeItem accountsDto.Item

      (accounts, item)

  type MakeFirebaseDtos = string -> string -> Account [] -> PlaidItem -> Firebase.SaveAccount.AccountDto.Root [] * Firebase.AddItem.PlaidItemDto.Root
  let makeFirebaseDtos: MakeFirebaseDtos =
      fun firebaseUserId accessToken accounts item ->
        let makeAccountDto account =
            Firebase.SaveAccount.AccountDto.Root
                ( firebaseUserId,
                  account.AccountId,
                  account.Name,
                  account.OfficialName,
                  account.Type,
                  account.SubType,
                  account.Mask,
                  account.InstitutionName
                )
        ( Array.map makeAccountDto accounts,
          Firebase.AddItem.PlaidItemDto.Root
            ( item.InstitutionId,
              item.ItemId,
              item.Webhook
            )
        )

  let tupleUp a b = (a,b)

  type GetAccountsWithInstitutionName = Plaid.Institutions.GetName -> Plaid.Accounts.AccountsDto -> Result<Plaid.Accounts.AccountsDto * string,MoneyAlarmsError>
  let getAccountsWithInstitutionName: GetAccountsWithInstitutionName =
      fun plaidGetInstitutionName (accountsDto: Plaid.Accounts.AccountsDto) ->
        let institutionNameR = DomainTypes.mapError <| plaidGetInstitutionName accountsDto.Item.InstitutionId

        Result.map <| tupleUp accountsDto <| institutionNameR

  type Run = Plaid.ExchangeToken -> Plaid.Accounts.Get -> Plaid.Institutions.GetName -> Firebase.SaveAccount.Run -> Firebase.AddItem.Run -> TokenExchangeDto -> Result<unit, MoneyAlarmsError>
  let run: Run =
      fun plaidExchangeToken plaidGetAccounts plaidGetInstitutionName firebaseCreateAccount firebaseAddItem dto ->
        let accessToken = DomainTypes.mapError <| plaidExchangeToken dto.PlaidPublicToken
        let accountsAndItem =
          accessToken
              |> DomainTypes.bindError plaidGetAccounts
              |> Result.bind (getAccountsWithInstitutionName plaidGetInstitutionName)
              |*> extractAccountsAndItem

        Result.bind
          (fun accessToken ->
            accountsAndItem
              |*> makeFirebaseDtos dto.FirebaseUserId accessToken
              |> resultTupleBind (fun accounts item ->
                        do
                          printfn "Saving accounts: %A" accounts
                          Array.map firebaseCreateAccount accounts |> ignore
                          firebaseAddItem dto.FirebaseUserId item
                        Result.map ignore accountsAndItem
                  )
          ) accessToken
