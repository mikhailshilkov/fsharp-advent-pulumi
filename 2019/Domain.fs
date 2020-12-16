module Domain

open Pulumi

type Recipient = Person of string

type KubeNodes = | Nodes of int
type BlockchainType = Public | Private | Federated
type Trigger = Http | Queue | Blob

type Runtime =
    | Node
    | Python
    | DotNet
    | Java
    | Go

type Resource =
    | Kubernetes of KubeNodes
    | Serverless of Runtime
    | Blockchain of BlockchainType
    | NoSQL

type Wish = {
    Recipient: Recipient
    Resource: Resource
}

type Identifier = | Url of Output<string>

type Response = Recipient * Identifier

type MakeResource = unit -> Identifier
type Elf = Wish -> MakeResource option
