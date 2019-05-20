using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFramework.Pan.Infrastructure
{
    public static class Extension
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (T obj in source.OrEmptyIfNull<T>())
                act(obj);
            return source;
        }
    }
}