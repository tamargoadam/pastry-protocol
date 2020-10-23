module Main

open System
open Akka.Actor
open Akka.FSharp

let system = ActorSystem.Create("FSharp")

let registryActor (numNodes: int) (mailbox : Actor<'a>)= 
    // TODO: spawn new actors and facilitate node joining with this actor
    // listen for child actors sending messages containing number of hops for them to recieve message
    // calculate and display average number of hops and terminate when recieved numNodes messages

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            return! loop()
        }
    loop()

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        Console.WriteLine("Invalid Input Provided")
        Console.WriteLine("Required Format: project3 <num_nodes> <num_requests>")
    else
        Console.WriteLine("Starting Pastry Protocol...")
    
    0 // return int exit code