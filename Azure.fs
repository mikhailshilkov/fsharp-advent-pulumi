module Azure

open Pulumi.Azure.AppService
open Pulumi.Azure.AppService.Inputs
open Pulumi.Azure.Core
open Pulumi.Azure.Storage
open Pulumi.FSharp

open Domain

let private resourceGroup = ResourceGroup "shared-rg"

let private storageAccount =
    Account("sharedsa",
        AccountArgs
           (ResourceGroupName = io resourceGroup.Name,
            AccountReplicationType = input "LRS",
            AccountTier = input "Standard"))

let private sku = PlanSkuArgs(Tier = input "Dynamic", Size = input "Y1")
let private plan = 
    Plan("shared-asp", 
        PlanArgs
           (ResourceGroupName = io resourceGroup.Name,
            Kind = input"FunctionApp",
            Sku = input sku))

let private makeFunctionApp (Person name) (runtime: Runtime) =
    match runtime with
    | Go -> None
    | _ -> Some (fun () -> 
        let lowerName = name.ToLowerInvariant()
        let r = runtime.ToString() |> input
        let app =
            FunctionApp(lowerName, 
                FunctionAppArgs
                   (ResourceGroupName = io resourceGroup.Name,
                    AppServicePlanId = io plan.Id,
                    AppSettings = inputMap ["runtime", r],
                    StorageConnectionString = io storageAccount.PrimaryConnectionString,
                    Version = input "~2"))
        Url app.DefaultHostname)

let azure (wish: Wish) : MakeResource option =
    match wish.Resource with
    | Serverless r -> makeFunctionApp wish.Recipient r
    | _ -> None
