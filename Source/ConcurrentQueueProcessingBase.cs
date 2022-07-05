using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConcurrentQueueProcessing.Source
{
    public abstract class ConcurrentQueueProcessingBase<TInput>
    {
        protected readonly ConcurrentQueue<TInput> Input;

        private readonly Task[] _tasks;
        private readonly Func<IEnumerable<TInput>> _collectionDataProvider;

        protected ConcurrentQueueProcessingBase(int maxTasksCount, Func<IEnumerable<TInput>> collectionDataProvider)
        {
            Input = new ConcurrentQueue<TInput>();
            _tasks = new Task[maxTasksCount];
            _collectionDataProvider = collectionDataProvider;
        }

        protected abstract void ReadQueue();

        public bool Run()
        {
            if (!FillQueue())
            {
                return false;
            }

            for (var i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = Task.Factory.StartNew(ReadQueue);
            }

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

        public void Continue()
        {
            while (true)
            {
                if (!Input.IsEmpty || Run())
                {
                    continue;
                }

                break;
            }

            Task.WaitAll(_tasks);
        }
    }
}
