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

        public Log()
        {
            filename = $"[{DateTime.Now.Year}.{DateTime.Now.Month}.{DateTime.Now.Day} {DateTime.Now.Hour}.{DateTime.Now.Minute}] Log.txt";
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
                    File.Create($@"{this.dir}\{this.filename}");
                }

            } catch (Exception ex)
            {
                Debug.Print($"Create log error: {ex.Message} | Error code: {ex.HResult}");
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
                Debug.Print($"Log writing error: {ex.Message} | {ex.HResult}");
            }
        }

    }
}
