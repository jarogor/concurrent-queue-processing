using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentQueueProcessing
{
    /// <summary>
    /// Обработка коллекции данных при помощи конкурентной очереди и возможностью подгрузки данных по мере опустошения очереди.
    /// С получением результатов обработки.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ConcurrentQueueProcessingReturnable<TInput, TOutput> : ConcurrentQueueProcessing<TInput>
    {
        private readonly ConcurrentQueue<TOutput> _output;
        private readonly Func<TInput, TOutput> _itemProcessing;

        /// <summary>
        /// Конструктор для использования возврата результатов обработки элементов очереди.
        /// </summary>
        /// <param name="maxTasksCount"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="collectionDataProvider"></param>
        /// <param name="output"></param>
        /// <param name="itemProcessing"></param>
        public ConcurrentQueueProcessingReturnable(
                int maxTasksCount,
                int timeoutMilliseconds,
                Func<IEnumerable<TInput>> collectionDataProvider,
                Func<TInput, TOutput> itemProcessing,
                ConcurrentQueue<TOutput> output
            ) : base(
                maxTasksCount,
                timeoutMilliseconds,
                collectionDataProvider
            )
        {
            _output = output;
            _itemProcessing = itemProcessing;
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
}
