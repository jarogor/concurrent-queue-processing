using System.Collections.Concurrent;
using Xunit;

namespace ConcurrentQueueProcessing.Tests
{
    public class ProblemExampleTest
    {
        [Fact]
        public void Test()
        {
            var queue = new ConcurrentQueue<string>();
            const int count = 100;
            // const int count = 1000000000000000; // <- problem

            for (var i = 0; i < count; i++)
            {
                queue.Enqueue("debug");
            }

            Assert.Equal(queue.Count, count);
        }
    }
}
