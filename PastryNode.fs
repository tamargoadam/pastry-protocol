module PastryNode

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic


// row and col constants for routing tables
let NUMROWS = 8
let NUMCOLS = 4

// participant message types
type PastryMsg = 
    | InitPastry of (string*IActorRef)[] * (string*IActorRef)[,] * int
    | RouteRequest of string * int

// supervisor message types
type SupervisorMsg =
    | StartPastry
    | DestinationReached of int
    | DestinationNotFound

let inline charToInt c = int c - int '0'


// this actor acts as a participant node in the pastry network
let participantActor (nodeId: string) (mailbox : Actor<PastryMsg>) = 

    let mutable numRequests = 0
    let mutable leafSet : (string*IActorRef)[] = null
    let mutable routingTable : (string*IActorRef)[,] = null

    // length of the prefix shared b/t id and nodeId
    let shl (id1: string) (id2: string)= 
        let mutable i = 0
        let mutable br = false
        while i < 8 && not br do
            if id1.Chars(i) <> id2.Chars(i) then
                br <- true
            i <- i + 1
        i


    // set leaf set, routing table, and num requests
    let init (ls: (string*IActorRef)[]) (rt: (string*IActorRef)[,]) (nr: int) =
        leafSet <- ls
        routingTable <- rt
        numRequests <- nr


    // route msg to next node; alert supervisor if destination reached
    let processReq (destinationId: string) (numHops: int) =
        if destinationId = nodeId then
            // the message is at its destination
            mailbox.Context.Parent <! DestinationReached (numHops)
        else if Math.Min((fst(leafSet.[0]) |> int), (nodeId |> int)) <= (destinationId |> int) && (fst(leafSet.[leafSet.Length-1]) |> int) >= (destinationId |> int) then
            // route by nearest in leaf set
            let mutable (nextHop: string*IActorRef) = ("-1", null)
            for i, j in leafSet do
                if shl i destinationId > shl (fst(nextHop)) destinationId || destinationId = i then
                    nextHop <- (i,j)
            snd(nextHop) <! RouteRequest (destinationId, numHops+1)
        else
            // route by routing table
            let pre = shl destinationId nodeId
            let dl = destinationId.Chars(pre) |> charToInt
            if fst(routingTable.[pre,dl]) <> "-1" then
                snd(routingTable.[pre,dl]) <! RouteRequest (destinationId, numHops+1)
            else
                //if not routing entry not availible route to id with shl longer than current pre & smaller difference
                let mutable found = false

                // if qulifies, send msg and return true
                let sendAttempt next = 
                    if shl (fst(next)) destinationId > pre && abs ((fst(next) |> int) - (destinationId |> int)) < abs ((nodeId |> int) - (destinationId |> int)) then
                        snd(next) <! RouteRequest (destinationId, numHops+1)
                        true
                    else
                        false

                // create combined list of possible routes
                let allRoutes = new List<string*IActorRef>()
                for i in leafSet do allRoutes.Add(i)
                for i in 0 .. NUMROWS-1 do
                    for j in 0.. NUMCOLS-1 do
                        if fst(routingTable.[i,j]) <> "-1" then allRoutes.Add(routingTable.[i,j])
                
                let mutable i = 0
                while i < allRoutes.Count && not found do
                    found <- sendAttempt allRoutes.[i]
                    i <- i+1
                if not found then
                    Console.WriteLine("Destination could not be found!")
                    mailbox.Context.Parent <! DestinationNotFound

                



    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            match msg with
            |InitPastry (ls, rt, nr) -> init ls rt nr
            |RouteRequest (id, nh) -> processReq id nh

            return! loop()
        }
    loop()