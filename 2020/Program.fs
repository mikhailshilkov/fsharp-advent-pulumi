open Farmer
open Farmer.Builders.Storage
open Farmer.Builders.WebApp

// Create a storage account
let myStorageAccount = storageAccount {
    name "farmerpulumisa"
}

// Create a web app with application insights that's connected to the storage account
let myWebApp = webApp {
    name "farmerpulumiweb"
    setting "storageKey" myStorageAccount.Key
}

// Create an ARM template
let deployment = arm {
    location Location.NorthEurope
    add_resources [
        myStorageAccount
        myWebApp
    ]
}

// Deploy with Pulumi
[<EntryPoint>]
let main _ = FarmerDeploy.run "my-resource-group" deployment
