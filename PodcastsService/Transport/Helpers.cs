using Windows.Foundation.Collections;

namespace Podcasts.Transport
{
    internal static class Helpers
    {
        private const string MessageType = "MessageType";
        private const string MessageBody = "MessageBody";

        public static ValueSet ToMessage<T>(T payload)
        {
            var message = new ValueSet();
            message.Add(MessageType, typeof(T).FullName);
            message.Add(MessageBody, JsonHelper.ToJson(payload));
            return message;
        }

        public static bool TryParseMessage<T>(ValueSet message, out T payload)
        {
            var type = (string)message[MessageType];
            if (type != typeof(T).FullName)
            {
                payload = default(T);
                return false;
            }

            payload = JsonHelper.FromJson<T>((string)message[MessageBody]);
            return true;
        }
    }
}
