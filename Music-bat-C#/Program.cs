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
			int count = files.Length;

			// 输出 mp3 文件个数
			Console.WriteLine("该文件夹中的 mp3 文件数量为：" + count);	
		
			
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
							File.Move(files[i].FullName, newPath,true);
						
							// 更新要播放的文件参数字符串
							arg += " \"" + newPath + "\"";
							Console.WriteLine("要播放的文件" + newPath);
						}
					}

                //使用 VLC 播放样本目录下的三个 MP3
              
                Console.WriteLine("启动 VLC 播放器...");
                Process.Start(playerPath, arg);

                //等待播放完毕后退出程序
                System.Threading.Thread.Sleep((int)(new TimeSpan(0, 0, 0, 0, (int)files[2].Length * 8).TotalMilliseconds));
				
				
				
				// 获取正在运行的 VLC 进程
					Process[] processes = Process.GetProcessesByName("vlc");

					if (processes.Length > 0)
					{
						// 设置计时器并等待15分钟
						System.Timers.Timer timer = new System.Timers.Timer(900000);
						timer.AutoReset = false;
						timer.Elapsed += (sender, e) =>
						{
							// 循环关闭所有运行中的 VLC 进程
							foreach (Process process in processes)
							{
								process.CloseMainWindow();
							}

							// 关闭 C# 程序
							Environment.Exit(0);
						};
						timer.Start();
					}
					else
					{
						// 如果没有找到 VLC 进程，则直接关闭 C# 程序
						Environment.Exit(0);
					}
            }
        }
    }
}
/*-------------------------
这段代码的作用是检查是否存在足够的 MP3 文件进行播放，如果存在，则先将最旧的三个文件移动到一个指定样本目录下，并使用 VLC 播放器播放这些文件。
-------------------------------
在处理原始代码时，我注意到当将文件移动到指定目录时，代码未将新路径添加到变量 arg 中。因此，在修正后的代码中，我将该变量更新为包含新路径的完整字符串，并将其传递给 VLC 播放器。
此外，我们注释掉了之前循环处理所有样本目录下的 MP3 文件的代码段，这是因为我们只需要播放已移动到指定目录内的三个 MP3 文件。

最后，我们等待指定播放时间之后退出程序，以确保 VLC 播放器有足够的时间来完成播放任务。
请注意，以上更新部分仅限于 MP3 文件的情况，请根据需求和可支持的文件类型相应调整。
*/