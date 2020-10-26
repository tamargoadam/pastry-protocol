module PastryNode

open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

let findNextHop (routingTable: Map<int,string>) (destinationId: int) =
    // TODO: return address of key closest to destinationId

    0

let participantActor (nodeId: int) (mailbox : Actor<'a>)= 
    // TODO: instantiate mutable ID -> Address map
    // manage different kinds of messages based on contents
    // if message ment to end at this participant is recieved, send message with number of hops to registryActor

    let routingTable = Map<int,string>

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            return! loop()
        }
    loop()