using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Podcasts
{
    public class LazyCacheEnumerable<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        internal List<T> Cache = new List<T>();
        internal IEnumerator<T> InternalEnumerator = null;

        public int Count
        {
            get
            {
                if (InternalEnumerator != null)
                {
                    return ((IEnumerable<T>)this).Count();
                }
                else
                {
                    return Cache.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (this.Cache.Count > index)
                {
                    return Cache[index];
                }
                else if (InternalEnumerator == null)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return this.ElementAt(index);
                }
            }
        }

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
                if (Enumerable.InternalEnumerator == null)
                {
                    return false;
                }

                if (CurrentCacheIndex < (Enumerable.Cache.Count - 1))
                {
                    CurrentCacheIndex++;
                    return true;
                }

                var result = Enumerable.InternalEnumerator.MoveNext();
                if (!result)
                {
                    Enumerable.InternalEnumerator.Dispose();
                    Enumerable.InternalEnumerator = null;
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