module PastrySupervisor

open PastryNode

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

let system = ActorSystem.Create("FSharp")


let getRandId (rand: Random) =
    let mutable n = NUMROWS // num didgets
    let mutable idString = ""
    for i in 0 .. n-1 do
        idString <- idString + rand.Next(0, 3).ToString()
    idString


let hasPrefix (prefix: string) (full: string) = 
    if prefix.Length < full.Length then
        full.Substring(0, prefix.Length).Equals(prefix)
    else false


// this actor spawns participant actors and facilitates starting and ending the protocol
let supervisorActor (numNodes: int) (numRequest: int) (mailbox : Actor<SupervisorMsg>)= 
    
    let mutable numReqDone = 0
    let mutable numHopsSum = 0
    let mutable terminateAddress = mailbox.Context.Parent
    let mutable topology = new Dictionary<string, IActorRef>()
    let mutable allDestinationsFound = true
    let mutable numFailed = 0

    let sortedIdList =
        let mutable l = 4 // num cols in routing table & entries in leaf set
        let idList = new List<string>()
        let rand = Random()
        let mutable newId = getRandId rand
        // create sorted list of random ids
        for i in 0 .. numNodes-1 do 
                while idList.Contains(newId) do
                    newId <- getRandId rand
                idList.Add(newId)
        idList.Sort()
        idList

    // fill id -> IActorRef map
    for j in 0 .. numNodes-1 do
        topology.Add(sortedIdList.[j], spawn mailbox ("worker"+sortedIdList.[j]) (participantActor (sortedIdList.[j]))) |> ignore

    // generate node tables and send init message to each node
    for i in 0 .. numNodes-1 do

        let currentNodeId = sortedIdList.[i]
        
        // generate leaf set
        let leafSet = new List<string*IActorRef>()
        for j in Math.Max(i-5, 0) .. Math.Min(i+5, numNodes-1) do
            if j <> i then leafSet.Add(sortedIdList.[j], topology.Item(sortedIdList.[j]))
        
        // generate routing table
        let routingTable : (string*IActorRef)[,] = Array2D.init NUMROWS NUMCOLS (fun x y -> ("-1", null))
        for row in 0 .. (NUMROWS-1) do
            let pre = currentNodeId.Substring(0, row)

            for col in 0 .. NUMCOLS-1 do
                let mutable found = false
                if col <> (currentNodeId.Chars(row) |> charToInt) then
                    let mutable listIndex = 0
                    let mutable id = sortedIdList.[listIndex]

                    // find id that meets index requierments; if none, leave -1
                    while fst(routingTable.[row, col]) = "-1" && listIndex < numNodes do
                        id <- sortedIdList.[listIndex]
                        if hasPrefix pre id && id.Chars(row) |> charToInt = col then
                            routingTable.[row, col] <- (id, topology.[id])
                            found <- true
                        listIndex <- listIndex + 1

                if not found then
                    routingTable.[row, col] <- ("-1", null)
                found <- false

        // send init message to node i
        topology.[currentNodeId] <! InitPastry (leafSet.ToArray(), routingTable, numRequest)

        // clear lists for next iteration
        leafSet.Clear()
         

    // signal each node to make a request to another node at random at a rate of 1 req/sec
    let beginSendingRequests (sender: IActorRef) =
        terminateAddress <- sender
        let rand = Random()
        let mutable currentNodeId = ""
        for i in 0 .. numRequest-1 do
            for j in 0 .. numNodes-1 do
                currentNodeId <- sortedIdList.[j]
                let mutable randIndex = rand.Next(0, numNodes)
                while randIndex = j do
                    randIndex <- rand.Next(0, numNodes)
                topology.[currentNodeId] <! RouteRequest (sortedIdList.[randIndex], 0)
            System.Threading.Thread.Sleep(1000)

    
    // add to total num hops and terminate process when all requests have reached their destination
    let processDoneMsg (numHops: int) =
        numReqDone <- numReqDone + 1
        if numHops < 0 then
            allDestinationsFound <- false
            numFailed <- numFailed + 1
        else
            // Console.WriteLine("Destination reached in {0} hops! ({1})", numHops, numReqDone)
            numHopsSum <- numHopsSum + numHops
        if numReqDone >= numRequest*numNodes then
            if allDestinationsFound then
                Console.WriteLine("All requests have been routed to their destination!")
            else
                Console.WriteLine("{0} out of {1} requests have been routed to their destination.", (numReqDone-numFailed), numReqDone)
            Console.WriteLine("Avg. number of hops per request: {0}", numHopsSum/numReqDone)
            terminateAddress <! "done"


    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            match msg with
            |StartPastry -> beginSendingRequests sender
            |DestinationReached numHops -> processDoneMsg numHops
            |DestinationNotFound -> processDoneMsg -1
            return! loop()
        }
    loop()