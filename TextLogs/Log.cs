using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace TextLogs
{
    public class Log
    {
        public string filename { get; set; }
        public string foldername { get; set; } = "Logs";
        public string dir { get; set; } = $@"{AppDomain.CurrentDomain.BaseDirectory}\Logs";
        private List<string> errorList = new List<string>();
        private bool autoDeleteLogs { get; set; }
        private int autoDeleteMaxDays { get; set; }
        private int autoDeleteMaxSize { get; set; }
        public Log(bool autoDeleteLogs = false, int autoDeleteMaxDays = 0, int autoDeleteMaxSize = 0)
        {
            filename = $"[{DateTime.Now.Year}.{DateTime.Now.Month}.{DateTime.Now.Day} {DateTime.Now.Hour}.{DateTime.Now.Minute}] Log.txt";
            this.autoDeleteLogs = autoDeleteLogs;
            this.autoDeleteMaxDays = autoDeleteMaxDays;
            this.autoDeleteMaxSize = autoDeleteMaxSize;
            CreateLog();
        }
        private void CreateLog()
        {
            try
            {
                if (!Directory.Exists(this.dir))
                    Directory.CreateDirectory(this.dir);

                if (!File.Exists($@"{this.dir}\{this.filename}"))
                    File.Create($@"{this.dir}\{this.filename}").Dispose();
                else
                {
                    File.Delete($@"{this.dir}\{this.filename}");
                    File.Create($@"{this.dir}\{this.filename}").Dispose();
                }

                if (this.autoDeleteLogs)
                    AutoDeleteLogs();

            } catch (Exception ex)
            {
                Print($"Create log error: {ex.Message} | Error code: {ex.HResult}", true);
            }
        }
        public void Write(string message, bool isError = false, bool showErrorCount = false)
        {
            StreamWriter writer;
            try
            {
                string messageType;
                DateTime now = DateTime.Now;
                writer = File.AppendText($@"{this.dir}\{this.filename}");
                if (isError)
                {
                    messageType = "ERROR";
                    errorList.Add(message);
                }
                else
                    messageType = "OK";

                writer.WriteLine($"[{now.Year}.{now.Month}.{now.Day} {now.Hour}:{now.Minute}:{now.Second}.{now.Millisecond}] - {messageType} - {message}");
                if (showErrorCount)
                {
                    writer.WriteLine("");
                    writer.WriteLine($"{now.Year}.{now.Month}.{now.Day} {now.Hour}:{now.Minute}:{now.Second}.{now.Millisecond} - STATS - Number of errors: {errorList.Count}");
                }
                writer.Close();

            } catch (Exception ex)
            {
                Print($"Log writing error: {ex.Message} | {ex.HResult}", true);
            }
        }
        private void AutoDeleteLogs()
        {
            try
            {
                foreach (string dir in Directory.GetFiles(this.dir, "*.txt"))
                {
                    if (dir.Contains("Log") & (!dir.Contains(this.filename)) & (dir.Contains("[")) & (dir.Contains("]")))
                    {
                        DateTime creationDate = File.GetCreationTime(dir);
                        double daysDiff = ((TimeSpan)(DateTime.Now - creationDate)).Days;
                        if (this.autoDeleteMaxDays > 0 & daysDiff >= this.autoDeleteMaxDays)
                        {
                            File.Delete(dir);
                            Print($"File '{dir}' deleted due to maximum days tolerance rule");
                        }
                    }
                }

                foreach (string dir in Directory.GetFiles(this.dir, "*.txt"))
                {
                    if (dir.Contains("Log") & (!dir.Contains(this.filename)) & (dir.Contains("[")) & (dir.Contains("]")))
                    {
                        FileInfo file = new FileInfo(dir);
                        long fileSize = file.Length;
                        if (this.autoDeleteMaxSize > 0 & ((fileSize / 1024f) / 1024f) > this.autoDeleteMaxSize)
                        {
                            File.Delete(dir);
                            Print($"File '{dir}' deleted due to maximum size tolerance rule");
                        }
                    }
                }
            } catch (Exception ex)
            {
                Print($"Log debug error: {ex.Message} | {ex.HResult}", true);
            }
        }
        private void Print(string message, bool isError = false)
        {
            Write(message, isError);
#if DEBUG
            Debug.Print(message);
#endif
        }
    }
}
