namespace WebSharper.PerformanceAPI.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.PerformanceAPI
open WebSharper.PerformanceAPI.PerformanceEntries

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"index.html", ClientLoad.FromDocument>

    let status = Var.Create "Ready."
    let entryCount = Var.Create "0"
    let paintCount = Var.Create "0"
    let paintTimings = Var.Create ([] : string list)
    let entries = Var.Create ([] : string list)

    let describe (e: PerformanceEntry) =
        let duration =
            if e.Duration > 0.0 then sprintf " · %.1f ms" e.Duration else ""
        sprintf "%s — %s · start %.1f ms%s" (string e.EntryType) e.Name e.StartTime duration

    let refresh () =
        let all = JS.Window.Performance.GetEntries() |> List.ofArray
        let paints = all |> List.filter (fun e -> e.EntryType = EntryType.Paint)
        entryCount := string all.Length
        paintCount := string paints.Length
        entries := all |> List.map describe
        paintTimings := paints |> List.map describe
        status := $"Collected {all.Length} entries from performance.getEntries()."

    let runMark () =
        let perf = JS.Window.Performance
        perf.Mark("sample-start") |> ignore
        perf.Mark("sample-end") |> ignore
        perf.Measure("sample-measure", "sample-start", "sample-end") |> ignore
        status := "Added a mark and a measure, then refreshed the snapshot."
        refresh ()

    [<SPAEntryPoint>]
    let Main () =
        // Watch paint entries as they are recorded by the browser. `buffered`
        // replays the paint entries that happened before the observer was set up.
        PerformanceObserver(fun (list: PerformanceObserver.EntryList) _ ->
            for e in list.GetEntries() do
                Console.Log $"observer: {e.EntryType} — {e.Name} @ {e.StartTime} ms"
            refresh ()
        ).Observe(PerformanceObserver.Options(EntryType.Paint, true))

        refresh ()

        IndexTemplate()
            .status(status.V)
            .entryCount(entryCount.V)
            .paintCount(paintCount.V)
            .paintTimings(
                Doc.BindSeqCached (fun s -> Doc.Element "li" [] [ Doc.TextNode s ]) paintTimings.View
            )
            .entries(
                Doc.BindSeqCached (fun s -> Doc.Element "li" [] [ Doc.TextNode s ]) entries.View
            )
            .refresh(fun _ -> refresh ())
            .runMark(fun _ -> runMark ())
            .Bind()
