using Password_Manager.Models;
using Password_Manager.Password_Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
            string email = Console.ReadLine() ?? string.Empty;

            Account account = new Account()
            {
                Website = website,
                UserName = userName,
                Password = password,
                Email = string.IsNullOrWhiteSpace(email) ? null : email
            };
            _accounts.Add(account);
            Save();
            Console.WriteLine($"✅ Account for {website} added successfully!");
        }

        public void ListAccounts()
        {
            if (_accounts.Count == 0)
            {
                Console.WriteLine("❌📭❌ There is no accounts");
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

        public void SearchAccounts()
        {
            Console.Write("🔍 Enter keyword to search: ");
            string keyword = Console.ReadLine()?.ToLower() ?? "";

            var results = _accounts
                .Where(a => a.Website.ToLower().Contains(keyword) || a.UserName.ToLower().Contains(keyword))
                .ToList();

            if (results.Count == 0)
            {
                Console.WriteLine("❌ No matching accounts found.");
                return;
            }

            for (int i = 0; i < results.Count; i++)
            {
                var acc = results[i];
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

        public void EditAccount()
        {
            ListAccounts();
            Console.Write("✏️ Enter account number to edit: ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > _accounts.Count)
            {
                Console.WriteLine("❌ Invalid number.");
                return;
            }

            var account = _accounts[index - 1];

            Console.Write($"🌐 Website ({account.Website}): ");
            string website = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(website)) account.Website = website;

            Console.Write($"👤 Username ({account.UserName}): ");
            string username = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(username)) account.UserName = username;

            Console.Write($"🔑 Password ({account.Password}): ");
            string password = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(password)) account.Password = password;

            Console.Write($"📧 Email ({account.Email ?? "(none)"}): ");
            string email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(email)) account.Email = email;

            Save();
            Console.WriteLine("✅ Account updated successfully.");
        }

        public void DeleteAccount()
        {
            ListAccounts();
            Console.Write("🗑️ Enter account number to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > _accounts.Count)
            {
                Console.WriteLine("❌ Invalid number.");
                return;
            }

            var acc = _accounts[index - 1];
            Console.WriteLine($"⚠️ Are you sure you want to delete '{acc.Website}'? (y/n)");
            var confirm = Console.ReadLine();
            if (confirm?.ToLower() == "y")
            {
                _accounts.RemoveAt(index - 1);
                Save();
                Console.WriteLine("✅ Account deleted successfully.");
            }
            else
            {
                Console.WriteLine("❌ Delete cancelled.");
            }
        }

        public void ChangeMasterPassword()
        {
            Console.Write("🔐 Enter current master password: ");
            var current = Program.ReadHidden();
            if (current != _masterPassword)
            {
                Console.WriteLine("❌ Incorrect master password.");
                return;
            }

            Console.Write("🆕 Enter new master password: ");
            var newPass1 = Program.ReadHidden();

            Console.Write("🔁 Re-enter new master password: ");
            var newPass2 = Program.ReadHidden();

            if (string.IsNullOrWhiteSpace(newPass1) || newPass1 != newPass2)
            {
                Console.WriteLine("❌ Passwords do not match or are empty.");
                return;
            }

            // Re-encrypt the file with the new master password
            var json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
            var encrypted = EncryptionHelper.EncryptString(json, newPass1);
            File.WriteAllBytes(_filePath, encrypted);

            // Update the current password in memory
            typeof(PasswordManager)
                .GetField("_masterPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(this, newPass1);

            Console.WriteLine("✅ Master password changed successfully.");
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
