using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // 获取当前程序所在目录。
        string currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        // 获取所有 EXE 文件（除了本程序）。
        var exeFiles = Directory.GetFiles(currentDirectory, "*.exe")
                               .Where(file => !file.EndsWith(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)));

        // 用于跟踪启动的进程。
        Process previousProcess = null;

        foreach (var exeFile in exeFiles.OrderBy(x => x))
        {
            try
            {
                // 检查是否已经启动该进程。
                bool alreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeFile)).Length > 0;
                if (alreadyRunning)
                {
                    Console.WriteLine($"进程 {exeFile} 已经在运行中，跳过启动。");
                    continue;
                }

                // 等待前一个进程退出。
                if (previousProcess != null)
                {
                    Console.WriteLine($"等待进程 {previousProcess.ProcessName} 退出...");
                    previousProcess.WaitForExit();
                }
                
                // 启动新的进程。
                Console.WriteLine($"启动进程 {exeFile}...");
                previousProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = exeFile,
                    UseShellExecute = false,
                    RedirectStandardInput = true // 打开标准输入流，以便我们可以向进程发送输入。
                });

                // 自动确认。
                previousProcess.StandardInput.WriteLine("yes"); // 输入您需要的确认内容。
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法启动进程 {exeFile}: {ex.Message}");
            }
        }

        Console.WriteLine("所有程序已经运行结束。");
    }
}