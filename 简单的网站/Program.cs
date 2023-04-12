using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SimpleWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string configFile = "c:/config.ini";
            string rootDir = "c:/web"; // 默认根目录为c:/web

            // 从配置文件中读取根目录
            if (File.Exists(configFile))
            {
                string[] lines = File.ReadAllLines(configFile, Encoding.UTF8);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("#")) // 处理注释行
                    {
                        continue;
                    }

                    string[] parts = trimmedLine.Split('=');

                    if (parts.Length == 2 && parts[0].Trim().ToLower() == "rootdir")
                    {
                        rootDir = parts[1].Trim();
                        break;
                    }
                }
            }

            if (!Directory.Exists(rootDir))
            {
                Console.WriteLine($"Error: Root directory {rootDir} does not exist.");
                return;
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{GetFreeTcpPort()}/");
            listener.Start();

            Console.WriteLine($"Web server is running on port {((HttpListenerPrefixCollection)listener.Prefixes)[0]}...");

            while (true)
            {
                try // 添加错误处理，防止一个请求出错导致整个程序崩溃
                {
                    HttpListenerContext context = listener.GetContext();

                    string requestPath = context.Request.Url.AbsolutePath.Substring(1).Replace('/', Path.DirectorySeparatorChar); // 去掉路径前面的斜杠并将虚拟路径分隔符替换为实际路径分隔符

                    string fullPath = Path.Combine(rootDir, requestPath);

                    if (string.IsNullOrWhiteSpace(requestPath) || !fullPath.StartsWith(rootDir)) // 显示根目录列表
                    {
                        RespondWithDirectoryListing(context, new DirectoryInfo(rootDir));
                    }
                    else if (File.Exists(fullPath)) // 请求的是文件
                    {
                        RespondWithFile(context, new FileInfo(fullPath));
                    }
                    else if (Directory.Exists(fullPath)) // 请求的是目录
                    {
                        RespondWithDirectoryListing(context, new DirectoryInfo(fullPath));
                    }
                    else // 路径不存在
                    {
                        context.Response.StatusCode = 404; // 文件或目录不存在
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message); // 输出异常信息到控制台
                }
            }
        }

        static void RespondWithFile(HttpListenerContext context, FileInfo file)
        {
            context.Response.ContentType = GetContentType(file.Extension);
            context.Response.ContentLength64 = file.Length;
            context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{file.Name}\"");

            using (Stream fileStream = file.OpenRead())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, bytesRead);
                }
            }

            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
        }

        static void RespondWithDirectoryListing(HttpListenerContext context, DirectoryInfo directory)
        {
            string html = @"
                <html>
                <head>
                    <meta charset='utf-8' />
                    <title>{0}</title>
                </head>
                <body>
                    <h1>{0}</h1>
                    <ul>
                        {1}
                    </ul>
                </body>
                </html>
            ";

            string listItemHtml = "<li><a href=\"{0}\">{1}</a></li>";

            DirectoryInfo[] subDirectories = directory.GetDirectories();
            FileInfo[] files = directory.GetFiles();

            StringBuilder fileListHtmlBuilder = new StringBuilder();

            // 列出子目录
            foreach (DirectoryInfo subDir in subDirectories)
            {
                if ((subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                string subDirName = subDir.Name + "/";
                string subDirVirtualPath = subDir.FullName.Substring(directory.FullName.Length).Replace(Path.DirectorySeparatorChar, '/');

                fileListHtmlBuilder.Append(string.Format(listItemHtml, subDirVirtualPath, WebUtility.HtmlEncode(subDirName)));
            }

            // 列出文件
            foreach (FileInfo file in files)
            {
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                string fileName = file.Name;
                string fileVirtualPath = file.FullName.Substring(directory.FullName.Length).Replace(Path.DirectorySeparatorChar, '/');

                fileListHtmlBuilder.Append(string.Format(listItemHtml, fileVirtualPath, WebUtility.HtmlEncode(fileName)));
            }

            byte[] buffer = Encoding.UTF8.GetBytes(string.Format(html, directory.Name, fileListHtmlBuilder));
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
        }

        static string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLowerInvariant())
            {
                case ".css":
                    return "text/css";
                case ".gif":
                    return "image/gif";
                case ".html":
                case ".htm":
                    return "text/html";
                case ".jpeg":
                case ".jpg":
                    return "image/jpeg";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            }
        }

        static int GetFreeTcpPort()
        {
            using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}