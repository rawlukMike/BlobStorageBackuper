using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlobBackuper.Helper;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.WindowsAzure.Storage.Auth;

namespace BlobBackuper
{
    class Program
    {
        static void Main(string[] args)
        {
            //keys.config zawiera connection string który jest kluczem. Nie dałem go na githubie aby nikt mi nie korzystał na mój rachunek.
            string key = System.IO.File.ReadAllText("keys.config");
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
