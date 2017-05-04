using System.Threading.Tasks;
using BlobBackuper.Helper;
using System.IO;
namespace BlobBackuper
{
    class Program
    {
        static void Main(string[] args)
        {
            //keys.config zawiera connection string który jest kluczem. Nie dałem go na githubie aby nikt mi nie korzystał na mój rachunek.
            string key = File.ReadAllText("keys.conf");
            var storage = new BlobManager(key);

            // Sztuczka aby mieć asynchroniczny kod wewnątrz zwykłego Main.
            // Tworzysz nowe zadanie/wątek Task i uruchamiasz w nim asynchroniczny kod async() => twój kod
            // Istotne że GetResultPoczeka aż się wykona i zablokuje dalszy kod. W konsoli fajnie, w gui źle.
            Task.Run(async () =>
            {
                await storage.UploadFile(@"C:\Private\Zdjecia\RogueOnek\Hovedoya.mp4", threads: 5);
            }).GetAwaiter().GetResult();
        }
    }
}
