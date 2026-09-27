using System;
using System.Collections.Generic;

public static class ShuffleExtensions
{
    private static readonly Random _rng = new Random();

    /// <summary>
    /// Shuffles an array in place using Fisher-Yates.
    /// </summary>
    public static void Shuffle<T>(this T[] array)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        array.Shuffle(_rng);
    }

    public static void Shuffle<T>(this T[] array, Random rng)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (rng == null) throw new ArgumentNullException(nameof(rng));

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    /// <summary>
    /// Shuffles an IList{T} (covers List{T}, arrays, etc.) in place using Fisher-Yates.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        list.Shuffle(_rng);
    }

    public static void Shuffle<T>(this IList<T> list, Random rng)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (rng == null) throw new ArgumentNullException(nameof(rng));

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}