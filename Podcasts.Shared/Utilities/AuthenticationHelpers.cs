using Microsoft.WindowsAzure.MobileServices;
using Podcasts.DataObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.Popups;

namespace Podcasts.Utilities
{
    public static class AuthenticationHelpers
    {
        public static async Task<MobileServiceUser> AuthenticateAsync(this MobileServiceClient client)
        {
            // This sample uses the MicrosoftAccount provider.
            var provider = "MicrosoftAccount";
            MobileServiceUser user = null;

            // Use the PasswordVault to securely store and access credentials.
            var vault = new PasswordVault();
            PasswordCredential credential = null;

            var showErrorDialog = new Func<string, Task>(async message =>
            {
                var ui = new MessageDialog(message);

                var command = await ui.ShowAsync();
            });

            while (credential == null)
            {
                try
                {
                    // Try to get an existing credential from the vault.
                    credential = vault.FindAllByResource(provider).FirstOrDefault();
                }
                catch (Exception)
                {
                    // When there is no matching resource an error occurs, which we ignore.
                }

                if (credential != null)
                {
                    // Create a user from the stored credentials.
                    user = new MobileServiceUser(credential.UserName);
                    credential.RetrievePassword();
                    user.MobileServiceAuthenticationToken = credential.Password;

                    // Set the user from the stored credentials.
                    client.CurrentUser = user;

                    try
                    {
                        // Try to return an item now to determine if the cached credential has expired.
                        await client.GetTable<Subscription>().Take(1).ToListAsync();
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            // Remove the credential with the expired token.
                            vault.Remove(credential);
                            credential = null;
                            continue;
                        }
                    }
                }
                else
                {
                    try
                    {
                        // Login with the identity provider.
                        user = await client.LoginAsync(provider, useSingleSignOn: true);

                        // Create and store the user credentials.
                        credential = new PasswordCredential(provider,
                            user.UserId, user.MobileServiceAuthenticationToken);
                        vault.Add(credential);
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        await showErrorDialog("You must log in. Login Required");
                        continue;
                    }
                }
            }

            return user;
        }
    }
}
