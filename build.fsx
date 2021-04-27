#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard"

#nowarn "0052"

open System.Text.RegularExpressions
open System
open Fake.Core
open Fake.DotNet
open Fake.IO

let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ -> failwith (tool + " was not found in path.")

let nodeTool = platformTool "node" "node.exe"
let npmTool = platformTool "npm" "npm.cmd"

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let runDotNet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

Target.create "Clean" (fun _ ->
    [ "src\\Client\\deploy" ]
    |> Shell.cleanDirs)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Npm version:"
    runTool npmTool "--version"  __SOURCE_DIRECTORY__
    runTool npmTool "install" __SOURCE_DIRECTORY__)

Target.create "Run" (fun _ ->
    async {
        runDotNet "fable watch src\\Client --run webpack serve" __SOURCE_DIRECTORY__
    }
    |> Async.RunSynchronously
    |> ignore)

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Run"

Target.runOrDefaultWithArguments "Run"
