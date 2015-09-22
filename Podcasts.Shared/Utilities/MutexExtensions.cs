using System;
using System.Threading;
using System.Threading.Tasks;

namespace Podcasts.Utilities
{
    public static class MutexExtensions
    {
        public static bool Acquire(this Mutex mutex, TimeSpan timeout, Action action)
        {
            if (mutex.WaitOne(timeout))
            {
                try
                {
                    action();
                    mutex.ReleaseMutex();
                }
                catch
                {
                    mutex.ReleaseMutex();
                    throw;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Acquire(this Mutex mutex, Action action)
        {
            return Acquire(mutex, new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: -1), action);
        }

        public static async Task<T> ExclusionRegionAsync<T>(this SemaphoreSlim semaphore, Func<Task<T>> func)
        {
            try
            {
                await semaphore.WaitAsync();
                var result = await func();
                semaphore.Release();
                return result;
            }
            catch
            {
                semaphore.Release();
                throw;
            }
        }

        public static async Task ExclusionRegionAsync(this SemaphoreSlim semaphore, Func<Task> action)
        {
            await semaphore.ExclusionRegionAsync(async () =>
            {
                await action();
                return 1;
            });
        }
    }
}