using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Podcasts
{
    public class PropertyChangeBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public async Task AwaitsPropertyChangeAsync<T>(string propertyName, T desiredValue, Func<T> getValue)
        {
            var tcs = new TaskCompletionSource<bool>();

            Func<bool> isEqual = () => desiredValue.Equals(getValue());

            PropertyChangedEventHandler action = (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                {
                    if (isEqual())
                    {
                        tcs.TrySetResult(true);
                    }
                }
            };

            PropertyChanged += action;

            try
            {
                if (isEqual())
                {
                    return;
                }

                await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                PropertyChanged -= action;
            }
        }
    }
}