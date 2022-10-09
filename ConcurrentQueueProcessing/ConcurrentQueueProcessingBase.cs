using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConcurrentQueueProcessing {
    public abstract class ConcurrentQueueProcessingBase<TInput> {
        protected readonly ConcurrentQueue<TInput> Input;

        private readonly Task[] _tasks;
        private readonly Func<IEnumerable<TInput>> _dataProvider;

        protected ConcurrentQueueProcessingBase(int maxTasksCount, Func<IEnumerable<TInput>> dataProvider) {
            Input = new ConcurrentQueue<TInput>();
            _tasks = new Task[maxTasksCount];
            _dataProvider = dataProvider;
        }

        protected abstract void ReadQueue();

        /// <summary>
        /// Запуск обработки для одного запроса дата провайдера:
        ///     - запросит порцию данных
        ///     - обработает
        ///     - завершится
        /// </summary>
        /// <returns></returns>
        public bool Start() {
            if (!FillQueue()) {
                return false;
            }

            for (var i = 0; i < _tasks.Length; i++) {
                _tasks[i] = Task.Factory.StartNew(ReadQueue);
            }

            return true;
        }

        /// <summary>
        /// Запуск с отслеживанием и пополнением очереди данными.
        /// Дата провайдер вызывается до тех пор, пока он что-либо возвращает.
        /// Обработка завершится только когда дата провайдер прекратит поставлять данные.
        /// </summary>
        public void StartTracking() {
            while (true) {
                if (!Input.IsEmpty || Start()) {
                    continue;
                }

                break;
            }

            var list = _tasks.ToList();
            list.RemoveAll(it => it == null);
            Task.WaitAll(list.ToArray());
        }

        private bool FillQueue() {
            try {
                var enumerable = _dataProvider().ToList();
                if (enumerable.Count == 0) {
                    return false;
                }

                foreach (var item in enumerable) {
                    Input.Enqueue(item);
                }

                return true;
            } catch {
                return false;
            }
        }
    }
}
