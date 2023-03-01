module LibPlcTag.FSharp.Test.Utils

open Expecto
open LibPlcTag.FSharp

let inline (==?) actual expected = Expect.equal actual expected ""

let inline withTag attributes mapper func () =
    use tag = Tag.Create(attributes, mapper)
    func tag
