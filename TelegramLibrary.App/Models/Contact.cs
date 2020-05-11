using TeleSharp.TL;

namespace TelegramLibrary.App.Models
{
    public class Contact
    {
        public Contact(string phone, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new System.ArgumentException("Telefone obrigatório", nameof(phone));
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new System.ArgumentException("Primeiro nome obrigatório", nameof(firstName));
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new System.ArgumentException("Sobrenome obrigatório", nameof(lastName));
            }

            Phone = phone;
            FirstName = firstName;
            LastName = lastName;
        }

        public string Phone { get; }
        public string FirstName { get; }
        public string LastName { get; }

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
