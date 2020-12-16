module Program

open Pulumi
open Pulumi.FSharp

open Domain
open Aws
open Azure
open Gcp
open DigitalOcean
open Utils

let freeVM () = Url (output "")

let fulfill wish = 
    let make =
        [aws; azure; gcp]
        |> shuffle
        |> List.tryPick (fun f -> f wish)
        |> Option.defaultValue (droplet wish) // Default to a DO droplet
    wish.Recipient, make()

let send (responses: Response list) =
    responses
    |> List.map (fun (Person name, Url url) -> name, url :> obj)
    |> dict

let wishes = [
    { Recipient = Person "Kelsey" ; Resource = Kubernetes (Nodes 25) }
    { Recipient = Person "Jeff"   ; Resource = Serverless DotNet }
    { Recipient = Person "Satoshi"; Resource = Blockchain Public }
]
    
let santaCloud () = wishes |> (List.map fulfill) |> send

[<EntryPoint>]
let main _ = Deployment.run santaCloud
