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
    /// Без получения результатов обработки.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class ConcurrentQueueProcessing<TInput>
    {
        private readonly Task[] _tasks;

        // Минимальное количество элементов в очереди для запуска процесса её пополнения.
        private readonly int _refillThreshold;

        // Таймаут для оценки остатка очереди.
        private readonly int _monitorTimeoutMilliseconds;

        protected readonly ConcurrentQueue<TInput> Input;

        private readonly Func<IEnumerable<TInput>> _collectionDataProvider;
        private readonly Action<TInput>? _itemProcessing;

        /// <summary>
        /// Базовый приватный конструктор.
        /// </summary>
        /// <param name="maxTasksCount"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="collectionDataProvider"></param>
        protected ConcurrentQueueProcessing(
                int maxTasksCount,
                int timeoutMilliseconds,
                Func<IEnumerable<TInput>> collectionDataProvider
            )
        {
            _tasks = new Task[maxTasksCount];
            _monitorTimeoutMilliseconds = timeoutMilliseconds;
            _refillThreshold = maxTasksCount;

            Input = new ConcurrentQueue<TInput>();
            _collectionDataProvider = collectionDataProvider;
        }

        /// <summary>
        /// Конструктор для использования БЕЗ возврата результатов обработки элементов очереди.
        /// </summary>
        /// <param name="maxTasksCount"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="collectionDataProvider"></param>
        /// <param name="itemProcessing"></param>
        public ConcurrentQueueProcessing(
                int maxTasksCount,
                int timeoutMilliseconds,
                Func<IEnumerable<TInput>> collectionDataProvider,
                Action<TInput> itemProcessing
            ) : this(
                maxTasksCount,
                timeoutMilliseconds,
                collectionDataProvider
            )
        {
            _itemProcessing = itemProcessing;
        }

        public bool Run()
        {
            // 1. Заполнение очереди элементами.
            if (!FillQueue())
            {
                return false;
            }

            // 2. Запуск потоков обработки элементов.
            Processing();

            // 3. Отслеживание состояния очереди и пополнение по мере её опустошения.
            MonitorAndRefillQueue();

            return true;
        }

        private bool FillQueue()
        {
            try
            {
                var enumerable = _collectionDataProvider().ToList();
                if (enumerable.Count == 0)
                {
                    return false;
                }

                foreach (var item in enumerable)
                {
                    Input.Enqueue(item);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Processing()
        {
            for (var i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = Task.Factory.StartNew(ReadQueue);
            }

            Task.WaitAll(_tasks);
        }

        protected virtual void ReadQueue()
        {
            try
            {
                while (Input.TryDequeue(out var item))
                {
                    _itemProcessing?.Invoke(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void MonitorAndRefillQueue()
        {
            while (true)
            {
                if (Input.Count > _refillThreshold)
                {
                    Thread.Sleep(_monitorTimeoutMilliseconds);
                }

                if (FillQueue())
                {
                    continue;
                }

                return;
            }
        }
    }
}
