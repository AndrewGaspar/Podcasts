using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Podcasts.Commands
{
    public abstract class CommandBase<T> : PropertyChangeBase, ICommand
        where T : class
    {
        public event EventHandler CanExecuteChanged;

        protected void InvokeCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs() { });
        }

        protected void ReevaluateCanExecute(T parameter, Action a)
        {
            var start = CanExecute(parameter);
            a();
            if(CanExecute(parameter) != start)
            {
                InvokeCanExecuteChanged();
            }
        }

        public bool CanExecute(object parameter)
        {
            Debug.Assert(parameter == null || parameter is T);

            return CanExecute(parameter as T);
        }

        public abstract bool CanExecute(T parameter);

        public void Execute(object parameter)
        {
            Debug.Assert(parameter is T);
            
            Debug.Assert(CanExecute(parameter as T));

            Execute(parameter as T);
        }

        public abstract void Execute(T parameter);
    }
}
