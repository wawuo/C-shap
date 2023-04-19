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
            Array.Sort<FileInfo>(files, (a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));

            if (!Directory.Exists(sampleDir))
                //如果sampleDir = @"F:\mp3样本文件夹"不存在，就创建他
            {
                Directory.CreateDirectory(sampleDir);
            }

            if (files.Length <= 2)
            {
                //如果文件数量小于等于2，则启动网易云音乐并设置退出时间为15分钟后
                Process.Start(neCloudMusicPath);
                Console.WriteLine("启动网易云音乐播放器...");
                System.Threading.Thread.Sleep(900000); //等待15分钟
            }
            else
            {
                //如果文件数量超过2，则将最新的三个 MP3 移至指定样本目录
                for (int i = 0; i < 3 && i < files.Length; i++)
                {
                    string newPath = Path.Combine(sampleDir, files[i].Name);

                    if (!File.Exists(newPath))
                        File.Copy(files[i].FullName, newPath);
                }

                //使用 VLC 播放样本目录下的三个 MP3
                string arg = @"--no-qt-privacy-ask --playlist-tree";
                foreach (FileInfo file in new DirectoryInfo(sampleDir).GetFiles("*.mp3"))
                {
                    arg += " \"" + file.FullName + "\"";
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = playerPath;
                psi.Arguments = arg;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                Process p = new Process();
                p.StartInfo = psi;
                p.Start();

                while (!p.StandardOutput.EndOfStream)
                {
                    string line = p.StandardOutput.ReadLine();
                    if (line.StartsWith("@mlist: Now playing "))
                    {
                        Console.WriteLine(line.Substring("@mlist: Now playing ".Length));
                    }
                }

                //等待播放完毕后退出程序
                System.Threading.Thread.Sleep((int)(new TimeSpan(0, 0, 0, 0, (int)files[2].Length * 8).TotalMilliseconds));
                p.Kill();
            }
        }
    }
}