using System;
using System.IO;
using System.Threading.Tasks;
using TelegramLibrary.App;

namespace ConsoleClient
{
    internal class Program
    {
        // Essas propriedades vem de https://my.telegram.org/
        private const int ApiId = 0;

        private const string AppHash = "";

        private static async Task Main(string[] args)
        {
            Console.Write("Digite o seu número de telefone (+5547999999999): ");
            var telefone = Console.ReadLine().Trim();

            var client = new Client(ApiId, AppHash, telefone);

            // Aqui são registrados os eventos para autorização
            client.OnAskUserCode += Client_OnAskUserCode;
            client.OnAskUserPassword += Client_OnAskUserPassword;

            await client.ConnectAsync();
            Console.WriteLine($"Bem vindo @{client.Username}");

            // identificador
            // Aqui pode ser o nome de usuário, nome de grupo ou número de telefone "+5547999999999"
            //var identificador = "";
            var identificador = new[] { "" };

            // Envio de mensagem
            await client.SendMessageAsync("testando", identificador);

            // Envio de imagem
            // Qualquer imagem pode ser enviada por aqui com um StreamReader
            var fileName = "image.png";
            using (var file = new FileStream(fileName, FileMode.Open))
            {
                using (var streamReader = new StreamReader(file))
                {
                    await client.SendImageAsync(fileName, streamReader, identificador, fileName);
                }
            }

            Console.ReadKey();
        }

        private static string Client_OnAskUserCode(object sender, PhoneNumberArgs args)
        {
            Console.Write($"Digite o código enviado para {args.PhoneNumber}: ");
            return Console.ReadLine().Trim();
        }

        private static string Client_OnAskUserPassword(object sender, PhoneNumberArgs args)
        {
            Console.Write($"Digite a senha para {args.PhoneNumber}: ");
            return Console.ReadLine().Trim();
        }
    }
}
