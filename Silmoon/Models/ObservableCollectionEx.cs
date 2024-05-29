﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Silmoon.Models
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        public ObservableCollectionEx()
        {

        }
        public ObservableCollectionEx(IEnumerable<T> collection)
        {
            _suppressNotification = true;
            foreach (var item in collection)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public ObservableCollectionEx(List<T> list)
        {
            _suppressNotification = true;
            list.ForEach(Add);
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification) base.OnCollectionChanged(e);
        }

        public void SuppressNotification(Func<bool> action)
        {
            _suppressNotification = true;
            var result = action.Invoke();
            _suppressNotification = false;
            if (result) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void Notify(NotifyCollectionChangedAction notifyCollectionChangedAction)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(notifyCollectionChangedAction));
        }
    }
}
