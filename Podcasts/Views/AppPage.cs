using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Podcasts.Views
{
    public abstract class AppPage : Page
    {
        public abstract string Title { get; }
    }
}