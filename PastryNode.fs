module PastryNode

open System
open Akka.Actor
open Akka.FSharp

// row and col constants for routing tables
let NUMROWS = 8
let NUMCOLS = 4


type PastryMsg = 
    | InitPastry of (string*IActorRef)[] * (string*IActorRef)[,] * int
    | RouteRequest of string * int

type SupervisorMsg =
    | StartPastry
    | DestinationReached of int

let participantActor (nodeId: string) (mailbox : Actor<PastryMsg>) = 
    // TODO: instantiate mutable ID -> Address map
    // manage different kinds of messages based on contents
    // if message ment to end at this participant is recieved, send message with number of hops to registryActor
    let mutable numRequests = 0
    let mutable leafSet : (string*IActorRef)[] = null
    let mutable routingTable : (string*IActorRef)[,] = null

    // set leaf set, routing table, and num requests
    let init (ls: (string*IActorRef)[]) (rt: (string*IActorRef)[,]) (nr: int) =
        leafSet <- ls
        routingTable <- rt
        numRequests <- nr

    let processReq (destinationId: string) (numHops: int) =
        if destinationId = nodeId then
            mailbox.Context.Parent <! DestinationReached (numHops)
        // else
        //     // TODO: implement routing here

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            match msg with
            |InitPastry (ls, rt, nr) -> init ls rt nr
            |RouteRequest (id, nh) -> processReq id nh

            return! loop()
        }
    loop()