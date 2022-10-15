using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ConcurrentQueueProcessing;

public class ConcurrentQueueProcessingReturnable<TInput, TOutput>
    : ConcurrentQueueProcessingBase<TInput>
{
    private readonly ConcurrentQueue<TOutput> _output;
    private readonly Func<TInput, TOutput> _itemProcessing;

    public ConcurrentQueueProcessingReturnable(
            int maxTasksCount,
            Func<IEnumerable<TInput>> dataProvider,
            Func<TInput, TOutput> itemProcessing,
            ref ConcurrentQueue<TOutput> output
        ) : base(
            maxTasksCount,
            dataProvider
        )
    {
        _itemProcessing = itemProcessing;
        _output = output;
    }

    protected override void ReadQueue()
    {
        try
        {
            while (Input.TryDequeue(out var item))
            {
                _output.Enqueue(_itemProcessing(item));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

