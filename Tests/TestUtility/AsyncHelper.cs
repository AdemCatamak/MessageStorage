using System;
using System.Threading.Tasks;

namespace TestUtility
{
    public static class AsyncHelper
    {
        public static async Task WaitFor(TimeSpan wait, double times = 1)
        {
            double waitTotalMilliseconds = wait.TotalMilliseconds * times;
            TimeSpan result = TimeSpan.FromMilliseconds(waitTotalMilliseconds);

            await Task.Delay(result);
        }
    }
}