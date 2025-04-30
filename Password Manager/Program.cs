using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Password_Manager // my master pass is hgsv123
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // to make the icon work
            Console.OutputEncoding = Encoding.UTF8;

            string msg;
            DateTime currentTime = DateTime.Now;
            if (currentTime.Hour < 12)
            {
                msg = "Good Morning!";
            }
            else if (currentTime.Hour < 18)
            {
                msg = "Good Afternoon!";
            }
            else
            {
                msg = "Good Evening!";
            }
            Console.WriteLine($"---------------------> {msg} Mr Ahmed <---------------------");

            string filePath = "Data/storage.json.enc";
            string? masterPassword;

            if (!File.Exists(filePath))
            {
                Console.WriteLine("🆕 First Time Setup");
                Console.Write("🔐 Enter master password: ");
                var first = ReadHidden();

                Console.Write("🔁 Re-enter master password: ");
                var second = ReadHidden();

                if (first != second || string.IsNullOrWhiteSpace(first))
                {
                    Console.WriteLine("❌ The password is not identical or is empty.");
                    return;
                }

                masterPassword = first;
                Console.WriteLine("✅ Done.");
            }
            else
            {
                Console.WriteLine("🔐 Enter the master password: ");
                masterPassword = ReadHidden();
            }

            PasswordManager manager = new PasswordManager(masterPassword);

            while (true)
            {
                Console.WriteLine("\n Choose an option:");
                Console.WriteLine("[1] Add Account");
                Console.WriteLine("[2] Show Accounts");
                Console.WriteLine("[0] Exit");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        manager.AddAccount();
                        break;
                    case "2":
                        manager.ListAccounts();
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        Thread.Sleep(500);
                        return;
                    default:
                        Console.WriteLine("❌ Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static string ReadHidden()
        {
            var pwd = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pwd.Length > 0)
                {
                    pwd = pwd[0..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    pwd += keyInfo.KeyChar;
                    Console.Write("*");
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return pwd;
        }
    }
}
