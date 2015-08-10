using System;
using System.Threading;

namespace PodcastsService
{
    public static class MutexExtensions
    {
        public static bool Acquire(this Mutex mutex, TimeSpan timeout, Action action)
        {
            if(mutex.WaitOne(timeout))
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
    }
}
