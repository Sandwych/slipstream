using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;

namespace SilverlightTable
{
    // ICollectionView implementation courtesy of the following blog post:
    // http://weblogs.asp.net/manishdalal/archive/2008/12/30/silverlight-datagrid-custom-sorting.aspx
    //
    // The Refresh() method has been modified to sort by our Row class via its 
    // string indexer.
    public class SortableCollectionView : ObservableCollection<Row>, ICollectionView
    {
        private CustomSortDescriptionCollection _sort;

        private bool _suppressCollectionChanged = false;

        private object _currentItem;

        private CultureInfo _culture;

        private int _currentPosition;

        private Predicate<object> _filter;


        public SortableCollectionView()
        {
            this._currentItem = null;
            this._currentPosition = -1;
        }

        public SortableCollectionView(IEnumerable<Row> rows)
            : base(rows)
        {
            this._currentItem = null;
            this._currentPosition = -1;
        }

        public SortableCollectionView(IList<Row> rows)
            : base(rows)
        {
            this._currentItem = null;
            this._currentPosition = -1;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressCollectionChanged)
                return;

            base.OnCollectionChanged(e);
        }


        protected override void InsertItem(int index, Row item)
        {
            base.InsertItem(index, item);

            if (0 == index || null == this._currentItem)
            {
                _currentItem = item;
                _currentPosition = index;
            }
        }

        public virtual object GetItemAt(int index)
        {
            if ((index >= 0) && (index < this.Count))
            {
                return this[index];
            }

            return null;
        }


        #region ICollectionView Members

        public bool CanFilter
        {
            get { return false; }
        }

        public bool CanGroup
        {
            get { return false; }
        }

        public bool CanSort
        {
            get { return true; }
        }

        public bool Contains(object item)
        {
            if (!IsValidType(item))
            {
                return false;
            }

            return this.Contains((Row)item);
        }

        private bool IsValidType(object item)
        {
            return item is Row;
        }

        public System.Globalization.CultureInfo Culture
        {
            get
            {
                return this._culture;
            }
            set
            {
                if (this._culture != value)
                {
                    this._culture = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Culture"));
                }
            }
        }

        public event EventHandler CurrentChanged;

        public event CurrentChangingEventHandler CurrentChanging;

        public object CurrentItem
        {
            get { return this._currentItem; }
        }

        public int CurrentPosition
        {
            get { return this._currentPosition; }
        }

        public IDisposable DeferRefresh()
        {
            return new SortableCollectionDeferRefresh(this);
        }

        public Predicate<object> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public ObservableCollection<GroupDescription> GroupDescriptions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ReadOnlyObservableCollection<object> Groups
        {
            get
            {
                return new ReadOnlyObservableCollection<object>(
                    new ObservableCollection<object>());
            }

        }

