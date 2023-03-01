open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO

let initTargets () =
    let solutionDir = Path.Join(__SOURCE_DIRECTORY__, "..")
    let libDir = Path.Join(solutionDir, "LibPlcTag.FSharp.Core")
    let packageDir = Path.Join(solutionDir, "packages", "LibPlcTag.FSharp")

    let runtimes =
        [ "linux-arm64"
          "linux-x64"
          "linux-x86"
          "osx-arm64"
          "osx-x64"
          "win-arm"
          "win-arm64"
          "win-x64"
          "win-x86" ]

    Target.create "Clean" (fun _ ->
        let bin = Path.Join(libDir, "bin")
        let obj = Path.Join(libDir, "obj")
        Shell.cleanDirs [ bin; obj ])

    Target.create "Build"
    <| fun _ ->
        DotNet.exec (fun opts -> { opts with WorkingDirectory = libDir }) "build" "-c Release /p:PlatformTarget=AnyCPU"
        |> ignore

        runtimes
        |> Seq.iter (fun rid ->
            DotNet.exec
                (fun opts -> { opts with WorkingDirectory = libDir })
                "build"
                $"-c Release -r {rid} --no-self-contained"
            |> ignore)

    Target.create "Pack" (fun _ -> Shell.Exec("nuget", "pack LibPlcTag.FSharp.nuspec", packageDir) |> ignore)

    "Clean" ==> "Build" ==> "Pack" |> ignore

[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets ()
    Target.runOrDefaultWithArguments "Pack"
    0
