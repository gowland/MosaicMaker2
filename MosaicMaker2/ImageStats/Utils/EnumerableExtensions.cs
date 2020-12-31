using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageStats.Utils
{
    public static class EnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }


        public static IEnumerable<int> IndicesOf<T>(this IEnumerable<T> list, T element)
        {
            return list.Select((value, index) => new { value, index })
                .Where(tuple => tuple.value.Equals(element))
                .Select(tuple => tuple.index);
        }

        public static int NthIndexOf<T>(this IEnumerable<T> list, T element, int n)
        {
            var indices = list.IndicesOf(element).ToList();
            return n > 0 && indices.Count >= n ? indices.Skip(n - 1).First() : -1;
        }

        public static T UnlikeValue<T, U>(this IEnumerable<T> numbers, Func<T, U> groupMethod)
        {
            return numbers.GroupBy(groupMethod)
                .Where(g => g.Count() == 1)
                .Select(g => g.Single())
                .Single();
        }

        // Lifted from: https://stackoverflow.com/a/3670089/20570	
        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        // Lifted from: https://stackoverflow.com/a/3670089/20570	
        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
        {
            var cnt = new Dictionary<T, int>(comparer);
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static IEnumerable<T> SkipUntil<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return list.SkipWhile(item => !predicate(item));
        }

        public static IEnumerable<IEnumerable<T>> Pairs<T>(this IEnumerable<T> source)
        {
            int count = source.Count();
            return source
                .Take(count - 1)
                .SelectMany((element, index) =>
                    source
                        .Skip(index + 1)
                        .Take(1)
                        .Select(c => new T[] { element, c }));
        }

        /// <summary>
        ///   Returns all combinations of a chosen amount of selected elements in the sequence.
        /// </summary>
        /// <typeparam name = "T">The type of the elements of the input sequence.</typeparam>
        /// <param name = "source">The source for this extension method.</param>
        /// <param name = "select">The amount of elements to select for every combination.</param>
        /// <param name = "repetition">True when repetition of elements is allowed.</param>
        /// <returns>All combinations of a chosen amount of selected elements in the sequence.</returns>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> source, int select, bool repetition = false)
        {
            return select == 0
                ? new[] { new T[0] }
                : source.SelectMany((element, index) =>
                    source
                        .Skip(repetition ? index : index + 1)
                        .Combinations(select - 1, repetition)
                        .Select(c => new[] { element }.Concat(c)));
        }

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> list)
        {
            return list ?? new T[0];
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }

        private static readonly Random _rand = new Random();

        public static IEnumerable<T> Random<T>(this IEnumerable<T> list)
        {
            return list.OrderBy(_ => _rand.Next());
        }

        public static IEnumerable<IEnumerable<T>> AsPages<T>(this IEnumerable<T> orig, int pageSize)
        {
            var q = orig;
            while (q.Any())
            {
                yield return q.Take(pageSize);
                q = q.Skip(pageSize);
            }
        }
    }
}