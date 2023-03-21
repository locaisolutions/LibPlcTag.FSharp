# LibPlcTag.FSharp

**LibPlcTag.FSharp** is a thin F# wrapper around [libplctag](https://github.com/libplctag/libplctag), the C library for PLC communication. It is designed to enable simple and efficient PLC communication from F#.

### Features

- *Event-driven* - True asynchronous I/0 via independent [MailboxProcessor](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-control-fsharpmailboxprocessor-1.html) instances
- *Minimalist* - Directly exposes much of the C API for maximum flexibility
- *Plug and play* - Define your own `TagMapper` instances to control data marshalling behavior

### Documentation
- Coming soon...

### Get it

.NET CLI
```bash
dotnet add package LibPlcTag.FSharp --version 1.0.1
```

Paket CLI
```bash
paket add LibPlcTag.FSharp --version 1.0.1
```

### Help wanted
- Documentation (using [FSharp.Formatting](https://fsprojects.github.io/FSharp.Formatting/), see docs folder)
  - examples
  - web hosting
  - logo (doesn't need to be fancy)
  - nuget package metadata
- Testing
  - ability to spin up multiple simulators at runtime for parallel test cases
  - building out the unit test suite
- Automation
  - keep wrapper up to date with libplctag 
  - build pipeline (with unit tests)
