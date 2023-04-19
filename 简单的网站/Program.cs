using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SimpleWebServer
{
class Program
{
static string rootDirectory = "c:/web"; // 默认根目录
static int port = 8111; // 默认端口

    static void Main(string[] args)
    {
        LoadConfigFile(); // 读取配置文件

        // 如果根目录不存在，发出提问是否创建一个
        if (!Directory.Exists(rootDirectory))
        {
            Console.WriteLine("Web root directory does not exist. Do you want to create it? (Y/N)");
            string answer = Console.ReadLine().ToUpper();

            if (answer == "Y")
            {
                Directory.CreateDirectory(rootDirectory);
            }
            else
            {
                Console.WriteLine("Web server cannot start without a root directory.");
                return;
            }
        }

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{port}/"); // 监听所有IP的请求
        listener.Start();

        Console.WriteLine($"Web server is listening on port {port}. Press Ctrl+C to stop.");

        // 监听HTTP请求
        while (true)
        {
            HttpListenerContext context = listener.GetContext(); // 获取请求上下文
            string url = context.Request.Url.AbsolutePath; // 获取请求的URL路径
            string filePath = rootDirectory + url; // 请求的文件路径

            if (url == "/admin" && context.Request.HttpMethod == "POST") // 处理管理员页面的请求
            {
                HandleAdminRequest(context);
            }
            else if (Directory.Exists(filePath)) // 如果请求的是目录
            {
                HandleDirectoryRequest(context, filePath);
            }
            else if (File.Exists(filePath)) // 如果请求的是文件
            {
                HandleFileRequest(context, filePath);
            }
            else // 如果请求的内容不存在
            {
                SendNotFoundResponse(context);
            }
        }
    }

    // 读取配置文件
    static void LoadConfigFile()
    {
        string configFile = "config.ini";

        if (File.Exists(configFile))
        {
            string[] lines = File.ReadAllLines(configFile);

            foreach (string line in lines)
            {
                string[] parts = line.Split('=');

                if (parts.Length == 2)
                {
                    if (parts[0] == "root_directory")
                    {
                        rootDirectory = parts[1];
                    }
                    else if (parts[0] == "port")
                    {
                        int.TryParse(parts[1], out port);
                    }
                }
            }
        }
        else // 如果不存在配置文件，创建一个新的配置文件
        {
            string[] defaultConfig = { "root_directory=c:/web", "port=8111" };
            File.WriteAllLines(configFile, defaultConfig);
        }
    }

    // 处理管理员页面的请求
    static void HandleAdminRequest(HttpListenerContext context)
    {
        string newRootDirectory = context.Request.QueryString.Get("root_directory");
        string newPort = context.Request.QueryString.Get("port");
        string configFile = "config.ini";

        if (!string.IsNullOrEmpty(newRootDirectory)) // 如果请求修改根目录
        {
            rootDirectory = newRootDirectory;

            string[] lines = File.ReadAllLines(configFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("root_directory="))
                {
                    lines[i] = $"root_directory={rootDirectory}";
                    break;
                }
            }

            File.WriteAllLines(configFile, lines);
        }

        if (!string.IsNullOrEmpty(newPort)) // 如果请求修改端口号
        {
            int.TryParse(newPort, out port);

            string[] lines = File.ReadAllLines(configFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("port="))
                {
                    lines[i] = $"port={port}";
                    break;
                }
            }

            File.WriteAllLines(configFile, lines);
        }

        SendAdminResponse(context); // 发送管理员页面的响应
    }

    // 发送管理员页面的响应
    static void SendAdminResponse(HttpListenerContext context)
    {
        string html = @"
<html> <head> <title>Web Server Administration</title> </head> <body> <h1>Web Server Administration</h1> <form method='post' action='/admin'> <label>Root Directory:</label> <input type='text' name='root_directory' value='{{rootDirectory}}'><br><br> <label>Port:</label> <input type='text' name='port' value='{{port}}'><br><br> <button type='submit'>Save</button> </form> </body> </html> ".Replace("{{rootDirectory}}", rootDirectory).Replace("{{port}}", port.ToString());
        byte[] buffer = Encoding.UTF8.GetBytes(html);

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    // 处理目录请求
    static void HandleDirectoryRequest(HttpListenerContext context, string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath);
        string[] directories = Directory.GetDirectories(directoryPath);

        StringBuilder html = new StringBuilder();
        html.Append("<html><head><title>Directory Listing</title></head><body>");
        html.Append("<h1>Directory Listing</h1>");

        if (directoryPath != rootDirectory) // 如果不是根目录，添加返回上一级链接
        {
            html.Append("<a href='..'>..</a><br><br>");
        }

        // 列出所有子目录
        foreach (string directory in directories)
        {
            string directoryName = Path.GetFileName(directory);
            html.Append($"<a href='{directoryName}/'>{directoryName}/</a><br>");
        }

        // 列出所有文件
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            html.Append($"<a href='{fileName}'>{fileName}</a><br>");
        }

        html.Append("</body></html>");

        byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    // 处理文件请求
    static void HandleFileRequest(HttpListenerContext context, string filePath)
    {
        byte[] buffer = File.ReadAllBytes(filePath);

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = GetMimeType(filePath);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    // 发送404 Not Found响应
    static void SendNotFoundResponse(HttpListenerContext context)
    {
        string html = "<html><head><title>404 Not Found</title></head><body><h1>404 Not Found</h1></body></html>";

        byte[] buffer = Encoding.UTF8.GetBytes(html);

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    // 获取文件的MIME类型
    static string GetMimeType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();

        switch (extension)
        {
            case ".htm":
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "text/javascript";
            case ".gif":
                return "image/gif";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".ico":
                return "image/x-icon";
            default:
                return "application/octet-stream";
        }
    }
}
}