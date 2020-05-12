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
        private readonly LogWriter _logger;

        public Client(int apiId, string appHash, string phoneNumber, DirectoryInfo storeBasePath = null, LogWriter logger = null)
        {
            PhoneNumber = phoneNumber;
            _logger = logger;
            _store = new FileSessionStore(storeBasePath);
            _client = new TelegramClient(apiId, appHash, _store);
            ContactStore = new ContactStore(_client);
        }

        public event AskUserCode OnAskUserCode;

        public event AskUserPassword OnAskUserPassword;

        public string PhoneNumber { get; }
        public string Username => _client.Session?.TLUser.Username;

        internal ContactStore ContactStore { get; set; }

        public async Task ConnectAsync()
        {
            try
            {
                await _client.ConnectAsync();

                await AuthorizeAsync();
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao conectar.", ex);
                throw;
            }
        }

        public async Task SendMessageAsync(string message, string identification)
        {
            try
            {
                var contact = await ContactStore.GetContact(identification);

                await _client.SendMessageAsync(contact, message);
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao enviar mensagem.", ex);
                throw;
            }
        }

        public async Task SendMessageAsync(string message, IEnumerable<string> identifications)
        {
            try
            {
                var contacts = await ContactStore.GetContact(identifications);
                foreach (var contact in contacts)
                {
                    await _client.SendMessageAsync(contact, message);
                }
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao enviar mensagem.", ex);
                throw;
            }
        }

        public async Task SendImageAsync(string fileName, StreamReader fileReader, string identification, string message = null)
        {
            try
            {
                var contact = await ContactStore.GetContact(identification);

                var fileResult = await _client.UploadFile(fileName, fileReader);

                await _client.SendUploadedPhoto(contact, fileResult, message ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao enviar imagem.", ex);
                throw;
            }
        }

        public async Task SendImageAsync(string fileName, StreamReader fileReader, IEnumerable<string> identifications, string message = null)
        {
            try
            {
                var fileResult = await _client.UploadFile(fileName, fileReader);

                var contacts = await ContactStore.GetContact(identifications);
                foreach (var contact in contacts)
                {
                    await _client.SendUploadedPhoto(contact, fileResult, message ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao enviar imagem.", ex);
                throw;
            }
        }

        public async Task AddContacts(IEnumerable<Contact> contacts)
        {
            try
            {
                await _client.ImportContactsAsync(contacts.Select(c => c.ToTlContact()).ToList());
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao adicionar contato.", ex);
                throw;
            }
        }

        public async Task AddContact(Contact contact)
        {
            try
            {
                await AddContacts(new[] { contact });
            }
            catch (Exception ex)
            {
                _logger?.Write("Erro ao adicionar contato.", ex);
                throw;
            }
        }

        private async Task<bool> AuthorizeAsync()
        {
            if (_client.IsUserAuthorized())
            {
                _logger?.Write("Usuário já autorizado.");
                return true;
            }

            if (OnAskUserCode is null)
            {
                throw new NotImplementedException($"{nameof(OnAskUserCode)} não implementado");
            }

            _logger?.Write("Iniciando fluxo de login com código");
            var hash = await _client.SendCodeRequestAsync(PhoneNumber);

            var code = OnAskUserCode(this, new PhoneNumberArgs { PhoneNumber = PhoneNumber });

            TLUser user = null;
            try
            {
                user = await _client.MakeAuthAsync(PhoneNumber, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                _logger?.Write("Iniciando fluxo de login com senha", ex);
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

            _logger?.Write("Usuário autorizado");

            return _client.IsUserAuthorized();
        }
    }
}
