module PastrySupervisor

open PastryNode

open System
open Akka.Actor
open Akka.FSharp
open System.Collections.Generic

let system = ActorSystem.Create("FSharp")

let NUMROWS = 8
let NUMCOLS = 4

let getRandId (rand: Random) =
    let mutable n = 8 // num didgets
    let mutable idString = ""
    for i in 0 .. n-1 do
        idString <- idString + rand.Next(0, 9).ToString()
    idString

let supervisorActor (numNodes: int) (mailbox : Actor<'a>)= 
    // TODO: spawn new actors and facilitate node joining with this actor
    // listen for child actors sending messages containing number of hops for them to recieve message
    // calculate and display average number of hops and terminate when recieved numNodes messages

    let topology = Map.empty<string, IActorRef>

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
        topology.Add(sortedIdList.[j], spawn system ("worker"+sortedIdList.[j]) (participantActor sortedIdList.[j])) |> ignore

    // generate node tables and spawn nodes
    for i in 0 .. numNodes-1 do
        
        // generate leaf node table
        let leafNodes = new List<string>()
        for j in Math.Max(i-5, 0) .. Math.Min(i+5, numNodes-1) do
            if j <> i then leafNodes.Add(sortedIdList.Item(j))
        Console.WriteLine("{0}:", sortedIdList.Item(i))
        Console.WriteLine(sprintf "%A" leafNodes)
        
        // generate routing table
        //let routingTable : (int*string)[,] = Array2D.zeroCreate NUMROWS NUMCOLS
        
        
        // clear lists for next iteration
        leafNodes.Clear()
         


    // nodeMap.Add(randId, (spawn mailbox ("worker"+randId.ToString()) (participantActor randId))) |> ignore       

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            return! loop()
        }
    loop()