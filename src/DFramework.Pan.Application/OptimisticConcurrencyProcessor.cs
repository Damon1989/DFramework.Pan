using System;
using System.Data.Entity.Infrastructure;

namespace DFramework.Pan
{
    public static class OptimisticConcurrencyProcessor
    {
        public static int Process(Func<int> func)
        {
            var needRetry = true;
            int ret = 0;
            do
            {
                try
                {
                    ret = func();
                    needRetry = false;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var e in (ex as DbUpdateConcurrencyException).Entries)
                    {
                        e.Reload();
                    }
                }
            } while (needRetry);
            return ret;
        }
    }
}