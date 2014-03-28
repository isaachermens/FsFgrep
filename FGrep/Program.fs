// This  module simply serves as an entry point into the FGrep utility
// The FGrep implementation is contained in the FGrepI module
module FGrep
    open FGrepI
    open System
    open Interop

    [<EntryPoint>]
    let main argv =
       //printfn "%A" argv
       ignore (FGrep argv)
       0 // return an integer exit code
