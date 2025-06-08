using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Piktosaur.Utils
{
    public static class Throttler
    {
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        private static DateTime lastCall = DateTime.MinValue;

        public static async Task ThrottleWinRT()
        {
            await semaphore.WaitAsync();

            await Task.Delay(5);

            semaphore.Release();
        }
    }
}
