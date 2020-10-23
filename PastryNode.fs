module PastryNode

open Akka.Actor
open Akka.FSharp

let ParticipantActor (mailbox : Actor<'a>)= 
    // TODO: instantiate mutable ID -> Address map
    // manage different kinds of messages based on contents
    // if message ment to end at this participant is recieved, send message with number of hops to registryActor

    let rec loop () = 
        actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender()
            return! loop()
        }
    loop()