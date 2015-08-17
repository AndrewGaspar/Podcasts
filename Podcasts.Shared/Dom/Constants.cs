namespace Podcasts.Dom
{


    public static class Constants
    {
        public const string ITunes = "itunes";
        public const string ITunesDtd = "http://www.itunes.com/dtds/podcast-1.0.dtd";

        public static readonly XmlNamespace ITunesNamespace = new XmlNamespace(ITunes, ITunesDtd);
    }
}
