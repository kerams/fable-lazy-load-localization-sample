module Common

open Fable.React
open Fable.Core

type ILocInfo =
    abstract Lang: string
    abstract Hello: string -> string

let localizationContext = createContext JS.undefined<ILocInfo>