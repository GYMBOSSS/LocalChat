using LocalChatClient.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private const int PORT = 8080;
    private const string CONFIG_FILE = "client_config.txt";
    private static TcpClient _tcpClient = new TcpClient();
    private static User? _user;
    private static bool _isRunning = true;
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private static bool _inPrivateChat = false;

    static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            Console.WriteLine("\nDisconnecting...");
        };

        Console.WriteLine("=== Chat Client ===");
        await RunClientAsync();
    }

    private static async Task RunClientAsync()
    {
        try
        {
            var userName = await GetUserNameAsync();

            if (!await ConnectToServerAsync(userName))
            {
                Console.WriteLine("Failed to connect to server. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Connected successfully! Type '/help' for available commands.");
            Console.WriteLine("------------------------------------------------------------");

            // Start message receiving task
            var receiveTask = Task.Run(() => HandleServerMessagesAsync(_cancellationTokenSource.Token));

            // Handle user input
            await HandleUserInputAsync();

            // Wait for receive task to complete
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    private static async Task<string> GetUserNameAsync()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                var savedName = (await File.ReadAllTextAsync(CONFIG_FILE)).Trim();
                if (!string.IsNullOrEmpty(savedName))
                {
                    Console.WriteLine($"Welcome back, {savedName}!");
                    Console.Write("Do you want to use this name? (Y/n): ");
                    var response = Console.ReadLine()?.Trim().ToLower();
                    if (response != "n" && response != "no")
                    {
                        return savedName;
                    }
                }
            }
            return await RequestUserNameAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return await RequestUserNameAsync();
        }
    }

    private static async Task<string> RequestUserNameAsync()
    {
        string userName;
        do
        {
            Console.Write("Enter your name: ");
            userName = (await Console.In.ReadLineAsync())?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Name cannot be empty. Please try again.");
            }
            else if (userName.Length > 20)
            {
                Console.WriteLine("Name is too long (max 20 characters). Please try again.");
                userName = string.Empty;
            }
        } while (string.IsNullOrWhiteSpace(userName));

        try
        {
            await File.WriteAllTextAsync(CONFIG_FILE, userName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not save username: {ex.Message}");
        }

        return userName;
    }

    private static async Task<bool> ConnectToServerAsync(string userName)
    {
        try
        {
            Console.WriteLine($"Connecting to server {IPAddress.Loopback}:{PORT}...");

            await _tcpClient.ConnectAsync(IPAddress.Loopback, PORT);

            // Send username to server
            await SendMessageAsync(userName, _tcpClient.GetStream());

            _user = new User(_tcpClient, userName);
            Console.WriteLine("Connection established!");

            return true;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
    }

    private static async Task HandleServerMessagesAsync(CancellationToken cancellationToken)
    {
        var stream = _tcpClient.GetStream();
        var buffer = new byte[4096];

        while (!cancellationToken.IsCancellationRequested && _tcpClient.Connected)
        {
            try
            {
                if (!stream.DataAvailable)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Use thread-safe console output
                await DisplayServerMessageAsync(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                Console.WriteLine("\n[SYSTEM] Connection to server lost.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] Error receiving message: {ex.Message}");
                break;
            }
        }
    }

    private static async Task DisplayServerMessageAsync(string message)
    {
        // Use lock for thread-safe console output if needed, but Console class is thread-safe
        await Task.Run(() =>
        {
            if (_inPrivateChat && !message.StartsWith("Подключенные пользователи:"))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if (message.Contains("[SYSTEM]") || message.StartsWith("Подключенные пользователи:"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            Console.WriteLine(message);
            Console.ResetColor();

            // Show prompt again if we're waiting for input
            if (!_inPrivateChat)
            {
                Console.Write("> ");
            }
        });
    }

    private static async Task HandleUserInputAsync()
    {
        while (_isRunning && _tcpClient.Connected)
        {
            try
            {
                if (!_inPrivateChat)
                {
                    Console.Write("> ");
                }

                var input = await Console.In.ReadLineAsync(_cancellationTokenSource.Token);

                if (string.IsNullOrWhiteSpace(input)) continue;

                await ProcessUserInputAsync(input.Trim());
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static async Task ProcessUserInputAsync(string input)
    {
        try
        {
            var stream = _tcpClient.GetStream();

            switch (input.ToUpper())
            {
                case "/HELP":
                    ShowHelp();
                    break;

                case "/USERS":
                    await SendMessageAsync("GETUSERS", stream);
                    break;

                case "/CHAT":
                    _inPrivateChat = true;
                    await SendMessageAsync("WRITE", stream);
                    break;

                case "/BACK":
                    if (_inPrivateChat)
                    {
                        await SendMessageAsync("/back", stream);
                        _inPrivateChat = false;
                    }
                    else
                    {
                        Console.WriteLine("You are not in a private chat.");
                    }
                    break;

                case "/EXIT":
                case "/QUIT":
                    _isRunning = false;
                    _cancellationTokenSource.Cancel();
                    Console.WriteLine("Disconnecting...");
                    break;

                default:
                    await SendMessageAsync(input, stream);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    private static async Task SendMessageAsync(string message, NetworkStream stream)
    {
        try
        {
            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(byteMessage, 0, byteMessage.Length);
            await stream.FlushAsync();
        }
        catch (IOException ex)
        {
            Console.WriteLine("\n[ERROR] Connection to server lost.");
            _isRunning = false;
            _cancellationTokenSource.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to send message: {ex.Message}");
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("\n=== Available Commands ===");
        Console.WriteLine("/help     - Show this help message");
        Console.WriteLine("/users    - Show connected users");
        Console.WriteLine("/chat     - Start private chat with another user");
        Console.WriteLine("/back     - Exit private chat");
        Console.WriteLine("/exit     - Disconnect from server");
        Console.WriteLine("---------------------------");
        Console.WriteLine("Just type and press Enter to send a public message");
        Console.WriteLine("---------------------------\n");
    }

    private static void Disconnect()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            _tcpClient?.Close();
            _tcpClient?.Dispose();
            Console.WriteLine("Disconnected from server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during disconnect: {ex.Message}");
        }
    }
}