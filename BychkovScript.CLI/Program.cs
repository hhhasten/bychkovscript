using BychkovScript.CLI;

if (args.Length == 0)
{
    Console.WriteLine("BychkovScript CLI");
    Console.WriteLine("Використання:");
    Console.WriteLine("  bs run <file.bs>   - Запустити скрипт");
    Console.WriteLine("  bs --version       - Показати версію");
    return;
}

string command = args[0];

switch (command)
{
    case "run":
        if (args.Length < 2)
        {
            Console.WriteLine("Помилка: Вкажіть файл для запуску. Наприклад: bs run main.bs");
            return;
        }
        
        string filePath = args[1];
        
        string fullPath = Path.GetFullPath(filePath);
        
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"RuntimeError: Файл '{filePath}' не знайдено. Ти точно там його зберіг?");
            return;
        }

        var runner = new ScriptRunner();
        ScriptRunner.ExecuteFile(fullPath);
        break;
        
    case "--version":
        Console.WriteLine("BychkovScript v1.0.0");
        break;
        
    default:
        Console.WriteLine($"Помилка: Невідома команда '{command}'.");
        break;
}