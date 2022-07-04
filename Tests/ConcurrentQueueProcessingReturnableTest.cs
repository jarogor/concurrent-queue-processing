using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ConcurrentQueueProcessing;
using Xunit;

namespace tests
{
    public class ConcurrentQueueProcessingReturnableTest
    {
        private int _numberOfProvides;

        /// <summary>
        /// Логика теста:
        ///  - на вход список строк, которые берутся из ключей словаря
        ///  - обработка элемента заключается в возврате длинны строки
        ///  - значения словаря -- длинна, контрольные параметры для теста
        /// </summary>
        private readonly Dictionary<string, int> _data = new()
        {
            {"abc", 3},
            {"a", 1},
            {"abcd", 4},
            {"ab", 2},
            {"abcde", 5},
        };

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 10)]
        public void Test(int maxTreads, int numberOfProvides)
        {
            _numberOfProvides = numberOfProvides;

            var outputQueue = new ConcurrentQueue<int>();

            var processing = new ConcurrentQueueProcessingReturnable<string, int>(
                    maxTreads,
                    0,
                    DataProvider,
                    ItemProcessing,
                    outputQueue
                );

            Assert.True(processing.Run());
            Assert.Equal(outputQueue.Count, _data.Values.Count);
            foreach (var item in outputQueue.ToList())
            {
                Assert.Contains(item, _data.Values);
            }
        }

        private IEnumerable<string> DataProvider()
        {
            if (_numberOfProvides == 0)
            {
                return new List<string>();
            }

            _numberOfProvides--;

            return _data.Keys;
        }

        private static int ItemProcessing(string item)
        {
            return item.Length;
        }
    }
}
