using System;
using System.Collections;
using System.Collections.Generic;

namespace Podcasts
{
    public class LazyCacheEnumerable<T> : IEnumerable<T>
    {
        internal List<T> Cache = new List<T>();
        internal IEnumerator<T> InternalEnumerator = null;

        public LazyCacheEnumerable(IEnumerable<T> enumerable)
        {
            InternalEnumerator = enumerable.GetEnumerator();
        }

        private class Enumerator : IEnumerator<T>
        {
            private const int UninitializedIndex = -1;

            private LazyCacheEnumerable<T> Enumerable;
            private int CurrentCacheIndex = UninitializedIndex;

            public Enumerator(LazyCacheEnumerable<T> enumerable)
            {
                Enumerable = enumerable;
            }

            public T Current
            {
                get
                {
                    try
                    {
                        return Enumerable.Cache[CurrentCacheIndex];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                // can't dispose the underlying enumerator since it's cached
                // not really compatible with manual resource management
            }

            public bool MoveNext()
            {
                if (CurrentCacheIndex < (Enumerable.Cache.Count - 1))
                {
                    CurrentCacheIndex++;
                    return true;
                }

                var result = Enumerable.InternalEnumerator.MoveNext();
                if (!result)
                {
                    return false;
                }

                Enumerable.Cache.Add(Enumerable.InternalEnumerator.Current);
                CurrentCacheIndex++;
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}