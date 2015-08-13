using System;
using System.Runtime.Serialization;

namespace Podcasts.Models
{
    [DataContract]
    public class Podcast : IComparable<Podcast>
    {
        public Podcast()
        {

        }

        public Podcast(Podcast other)
        {
            Id = other.Id;
            Title = other.Title;
            Location = other.Location;
            Image = other.Image;
        }

        [DataMember(IsRequired = true)]
        public Guid Id { get; set; }

        [DataMember(IsRequired = true)]
        public string Title { get; set; }

        [DataMember(IsRequired = true)]
        public Uri Location { get; set; }

        [DataMember]
        public Uri Image { get; set; }

        public override bool Equals(object obj) => Equals(obj as Podcast);

        public bool Equals(Podcast obj)
        {
            return Id.Equals(obj.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(Podcast other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
