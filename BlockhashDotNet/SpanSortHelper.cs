// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlockhashDotNet
{
    internal static class IntrospectiveSortUtilities
    {
        // This is the threshold where Introspective sort switches to Insertion sort.
        // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
        // Large value types may benefit from a smaller number.
        internal const int IntrosortSizeThreshold = 16;

        internal static int FloorLog2PlusOne(int n)
        {
            int result = 0;
            while (n >= 1)
            {
                result++;
                n = n / 2;
            }
            return result;
        }

        internal static void ThrowOrIgnoreBadComparer(object comparer)
        {
            throw new ArgumentException($"Bad Comparer: {comparer}");
        }
    }

    internal static class SpanSortHelper
    {
        public static void Sort<T>(this Span<T> keys, IComparer<T> comparer = null)
        {
            Sort<T>(keys, comparer != null ? new Comparison<T>(comparer.Compare) : Comparer<T>.Default.Compare);
        }

        public static void Sort<T>(this Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(keys != null, "Check the arguments in the caller!");
            Debug.Assert(keys.Length >= 0, "Check the arguments in the caller!");

            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            // Add a try block here to detect bogus comparisons
            try
            {
                IntrospectiveSort(keys, comparer);
            }
            catch (IndexOutOfRangeException)
            {
                IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("IComparerFailed", e);
            }
        }

        private static void SwapIfGreater<T>(Span<T> keys, Comparison<T> comparer, int a, int b)
        {
            if (a != b)
            {
                if (comparer(keys[a], keys[b]) > 0)
                {
                    T key = keys[a];
                    keys[a] = keys[b];
                    keys[b] = key;
                }
            }
        }

        private static void Swap<T>(Span<T> a, int i, int j)
        {
            if (i != j)
            {
                var t = a[i];
                a[i] = a[j];
                a[j] = t;
            }
        }

        internal static void IntrospectiveSort<T>(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(keys != null);
            Debug.Assert(comparer != null);
            Debug.Assert(keys.Length >= 0);

            if (keys.Length < 2)
                return;

            IntroSort(keys, 0, keys.Length - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length), comparer);
        }

        private static void IntroSort<T>(Span<T> keys, int lo, int hi, int depthLimit, Comparison<T> comparer)
        {
            Debug.Assert(keys != null);
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);
            Debug.Assert(hi < keys.Length);

            while (hi > lo)
            {
                int partitionSize = hi - lo + 1;
                if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                {
                    if (partitionSize == 1)
                    {
                        return;
                    }
                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, lo, hi);
                        return;
                    }
                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, lo, hi - 1);
                        SwapIfGreater(keys, comparer, lo, hi);
                        SwapIfGreater(keys, comparer, hi - 1, hi);
                        return;
                    }

                    InsertionSort(keys, lo, hi, comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    Heapsort(keys, lo, hi, comparer);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(keys, lo, hi, comparer);
                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys, p + 1, hi, depthLimit, comparer);
                hi = p - 1;
            }
        }

        private static int PickPivotAndPartition<T>(Span<T> keys, int lo, int hi, Comparison<T> comparer)
        {
            Debug.Assert(keys != null);
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);
            Debug.Assert(hi > lo);
            Debug.Assert(hi < keys.Length);

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int middle = lo + ((hi - lo) / 2);

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, lo, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, lo, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer(keys[++left], pivot) < 0) ;
                while (comparer(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            Swap(keys, left, (hi - 1));
            return left;
        }

        private static void Heapsort<T>(Span<T> keys, int lo, int hi, Comparison<T> comparer)
        {
            Debug.Assert(keys != null);
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);
            Debug.Assert(hi > lo);
            Debug.Assert(hi < keys.Length);

            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; i = i - 1)
            {
                DownHeap(keys, i, n, lo, comparer);
            }
            for (int i = n; i > 1; i = i - 1)
            {
                Swap(keys, lo, lo + i - 1);
                DownHeap(keys, 1, i - 1, lo, comparer);
            }
        }

        private static void DownHeap<T>(Span<T> keys, int i, int n, int lo, Comparison<T> comparer)
        {
            Debug.Assert(keys != null);
            Debug.Assert(comparer != null);
            Debug.Assert(lo >= 0);
            Debug.Assert(lo < keys.Length);

            T d = keys[lo + i - 1];
            int child;
            while (i <= n / 2)
            {
                child = 2 * i;
                if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                {
                    child++;
                }
                if (!(comparer(d, keys[lo + child - 1]) < 0))
                    break;
                keys[lo + i - 1] = keys[lo + child - 1];
                i = child;
            }
            keys[lo + i - 1] = d;
        }

        private static void InsertionSort<T>(Span<T> keys, int lo, int hi, Comparison<T> comparer)
        {
            Debug.Assert(keys != null);
            Debug.Assert(lo >= 0);
            Debug.Assert(hi >= lo);
            Debug.Assert(hi <= keys.Length);

            int i, j;
            T t;
            for (i = lo; i < hi; i++)
            {
                j = i;
                t = keys[i + 1];
                while (j >= lo && comparer(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }
                keys[j + 1] = t;
            }
        }
    }
}
