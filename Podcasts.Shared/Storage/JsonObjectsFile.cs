using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Podcasts.Utilities;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Podcasts.Storage
{
    public abstract class JsonObjectsFile<T> where T : IComparable<T>
    {
        private static JsonSerializer<SortedSet<T>> Serializer = new JsonSerializer<SortedSet<T>>();

        private SemaphoreSlim AsyncMutex = new SemaphoreSlim(1);

        public string DatabaseFileName { get; private set; }

        private StorageFile File = null;

        public JsonObjectsFile(string fileName)
        {
            DatabaseFileName = fileName;
        }

        public async Task<StorageFile> GetFileAsync()
        {
            if (File != null)
            {
                return File;
            }

            return File = await ApplicationData.Current
                .RoamingFolder.CreateFileAsync(DatabaseFileName, CreationCollisionOption.OpenIfExists);
        }

        private Task<U> UseFileAsync<U>(Func<StorageFile, Task<U>> func) =>
            AsyncMutex.ExclusionRegionAsync(async () =>
            {
                var file = await GetFileAsync().ConfigureAwait(false);
                return await func(file).ConfigureAwait(false);
            });

        private Task UseFileAsync(Func<StorageFile, Task> action) =>
            UseFileAsync<bool>(async file =>
            {
                await action(file).ConfigureAwait(false);
                return true;
            });

        public Task EraseFileAsync() =>
            UseFileAsync(async file =>
            {
                await file.DeleteAsync().AsTask().ConfigureAwait(false);
                File = null;
            });

        public async Task<IList<T>> ReadObjectsAsync() =>
            await UseFileAsync(async file =>
            {
                using (var readStream = await file.OpenReadAsync().AsTask().ConfigureAwait(false))
                {
                    return BlockingReadObjectsFromStream(readStream).ToList();
                }
            }).ConfigureAwait(false);

        private SortedSet<T> BlockingReadObjectsFromStream(IRandomAccessStream readStream)
        {
            if (readStream.Size == 0)
            {
                return new SortedSet<T>();
            }

            using (var inputStream = readStream.GetInputStreamAt(0))
            using (var classicStream = inputStream.AsStreamForRead())
                return Serializer.ReadJson(classicStream);
        }

        private void BlockingWriteObjectsToStream(IRandomAccessStream writeStream, SortedSet<T> objects)
        {
            // does not own the stream object
            var classicStream = writeStream.AsStreamForWrite();

            Serializer.WriteJson(classicStream, objects);

            // truncate any extra data
            writeStream.Size = writeStream.Position;
        }

        private static Task<IRandomAccessStream> OpenFileReadWriteAsync(StorageFile file) =>
            file.OpenAsync(FileAccessMode.ReadWrite).AsTask();

        private Task ModifyObjectsAsync(Predicate<SortedSet<T>> manipulation) => ModifyObjectsAsync(manipulation, () => { });

        private Task ModifyObjectsAsync(Predicate<SortedSet<T>> manipulation, Action onSuccess) =>
            UseFileAsync(async file =>
            {
                using (var writeStream = await OpenFileReadWriteAsync(file).ConfigureAwait(false))
                {
                    var objects = BlockingReadObjectsFromStream(writeStream);

                    var shouldWrite = manipulation(objects);

                    if (shouldWrite)
                    {
                        BlockingWriteObjectsToStream(writeStream, objects);

                        await writeStream.FlushAsync().AsTask().ConfigureAwait(false);

                        onSuccess();
                    }
                }
            });

        protected virtual T PrepareAddObject(T obj)
        {
            // do nothing in base case
            // example behavior: allocate ID
            return obj;
        }

        protected virtual void OnAddObjectSuccess(T original, T addedObject)
        {
            // for updating default initialized values on successful add
            // to database
        }

        public Task AddObjectAsync(T obj)
        {
            return AddObjectsAsync(obj);
        }

        public Task AddObjectsAsync(params T[] newObjects)
        {
            return AddObjectsAsync(newObjects as IEnumerable<T>);
        }

        public async Task AddObjectsAsync(IEnumerable<T> newObjects)
        {
            var pairs = newObjects.Select(newObject =>
                new
                {
                    Original = newObject,
                    Prepared = PrepareAddObject(newObject)
                }).ToList();

            await ModifyObjectsAsync(objects =>
            {
                var startingSize = objects.Count;

                foreach (var pair in pairs)
                {
                    objects.Add(pair.Prepared);
                }

                return objects.Count > startingSize;
            },
            // called on success
            () =>
            {
                foreach (var pair in pairs)
                {
                    OnAddObjectSuccess(pair.Original, pair.Prepared);
                }
            }).ConfigureAwait(false);
        }

        public Task RemoveObjectAsync(T obj) => ModifyObjectsAsync(objects => objects.Remove(obj));

        public Task RemoveMatchingObjectsAsync(IEnumerable<T> items) =>
            ModifyObjectsAsync(objects =>
                items.Aggregate(false, (change, item) => change || objects.Remove(item)));

        public Task RemoveObjectsWhereAsync(Predicate<T> test) =>
            ModifyObjectsAsync(objects => objects.RemoveWhere(test) != 0);

        public Task UpdateAnObjectAsync(Predicate<T> test, Action<T> modification) =>
            ModifyObjectsAsync(objects =>
            {
                foreach (var obj in objects)
                {
                    if(test(obj))
                    {
                        modification(obj);
                        return true;
                    }
                }
                return false;
            });
    }
}
