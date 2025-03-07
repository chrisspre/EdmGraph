using System.Text;

namespace omm.generator;

static class EnumerableExtensions
{
    //     public static IEnumerable<(T, Q)> OfPropertyType<T, P, Q>(this IEnumerable<T> source, Func<T, P> selector)
    //         where Q : P
    //     {
    //         foreach (var item in source)
    //         {
    //             if (selector(item) is Q q)
    //             {
    //                 yield return (item, q);
    //             }
    //         }
    //     }

    //     public delegate bool TryCast<P, Q>(P input, out Q output);

    //     public static IEnumerable<(T, P)> OfPropertyType<T, P>(this IEnumerable<T> source, TryCast<T, P> tryCast)
    //     {
    //         foreach (var item in source)
    //         {
    //             if (tryCast(item, out var prop))
    //             {
    //                 yield return (item, prop);
    //             }
    //         }
    //     }


    public static string Join<T>(this IEnumerable<T> enumerable, string separator, string conjunction)
    {
        var builder = new StringBuilder();

        foreach (var (first, item, last) in enumerable.WithFirstAndLast())
        {
            if (!first)
            {
                builder.Append(last ? conjunction : separator);
            }
            builder.Append(item);
        }
        return builder.ToString();
    }

    public static IEnumerable<(bool, T, bool)> WithFirstAndLast<T>(this IEnumerable<T> enumerable)
    {
        var first = true;
        var enumerator = enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            yield break;
        }
        var previous = enumerator.Current;
        while (enumerator.MoveNext())
        {
            yield return (first, previous, false);
            previous = enumerator.Current;
            first = false;
        }
        yield return (first, previous, true);
    }
}
