module Gcp

open Pulumi.FSharp
open Pulumi.Gcp.Container
open Pulumi.Gcp.Container.Inputs

open Domain

let private makeGkeCluster (Person name) (Nodes nodeCount) () =
    let masterVersion = input "1.15.1"
    let lowerName = name.ToLowerInvariant()

    let cluster =
        Cluster(lowerName,
            ClusterArgs
               (InitialNodeCount = input nodeCount,
                MinMasterVersion = masterVersion,
                NodeVersion = masterVersion,
                NodeConfig = input (
                    ClusterNodeConfigArgs
                       (MachineType = input "n1-standard-1",
                        OauthScopes = inputList [
                            input "https://www.googleapis.com/auth/compute"
                            input "https://www.googleapis.com/auth/devstorage.read_only"
                            input "https://www.googleapis.com/auth/logging.write"
                            input "https://www.googleapis.com/auth/monitoring"
                        ]))))
    Url cluster.Endpoint

let gcp (wish: Wish) : MakeResource option =
    match wish.Resource with
    | Kubernetes k8s -> makeGkeCluster wish.Recipient k8s |> Some
    | _ -> None
