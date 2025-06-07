using System.Text.RegularExpressions;
using CVSWithLibary;

string usersFile = "Users.txt";
string logFile = "log.txt";
string csvFile = "people.csv";

var validator = new UserValidation(usersFile, logFile);

if (!validator.Login())
{
    Console.WriteLine("No se pudo autenticar. Saliendo del programa...");
    return;
}

var helper = new CsvHelperExample();
var readPeople = helper.Read("people.csv").ToList();

string? currentUser = validator.CurrentUsers;

var opc = "0";
do
{
    opc = Menu();
    Console.WriteLine("====================================================");
    switch (opc)
    {
        case "1":
            foreach (var person in readPeople)
            {
                Console.WriteLine(person);
            }
            break;

        case "2":
            {
                Console.Write("Enter the ID: ");
                if (!int.TryParse(Console.ReadLine(), out var id))
                {
                    Console.WriteLine("Invalid ID. Must be a number.");
                    break;
                }
                if (readPeople.Any(p => p.Id == id))
                {
                    Console.WriteLine("ID already exists.");
                    break;
                }
                Console.Write("Enter the First name: ");
                var firstName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.WriteLine("First name is required.");
                    break;
                }
                Console.Write("Enter the Last name: ");
                var lastName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    Console.WriteLine("Last name is required.");
                    break;
                }
                Console.Write("Enter the phone: ");
                var phone = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
                {
                    Console.WriteLine("Invalid phone number.");
                    break;
                }
                Console.Write("Enter the city: ");
                var city = Console.ReadLine();
                Console.Write("Enter the balance: ");
                if (!decimal.TryParse(Console.ReadLine(), out var balance) || balance < 0)
                {
                    Console.WriteLine("Invalid balance. Must be a positive number.");
                    break;
                }

                var newPerson = new Person
                {
                    Id = id,
                    FirstName = firstName!,
                    LastName = lastName!,
                    Phone = phone!,
                    City = city!,
                    Balance = balance
                };

                readPeople.Add(newPerson);

                using var log = new LogWriter(logFile);
                log.WriteLog("INFO", currentUser!, $"Added person with ID {id}.");
                break;
            }
        case "3":
            SaveChanges();
            break;
        case "4":
            {
                Console.Write("Enter the ID of the person to edit: ");
                if (!int.TryParse(Console.ReadLine(), out var editId))
                {
                    Console.WriteLine("Invalid ID.");
                    break;
                }

                var personToEdit = readPeople.FirstOrDefault(p => p.Id == editId);
                if (personToEdit == null)
                {
                    Console.WriteLine("Person not found.");
                    break;
                }

                Console.WriteLine("Press ENTER to keep current value.");

                Console.Write($"First name ({personToEdit.FirstName}): ");
                var newFirst = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newFirst)) personToEdit.FirstName = newFirst;

                Console.Write($"Last name ({personToEdit.LastName}): ");
                var newLast = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newLast)) personToEdit.LastName = newLast;

                Console.Write($"Phone ({personToEdit.Phone}): ");
                var newPhone = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newPhone)) personToEdit.Phone = newPhone;

                Console.Write($"City ({personToEdit.City}): ");
                var newCity = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newCity)) personToEdit.City = newCity;

                Console.Write($"Balance ({personToEdit.Balance}): ");
                var newBalance = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newBalance) &&
                    decimal.TryParse(newBalance, out var updatedBalance) && updatedBalance >= 0)
                {
                    personToEdit.Balance = updatedBalance;
                }

                Console.WriteLine("Person updated.");
                using var logEdit = new LogWriter("log.txt");
                logEdit.WriteLog("INFO", currentUser!, $"Edited person with ID: {editId}");
                break;
            }
        case "5":
            Console.Write("Enter the ID of the person to delete: ");
            if (!int.TryParse(Console.ReadLine(), out var delId))
            {
                Console.WriteLine("Invalid ID.");
                break;
            }

            var personToDelete = readPeople.FirstOrDefault(p => p.Id == delId);
            if (personToDelete == null)
            {
                Console.WriteLine("Person not found.");
                break;
            }

            Console.WriteLine("Person data:");
            Console.WriteLine(personToDelete);

            Console.Write("Are you sure you want to delete this person? (Y/N): ");
            var confirm = Console.ReadLine();
            if (confirm?.ToUpper() == "Y")
            {
                readPeople.Remove(personToDelete);
                Console.WriteLine("Person deleted.");
                using var logDelete = new LogWriter("log.txt");
                logDelete.WriteLog("INFO", currentUser!, $"Deleted person with ID: {delId}");
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
            break;
        case "6":
            {
                var groups = readPeople.GroupBy(p => p.City);
                decimal totalGeneral = 0;

                foreach (var group in groups)
                {
                    Console.WriteLine($"Ciudad: {group.Key}\n");
                    Console.WriteLine($"{"ID",-4} {"Nombres",-15} {"Apellidos",-15} {"Saldo",15}");
                    Console.WriteLine($"{new string('—', 4)} {new string('—', 15)} {new string('—', 15)} {new string('—', 15)}");

                    decimal subtotal = 0;
                    foreach (var p in group)
                    {
                        Console.WriteLine($"{p.Id,-4} {p.FirstName,-15} {p.LastName,-15} {p.Balance,15:N2}");
                        subtotal += p.Balance;
                    }

                    Console.WriteLine($"{new string('=', 15),52}");
                    Console.WriteLine($"Total: {group.Key,-29} {subtotal,15:N2}\n");

                    totalGeneral += subtotal;
                }

                Console.WriteLine($"{new string('=', 15),52}");
                Console.WriteLine($"{"Total General:",-36} {totalGeneral,15:N2}");
                break;
            }
    }
} while (opc != "0");
SaveChanges();

void SaveChanges()
{
    helper.Write(csvFile, readPeople);
    using var log = new LogWriter(logFile);
    log.WriteLog("INFO", currentUser!, "Changes saved to people.csv");
}

string Menu()
{
    Console.WriteLine("====================================================");
    Console.WriteLine("1. Show content");
    Console.WriteLine("2. Add person");
    Console.WriteLine("3. Save changes");
    Console.WriteLine("4. Edit person");
    Console.WriteLine("5. Delete person");
    Console.WriteLine("6. Report by city");
    Console.WriteLine("0. Exit");
    Console.Write("Choose an option: ");
    return Console.ReadLine() ?? "0";
}