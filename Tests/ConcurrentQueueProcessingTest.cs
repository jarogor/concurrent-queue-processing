using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ConcurrentQueueProcessing;
using Xunit;

namespace tests
{
    public class ConcurrentQueueProcessingTest
    {
        private readonly ConcurrentBag<int> _result = new ();
        private int _numberOfProvides;

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

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(10, 10)]
        public void Test(int maxTreads, int numberOfProvides)
        {
            _numberOfProvides = numberOfProvides;

            var processing = new ConcurrentQueueProcessing<string>(
                    maxTreads,
                    0,
                    DataProvider,
                    ItemProcessing
                );

            Assert.True(processing.Run());
            var expected = _data.Sum(it => it.Length);
            Assert.Equal(expected, _result.Sum());
        }

        private IEnumerable<string> DataProvider()
        {
            if (_numberOfProvides == 0)
            {
                return new List<string>();
            }

            _numberOfProvides--;

            return _data;
        }

        private void ItemProcessing(string item)
        {
            _result.Add(item.Length);
        }
    }
}
