# LibPlcTag.FSharp

**LibPlcTag.FSharp** is a thin F# wrapper around [libplctag](https://github.com/libplctag/libplctag), the C library for PLC communication. It is designed to enable simple and efficient PLC communication from F#.

### Features

- *Event-driven* - True asynchronous I/0 via independent [MailboxProcessor](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-control-fsharpmailboxprocessor-1.html) instances.
- *Minimalist* - Directly exposes much of the C API for maximum flexibility.
- *Plug and play* - Define your own `TagMapper` instances to control data marshalling behavior. 💪
