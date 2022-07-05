using System;
using System.Collections.Generic;

namespace ConcurrentQueueProcessing.Source
{
    public class ConcurrentQueueProcessing<TInput> : ConcurrentQueueProcessingBase<TInput>
    {
        private readonly Action<TInput> _itemProcessing;

        public ConcurrentQueueProcessing(
                int maxTasksCount,
                Func<IEnumerable<TInput>> collectionDataProvider,
                Action<TInput> itemProcessing
            ) : base(maxTasksCount, collectionDataProvider)
        {
            _itemProcessing = itemProcessing;
        }

        protected override void ReadQueue()
        {
            try
            {
                while (Input.TryDequeue(out var item))
                {
                    _itemProcessing(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
