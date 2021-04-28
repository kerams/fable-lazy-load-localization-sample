module Fr

Fable.Core.JsInterop.exportDefault { new Common.ILocInfo with
    member _.Lang = "French"
    member _.Hello f = sprintf "Salut, %s." f }