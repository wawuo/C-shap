using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // 指定服务器应用根目录
        string appRoot = @"F:\";

        // 获取服务器应用根目录下的 Music 文件夹路径
        string musicFolder = Path.Combine(appRoot, "Music");

        // 获取 Music 文件夹中的所有 MP3 和 APE 文件
        var musicFiles = Directory.EnumerateFiles(musicFolder)
            .Where(file => file.EndsWith(".mp3") || file.EndsWith(".ape"))
            .ToList();

        if (musicFiles.Count == 0)
        {
            Console.WriteLine("No music files found in the Music folder.");
            return;
        }

        // 显示所有可用的音乐文件
        Console.WriteLine("Available songs:");
        for (int i = 0; i < musicFiles.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(musicFiles[i])}");
        }

        // 提示用户选择音乐文件
        Console.Write("Enter the number of the song you want to play: ");
        int songIndex = int.Parse(Console.ReadLine()) - 1;
        string fileName = musicFiles[songIndex];

        Console.WriteLine($"Playing song: {Path.GetFileNameWithoutExtension(fileName)}");

        // 使用 VLC 命令行工具播放本地音乐文件
        Process.Start("vlc.exe", fileName);

        Console.WriteLine("Music playback started.");

        // 等待用户按下回车键并停止播放
        Console.ReadLine();

        Console.WriteLine("Music playback stopped.");
    }
}
