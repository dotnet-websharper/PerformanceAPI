namespace WebSharper.Bindings.PerformanceAPI

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =
    [<AutoOpen>]
    module Types = 
        let DOMHighResTimeStamp = T<double>
    module Methods =
        let ToJson = "toJSON" => T<unit> ^-> T<string>

    let Size = 
        Class "Size"
        |+> Instance [
            "width" =? T<uint>
            "height" =? T<uint>
        ]
    
    let PerformanceEntryType =
        Pattern.EnumStrings "PerformanceEntry.EntryType" [
            "element"
            "event"
            "first-input"
            "largest-contentful-paint"
            "layout-shift"
            "longtask"
            "mark"
            "measure"
            "navigation"
            "paint"
            "resource"
            "taskattribution"
            "visibility-state"
        ]

    let PerformanceEntry = 
        AbstractClass "PerformanceEntry"
        |+> Instance [
            "name" =? T<string>
            "entryType" =? PerformanceEntryType
            "startTime" =? DOMHighResTimeStamp
            "duration" =? DOMHighResTimeStamp
            Methods.ToJson
        ]
    module PerformanceEntries =
        let perfEntry name =
            Class name
            |=> Inherits PerformanceEntry
        let InitiatorType =
            Pattern.EnumStrings "PerformanceResourceTiming.InitiatorType" [
                "audio"
                "beacon"
                "body"
                "css"
                "early-hint"
                "embed"
                "fetch"
                "frame"
                "iframe"
                "image"
                "img"
                "input"
                "link"
                "navigation"
                "object"
                "ping"
                "script"
                "track"
                "video"
                "xmlhttprequest"
            ]
            |+> Static [
                "icon" =? TSelf
                |> WithComment "Non-standard and only reported by Safari"
            ]
        let PerformanceMark =
            perfEntry "PerformanceMark"
            |+> Static [
                Constructor T<unit>
            ]
            |+> Instance [
                "detail" =? T<obj>
            ]
        let PerformanceMeasure =
            perfEntry "PerformanceMeasure"
            |+> Static [
                Constructor T<unit>
            ]
            |+> Instance [
                "detail" =? T<obj>
            ]

        let LargestContentfulPaint =
            perfEntry "LargestContentfulPaint"
            |+> Instance [
                "element" =? T<Dom.Element>
                "renderTime" =? DOMHighResTimeStamp
                "loadTime" =? DOMHighResTimeStamp
                "size" =? Size
                "id" =? T<string>
                "url" =? !?T<string>
                
            ]
        
        let LayoutShift =
            let LayoutShiftAttribution =
                Class "LayoutShiftAttribution"
                |+> Instance [
                    "node" =? T<Dom.Node>
                    "previousRect" =? T<Dom.Rect> // TODO readonlyrect
                    "currentRect" =? T<Dom.Rect>
                    Methods.ToJson
                ]
            perfEntry "LayoutShift"
            |=> Nested [LayoutShiftAttribution]
            |+> Instance [
                "value" =? T<float>
                "hadRecentInput" =? T<bool>
                    |> WithComment "Returns true if lastInputTime is less than 500 milliseconds in the past."
                "lastInputTime" =? DOMHighResTimeStamp
                "sources" =? !|LayoutShiftAttribution
            ]

        let PerformanceElementTiming =
            perfEntry "PerformanceElementTiming"
            |+> Instance [
                "element" =? T<Dom.Element>
                "id" =? T<string>
                "identifier" =? T<string>
                "intersectionRect" =? T<Dom.Rect> // todo readonlyrect
                "loadTime" =? DOMHighResTimeStamp
                "naturalHeight" =? T<uint>
                "naturalWidth" =? T<uint>
                "renderTime" =? DOMHighResTimeStamp
                "url" =? T<string> + T<int> 
                    |> WithComment "Initial URL of the resources request for images, 0 for text"
            ]

        let PerformanceEventTiming =
            perfEntry "PerformanceEventTiming"
            |+> Instance [
                "cancelable" =? T<bool>
                "interactionId" =? T<string>
                "processingStart" =? DOMHighResTimeStamp
                "processingEnd" =? DOMHighResTimeStamp
                "target" =? T<Dom.Node>
            ]
        
        let TaskAttributionTiming =
            Class "TaskAttributionTiming"
            |+> Instance [
                "containerType" =? T<string>
                "containerSrc" =? T<string>
                "containerId" =? T<string>
                "containerName" =? T<string>
            ]
        let PerformanceLongTaskTiming =
            perfEntry "PerformanceLongTaskTiming"
            |+> Instance [
                "attribution" =? TaskAttributionTiming
            ]

        let PerformanceNavigationTiming =
            let PerformanceNavigationTimingType =
                Pattern.EnumStrings "PerformanceNavigationTiming.Type" [
                    "navigate"
                    "reload"
                    "back_forward"
                    "prerender"
                ]

            perfEntry "PerformanceNavigationTiming"
            |=> Nested [PerformanceNavigationTimingType]
            |+> Instance [
                "initiatorType" =? InitiatorType
                "domComplete" =? DOMHighResTimeStamp
                for n in ["domContentLoaded";"load";"unload"] do
                    $"{n}EventStart" =? DOMHighResTimeStamp
                    $"{n}EventEnd" =? DOMHighResTimeStamp
                "domInteractive" =? DOMHighResTimeStamp
                "redirectCount" =? T<int>
                "type" =? PerformanceNavigationTimingType
            ]
            
        let PerformanceServerTiming =
            Class "PerformanceServerTiming"
            |+> Instance [
                "description" =? T<string>
                "duration" =? T<double>
                "name" =? T<string>
                Methods.ToJson
            ]
        
        let PerformanceResourceTiming =
            perfEntry "PerformanceResourceTiming"
            |+> Instance [
                for n in ["connect";"domainLookup";"redirect";"request";"response"] do
                    $"{n}Start" =? DOMHighResTimeStamp
                    $"{n}End" =? DOMHighResTimeStamp
                "workerStart" =? DOMHighResTimeStamp
                "fetchStart" =? DOMHighResTimeStamp
                "secureConnectionStart" =? DOMHighResTimeStamp
                "firstInterimResponseStart" =? DOMHighResTimeStamp

                "decodedBodySize" =? T<int>
                "encodedBodySize" =? T<int>
                "initiatorType" =? InitiatorType
                "nextHopProtocol" =? T<string>
                "renderBlockingStatus" =? T<string>
                "responseStatus" =? T<int>
                "transferSize" =? T<int>
                "serverTiming" =? !|PerformanceServerTiming
                "deliveryType" =? T<string>
            ]

        let PerformancePaintTiming =
            perfEntry "PerformancePaintTiming"
            
        let VisibilityStateEntry =
            perfEntry "VisibilityStateEntry"
            |+> Instance [
                "entryType" =? T<string>
            ]


    let PerformanceObserver =

        let PerformanceObserverOptions =
            Class "PerformanceObserver.Options"
            |+> Static [
                ObjectConstructor (
                    (!|PerformanceEntryType)?entryTypes * !?T<bool>?buffered * !?DOMHighResTimeStamp?durationThreshold
                )
                ObjectConstructor (
                    PerformanceEntryType?``type`` * !?T<bool>?buffered * !?DOMHighResTimeStamp?durationThreshold
                )
            ]
        let PerformanceObserverEntryList =
            Class "PerformanceObserver.EntryList"
            |+> Instance [
                "getEntries" => T<unit> ^-> !|PerformanceEntry
                "getEntriesByType" => T<string>?name ^-> !|PerformanceEntry
                "getEntriesByName" => T<string>?name * !?T<string>?``type`` ^-> !|PerformanceEntry

            ]
        let PerformanceObserverCallback = PerformanceObserverEntryList?entries * TSelf?observer * (!?T<int>)?droppedEntriesCount ^-> T<unit>

        Class "PerformanceObserver"
        |=> Nested [PerformanceObserverOptions; PerformanceObserverEntryList]
        |+> Static [
            Constructor  (
                PerformanceObserverCallback?callback
            )
            "supportedEntryTypes" =? !| PerformanceEntryType
        ]
        |+> Instance [
            
            "observe" => PerformanceObserverOptions?options ^-> T<unit>
            "disconnect" => T<unit> ^-> T<unit>
            "takeRecords" => T<unit> ^-> !|PerformanceEntry
        ]


    let Performance = 
    
        let PerformanceMeasureOptions =
            Class "Performance.MeasureOptions"
            |+> Static [
                ObjectConstructor (
                    !?T<obj>?detail * !?DOMHighResTimeStamp?start * !?DOMHighResTimeStamp?duration * !?DOMHighResTimeStamp?``end``
                )
            ]
        let PerformanceMarkOptions =
            Class "Performance.MarkOptions"
            |+> Static [
                ObjectConstructor(
                    !?T<obj>?detail * !?DOMHighResTimeStamp?startTime
                )
            ]
        Class "performance"
        |> WithSourceName "Performance"
        |=> Nested [PerformanceMeasureOptions; PerformanceMarkOptions]
        |+> Instance [
            "eventCounts" =? T<System.Collections.Generic.Dictionary<string,int>> // TODO is a dictionary okay here?
            "timeOrigin" =? DOMHighResTimeStamp

            "clearMarks" => !?T<string>?name ^-> T<unit>
            "clearMeasures" => !?T<string>?name ^-> T<unit>
            "clearResourceTimings" => T<unit> ^-> T<unit>
            "getEntries" => T<unit> ^-> !|PerformanceEntry
            "getEntriesByName" => T<string>?name * (!? T<string>?``type``) ^-> !| PerformanceEntry
            "getEntriesByType" => T<string>?``type`` ^-> !| PerformanceEntry
            "mark" => T<string>?name * (!?PerformanceMarkOptions)?options ^-> PerformanceEntries.PerformanceMark
            "measure" => T<string>?measureName * !?DOMHighResTimeStamp?startMark * !?DOMHighResTimeStamp?endMark ^-> DOMHighResTimeStamp
            "measure" => T<string>?measureName * PerformanceMeasureOptions?measureOptions * !?DOMHighResTimeStamp?endMark ^-> DOMHighResTimeStamp
            "measureAgentSpecificMemory" => T<string>?name ^-> T<Promise<_>>[DOMHighResTimeStamp]
            "now" => T<unit> ^-> DOMHighResTimeStamp
            "setResourceTimingBufferSize" => T<uint>?maxSize ^-> T<unit>
            Methods.ToJson
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.PerformanceAPI" [
                Size
                PerformanceEntry
                PerformanceEntryType

                PerformanceObserver
                Performance
            ]
            Namespace "WebSharper.PerformanceAPI.PerformanceEntries" [
                PerformanceEntries.InitiatorType
                PerformanceEntries.PerformanceMark
                PerformanceEntries.PerformanceMeasure
                PerformanceEntries.LargestContentfulPaint
                PerformanceEntries.LayoutShift
                PerformanceEntries.PerformanceElementTiming
                PerformanceEntries.PerformanceEventTiming
                PerformanceEntries.TaskAttributionTiming
                PerformanceEntries.PerformanceLongTaskTiming
                PerformanceEntries.PerformanceNavigationTiming
                PerformanceEntries.PerformanceServerTiming
                PerformanceEntries.PerformanceResourceTiming
                PerformanceEntries.PerformancePaintTiming
                PerformanceEntries.VisibilityStateEntry
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
