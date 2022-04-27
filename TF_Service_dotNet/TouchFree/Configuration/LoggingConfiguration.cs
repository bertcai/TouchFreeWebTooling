﻿using System;
using System.IO;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public static class LoggingConfiguration
    {
        public static void SetUpLogging()
        {
            var loggingFileDirectory = Path.Combine(ConfigFileUtils.ConfigFileDirectory, "..\\Logs\\");

            if (loggingFileDirectory != "")
            {
                Directory.CreateDirectory(loggingFileDirectory);
            }

            var filename = loggingFileDirectory + "log.txt";

            if (File.Exists(filename))
            {
                var fileInfo = new FileInfo(filename);
                var fileSize = fileInfo.Length;
                if (fileSize > 100000)
                {
                    File.Move(filename, filename.Replace("log.txt", "log_old.txt"), true);
                }
            }

            FileStream filestream = new FileStream(filename, FileMode.Append);
            StreamWriter streamwriter = new StreamWriter(filestream)
            {
                AutoFlush = true
            };

            Console.SetOut(streamwriter);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now} - Starting Service");
            Console.WriteLine();
        }
    }
}
