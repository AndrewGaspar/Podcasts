using Podcasts.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podcasts.ViewModels
{
    public class PodcastListViewModel : PropertyChangeBase
    {
        public ObservableCollection<SubscriptionModel> Subscriptions => SubscriptionManager.Current.Subscriptions;
    }
}
