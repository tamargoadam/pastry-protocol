module PastryNode

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

// Config parameters for routing table
let B = 4
let NUMROWS = 9
let NUMCOLS = int(2.0**float(B)) - 1

let routingTable : (int*string)[,] = Array2D.zeroCreate NUMROWS NUMCOLS


let findNextHop (destinationId: int) =
    // TODO: return address of key closest to destinationId
    0


let forward msg =
    // fwd msg to next hop
    let _, _, destinationId = msg
    let nextHop = findNextHop destinationId
    // nextHop <! msg
    0


let join msg = 
    // fwd msg to next hop and send state table
    let _, _, joinNodeId = msg
    let nextHop = findNextHop joinNodeId
    // add joining node to tables?
    // send tables to joining node 
    // nextHop <! msg
    0


let handleMessage msg = 
    // msg must be 3-tuple with: 
    // fst = msg type
    // snd = content
    // thrd = destination ID
    let msgType, _, _ = msg
    match msgType with
    | 1 -> join msg // join request
    | 2 // state tables
    |_ -> forward msg // standard msg


let participantActor (nodeId: int) (mailbox : Actor<'a>)= 
    // TODO: instantiate mutable ID -> Address map
    // manage different kinds of messages based on contents
    // if message ment to end at this participant is recieved, send message with number of hops to registryActor

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            handleMessage msg
            return! loop()
        }
    loop()