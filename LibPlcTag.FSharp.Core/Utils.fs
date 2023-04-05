module LibPlcTag.Utils

open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

[<Extension>]
type IAsyncEnumerableExtension =

    [<Extension>]
    static member ForEachAsync<'TSource>
        (
            source: IAsyncEnumerable<'TSource>,
            cancellationToken: CancellationToken,
            body: 'TSource -> CancellationToken -> ValueTask
        ) : Task =
        task {
            let asyncEnumerator = source.GetAsyncEnumerator(cancellationToken)
            let mutable canMoveNext = true

            while canMoveNext do
                match! asyncEnumerator.MoveNextAsync() with
                | true -> do! body asyncEnumerator.Current cancellationToken
                | false -> canMoveNext <- false
        }
        :> Task

    [<Extension>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline ForEachAsync<'TSource>
        (
            source: IAsyncEnumerable<'TSource>,
            body: 'TSource -> CancellationToken -> ValueTask
        ) : Task =
        source.ForEachAsync(CancellationToken.None, body)
