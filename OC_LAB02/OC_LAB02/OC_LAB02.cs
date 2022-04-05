using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading.Channels;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        const string PATH = "passwordHashes.txt";
        static public bool foundFlag = false;

        static void printMenu()
        {
            bool flag = true;
            while (flag)
            {
                Console.WriteLine("1. Выполнить перебор");
                Console.WriteLine("2. Очистить консоль");
                Console.Write("Выбор-> ");
                int choice = int.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("\nВыберите по хэш для перебора: ");
                        Console.WriteLine("1. 1115dd800feaacefdf481f1f9070374a2a81e27880f187396db67958b207cbad");
                        Console.WriteLine("2. 3a7bd3e2360a3d29eea436fcfb7e44c735d117c42d1c1835420b6b9942dd4f1b");
                        Console.WriteLine("3. 74e1bb62f8dabb8125a58852b63bdf6eaef667cb56ac7f7cdba6d7305c50a22f");
                        Console.WriteLine("4. 502f4a25e986c7f7cd7abd7ed9b1f698e6774e3765e1eb5854a9dc0aaa751cda");
                        Console.WriteLine("5. bc4422f3121942ef9f6c728ff386342f83405bc2eeca80a4fcf62c7087741c27\n");
                        Console.Write("Выбор-> ");
                        int sign = int.Parse(Console.ReadLine());
                        string[] readText = File.ReadAllLines(PATH);
                        string passwordHash = readText[sign - 1].ToUpper();
                        Console.Write("Введите количество потоков: ");
                        int countStream = int.Parse(Console.ReadLine());
                        Console.WriteLine("Ожидайте окончание подбора пароля...");

                        Channel<string> channel = Channel.CreateBounded<string>(countStream);
                        Stopwatch time = new();
                        time.Reset();
                        time.Start();
                        var prod = Task.Run(() => { new Producer(channel.Writer); });
                        Task[] streams = new Task[countStream + 1];
                        streams[0] = prod;
                        for (int i = 1; i < countStream + 1; i++)
                        {
                            streams[i] = Task.Run(() => { new Consumer(channel.Reader, passwordHash); });
                        }
                        Task.WaitAny(streams);
                        time.Stop();
                        Console.WriteLine($"Затраченное время на подбор пароля: {time.Elapsed}");
                        Console.WriteLine("\nНажмите ENTER, чтобы вернуться в главное меню.");
                        Console.WriteLine();
                        Console.ReadKey();
                        foundFlag = false;
                        break;
                    case 2:
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine("Выбранного пункта нет в меню.");
                        break;
                }
            }
        }

        static public void Main()
        {
            printMenu();
        }
    }
    class Producer
    {
        private ChannelWriter<string> Writer;

        public Producer(ChannelWriter<string> _writer)
        {
            Writer = _writer;
            Task.WaitAll(Run());
        }

        private async Task Run()
        {
            while (await Writer.WaitToWriteAsync())
            {
                char[] word = new char[5];
                for (int i = 97; i < 123; i++)
                {
                    word[0] = (char)i;
                    for (int k = 97; k < 123; k++)
                    {
                        word[1] = (char)k;
                        for (int l = 97; l < 123; l++)
                        {
                            word[2] = (char)l;
                            for (int m = 97; m < 123; m++)
                            {
                                word[3] = (char)m;
                                for (int n = 97; n < 123; n++)
                                {
                                    word[4] = (char)n;
                                    if (!Program.foundFlag)
                                    {
                                        await Writer.WriteAsync(new string(word));
                                    }
                                    else
                                    {
                                        Writer.Complete();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    class Consumer
    {
        private ChannelReader<string> Reader;
        private string PasswordHash;

        public Consumer(ChannelReader<string> _reader, string _passwordHash)
        {
            Reader = _reader;
            PasswordHash = _passwordHash;
            Task.WaitAll(Run());
        }

        private async Task Run()
        {
            while (await Reader.WaitToReadAsync())
            {
                if (!Program.foundFlag)
                {
                    var item = await Reader.ReadAsync();
                    if (FoundHash(item.ToString()) == PasswordHash)
                    {
                        Console.WriteLine($"Пароль подобран - {item}");
                        Program.foundFlag = true;
                    }
                }
                else return;
            }
        }

        static public string FoundHash(string str)
        {
            SHA256 sha256Hash = SHA256.Create();
            byte[] sourceBytes = Encoding.ASCII.GetBytes(str);
            byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
            string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            return hash;
        }

    }
}