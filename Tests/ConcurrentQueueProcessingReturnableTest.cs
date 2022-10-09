using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using ConcurrentQueueProcessing;

using Xunit;

namespace Tests {
    public class ConcurrentQueueProcessingReturnableTest {
        /// <summary>
        /// Логика теста:
        ///  - на вход список строк, которые берутся из ключей словаря
        ///  - обработка элемента заключается в возврате длинны строки
        ///  - значения словаря -- длинна, контрольные параметры для теста
        /// </summary>
        private readonly Dictionary<string, int> _data = new() {
            {"abc", 3},
            {"a", 1},
            {"abcd", 4},
            {"ab", 2},
            {"abcde", 5},
        };

        private int _times;

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(3, 1)]
        [InlineData(1, 10)]
        [InlineData(3, 10)]
        [InlineData(10, 10)]
        public void Test(int maxTreads, int numberOfProvides) {
            _times = numberOfProvides;
            var outputQueue = new ConcurrentQueue<int>();
            var processing = new ConcurrentQueueProcessingReturnable<string, int>(
                    maxTreads,
                    DataProvider,
                    ItemProcessing,
                    ref outputQueue
                );

            processing.StartTracking();

            Assert.Equal(_data.Values.Count * numberOfProvides, outputQueue.Count);
            foreach (var item in outputQueue.ToList()) {
                Assert.Contains(item, _data.Values);
            }
        }

        private IEnumerable<string> DataProvider() {
            if (_times == 0) {
                return new List<string>();
            }
            _times--;

            return _data.Keys;
        }

        private static int ItemProcessing(string item) {
            return item.Length;
        }
    }
}
