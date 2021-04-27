module Client

open Elmish
open Fable.React
open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.React.Props
open Common

type Msg =
    | LanguageChanged of string
    | LocalizationReceived of Result<obj, exn>

type Model = { Localization: ILocInfo option; SupportedLanguages: string list; SelectedLanguage: string }

let getPreferredLanguage () =
    if DateTime.Today.Day = 1 then "en" else "fr"

let changeLanguageCommand lang =
    Cmd.OfPromise.either (fun () -> importDynamic ("./Localization." + lang + ".fs.js")) () (Ok >> LocalizationReceived) (Error >> LocalizationReceived)

let init () =
    let model = { Localization = None; SupportedLanguages = [ "en"; "fr" ]; SelectedLanguage = getPreferredLanguage () }
    model, changeLanguageCommand model.SelectedLanguage

let update msg model =
    match msg with
    | LocalizationReceived l ->
        match l with
        | Ok l ->
            { model with Localization = Some (l?``default``) }, []
        | Error e ->
            Browser.Dom.console.error e
            model, []
    | LanguageChanged lang ->
        { model with SelectedLanguage = lang }, changeLanguageCommand lang

[<Feliz.ReactComponent>]
let DeepComponent () =
    let localization = Hooks.useContext localizationContext
    str localization.Hello

[<Feliz.ReactComponent>]
let OtherComponent () =
    contextConsumer localizationContext (fun x -> str x.Lang)

[<Feliz.ReactComponent>]
let LanguageSelector supportedLanguages selected dispatch =
    select [ Value selected; OnChange (fun x -> LanguageChanged x.target?value |> dispatch) ] [
        for l in supportedLanguages do
            option [ Value l ] [ str l ]
    ]

let view model dispatch =
    match model.Localization with
    | Some l ->
        contextProvider localizationContext l [
            LanguageSelector model.SupportedLanguages model.SelectedLanguage dispatch
            DeepComponent ()
            OtherComponent ()
        ]
    | _ -> str "Loading localization"

#if DEBUG
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "app"
|> Program.run
