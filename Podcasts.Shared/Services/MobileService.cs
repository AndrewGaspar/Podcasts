using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Podcasts.Services
{
    using DataObjects;
    using Utilities;

    public class MobileService
    {
        private enum InitializationState
        {
            Uninitialized,
            Initializing,
            Initialized
        }

        private InitializationState initState = InitializationState.Uninitialized;

        private SemaphoreSlim mutex = new SemaphoreSlim(1);

        private MobileServiceClient client = null;
        
        public IMobileServiceSyncTable<Subscription> SubscriptionsTable { get; private set; }
        
        public static MobileService Current = new MobileService();

        private async Task AuthenticateAsync()
        {
            await AuthenticationHelpers.AuthenticateAsync(client);
        }

        public async Task InitializeAsync()
        {
            await mutex.ExclusionRegionAsync(async () =>
            {
                if(initState != InitializationState.Uninitialized)
                {
                    return;
                }

                initState = InitializationState.Initializing;

                try
                {
                    client = new MobileServiceClient(
                        "https://podcast-pelican.azure-mobile.net/",
                        "xDLEXTRjnYjTyxzzcutGHEBdhRnyTu12"
                    );

                    await AuthenticateAsync();

                    SubscriptionsTable = client.GetSyncTable<Subscription>();

                    if (!client.SyncContext.IsInitialized)
                    {
                        var store = new MobileServiceSQLiteStore("localstore.db");
                        store.DefineTable<Subscription>();
                        await client.SyncContext.InitializeAsync(store);
                    }
                }
                catch
                {
                    initState = InitializationState.Uninitialized;
                    client = null;
                    SubscriptionsTable = null;

                    throw;
                }

                initState = InitializationState.Initialized;
            });

            await SyncAsync();
        }

        public Task PushAsync() => client.SyncContext.PushAsync();
        public Task PullAsync() => SubscriptionsTable.PullAsync("subscriptionsSync", SubscriptionsTable.CreateQuery());

        public async Task SyncAsync()
        {
            await PushAsync();
            await PullAsync();
        }
    }
}
