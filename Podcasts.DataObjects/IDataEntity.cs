namespace Podcasts.DataObjects
{
    public interface IEntityData
    {
        string Id { get; set; }
    }

    public interface IUserOwnedEntityData : IEntityData
    {
        string UserId { get; set; }
    }
}
