namespace WebSharper.PerformanceAPI.Sample

open WebSharper
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.UI.Templating
open WebSharper.JavaScript

[<JavaScript>]
module Templates =
    type MainTemplate = Template<"index.html", ClientLoad.FromDocument>

[<JavaScript>]
module Client =
    open WebSharper.PerformanceAPI
    open WebSharper.PerformanceAPI.PerformanceEntries

    let sampleList = Var.Create<int list> []
    [<SPAEntryPoint>]
    let Main() = 

        PerformanceObserver(fun list observer -> (
            for r in list.GetEntries() do
                Console.Log $"OBSERVER {r.EntryType}: evt start: {r.StartTime} | evt end: {r.StartTime+r.Duration}"
                ()
        )).Observe(PerformanceObserver.Options(EntryType.Paint))
        Doc.Concat [
            div [
                on.afterRender (fun el -> 
                    for entry:PerformanceEntry in JS.Window.Performance.GetEntries() do
                        match entry.EntryType with
                        | e when e = EntryType.Paint -> Console.Log $"largest contentful paint id: %s{(e |> As<LargestContentfulPaint>).Id}"
                        | _ -> Console.Log entry.EntryType
                    PerformanceObserver(fun ((list:PerformanceObserver.EntryList),(b: PerformanceObserver)) -> (
                        for r in list.GetEntries() do
                            Console.Log $"OBSERVER {r.EntryType}: id: evt start: {r.StartTime} | evt end: {r.StartTime+r.Duration}"
                            ()
                    )).Observe(PerformanceObserver.Options(EntryType.Layout_shift))
                    
                    let ele = JS.Document.GetElementById("examplelist")
                    promise {
                        for i in 0..10 do
                            do! Async.Sleep 500
                            sampleList.Update (fun li -> i::li)

                    } |> ignore

                )
            ] [
                h2 [] [text "Sample list goes here"]
                h3 [] [text "Check the console for paint event logs"]
                ol [] [
                    sampleList.View
                    |> Doc.BindSeqCached (fun x -> li [attr.id(string x)] [text "sample list element"])
                ]
                
            ]
        ] |> Doc.RunById "main"