module PastrySupervisor

open PastryNode

open System
open Akka.Actor
open Akka.FSharp
open System.Security.Cryptography

let system = ActorSystem.Create("FSharp")

let generateNodeId =
    let rand = Random()
    let id = rand.Next(0, typeof<int>.GetField("MaxValue").GetValue() |> unbox)
    id
    // Console.WriteLine("{0}", id)
    // Convert.ToString(id, 2)


let supervisorActor (numNodes: int) (mailbox : Actor<'a>)= 
    // TODO: spawn new actors and facilitate node joining with this actor
    // listen for child actors sending messages containing number of hops for them to recieve message
    // calculate and display average number of hops and terminate when recieved numNodes messages

    // let f = (spawn mailbox ("worker") (participantActor (1)))

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            return! loop()
        }
    loop()