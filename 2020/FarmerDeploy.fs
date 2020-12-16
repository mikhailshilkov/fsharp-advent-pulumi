module FarmerDeploy

open System.Collections.Generic
open System.Collections.Immutable
open Pulumi
open Pulumi.FSharp
open Farmer

type private ArmTemplateDecodeArgs() =
    inherit InvokeArgs()
    [<Input("text")>]
    member val Text = "" with get, set

[<OutputType>]
type private ArmTemplateDecodeResult [<OutputConstructor>] private(result: ImmutableArray<ImmutableDictionary<string, obj>>) =
    inherit InvokeArgs()
    [<Input("text")>]
    member val Result = result

let private armTemplateDecode (text: string): Async<ImmutableArray<ImmutableDictionary<string, obj>>> = async {
    let args = ArmTemplateDecodeArgs(Text = text)
    let! result = Deployment.Instance.InvokeAsync<ArmTemplateDecodeResult>("azure-nextgen:armtemplate:decode", args) |> Async.AwaitTask
    return result.Result.ToImmutableArray()
}

let private infra (resourceGroupName: string) (deployment: CoreTypes.Deployment): Async<IDictionary<string, obj>> = async {
    let template = Writer.toJson deployment.Template
    let! resources = armTemplateDecode template
    let knownResources = Dictionary<string, Resource>()
    resources
    |> Seq.map(
        fun dict ->
            let token = dict.["token"].ToString()
            let name = dict.["name"].ToString()
            let args = dict.["args"] :?> ImmutableDictionary<string, obj>
            let args = args.SetItem("resourceGroupName", resourceGroupName)
            // TODO: remove when arm2pulumi parses tags correctly
            let args = args.SetItem("tags", null)
            let dependsOn = dict.["dependsOn"] :?> ImmutableArray<obj>
            token, name, args, dependsOn |> Seq.map (fun o -> o.ToString()) |> List.ofSeq)
    |> List.ofSeq
    // TODO: proper dependency graph calculation instead of a dumb sorting on length
    |> List.sortBy (fun (_, _, _, dependsOn) -> Seq.length dependsOn)
    |> Seq.iter (
        fun (token, name, args, dependsOn) ->
            let deps = dependsOn |> Seq.filter (knownResources.ContainsKey) |> Seq.map (fun name -> knownResources.[name]) |> Seq.map input
            let options = CustomResourceOptions(DependsOn=inputList deps)
            let resource = CustomResource(token, name, DictionaryResourceArgs args, options)
            do knownResources.Add(name, resource))
    
    return dict []
}

let run (resourceGroupName: string) deployment =
    Deployment.runAsync (fun() -> infra resourceGroupName deployment)
