
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SimpleWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string configFile = "c:/config.ini";
            int port = 8111; // 默认使用8111端口

            // 从配置文件中读取端口号
            if (File.Exists(configFile))
            {
                string configText = File.ReadAllText(configFile);

                string[] configLines = configText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in configLines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("#")) // 处理注释行
                    {
                        continue;
                    }

                    string[] parts = trimmedLine.Split('=');

                    if (parts.Length == 2 && parts[0].Trim().ToLower() == "port")
                    {
                        if (int.TryParse(parts[1].Trim(), out int readPort))
                        {
                            port = readPort;
                        }
                        break;
                    }
                }
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{port}/");
            listener.Start();

            Console.WriteLine($"Web server is running on port {port}...");

            while (true)
            {
                try // 添加错误处理，防止一个请求出错导致整个程序崩溃
                {
                    HttpListenerContext context = listener.GetContext();

                    string requestPath = context.Request.Url.AbsolutePath.Substring(1); // 去掉路径前面的斜杠

                    if (requestPath == "")
                    {
                        DirectoryInfo requestedDir = new DirectoryInfo(Directory.GetCurrentDirectory()); // 获取当前工作目录

                        // 检查当前工作目录是否包含config.ini文件，如果包含，则排除它
                        if (requestedDir.GetFiles().Any(f => f.Name.ToLower() == "config.ini"))
                        {
                            requestedDir = requestedDir.Parent; // 获取父级目录
                        }

                        RespondWithDirectoryListing(context, requestedDir);
                    }
                    else // 请求路径不为空
                    {
                        FileSystemInfo requestedItem;

                        try
                        {
                            requestedItem = new DirectoryInfo(requestPath);
                        }
                        catch (Exception)
                        {
                            requestedItem = new FileInfo(requestPath);
                        }

                        // 检查请求的是文件还是文件夹
                        if (requestedItem.Exists && (requestedItem.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            DirectoryInfo requestedDir = (DirectoryInfo)requestedItem;

                            // 如果请求的目录是当前工作目录下的config.ini文件所在目录，则返回403禁止访问错误
                            if (requestedDir.GetFiles().Any(f => f.Name.ToLower() == "config.ini"))
                            {
                                context.Response.StatusCode = 403; // 禁止访问
                                context.Response.Close();
                            }
                            else
                            {
                                RespondWithDirectoryListing(context, requestedDir);
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 404; // 文件或目录不存在
                            context.Response.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message); // 输出异常信息到控制台
                }
            }
        }

        static void RespondWithDirectoryListing(HttpListenerContext context, DirectoryInfo directory)
        {
            string html = @"
                <html>
                <head>
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

            string fileListHtml = "";

            // 列出子目录
            foreach (DirectoryInfo subDir in subDirectories)
            {
                if (subDir.Name.ToLowerInvariant() == "config.php" || subDir.Name.ToLowerInvariant() == "webserver.exe")
                {
                    continue;
                }
                fileListHtml += string.Format(listItemHtml, subDir.Name + "/", subDir.Name);
            }

            // 列出文件
            foreach (FileInfo file in files)
            {
                if (file.Name.ToLowerInvariant() == "config.ini" || file.Name.ToLowerInvariant() == "webserver.exe")
                {
                    continue;
                }
                fileListHtml += string.Format(listItemHtml, file.Name, file.Name);
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(string.Format(html, directory.Name, fileListHtml));
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
        }
    }
}