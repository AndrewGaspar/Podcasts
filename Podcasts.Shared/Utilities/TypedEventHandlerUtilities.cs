using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Podcasts.Utilities
{
    public class TypedEventAsyncHandler<Source, Args>
    {
        private TaskCompletionSource<Args> _tcs = new TaskCompletionSource<Args>();

        public TypedEventHandler<Source, Args> Handler { get; }
        public Task<Args> Task => _tcs.Task;

        public TypedEventAsyncHandler()
        {
            Handler = (source, args) =>
            {
                _tcs.SetResult(args);
            };
        }
    }
}