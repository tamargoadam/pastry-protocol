module Main

open Akka.FSharp
open PastrySupervisor

open System

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        Console.WriteLine("Invalid Input Provided")
        Console.WriteLine("Required Format: dotnet run <num_nodes> <num_requests>")
        -1
    else
        Console.WriteLine("Starting Pastry Protocol...")
        let numNodes: int = int argv.[0]
        let numRequests: int = int argv.[1]
        let supervisor = spawn system ("supervisor") (supervisorActor numNodes numRequests)
        let res = supervisor <? PastryNode.SupervisorMsg.StartPastry |> Async.RunSynchronously
        0