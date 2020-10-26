module PastrySupervisor

open PastryNode

open System
open Akka.Actor
open Akka.FSharp


let system = ActorSystem.Create("FSharp")

let generateNodeId =
    let rand = Random()
    let id = rand.Next(0, typeof<int>.GetField("MaxValue").GetValue() |> unbox)
    id


let supervisorActor (numNodes: int) (mailbox : Actor<'a>)= 
    // TODO: spawn new actors and facilitate node joining with this actor
    // listen for child actors sending messages containing number of hops for them to recieve message
    // calculate and display average number of hops and terminate when recieved numNodes messages

    let nodeMap = Map.empty

    let mutable randId = generateNodeId
    for i in 0 .. numNodes-1 do 
                while  nodeMap.ContainsKey(randId) do
                    randId <- generateNodeId
                nodeMap.Add(randId, (spawn mailbox ("worker"+randId.ToString()) (participantActor randId))) |> ignore
                
    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            return! loop()
        }
    loop()