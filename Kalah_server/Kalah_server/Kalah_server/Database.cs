using System;
using System.IO;
using System.Linq;

public static class Database
{
    private static readonly string FilePath = "users.txt";  // Путь к файлу с данными пользователей

    // Метод для инициализации базы данных (создание файла, если его нет)
    static Database()
    {
        if (!File.Exists(FilePath))
        {
            // Если файл не существует, создаем новый
            CreateDatabase();
        }
    }

    // Метод для авторизации пользователя
    public static bool Authorize(string username, string password)
    {
        // Проверяем, существует ли файл с данными пользователей
        if (!File.Exists(FilePath))
        {
            Console.WriteLine("Файл с пользователями не существует.");
            return false;
        }

        // Читаем все строки из файла
        var users = File.ReadAllLines(FilePath);

        // Ищем пользователя с указанным именем и паролем
        foreach (var user in users)
        {
            var parts = user.Split(',');

            if (parts.Length >= 2 && parts[0] == username && parts[1] == password)
            {
                return true;  // Пользователь найден и пароль совпадает
            }
        }

        return false;  // Пользователь не найден или пароль неверный
    }

    // Метод для создания базы данных (пересоздает файл)
    private static void CreateDatabase()
    {
        Console.WriteLine("База данных не найдена. Создается новый файл users.txt.");

        // Создаем новый файл с дефолтными данными (например, администратор)
        using (var fileStream = File.Create(FilePath))
        {
            // Заполняем файл дефолтным пользователем
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("admin,1234");  // Добавляем пользователя по умолчанию
            }
        }
    }
    public static bool UserExists(string username)
    {
        return File.ReadLines(FilePath).Any(line => line.Split(',')[0] == username);
    }

    public static int GetUserId(string username)
    {
        var users = File.ReadLines(FilePath)
            .Select((line, index) => new { line, index })
            .FirstOrDefault(entry => entry.line.Split(',')[0] == username);

        return users != null ? users.index + 1 : -1;
    }

    public static void AddUser(string username, string password)
    {
        var id = File.ReadLines(FilePath).Count() + 1;
        File.AppendAllText(FilePath, $"{username},{password}\n");
    }

}
