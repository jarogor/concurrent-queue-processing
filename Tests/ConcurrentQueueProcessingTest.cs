using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ConcurrentQueueProcessing.Source;
using Xunit;

namespace ConcurrentQueueProcessing.Tests
{
    public class ConcurrentQueueProcessingTest
    {
        /// <summary>
        /// Логика теста:
        ///  - на вход список строк
        ///  - обработка элемента заключается в суммировании длин строк в обработчике
        /// </summary>
        private readonly List<string> _data = new()
        {
            "a",
            "ab",
            "abc",
        };

        private int _times;
        private readonly ConcurrentQueue<int> _result = new();

        [Theory]
        [InlineData(1, 1)]
        [InlineData(3, 1)]
        [InlineData(1, 10)]
        [InlineData(3, 10)]
        [InlineData(10, 10)]
        public void Test(int maxTreads, int numberOfProvides)
        {
            _times = numberOfProvides;
            var processing = new ConcurrentQueueProcessing<string>(maxTreads, DataProvider, ItemProcessing);

            Assert.True(processing.Run());

            processing.Continue();

            var expected = numberOfProvides * _data.Sum(it => it.Length);

            Assert.Equal(expected, _result.Sum());
        }

        private IEnumerable<string> DataProvider()
        {
            if (_times == 0)
            {
                return new List<string>();
            }
            _times--;

            return _data;
        }

        private void ItemProcessing(string item)
        {
            _result.Enqueue(item.Length);
        }
    }
}
