using System;
using System.Runtime.Serialization;

namespace Podcasts.Models
{
    [DataContract]
    public class Podcast
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
        public Guid? Id { get; set; }

        [DataMember(IsRequired = true)]
        public string Title { get; set; }

        [DataMember(IsRequired = true)]
        public Uri Location { get; set; }

        [DataMember]
        public Uri Image { get; set; }

        public override bool Equals(object obj) => Equals(obj as Podcast);

        public bool Equals(Podcast obj)
        {
            if(obj == null)
            {
                return false;
            }

            return Id == obj.Id && Title == obj.Title && Location == obj.Location && Image == obj.Image;
        }
    }
}
