// Серверная часть с поддержкой нескольких игроков
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class KalahServer
{
    private static List<Socket> clients = new List<Socket>();
    private static Dictionary<Socket, Socket> playerPairs = new Dictionary<Socket, Socket>();
    private static object lockObj = new object();

    static void Main()
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.101"), 4563));
        serverSocket.Listen(10);
        Console.WriteLine("Сервер запущен, ожидает подключения...");

        try
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("Новый клиент подключен.");
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            serverSocket.Close();
        }
    }

    static void HandleClient(Socket clientSocket)
    {
        lock (lockObj)
        {
            clients.Add(clientSocket);
        }

        try
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = clientSocket.Receive(buffer);
                if (bytesRead == 0) break;

                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Получено от клиента: {receivedData}");

                if (receivedData == "PLAY_WITH_PLAYER")
                {
                    lock (lockObj)
                    {
                        Socket opponent = FindOpponent(clientSocket);
                        if (opponent != null)
                        {
                            playerPairs[clientSocket] = opponent;
                            playerPairs[opponent] = clientSocket;

                            SendMessage(clientSocket, "START_GAME_WITH_PLAYER");
                            SendMessage(opponent, "START_GAME_WITH_PLAYER");
                        }
                        else
                        {
                            SendMessage(clientSocket, "WAITING_FOR_PLAYER");
                        }
                    }
                }
                else if (receivedData == "PLAY_WITH_COMPUTER")
                {
                    SendMessage(clientSocket, "START_GAME_WITH_COMPUTER");
                }
                else if (playerPairs.ContainsKey(clientSocket))
                {
                    // Пересылка хода другому игроку
                    Socket opponent = playerPairs[clientSocket];
                    SendMessage(opponent, receivedData);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(clientSocket);
                if (playerPairs.ContainsKey(clientSocket))
                {
                    Socket opponent = playerPairs[clientSocket];
                    playerPairs.Remove(clientSocket);
                    playerPairs.Remove(opponent);
                    SendMessage(opponent, "PLAYER_DISCONNECTED");
                }
            }
            clientSocket.Close();
        }
    }

    static Socket FindOpponent(Socket client)
    {
        foreach (Socket otherClient in clients)
        {
            if (otherClient != client && !playerPairs.ContainsKey(otherClient))
            {
                return otherClient;
            }
        }
        return null;
    }

    static void SendMessage(Socket client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        client.Send(data);
    }
}
