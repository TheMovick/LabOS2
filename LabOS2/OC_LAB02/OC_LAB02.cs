using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Linq;
public class OC_LAB02
{
    #region Постоянные

    static string password;
    private static string result1;
    private static string result2;
    private static readonly Encoding encoding = Encoding.UTF8;
    private static string path = @"D:\Documents";

    private static bool isMatched = false;


    private static int charactersToTestLength = 0;
    private static long computedKeys = 0;

    private static char[] charactersToTest =
    {
'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
'u', 'v', 'w', 'x', 'y', 'z'
};

    #endregion

    #region Главное меню
    public static void Main(string[] args)
    {
    mark:
        Console.WriteLine("1-Файловая система\n2-Брутфорс\n ");
        string choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                CrateFile();
                goto mark;
            case "2":
                Console.WriteLine("\n1-Хэш из файла\n2-Вводимый хэш:\n ");
                string hash = Console.ReadLine();
                switch (hash)
                {
                    case "1":
                        Console.WriteLine("Введите название:");
                        string name = Console.ReadLine();
                        using (FileStream fstream = File.OpenRead($"{path}\\{name}"))
                        {

                            byte[] array = new byte[fstream.Length];

                            fstream.Read(array, 0, array.Length);

                            password = System.Text.Encoding.Default.GetString(array);
                            Console.WriteLine($"Текст из файла:{password}");
                        }
                        break;
                    case "2":
                        Console.WriteLine("\nВведите хэш:");
                        password = Console.ReadLine();
                        break;
                    default:
                        break;
                }
                Theard();
                Thread.Sleep(100000);
                goto mark;
            default:
                break;

        }
    }
    #endregion

    #region Запись,чтение,удаление
    static void CrateFile()
    {
    mark:
        Console.WriteLine("\n1-Запись в файл \n2-Чтение файла\n3-Удаление файла\n0-Выход в главное меню\n");
        string choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                Write();
                goto mark;
            case "2":
                Read();
                goto mark;
            case "3":
                Del();
                goto mark;
            default:
                break;
        }




    }

    static void Write()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }
        Console.WriteLine("Название файла:");
        string name = Console.ReadLine();
        Console.WriteLine("Введите данные для записи в файл:");
        string text = Console.ReadLine();

        using (FileStream fstream = new FileStream($"{path}\\{name}", FileMode.OpenOrCreate))
        {
            byte[] array = System.Text.Encoding.Default.GetBytes(text);
            fstream.Write(array, 0, array.Length);
            Console.WriteLine("данные записаны в файл");
        }
    }
    static void Read()
    {
        Console.WriteLine("Название файла:");
        string name = Console.ReadLine();
        using (FileStream fstream = File.OpenRead($"{path}\\{name}"))
        {

            byte[] array = new byte[fstream.Length];
            fstream.Read(array, 0, array.Length);
            string textFromFile = System.Text.Encoding.Default.GetString(array);
            Console.WriteLine($"Текст из файла: {textFromFile}");

        }
    }
    static void Del()
    {
        Console.WriteLine("Название файла:");
        string name = Console.ReadLine();
        File.Delete($"{path}\\{name}");
        Console.WriteLine("Файл удалён");
        Console.ReadLine();
    }
    #endregion

    #region Блок брутфорса
    public static void Brutmain()
    {
        var timeStarted = DateTime.Now;
        Console.WriteLine("Начало брутфорса - {0}", timeStarted.ToString());

    charactersToTestLength = charactersToTest.Length;

        var estimatedPasswordLength = 5;

        while (!isMatched)
        {
            startBruteForce(estimatedPasswordLength);
        }

        Console.WriteLine("Пароль подобран - {0}", DateTime.Now.ToString());
        Console.WriteLine("Затраченное время: {0}s", DateTime.Now.Subtract(timeStarted).TotalSeconds);
        Console.WriteLine("Получившийся пароль: {0}", result1);
        Console.WriteLine("Использованный хэш: {0}", result2);
        Console.WriteLine("Полученный ключ: {0}", computedKeys);

        Thread.Sleep(1000);

    }

    private static void startBruteForce(int keyLength)
    {
        var keyChars = createCharArray(keyLength, charactersToTest[0]);
        var indexOfLastChar = keyLength - 1;
        createNewKey(0, keyChars, keyLength, indexOfLastChar);
    }

    private static char[] createCharArray(int length, char defaultChar)
    {
        return (from c in new char[length] select defaultChar).ToArray();
    }

    private static void createNewKey(int currentCharPosition, char[] keyChars, int keyLength, int indexOfLastChar)
    {
        var nextCharPosition = currentCharPosition + 1;
        for (int i = 0; i < charactersToTestLength; i++)
        {
            keyChars[currentCharPosition] = charactersToTest[i];

            if ((currentCharPosition < indexOfLastChar))
            {
                createNewKey(nextCharPosition, keyChars, keyLength, indexOfLastChar);
            }
            else
            {
                computedKeys++;
                using (SHA256 sha256Hash = SHA256.Create())
                    if ((GetHash(sha256Hash, new String(keyChars)) == password) || (isMatched == true))
                    {
                        if (!isMatched)
                        {

                            isMatched = true;
                            result1 = new String(keyChars);
                            result2 = GetHash(sha256Hash, new String(keyChars));


                        }
                        return;
                    }
            }

        }
    }
    private static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        var sBuilder = new StringBuilder();
    for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    #endregion

    #region Вывод результатов
    private static void Theard()
    {
        object locker = new();
        Console.WriteLine("Колл-во потоков:");
        int theards = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Дождитесь!\n");
        for (int i = 1; i <= theards; i++)
        {
            lock (locker)
            {
                Thread myThread = new(Brutmain);
                myThread.Name = $"Поток {i}"; 
                myThread.Start();

            }
            if (isMatched != true)
                break;
        }

    }

    #endregion
}