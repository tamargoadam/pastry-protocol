module Main

open PastrySupervisor

open System

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        Console.WriteLine("Invalid Input Provided")
        Console.WriteLine("Required Format: project3 <num_nodes> <num_requests>")
    else
        Console.WriteLine("Starting Pastry Protocol...")
    
    Console.WriteLine("{0}", generateNodeId)
    0 // return int exit code