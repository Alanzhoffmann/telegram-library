using System;
using System.Threading.Tasks;
using TelegramLibrary.App;

namespace ConsoleClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Write("Enter phone number: ");
            var phoneNumber = Console.ReadLine().Trim();
            var client = new Client(phoneNumber);

            client.OnAskUserCode += Client_OnAskUserCode;
            client.OnAskUserPassword += Client_OnAskUserPassword;

            await client.ConnectAsync();
            await client.AuthenticateAsync();

            var contacts = await client.GetUsersAsync();
            foreach (var i in contacts)
            {
                Console.WriteLine(i.ToString());
            }
            Console.ReadKey();
        }

        private static string Client_OnAskUserCode(object sender, Client.PhoneNumberArgs args)
        {
            Console.Write($"Enter code sent to {args.PhoneNumber}: ");
            return Console.ReadLine().Trim();
        }

        private static string Client_OnAskUserPassword(object sender, Client.PhoneNumberArgs args)
        {
            Console.Write($"Enter password for {args.PhoneNumber}: ");
            return Console.ReadLine().Trim();
        }
    }
}
