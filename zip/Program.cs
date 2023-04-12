using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        var exeFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe");
        Array.Sort(exeFiles);

        RunExeFiles(exeFiles, 0, null);
        
        Console.WriteLine("已经执行完所有 EXE 文件。");
    }

    static void RunExeFiles(string[] exeFiles, int index, string lastExeFile)
    {
        if (index >= exeFiles.Length)
        {
            return;
        }

        var exeFile = exeFiles[index];

        if (exeFile.EndsWith("\\zip.exe", StringComparison.OrdinalIgnoreCase))
        {
            // 跳过 zip.exe 文件。
            RunExeFiles(exeFiles, index + 1, null);
        }
        else if (string.Equals(exeFile, lastExeFile, StringComparison.OrdinalIgnoreCase))
        {
            // 跳过重复运行的 exe 文件。
            RunExeFiles(exeFiles, index + 1, lastExeFile);
        }
        else
        {
            Console.WriteLine($"正在执行 {exeFile}...");

            // 启动进程并等待其退出。
            using (var process = new Process())
            {
                process.StartInfo.FileName = exeFile;
                process.Start();
                process.WaitForExit();
            }

            Console.WriteLine($"{exeFile} 完成执行。");

            // 递归调用 RunExeFiles() 方法，以便继续执行下一个文件。
            RunExeFiles(exeFiles, index + 1, exeFile);
        }
    }
}