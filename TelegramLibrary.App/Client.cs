using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Exceptions;

namespace TelegramLibrary.App
{
    public class Client
    {
        private readonly string _phoneNumber;
        private readonly FileSessionStore _store;
        private readonly TelegramClient _client;

        public Client(string phoneNumber) : this(1294343, "e68d1f0e350e72734ed7c64eb769a420", phoneNumber)
        {
        }

        public Client(int apiId, string appHash, string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            _store = new FileSessionStore();
            _client = new TelegramClient(apiId, appHash, _store);
        }

        public delegate string AskUserCode(object sender, PhoneNumberArgs args);

        public delegate string AskUserPassword(object sender, PhoneNumberArgs args);

        public event AskUserCode OnAskUserCode;

        public event AskUserPassword OnAskUserPassword;

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();
        }

        public async Task<bool> AuthenticateAsync()
        {
            if (_client.IsUserAuthorized())
            {
                return true;
            }

            var hash = await _client.SendCodeRequestAsync(_phoneNumber);

            if (OnAskUserCode is null)
            {
                throw new NotImplementedException($"{nameof(OnAskUserCode)} not implemented");
            }

            var code = OnAskUserCode(this, new PhoneNumberArgs { PhoneNumber = _phoneNumber });

            TLUser user = null;
            try
            {
                user = await _client.MakeAuthAsync(_phoneNumber, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                var passwordSetting = await _client.GetPasswordSetting();
                if (OnAskUserPassword is null)
                {
                    throw new NotImplementedException($"{nameof(OnAskUserPassword)} not implemented");
                }

                var password = OnAskUserPassword(this, new PhoneNumberArgs { PhoneNumber = _phoneNumber });

                user = await _client.MakeAuthWithPasswordAsync(passwordSetting, password);
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception("CodeToAuthenticate is wrong in the app.config file, fill it with the code you just got now by SMS/Telegram", ex);
            }

            return _client.IsUserAuthorized();
        }

        public async Task<IList<object>> GetUsersAsync()
        {
            try
            {
                var result = await _client.GetContactsAsync();
                return result.Users.Select(u => u is TLUser user ? user.Username as object : null).Where(a => a != null).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public class PhoneNumberArgs : EventArgs
        {
            public string PhoneNumber { get; set; }
        }
    }
}
