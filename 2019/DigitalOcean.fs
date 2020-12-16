module DigitalOcean

open Pulumi.FSharp
open Pulumi.Digitalocean

open Domain

let private userData =
    @"#!/bin/bash
sudo apt-get update
sudo apt-get install -y nginx"

let droplet (wish: Wish) = fun () ->
    let (Person name) = wish.Recipient
    let lowerName = name.ToLowerInvariant()
    let droplet = 
        Droplet(lowerName, 
            DropletArgs
               (Image = input "ubuntu-18-04-x64",
                Region = input "nyc3",
                PrivateNetworking = input true,
                Size = input "512mb",
                UserData = input userData))
    Url droplet.Ipv4Address
