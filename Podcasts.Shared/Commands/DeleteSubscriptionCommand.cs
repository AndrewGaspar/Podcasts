namespace Podcasts.Commands
{
    using Models;
    using Utilities;

    public class DeleteSubscriptionCommand : CommandBase<SubscriptionModel>
    {
        public override bool CanExecute(SubscriptionModel parameter)
        {
            return true;
        }

        public override void Execute(SubscriptionModel parameter)
        {
            SubscriptionManager.Current.DeleteSubscriptionAsync(parameter).Ignore();
        }
    }
}
