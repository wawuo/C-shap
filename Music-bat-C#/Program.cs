using System;
using System.IO;
using System.Diagnostics;

namespace PlayMp3Files
{
    class Program
    {
        static void Main(string[] args)
        {
            string musicDir = @"Y:\Music";
            string sampleDir = @"F:\mp3样本";
            string playerPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            string neCloudMusicPath = @"E:\Program Files\Netease\CloudMusic\cloudmusic.exe";

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

            if (File.Exists(configPath))
            {
                // 如果 config.ini 文件存在，则从中读取配置信息
                string[] lines = File.ReadAllLines(configPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("musicDir="))
                        musicDir = line.Substring("musicDir=".Length);
                    else if (line.StartsWith("sampleDir="))
                        sampleDir = line.Substring("sampleDir=".Length);
                    else if (line.StartsWith("playerPath="))
                        playerPath = line.Substring("playerPath=".Length);
                    else if (line.StartsWith("neCloudMusicPath="))
                        neCloudMusicPath = line.Substring("neCloudMusicPath=".Length);
                }
            }
            else
            {
                // 如果 config.ini 文件不存在，则创建它并写入默认值
                string[] lines = { "musicDir=" + musicDir, "sampleDir=" + sampleDir, "playerPath=" + playerPath, "neCloudMusicPath=" + neCloudMusicPath };
                File.WriteAllLines(configPath, lines);
            }

            //获取目录下所有的 MP3 文件并按修改时间排序
            DirectoryInfo di = new DirectoryInfo(musicDir);
            FileInfo[] files = di.GetFiles("*.mp3");
            Array.Sort<FileInfo>(files, (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime)); // 按照最旧的文件优先排序

            if (!Directory.Exists(sampleDir))
            {
                //如果sampleDir不存在，就创建它
                Directory.CreateDirectory(sampleDir);
            }

            if (files.Length < 3)
            {
                //如果文件数量小于3，则启动网易云音乐并设置退出时间为15分钟后
                Process.Start(neCloudMusicPath);
                Console.WriteLine("启动网易云音乐播放器...");
                System.Threading.Thread.Sleep(900000); //等待15分钟
            }
            else
            {
                string arg = @"--no-qt-privacy-ask --playlist-tree";

                //如果文件数量大于等于3，则将最旧的三个 MP3 移至指定样本目录
                for (int i = 0; i < 3 && i < files.Length; i++)
                {
                    string newPath = Path.Combine(sampleDir, files[i].Name);

                    if (!File.Exists(newPath))
                    {
                        //剪切文件到指定目录
                        File.Move(files[i].FullName, newPath);
                    }
                }

                //使用 VLC 播放样本目录下的三个 MP3
                foreach (FileInfo file in new DirectoryInfo(sampleDir).GetFiles("*.mp3"))
                {
                    arg += " \"" + file.FullName + "\"";
                }
                Console.WriteLine("启动 VLC 播放器...");
                Process.Start(playerPath, arg);

                //等待播放完毕后退出程序
                System.Threading.Thread.Sleep((int)(new TimeSpan(0, 0, 0, 0, (int)files[2].Length * 8).TotalMilliseconds));
            }
        }
    }
}
/* ```


更改了几个地方：

1. 注释中提到的小错误已经修正。

2. 在复制文件操作的基础上，使用 `File.Move()` 方法将最新的 3 个 MP3 文件“剪切”到指定样本目录，而不是复制到该目录。

3. 最后，在播放完毕后等待最后一个文件播放结束，并关闭程序。文件在 `sampleDir` 目录中保持不变，不删除任何文件。

希望这个修改能够解决您的问题。如有任何问题，请随时告诉我。 
-------------------------
在此版本中，我修改了 `Array.Sort()` 的排序方式，将最旧的文件排在前面。同时，在复制文件操作的基础上，使用 `File.Move()` 方法将最旧的3个 MP3 文件“剪切”到指定样本目录，而不是复制到该目录。然后使用 VLC 播放器播放这三个文件，并等待最后一个文件播放结束后退出程序。
/* ```