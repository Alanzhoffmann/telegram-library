using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelegramLibrary.App.Internal;
using TelegramLibrary.App.Models;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Exceptions;
using TLSharp.Core.Utils;
using static TelegramLibrary.App.Events;

namespace TelegramLibrary.App
{
    public class Client
    {
        private readonly FileSessionStore _store;
        private readonly TelegramClient _client;
        private readonly ContactStore _contactStore;

        public Client(int apiId, string appHash, string phoneNumber, DirectoryInfo storeBasePath = null)
        {
            PhoneNumber = phoneNumber;

            _store = new FileSessionStore(storeBasePath);
            _client = new TelegramClient(apiId, appHash, _store);
            _contactStore = new ContactStore(_client);
        }

        public event AskUserCode OnAskUserCode;

        public event AskUserPassword OnAskUserPassword;

        public string PhoneNumber { get; }
        public string Username => _client.Session?.TLUser.Username;

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();

            if (!_client.IsUserAuthorized())
            {
                await AuthorizeAsync();
            }
        }

        public async Task SendMessageAsync(string message, string identification)
        {
            var contact = await _contactStore.GetContact(identification);

            await _client.SendMessageAsync(contact, message);
        }

        public async Task SendMessageAsync(string message, IEnumerable<string> identifications)
        {
            var contacts = await _contactStore.GetContact(identifications);
            foreach (var contact in contacts)
            {
                await _client.SendMessageAsync(contact, message);
            }
        }

        public async Task SendImageAsync(string fileName, StreamReader fileReader, string identification, string message = null)
        {
            var contact = await _contactStore.GetContact(identification);

            var fileResult = await _client.UploadFile(fileName, fileReader);

            await _client.SendUploadedPhoto(contact, fileResult, message ?? string.Empty);
        }

        public async Task SendImageAsync(string fileName, StreamReader fileReader, IEnumerable<string> identifications, string message = null)
        {
            var fileResult = await _client.UploadFile(fileName, fileReader);

            var contacts = await _contactStore.GetContact(identifications);
            foreach (var contact in contacts)
            {
                await _client.SendUploadedPhoto(contact, fileResult, message ?? string.Empty);
            }
        }

        public async Task AddContact(IEnumerable<Contact> contacts)
        {
            await _client.ImportContactsAsync(contacts.Select(c => c.ToTlContact()).ToList());
        }

        public async Task AddContact(Contact contact)
        {
            await AddContact(new[] { contact });
        }

        private async Task<bool> AuthorizeAsync()
        {
            if (_client.IsUserAuthorized())
            {
                return true;
            }

            if (OnAskUserCode is null)
            {
                throw new NotImplementedException($"{nameof(OnAskUserCode)} não implementado");
            }

            var hash = await _client.SendCodeRequestAsync(PhoneNumber);

            var code = OnAskUserCode(this, new PhoneNumberArgs { PhoneNumber = PhoneNumber });

            TLUser user = null;
            try
            {
                user = await _client.MakeAuthAsync(PhoneNumber, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                if (OnAskUserPassword is null)
                {
                    throw new NotImplementedException($"{nameof(OnAskUserPassword)} não implementado");
                }

                var passwordSetting = await _client.GetPasswordSetting();

                var password = OnAskUserPassword(this, new PhoneNumberArgs { PhoneNumber = PhoneNumber });

                user = await _client.MakeAuthWithPasswordAsync(passwordSetting, password);
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception("Código de autenticação inválido.", ex);
            }

            return _client.IsUserAuthorized();
        }
    }
}
