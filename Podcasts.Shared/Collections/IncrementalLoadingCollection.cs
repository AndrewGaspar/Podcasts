using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Podcasts.Collections
{
    public abstract class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        protected CancellationToken CurrentCancellationToken { get; private set; }

        protected abstract uint MaxItems { get; }

        bool ISupportIncrementalLoading.HasMoreItems
        {
            get
            {
                if (CurrentCancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                return this.Count < MaxItems;
            }
        }

        protected abstract Task<T> LoadItemAsync(uint index);

        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async cts =>
            {
                CurrentCancellationToken = cts;

                var baseIndex = (uint)this.Count;
                var numberOfItemsToCreate = baseIndex + count < MaxItems ? count : MaxItems - (uint)baseIndex;

                var items = await Task.WhenAll(
                    Enumerable.Range((int)baseIndex, (int)numberOfItemsToCreate)
                        .Select(index => LoadItemAsync((uint)index)));

                foreach (var item in items)
                {
                    this.Add(item);
                }

                return new LoadMoreItemsResult() { Count = (uint)items.Length };
            });
        }
    }
}