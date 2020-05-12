using System.Collections.Generic;
using System.IO;

namespace TelegramLibrary.App
{
    public class ClientFactory
    {
        private static Dictionary<string, Client> _instances = new Dictionary<string, Client>();

        public static Client Build(int apiId, string appHash, string phoneNumber, DirectoryInfo storeBasePath = null)
        {
            if (_instances.TryGetValue(phoneNumber, out var client))
            {
                return client;
            }

            client = new Client(apiId, appHash, phoneNumber, storeBasePath, new Internal.LogWriter("."));
            _instances.Add(phoneNumber, client);

            return client;
        }

        public static void Clear()
        {
            _instances.Clear();
        }
    }
}
