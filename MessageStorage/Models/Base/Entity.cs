namespace MessageStorage.Models.Base
{
    public abstract class Entity
    {
        public string Id { get; protected set; }

        protected Entity(string id)
        {
            Id = id;
        }
    }
}