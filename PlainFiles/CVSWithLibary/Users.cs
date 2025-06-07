using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSWithLibary;

public class Users
{
    public string UserName {  get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool IsActive { get; set; }

    public static Users? FromCsv(string csvLine)
    {
        if (string.IsNullOrWhiteSpace(csvLine))
            return null;

        var parts = csvLine.Split(',');

        if (parts.Length != 3)
            return null;

        var user = new Users();

        user.UserName = parts[0].Trim();
        user.Password = parts[1].Trim();

        if (!bool.TryParse(parts[2].Trim(), out var isActive))
            return null; 

        user.IsActive = isActive;

        return user;
    }

    public override string ToString()
    {
        return $"{UserName},{Password},{IsActive}";
    }
}
 
