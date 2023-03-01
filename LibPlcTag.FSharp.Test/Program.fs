open Expecto

[<EntryPoint>]
let main args =
    runTestsInAssemblyWithCLIArgs [| CLIArguments.Sequenced |] args
