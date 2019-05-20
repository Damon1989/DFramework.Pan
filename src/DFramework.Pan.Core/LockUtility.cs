using System;
using System.Collections;

namespace DFramework.Pan
{
    public class LockUtility
    {
        private static readonly Hashtable _lockPool = new Hashtable();

        private static void ReleaseLock(object key, LockUtility.LockObject lockObj)
        {
            lock (LockUtility._lockPool)
            {
                lockObj.Decrement();
                if (lockObj.Counter != 0)
                    return;
                LockUtility._lockPool.Remove(key);
            }
        }

        private static LockUtility.LockObject GetLock(object key)
        {
            lock (LockUtility._lockPool)
            {
                LockUtility.LockObject lockObject = LockUtility._lockPool[key] as LockUtility.LockObject;
                if (lockObject == null)
                {
                    lockObject = new LockUtility.LockObject();
                    LockUtility._lockPool[key] = (object)lockObject;
                }
                lockObject.Increate();
                return lockObject;
            }
        }

        public static void Lock(object key, Action action)
        {
            LockUtility.LockObject lockObj = LockUtility.GetLock(key);
            try
            {
                lock (lockObj)
                    action();
            }
            finally
            {
                LockUtility.ReleaseLock(key, lockObj);
            }
        }

        private class LockObject
        {
            private volatile int _Counter;

            public int Counter
            {
                get
                {
                    return this._Counter;
                }
            }

            internal void Decrement()
            {
                --this._Counter;
            }

            internal void Increate()
            {
                ++this._Counter;
            }
        }
    }
}