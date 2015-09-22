namespace Podcasts.Dom
{
    public class XmlNamespace
    {
        public string Namespace { get; private set; }

        public string Dtd { get; private set; }

        private string formatted = null;

        public XmlNamespace(string ns, string dtd)
        {
            Namespace = ns;
            Dtd = dtd;

            formatted = $"xmlns:{Namespace}='{Dtd}'";
        }

        public override string ToString()
        {
            return formatted;
        }
    }
}