        public bool IsCurrentAfterLast
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return (this.CurrentPosition >= this.Count);
                }
                return true;
            }
        }

        public bool IsCurrentBeforeFirst
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return (this.CurrentPosition < 0);
                }
                return true;
            }
        }

        protected bool IsCurrentInSync
        {
            get
            {
                if (this.IsCurrentInView)
                {
                    return (this.GetItemAt(this.CurrentPosition) == this.CurrentItem);
                }

                return (this.CurrentItem == null);
            }
        }

        private bool IsCurrentInView
        {
            get
            {
                return ((0 <= this.CurrentPosition) && (this.CurrentPosition < this.Count));
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.Count == 0);
            }
        }

        public bool MoveCurrentTo(object item)
        {
            if (!IsValidType(item))
            {
                return false;
            }

            if (object.Equals(this.CurrentItem, item) && ((item != null) || this.IsCurrentInView))
            {
                return this.IsCurrentInView;
            }

            int index = this.IndexOf((Row)item);

            return this.MoveCurrentToPosition(index);
        }

        public bool MoveCurrentToFirst()
        {
            return this.MoveCurrentToPosition(0);
        }

        public bool MoveCurrentToLast()
        {
            return this.MoveCurrentToPosition(this.Count - 1);
        }

        public bool MoveCurrentToNext()
        {
            return ((this.CurrentPosition < this.Count) && this.MoveCurrentToPosition(this.CurrentPosition + 1));
        }

        public bool MoveCurrentToPrevious()
        {
            return ((this.CurrentPosition >= 0) && this.MoveCurrentToPosition(this.CurrentPosition - 1));
        }

        public bool MoveCurrentToPosition(int position)
        {
            if ((position < -1) || (position > this.Count))
            {
                throw new ArgumentOutOfRangeException("position");
            }

            if (((position != this.CurrentPosition) || !this.IsCurrentInSync) && this.OKToChangeCurrent())
            {
                bool isCurrentAfterLast = this.IsCurrentAfterLast;
                bool isCurrentBeforeFirst = this.IsCurrentBeforeFirst;

                ChangeCurrentToPosition(position);

                OnCurrentChanged();

                if (this.IsCurrentAfterLast != isCurrentAfterLast)
                {
                    this.OnPropertyChanged("IsCurrentAfterLast");
                }

                if (this.IsCurrentBeforeFirst != isCurrentBeforeFirst)
                {
                    this.OnPropertyChanged("IsCurrentBeforeFirst");
                }

                this.OnPropertyChanged("CurrentPosition");
                this.OnPropertyChanged("CurrentItem");
            }

            return this.IsCurrentInView;
        }

        private void ChangeCurrentToPosition(int position)
        {
            if (position < 0)
            {
                this._currentItem = null;
                this._currentPosition = -1;
            }
            else if (position >= this.Count)
            {
                this._currentItem = null;
                this._currentPosition = this.Count;
            }
            else
            {
                this._currentItem = this[position];
                this._currentPosition = position;
            }
        }

        protected bool OKToChangeCurrent()
        {
            CurrentChangingEventArgs args = new CurrentChangingEventArgs();
            this.OnCurrentChanging(args);
            return !args.Cancel;
        }

        protected virtual void OnCurrentChanged()
        {
            if (this.CurrentChanged != null)
            {
                this.CurrentChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnCurrentChanging(CurrentChangingEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            if (this.CurrentChanging != null)
            {
                this.CurrentChanging(this, args);
            }
        }

        protected void OnCurrentChanging()
        {
            this._currentPosition = -1;
            this.OnCurrentChanging(new CurrentChangingEventArgs(false));
        }

        protected override void ClearItems()
        {
            OnCurrentChanging();
            base.ClearItems();
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            IEnumerable<Row> rows = this;
            IOrderedEnumerable<Row> orderedRows = null;

            // use the OrderBy and ThenBy LINQ extension methods to
            // sort our data
            bool firstSort = true;
            for (int sortIndex = 0; sortIndex < _sort.Count; sortIndex++)
            {
                SortDescription sort = _sort[sortIndex];
                Func<Row, object> function = row => row[sort.PropertyName];
                if (firstSort)
                {
                    orderedRows = sort.Direction == ListSortDirection.Ascending ?
                        rows.OrderBy(function) : rows.OrderByDescending(function);

                    firstSort = false;
                }
                else
                {
                    orderedRows = sort.Direction == ListSortDirection.Ascending ?
                        orderedRows.ThenBy(function) : orderedRows.ThenByDescending(function);
                }
            }

            _suppressCollectionChanged = true;

            // re-order this collection based on the result if the above
            int index = 0;
            foreach (var row in orderedRows)
            {
                this[index++] = row;
            }

            _suppressCollectionChanged = false;

            // raise the required notification
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if (this._sort == null)
                {
                    this.SetSortDescriptions(new CustomSortDescriptionCollection());
                }
                return this._sort;
            }
        }

        private void SetSortDescriptions(CustomSortDescriptionCollection descriptions)
        {
            if (this._sort != null)
            {
                this._sort.MyCollectionChanged -= new NotifyCollectionChangedEventHandler(this.SortDescriptionsChanged);
            }

            this._sort = descriptions;

            if (this._sort != null)
            {
                this._sort.MyCollectionChanged += new NotifyCollectionChangedEventHandler(this.SortDescriptionsChanged);
            }
        }

        private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.NewStartingIndex == -1 && SortDescriptions.Count > 0)
            {
                return;
            }
            if (((e.Action != NotifyCollectionChangedAction.Reset) || (e.NewItems != null))
                || (((e.NewStartingIndex != -1) || (e.OldItems != null)) || (e.OldStartingIndex != -1)))
            {
                this.Refresh();
            }
        }

        public System.Collections.IEnumerable SourceCollection
        {
            get
            {
                return this;
            }
        }

        #endregion

    }

    public class SortableCollectionDeferRefresh : IDisposable
    {
        private readonly SortableCollectionView _collectionView;

        internal SortableCollectionDeferRefresh(SortableCollectionView collectionView)
        {
            _collectionView = collectionView;
        }

        public void Dispose()
        {
            // refresh the collection when disposed.
            _collectionView.Refresh();
        }
    }


    public class CustomSortDescriptionCollection : SortDescriptionCollection
    {

        public event NotifyCollectionChangedEventHandler MyCollectionChanged
        {
            add
            {
                this.CollectionChanged += value;
            }
            remove
            {
                this.CollectionChanged -= value;
            }
        }
    }
}
