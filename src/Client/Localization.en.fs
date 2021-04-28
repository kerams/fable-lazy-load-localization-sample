module En

Fable.Core.JsInterop.exportDefault { new Common.ILocInfo with
    member _.Lang = "English"
    member _.Hello f = sprintf "Hello, %s." f }
