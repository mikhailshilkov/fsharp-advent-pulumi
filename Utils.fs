module Utils

open System
open Pulumi

let output (v: 'a) = Output.Create<'a> v


let shuffleR (r : Random) xs = xs |> List.sortBy (fun _ -> r.Next())

let private r = System.Random 0 // use a fixed seed for stability
let shuffle xs = shuffleR r xs