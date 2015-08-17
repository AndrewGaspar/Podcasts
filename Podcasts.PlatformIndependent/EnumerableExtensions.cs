using System;
using System.Collections.Generic;
using System.Linq;

namespace Podcasts
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Tuple<T, U>> Pairwise<T, U>(this IEnumerable<T> first, IEnumerable<U> second) =>
            first.Zip(second, (item1, item2) => new Tuple<T, U>(item1, item2));

        public static LazyCacheEnumerable<T> CacheResults<T>(this IEnumerable<T> enumerable) =>
            new LazyCacheEnumerable<T>(enumerable);
    }
}
