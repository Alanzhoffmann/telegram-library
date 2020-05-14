using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace TelegramLibrary.App.Extensions
{
    public static class TelegramClientExtensions
    {
        private static readonly char[] StyleChars = { '*', '_' };

        public static Task<TLAbsUpdates> SendMarkdownMessageAsync(this TelegramClient client, TLAbsInputPeer peer, string message, CancellationToken token = default(CancellationToken))
        {
            if (!client.IsUserAuthorized())
                throw new InvalidOperationException("Authorize user first!");

            Dictionary<char, List<int>> dictionary = GetIndexes(ref message);
            List<TLAbsMessageEntity> styles = new List<TLAbsMessageEntity>();

            var pair = new int[2];
            foreach (var item in dictionary)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        pair[0] = item.Value[i];
                    }
                    else
                    {
                        pair[1] = item.Value[i];
                        int length = pair[1] - pair[0];
                        if (length == 0)
                        {
                            continue;
                        }

                        switch (item.Key)
                        {
                            case '*':
                                styles.Add(new TLMessageEntityBold
                                {
                                    Offset = pair[0],
                                    Length = length
                                });
                                break;

                            case '_':
                                styles.Add(new TLMessageEntityItalic
                                {
                                    Offset = pair[0],
                                    Length = length
                                });
                                break;
                        }
                    }
                }
            }

            return client.SendRequestAsync<TLAbsUpdates>(new TLRequestSendMessage()
            {
                Peer = peer,
                Message = message,
                RandomId = Helpers.GenerateRandomLong(),
                Entities = new TLVector<TLAbsMessageEntity>(styles)
            }, token);
        }

        private static Dictionary<char, List<int>> GetIndexes(ref string message)
        {
            var dictionary = new Dictionary<char, List<int>>();
            var index = 0;
            while (index < message.Length)
            {
                var @char = message[index];
                if (StyleChars.Contains(@char))
                {
                    if (!dictionary.TryGetValue(@char, out var charList))
                    {
                        charList = new List<int>();
                        dictionary.Add(@char, charList);
                    }

                    if (message.Substring(index + 1).Contains(@char) || charList.Count % 2 == 1)
                    {
                        charList.Add(index);
                        message = message.Substring(0, index) + message.Substring(index + 1);
                        continue;
                    }
                }
                index++;
            }

            return dictionary;
        }
    }
}
