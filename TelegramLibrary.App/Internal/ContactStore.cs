using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TelegramLibrary.App.Internal
{
    internal class ContactStore
    {
        private readonly Dictionary<string, TLAbsInputPeer> _contactHistory;
        private readonly TelegramClient _client;
        private TLContacts _contacts;
        private TLDialogs _dialogs;

        public ContactStore(TelegramClient client)
        {
            _client = client;
            _contactHistory = new Dictionary<string, TLAbsInputPeer>();
        }

        public async Task<TLAbsInputPeer> GetContact(string identification, bool retry = false)
        {
            if (string.IsNullOrWhiteSpace(identification))
            {
                throw new ArgumentException("É obrigatório passar um identificador");
            }

            if (identification[0] == '+' && identification.Skip(1).All(c => char.IsDigit(c)))
            {
                identification = identification.Substring(1);
            }

            if (_contactHistory.TryGetValue(identification, out var contact))
            {
                return contact;
            }

            var user = _contacts?.Users
                .OfType<TLUser>()
                .FirstOrDefault(u => u.Phone == identification || u.Username == identification);
            if (user != null)
            {
                TLInputPeerUser tLInputPeerUser = new TLInputPeerUser { UserId = user.Id };
                _contactHistory.Add(identification, tLInputPeerUser);
                return tLInputPeerUser;
            }

            var chat = _dialogs?.Chats
                .OfType<TLChat>()
                .FirstOrDefault(c => c.Title == identification);
            if (chat != null)
            {
                TLInputPeerChat tLInputPeerChat = new TLInputPeerChat { ChatId = chat.Id };
                _contactHistory.Add(identification, tLInputPeerChat);
                return tLInputPeerChat;
            }

            var channel = _dialogs?.Chats
                .OfType<TLChannel>()
                .FirstOrDefault(c => c.Title == identification);
            if (channel != null)
            {
                TLInputPeerChannel tLInputPeerChannel = new TLInputPeerChannel { ChannelId = channel.Id, AccessHash = channel.AccessHash.Value };
                _contactHistory.Add(identification, tLInputPeerChannel);
                return tLInputPeerChannel;
            }

            if (retry)
            {
                throw new ArgumentException("Usuário/Telefone não encontrado");
            }

            _contacts = await _client.GetContactsAsync();
            _dialogs = (TLDialogs)await _client.GetUserDialogsAsync();

            return await GetContact(identification, true);
        }

        public async Task<IList<TLAbsInputPeer>> GetContact(IEnumerable<string> identifications)
        {
            var result = new List<TLAbsInputPeer>();
            foreach (var id in identifications.Distinct())
            {
                var contact = await GetContact(id);
                switch (contact)
                {
                    case TLInputPeerUser user when !result.Any(r => r is TLInputPeerUser u && u.UserId == user.UserId):
                    case TLInputPeerChat chat when !result.Any(r => r is TLInputPeerChat c && c.ChatId == chat.ChatId):
                    case TLInputPeerChannel channel when !result.Any(r => r is TLInputPeerChannel c && c.ChannelId == channel.ChannelId):
                        result.Add(contact);
                        break;
                }
            }
            return result;
        }
    }
}
