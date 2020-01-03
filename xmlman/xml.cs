using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TextLogs;
using System.Xml;
using System.Diagnostics;

namespace xmlman
{
    public class Xml
    {
        /// <summary>
        /// Sets a specific name for the .xml file. Default is 'Config.xml'.
        /// </summary>
        public string filename { get; set; } = "Config.xml";
        /// <summary>
        /// Sets a specific directory to store the .xml file.
        /// </summary>
        public string dir { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        public DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        private Log log;
        private bool writeLogs = false;
        /// <summary>
        /// Creates a new xml object along with the xml file.
        /// </summary>
        /// <param name="log">Optional: Sets a log object if any.</param>
        /// <param name="path">Optional: Sets a specific directory path to store the xml files. Default is base directory.</param>
        /// <param name="filename">Optional: Sets a specific name for the xml file. Default is 'Config.xml'</param>
        public Xml(Log log = null, string path = null, string filename = null)
        {
            if (log != null)
            {
                this.log = log;
                writeLogs = true;
            }
            CreateFile(path, filename);
        }
        /// <summary>
        /// Creates a new xml file.
        /// </summary>
        /// <param name="path">Optional: Sets a specific directory path to store the xml files. Default is base directory.</param>
        /// <param name="filename">Optional: Sets a specific name for the xml file. Default is 'Config.xml'.</param>
        public void CreateFile(string path = null, string filename = null)
        {
            try
            {
                if (path != null)
                    this.dir = path;

                if (filename != null)
                    this.filename = filename;

                if (!File.Exists($@"{this.dir}\{this.filename}"))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;

                    using (XmlWriter writer = XmlWriter.Create($@"{this.dir}\{this.filename}", settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement(this.filename);
                        writer.WriteElementString($"LastExecution", $"{DateTime.Today.Year}.{DateTime.Today.Month}.{DateTime.Today.Day} {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}");
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        writer.Dispose();
                    }
                    Print($"Xml file ({this.filename}) created at {this.dir}");
                }
                else
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load($@"{this.dir}\{this.filename}");
                    foreach (XmlNode e in xmlDoc.ChildNodes)
                    {
                        if ((e.Name ?? "") == this.filename)
                        {
                            foreach (XmlNode i in e.ChildNodes)
                            {
                                if ((i.Name ?? "") == "LastExecution")
                                {
                                    i.InnerText = $"{DateTime.Today.Year}.{DateTime.Today.Month}.{DateTime.Today.Day} {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
                                    xmlDoc.Save($@"{this.dir}\{this.filename}");
                                    Print($"Xml file ({this.filename}) last execution node updated");
                                    break;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                Print($"Error creating xml file: {ex.Message} | {ex.HResult}", true);
            }
        }si 

        /// <summary>
        /// Creates a new node on the xml file.
        /// </summary>
        /// <param name="tag">Sets the name of the new node.</param>
        /// <param name="innerText">Optional: Sets the inner text on the new node (if any). Default is NULL.</param>
        /// <param name="underTag1">Optional: Specifies if the new node goes under an already existing one.</param>
        public void CreateNode(string tag, string innerText = null, string underTag1 = null)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement element = default(XmlElement);
            bool exists = false;
            try
            {
                xmlDoc.Load($@"{this.dir}\{this.filename}");
                if (underTag1 != null)
                    element = xmlDoc.DocumentElement[underTag1];
                else
                    element = xmlDoc.DocumentElement;

                foreach (XmlElement e in element.GetElementsByTagName(tag))
                {
                    exists = true;
                    return;
                }

                if (!exists)
                {
                    XmlElement newElement = xmlDoc.CreateElement(tag);
                    newElement.InnerText = innerText;
                    element.AppendChild(newElement);
                    xmlDoc.Save($@"{this.dir}\{this.filename}");
                    xmlDoc = null;
                    Print($"Creating xml node {newElement.Name} on file {this.filename}");
                }
            } catch (Exception ex)
            {
                Print($"Error creating node in {this.filename}: {ex.Message} | {ex.HResult}", true);
            }
        }
        /// <summary>
        /// Reads a specific node on an xml file. Supports up to child elements (Parent > Child).
        /// </summary>
        /// <param name="tag">Specifies the tag name of the node to read.</param>
        /// <param name="underTag1">Optional: Specifies the parent node name of the tag to read. Default is none.</param>
        /// <returns></returns>
        public string ReadSingleNode(string tag, string underTag1 = null)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement element = default(XmlElement);
            bool exists = false;
            string innerText = null;
            try
            {
                xmlDoc.Load($@"{this.dir}\{this.filename}");
                if (underTag1 != null)
                    element = xmlDoc.DocumentElement[underTag1];
                else
                    element = xmlDoc.DocumentElement;

                foreach (XmlElement e in element.GetElementsByTagName(tag))
                {
                    if ((e.Name ?? "") == (tag ?? ""))
                    {
                        exists = true;
                        innerText = e.InnerText.Trim();
                    }
                }
            } catch (Exception ex)
            {
                Print($"Error reading xml single node {tag} on file {this.filename}: {ex.Message} | {ex.HResult}", true);
            }

            return innerText;
        }
        /// <summary>
        /// Writes log traces if log object specified on object declaration.
        /// Shows debug output traces if executed on Debug mode.
        /// </summary>
        /// <param name="message">Specifies the message to be written on the trace.</param>
        /// <param name="isError">Optional: Specifies if the event of the trace is an error or not. Default is false.</param>
        private void Print(string message, bool isError = false)
        {
            if (this.writeLogs)
                log.Write($"{message}", isError);

#if DEBUG
            Debug.Print($"{message}");
#endif
        }
    }
}
