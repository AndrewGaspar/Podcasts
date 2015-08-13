using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Linq;

namespace Podcasts
{
    public class JsonObjectsFile<T>
    {
        private static JsonSerializer<List<T>> Serializer = new JsonSerializer<List<T>>();

        private AsyncMutex AsyncMutex = new AsyncMutex();

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
                    return BlockingReadObjectsFromStream(readStream);
                }
            }).ConfigureAwait(false);

        private List<T> BlockingReadObjectsFromStream(IRandomAccessStream readStream)
        {
            if (readStream.Size == 0)
            {
                return new List<T>();
            }

            using (var inputStream = readStream.GetInputStreamAt(0))
            using (var classicStream = inputStream.AsStreamForRead())
                return Serializer.ReadJson(classicStream);
        }

        private void BlockingWriteObjectsToStream(IRandomAccessStream writeStream, List<T> objects)
        {
            // does not own the stream object
            var classicStream = writeStream.AsStreamForWrite();
            
            Serializer.WriteJson(classicStream, objects);

            // truncate any extra data
            writeStream.Size = writeStream.Position;
        }

        private static Task<IRandomAccessStream> OpenFileReadWriteAsync(StorageFile file) =>
            file.OpenAsync(FileAccessMode.ReadWrite).AsTask();

        private Task ModifyObjectsAsync(Predicate<List<T>> manipulation) => ModifyObjectsAsync(manipulation, () => { });

        private Task ModifyObjectsAsync(Predicate<List<T>> manipulation, Action onSuccess) =>
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

                objects.AddRange(pairs.Select(pair => pair.Prepared));

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

        public Task RemoveMatchingObjectsAsync(Predicate<T> test) =>
            ModifyObjectsAsync(objects => objects.RemoveAll(test) != 0);
    }
}
