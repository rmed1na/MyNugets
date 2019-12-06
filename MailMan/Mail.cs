using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using TextLogs;
using System.Diagnostics;

namespace MailMan
{
    public class Mail
    {
        public string from { get; set; }
        public string password { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public List<string> recipients { get; set; } = new List<string>();
        public List<string> carbonCopyRecipients { get; set; } = new List<string>();
        public List<string> attachmentPaths { get; set; } = new List<string>();
        public SmtpClient smtp = new SmtpClient();
        private bool writeLogs = false;
        public bool isHtml { get; set; } = true;
        private Log log;
        
        public Mail(Log log = null)
        {
            if (log != null)
            {
                this.log = log;
                writeLogs = true;
            }
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
        }

        public bool Send()
        {
            bool success = false;
            MailMessage mailMessage = new MailMessage();
            Attachment attachment = null;
            try
            {
                if (!smtp.UseDefaultCredentials)
                    smtp.Credentials = new System.Net.NetworkCredential(from, password);

                mailMessage.From = new MailAddress(from);
                
                foreach (var i in recipients)
                    mailMessage.To.Add(i);

                foreach (var i in carbonCopyRecipients)
                    mailMessage.CC.Add(i);

                foreach (var i in attachmentPaths)
                    mailMessage.Attachments.Add(new Attachment(i));

                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = isHtml;
                mailMessage.Body = body;

                smtp.Send(mailMessage);
                mailMessage.Attachments.Dispose();
                success = true;
                Print($"Mail sent");
            } catch (Exception ex)
            {
                success = false;
                Print($"Error sending mail: {ex.Message} | {ex.HResult}", true);
            }
            return success;
        }

        private void Print(string message, bool isError = false)
        {
            if (writeLogs)
                log.Write(message: message, isError: isError);

#if DEBUG
            Debug.Print(message);
#endif
        }
    }
}
