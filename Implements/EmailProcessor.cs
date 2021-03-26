using Newtonsoft.Json;
using RabbitMQ.Client.Exceptions;
using RabbitMQExample.Interfaces;
using RabbitMQExample.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.Implements
{
    public class EmailProcessor : IEmailProcessor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailProcessor));
        private SmtpConfig _smtpConfig = null;
        public EmailProcessor(SmtpConfig smtpConfig)
        {
            _smtpConfig = smtpConfig;
        }
        public void ProcessEmail(string msg)
        {
            try
            {
                Email messageObj = JsonConvert.DeserializeObject<Email>(msg);
                messageObj.To = messageObj.To.Trim();
                SendMail(messageObj, _smtpConfig);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void SendMail(Email email, SmtpConfig smtpConfig)
        {
            try
            {
                //System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(RemoteServerCertificateValidationCallback);
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage(smtpConfig.UserName, email.To, email.Subject, email.Body);
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;
                NEVER_EAT_POISON_Disable_CertificateValidation();
                SmtpClient smtpClient = new SmtpClient();                                
                smtpClient.Host = smtpConfig.Host;
                smtpClient.Port = smtpConfig.Port;
                smtpClient.Credentials = new NetworkCredential(smtpConfig.UserName, smtpConfig.Password);
                smtpClient.EnableSsl = smtpConfig.EnableSSL;
                X509Certificate x509Certificate = GetCertificate();
                smtpClient.ClientCertificates.Add(x509Certificate);
                smtpClient.DeliveryFormat = SmtpDeliveryFormat.International;                
                smtpClient.Send(mail);
                smtpClient.Dispose();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }           
        }

        static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                ) {
                    return true;
                };
        }

        //private bool RemoteServerCertificateValidationCallback(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        //{
        //    log.Error(certificate);        
        //    return true;
        //}

        public static bool RemoteServerCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // if got an cert auth error
            if (sslPolicyErrors != SslPolicyErrors.RemoteCertificateNameMismatch) return false;
            const string sertFileName = "base64/der.cer";

            // check if cert file exists
            if (File.Exists(sertFileName))
            {
                var actualCertificate = X509Certificate.CreateFromCertFile(sertFileName);
                return certificate.Equals(actualCertificate);
            }

            // export and check if cert not exists
            using (var file = File.Create(sertFileName))
            {
                var cert = certificate.Export(X509ContentType.Cert);
                file.Write(cert, 0, cert.Length);
            }
            var createdCertificate = X509Certificate.CreateFromCertFile(sertFileName);
            return certificate.Equals(createdCertificate);
        }

        private X509Certificate GetCertificate()
        {
            const string sertFileName = "cer/der.cer";

            if (File.Exists(sertFileName))
            {
                return X509Certificate.CreateFromCertFile(sertFileName);                
            }

            return new X509Certificate();
        }
    }
}
