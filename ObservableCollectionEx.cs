using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Brain2CPU.MvvmEssence
{
    public class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        private bool _notificationSuppressed = false;

        private bool _suppressNotification = false;
        public bool SuppressNotification
        {
            get => _suppressNotification;
            set
            {
                if(_suppressNotification == value)
                    return;

                _suppressNotification = value;

                if(!_suppressNotification && _notificationSuppressed)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    _notificationSuppressed = false;
                }
            }
        }

        public ObservableCollectionEx()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> list)
        {
            AddRange(list);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RegisterPropertyChanged(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnRegisterPropertyChanged(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UnRegisterPropertyChanged(e.OldItems);
                    RegisterPropertyChanged(e.NewItems);
                    break;
            }

            if(SuppressNotification)
            {
                _notificationSuppressed = true;
                return;
            }

            base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            if(list == null)
                return;

            bool enable;
            if(SuppressNotification)
            {
                enable = false;
            }
            else
            {
                enable = true;
                SuppressNotification = true;
            }

            foreach(T t in list)
            {
                Add(t);
            }

            if(enable)
                SuppressNotification = false;
        }

        public void RemoveRange(IEnumerable<T> list)
        {
            if(list == null)
                return;

            bool enable;
            if(SuppressNotification)
            {
                enable = false;
            }
            else
            {
                enable = true;
                SuppressNotification = true;
            }

            foreach(T t in list)
            {
                Remove(t);
            }

            if(enable)
                SuppressNotification = false;
        }

        protected override void ClearItems()
        {
            UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        private void RegisterPropertyChanged(IList items)
        {
            foreach(INotifyPropertyChanged item in items)
            {
                if(item != null)
                    item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void UnRegisterPropertyChanged(IList items)
        {
            foreach(INotifyPropertyChanged item in items)
            {
                if(item != null)
                    item.PropertyChanged -= Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(SuppressNotification)
            {
                _notificationSuppressed = true;
                return;
            }

            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender));
        }
    }
}
