using System;
using TeleSharp.TL;

namespace TelegramLibrary.App.Models
{
    public class Contact
    {
        public Contact(string phone, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException($"Telefone obrigatório no contato {this}", nameof(phone));
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException($"Primeiro nome obrigatório no contato {this}", nameof(firstName));
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException($"Sobrenome obrigatório no contato {this}", nameof(lastName));
            }

            Phone = phone;
            FirstName = firstName;
            LastName = lastName;
        }

        public string Phone { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public override string ToString()
        {
            return $"{Phone}: {FirstName} {LastName}";
        }

        internal TLInputPhoneContact ToTlContact()
        {
            return new TLInputPhoneContact
            {
                Phone = Phone,
                FirstName = FirstName,
                LastName = LastName
            };
        }
    }
}
