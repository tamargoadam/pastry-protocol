module Main

open Akka.FSharp
open PastrySupervisor

open System

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        Console.WriteLine("Invalid Input Provided")
        Console.WriteLine("Required Format: project3 <num_nodes> <num_requests>")
        Console.WriteLine("{0}", argv.Length)
    else
        Console.WriteLine("Starting Pastry Protocol...")
    
    let supervisor = spawn system ("supervisor") (supervisorActor 100 10)
    let res = supervisor <? PastryNode.SupervisorMsg.StartPastry |> Async.RunSynchronously
    0 // return int exit code