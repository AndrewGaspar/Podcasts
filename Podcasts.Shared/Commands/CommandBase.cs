using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Podcasts.Commands
{
    public abstract class CommandBase : PropertyChangeBase, ICommand
    {
        public event EventHandler CanExecuteChanged;

        protected void InvokeCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs() { });
        }

        protected void ReevaluateCanExecute(object parameter, Action a)
        {
            var start = CanExecute(parameter);
            a();
            if (CanExecute(parameter) != start)
            {
                InvokeCanExecuteChanged();
            }
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);
    }

    public abstract class CommandBase<T> : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            Debug.Assert(parameter == null || parameter is T);

            return CanExecute((T)parameter);
        }

        public abstract bool CanExecute(T parameter);

        public override void Execute(object parameter)
        {
            Debug.Assert(parameter is T);

            var t = (T)parameter;

            Debug.Assert(CanExecute(t));

            Execute(t);
        }

        public abstract void Execute(T parameter);
    }
}
