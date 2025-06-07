using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSWithLibary;

public class UserValidation
{
    private readonly string _userFilePath;
    private readonly string _logPath;
    private List<Users> _users;

    public string? CurrentUsers {  get; set; }

    public UserValidation(string userFilePath, string logPath)
    {
        _userFilePath = userFilePath;
        _logPath = logPath;
        _users = LoadUsers();
    }

    private List<Users> LoadUsers()
    {
        var users = new List<Users>();

        try
        {
            if (!File.Exists(_userFilePath))
                return users;

            foreach (var line in File.ReadAllLines(_userFilePath))
            {
                var user = Users.FromCsv(line);
                if (user != null)
                    users.Add(user);
                else
                    Console.WriteLine($"Invalid line skipped in users file: {line}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading users file: {ex.Message}");
        }

        return users;
    }

    private void SaveUsers()
    {
        var lines = _users.Select(u => u.ToString());
        File.WriteAllLines(_userFilePath, lines);
    }

    public bool Login()
    {
        int attempts = 0;

        while (attempts < 3)
        {
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            var user = _users.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                Log("ERROR", $"Login failed: user '{username}' does not exist.");
                attempts++;
                continue;
            }

            if (!user.IsActive)
            {
                Console.WriteLine("User is blocked.");
                Log("ERROR", $"Login failed: user '{username}' is blocked.");
                return false;
            }

            if (user.Password == password)
            {
                CurrentUsers = user.UserName;
                Log("INFO", $"Login successful for user '{username}'.");
                return true;
            }
            else
            {
                Log("ERROR", $"Incorrect password for user '{username}'.");
                attempts++;
            }

            if (attempts >= 3)
            {
                user.IsActive = false;
                SaveUsers();
                Log("WARNING", $"User '{username}' has been blocked due to failed attempts.");
                Console.WriteLine("User has been blocked due to too many failed attempts.");
            }
        }

        return false;
    }

    private void Log(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("s");
        var userPart = string.IsNullOrEmpty(CurrentUsers) ? "" : $" User: {CurrentUsers}";
        var fullMessage = $"{timestamp} [{level}]{userPart} {message}";
        File.AppendAllText(_logPath, fullMessage + Environment.NewLine, Encoding.UTF8);
    }
}