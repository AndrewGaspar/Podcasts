using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Podcasts
{
    // Shamelessly stolen from http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx
    public class AsyncSemaphore
    {
        private int m_currentCount;
        private readonly Queue<TaskCompletionSource<bool>> m_waiters
            = new Queue<TaskCompletionSource<bool>>();
        private readonly static Task s_completed = Task.FromResult(true);

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
            m_currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (m_waiters)
            {
                if (m_currentCount > 0)
                {
                    --m_currentCount;
                    return s_completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    m_waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (m_waiters)
            {
                if (m_waiters.Count > 0)
                {
                    toRelease = m_waiters.Dequeue();
                }
                else
                {
                    ++m_currentCount;
                }
            }

            if (toRelease != null)
            {
                toRelease.SetResult(true);
            }
        }

        public async Task<T> ExclusionRegionAsync<T>(Func<Task<T>> func)
        {
            try
            {
                await WaitAsync();
                var result = await func();
                Release();
                return result;
            }
            catch
            {
                Release();
                throw;
            }
        }

        public async Task ExclusionRegionAsync(Func<Task> action)
        {
            await ExclusionRegionAsync(async () =>
            {
                await action();
                return 1;
            });
        }
    }

    public class AsyncMutex : AsyncSemaphore
    {
        public AsyncMutex() : base(1)
        {

        }
    }
}
