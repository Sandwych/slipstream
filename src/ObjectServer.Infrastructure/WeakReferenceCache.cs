using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.ConstrainedExecution;

namespace ObjectServer
{

    //源自：http://www.cnblogs.com/kevinShan/archive/2009/08/05/1539632.html

    public class WeakReferenceCache<TKey, TItem>
        where TItem : class
    {
        #region The Cleaner class

        private class Cleaner : CriticalFinalizerObject
        {
            #region Instance Data

            private WeakReferenceCache<TKey, TItem> _owner;

            #endregion

            #region Constructor & Finalizer

            public Cleaner(WeakReferenceCache<TKey, TItem> owner)
            {
                this._owner = owner;
            }

            ~Cleaner()
            {
                if (this._owner._autoCleanAbandonedItems)
                {
                    this._owner.CleanAbandonedItems();
                    GC.ReRegisterForFinalize(this);
                }
            }

            #endregion
        }

        #endregion

        #region Instance Data

        private const int LOCK_TIMEOUT_MSECS = 500;
        private Dictionary<TKey, WeakReference> _cachePool;
        private bool _autoCleanAbandonedItems;
        private ReaderWriterLockSlim _cacheLock;

        #endregion

        #region Constructor & Finalizer

        public WeakReferenceCache() : this(true) { }

        public WeakReferenceCache(bool autoCleanAbandonedItems)
        {
            this._cacheLock = new ReaderWriterLockSlim();
            this._cachePool = new Dictionary<TKey, WeakReference>();
            this._autoCleanAbandonedItems = autoCleanAbandonedItems;
            if (this._autoCleanAbandonedItems)
            {
                new Cleaner(this);
            }
            else
            {
                GC.SuppressFinalize(this);
            }
        }

        ~WeakReferenceCache()
        {
            this._autoCleanAbandonedItems = false;
        }

        #endregion

        #region Properties

        public bool AutoCleanAbandonedItems
        {
            get
            {
                return this._autoCleanAbandonedItems;
            }
        }

        public TItem this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (this._cacheLock.TryEnterReadLock(LOCK_TIMEOUT_MSECS))
                {
                    try
                    {
                        WeakReference weakReference;
                        if (_cachePool.TryGetValue(key, out weakReference))
                        {
                            return (TItem)weakReference.Target;
                        }
                    }
                    finally
                    {
                        this._cacheLock.ExitReadLock();
                    }
                }

                return null;
            }
            set
            {
                this.Add(key, value);
            }
        }

        #endregion

        #region Methods

        public void Add(TKey key, TItem item)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (this._cacheLock.TryEnterWriteLock(LOCK_TIMEOUT_MSECS))
            {
                try
                {
                    _cachePool[key] = new WeakReference(item);
                }
                finally
                {
                    this._cacheLock.ExitWriteLock();
                }
            }
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (this._cacheLock.TryEnterWriteLock(LOCK_TIMEOUT_MSECS))
            {
                try
                {
                    this._cachePool.Remove(key);
                }
                finally
                {
                    this._cacheLock.ExitWriteLock();
                }
            }
        }

        public void Clear()
        {
            if (this._cacheLock.TryEnterWriteLock(LOCK_TIMEOUT_MSECS))
            {
                try
                {
                    this._cachePool.Clear();
                }
                finally
                {
                    this._cacheLock.ExitWriteLock();
                }
            }
        }

        public void CleanAbandonedItems()
        {
            if (this._cacheLock.TryEnterWriteLock(LOCK_TIMEOUT_MSECS))
            {
                try
                {
                    var newCachePool = new Dictionary<TKey, WeakReference>();
                    foreach (var keyValuePair in _cachePool)
                    {
                        if (keyValuePair.Value.IsAlive)
                        {
                            newCachePool[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }

                    this._cachePool = newCachePool;
                }
                finally
                {
                    this._cacheLock.ExitWriteLock();
                }
            }
        }

        #endregion

    }
}
