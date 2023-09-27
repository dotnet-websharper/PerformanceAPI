namespace WebSharper.PerformanceAPI

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type WindowOrWorkerGlobalScope with 
        [<Inline "$this.performance">]
        member this.Performance = As<Performance> null
