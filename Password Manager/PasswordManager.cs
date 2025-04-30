using Password_Manager.Models;
using Password_Manager.Password_Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Password_Manager
{
    public class PasswordManager
    {
        private List<Account> _accounts = new List<Account>();
        private readonly string _filePath = "Data/storage.json.enc";
        private readonly string _masterPassword;

        public PasswordManager(string masterPassword)
        {
            _masterPassword = masterPassword;
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            if (File.Exists(_filePath))
            {
                try
                {
                    Load();
                }
                catch
                {
                    Console.WriteLine("❌ Wrong password ❌");
                    Environment.Exit(1);
                }
            }
            else
            {
                Save();
            }
        }



        public void AddAccount()
        {
            Console.WriteLine("🌐* Website Name: ");
            string website = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(website))
            {
                Console.WriteLine("❌ Website is required.");
                return;
            }

            Console.WriteLine("👤* UserName: ");
            string userName = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("❌ userName is required.");
                return;
            }

            Console.WriteLine("🔑* Password: ");
            string password = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("❌ password is required.");
                return;
            }

            Console.WriteLine("📧- Used Email: ");
            string email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
            {
                email = null;
            }

            Account account = new Account()
            {
                Website = website,
                UserName = userName,
                Password = password,
                Email = email
            };
            _accounts.Add(account);
            Save();
            Console.WriteLine($"✅ Account for {website} added successfully!");
        }

        public void ListAccounts()
        {
            if(_accounts.Count == 0)
            {
                Console.WriteLine("❌📭❌ No accounts found.");
                return;
            }

            for (int i = 0; i < _accounts.Count; i++)
            {
                var acc = _accounts[i];
                Console.WriteLine($"""
                 ------------------------------
                 #{i + 1}
                 🌐 Website  : {acc.Website}
                 👤 Username : {acc.UserName}
                 🔑 Password : {acc.Password}
                 📧 Email    : {acc.Email ?? "(none)"}
                 """);
            }

        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
            var encrypted = EncryptionHelper.EncryptString(json, _masterPassword);
            File.WriteAllBytes(_filePath, encrypted);
        }

        private void Load()
        {
            var encrypted = File.ReadAllBytes(_filePath);
            var json = EncryptionHelper.DecryptBytes(encrypted, _masterPassword);
            _accounts = JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
        }

    }
}
