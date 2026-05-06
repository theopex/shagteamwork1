using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client1
{
    class Program
    {
        static string number;
        static bool turn = true; 
        static UdpClient udpClient;
        static IPEndPoint remoteEndPoint;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Гравець 1 Порт 6000");

            while (true)
            {
                Console.Write("4-значне число: ");
                number = Console.ReadLine();
                if (number.Length == 4 && number.All(char.IsDigit)) break;
                Console.WriteLine("Помилка введiть число з 4 цифр");
            }

            udpClient = new UdpClient(6000); 
            remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6001);

            Thread listenerThread = new Thread(ReceiveMessages);
            listenerThread.IsBackground = true;
            listenerThread.Start();

            while (true)
            {
                if (turn)
                {
                    Console.Write("\nВаш хiд, введiть число з 4 цифр: ");
                    string guess = Console.ReadLine();

                    if (guess.Length != 4 || !guess.All(char.IsDigit))
                    {
                        Console.WriteLine("Помилка введiть число з 4 цифр");
                        continue;
                    }

                    SendMessage($"GUESS:{guess}");
                    turn = false;
                    Console.WriteLine("Ходить гравець 2");
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        static void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, remoteEndPoint);
        }

        static void ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    byte[] response = udpClient.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(response);

                    if (message.StartsWith("GUESS:"))
                    {
                        string oppGuess = message.Substring(6);

                        if (oppGuess == number)
                        {
                            Console.WriteLine($"\nГравець 2 відгадав ваше число ({number})!");
                            Console.WriteLine("Ви програли");
                            SendMessage("WIN:");
                        }
                        else
                        {
                            char[] hintMask = new char[4];
                            for (int i = 0; i < 4; i++)
                            {
                                if (oppGuess[i] == number[i])
                                {
                                    hintMask[i] = number[i]; 
                                }
                                else
                                {
                                    hintMask[i] = '-';
                                }
                            }

                            string hintResult = new string(hintMask);

                            SendMessage($"HINT:{hintResult}");
                            Console.WriteLine($"\nВарiант гравця 2: {oppGuess}.");

                            turn = true; 
                        }
                    }
                    else if (message.Contains("HINT:"))
                    {
                        string hint = message.Substring(5);
                        Console.WriteLine($"Ваш хiд: {hint}");
                    }
                    else if (message.Contains("WIN:"))
                    {
                        Console.WriteLine("\nВи вiдгадали число гравця 2");
                        Console.WriteLine("Ви виграли!");
                    }
                }
                catch (Exception ex) { }
            }
        }
    }
}