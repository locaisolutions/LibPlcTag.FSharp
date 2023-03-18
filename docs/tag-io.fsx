(**

# Tag I/O

There are 3 primary approaches to reading and writing PLC tags. 
Prefer the event based approach whenever possible as it offers the
most flexibility and can greatly simplify application code.

### Synchronous
- non-zero timeout value in milliseconds is required (default value is 5000)
- throws an exception if the operation did not complete successfully before the timeout

*)

try
    // read data synchronously
    let myData = tag.Read(timeout = 5000)

    // do something with myData
    // ...

    // write data synchronously
    tag.Write(myData, timeout = 5000)
with :? LibPlcTagError as ex ->
    // handle I/O failure

(**

### Asynchronous - using polling
- non-zero polling interval in milliseconds is required (default value is 100)
- throws an exception if the operation did not complete successfully

*)

task {
    try
        // read data asynchronously
        let! myData = tag.ReadAsync(pollingInterval = 100, cancellationToken = myToken)

        // do something with myData
        // ...

        // write data asynchronously
        do! tag.WriteAsync(myData, pollingInterval = 100, cancellationToken = myToken)
    with :? LibPlcTagError as ex ->
        // handle I/O failure
}

(**

### Asynchronous - event based (prefered)
- events are automatically triggered for every I/O operation
- frees the caller from having to explicitly wait for the operation to complete
- requires a final call to `GetData()` to retrieve the result

*)

use _ =
    tag.ReadCompleted.Subscribe(fun status ->
        if status = Status.Ok then
            // read operation completed successfully
            myData = tag.GetData()

            // do something with myData
            // ...
        else
            // read operation was not successful,
            // handle error case or throw an exception
            ())

// listen to other events 
// ...

// begin write operation (fire and forget)
tag.BeginWrite(myData)