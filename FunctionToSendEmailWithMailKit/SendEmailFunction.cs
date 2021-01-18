using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Utils;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Collections.Generic;

namespace FunctionToSendEmailWithMailKit
{
    public static class SendEmailFunction
    {
        [FunctionName("SendEmailFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Parsing the request body to retrieve the email to send...");
                EmailToSend emailToSend = await JsonSerializer.DeserializeAsync<EmailToSend>(req.Body);

                log.LogInformation("Parsing the To Emails...");
                List<string> toEmailsList = GetToEmailAddressList(emailToSend.To);

                log.LogInformation("Calling the SendEmail method...");
                await SendEmail(
                    Environment.GetEnvironmentVariable("EmailHost"),
                    int.Parse(Environment.GetEnvironmentVariable("EmailPort")),
                    bool.Parse(Environment.GetEnvironmentVariable("EmailHostUsesLocalCertificate")),
                    Environment.GetEnvironmentVariable("EmailUser"),
                    Environment.GetEnvironmentVariable("EmailPassword"),
                    Environment.GetEnvironmentVariable("EmailFromName"),
                    Environment.GetEnvironmentVariable("EmailFromEmail"),
                    toEmailsList,
                    emailToSend.Subject,
                    emailToSend.PlainBody,
                    emailToSend.HtmlBody,
                    null,
                    null
                );

                log.LogInformation($"Email to: {String.Join(";", toEmailsList.ToArray()).ToString()} sent successfully!");
                return new OkObjectResult("Email sent successfully!");
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        public static List<string> GetToEmailAddressList(string toEmails)
        {
            // If there are no "ToEmails", then send it to the default "FromEmail"
            if (string.IsNullOrWhiteSpace(toEmails))
                return new List<string>() { Environment.GetEnvironmentVariable("EmailFromEmail") };

            return new List<string>(toEmails.Split(";"));
        }

        public static async Task SendEmail(string host, int port, bool hostUsesLocalCertificate, string user, string password,
            string fromName, string fromEmail, List<string> ToEmails, string subject, string bodyPlain, string bodyHtml,
            string linkedResourcePath, string attachmentPath)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(fromName, fromEmail));

            message.Subject = subject;

            foreach (string toEmail in ToEmails)
            {
                message.To.Add(new MailboxAddress(toEmail, toEmail));
            }

            var builder = new BodyBuilder();

            if (!string.IsNullOrWhiteSpace(bodyPlain))
                builder.TextBody = bodyPlain;

            if (!string.IsNullOrWhiteSpace(bodyHtml))
            {
                if (!string.IsNullOrWhiteSpace(linkedResourcePath))
                {
                    MimeEntity image = builder.LinkedResources.Add(linkedResourcePath);
                    image.ContentId = MimeUtils.GenerateMessageId();
                    builder.HtmlBody = string.Format(bodyHtml, image.ContentId);
                }
                else
                {
                    builder.HtmlBody = bodyHtml;
                }
            }

            // Attachment
            if (!string.IsNullOrWhiteSpace(attachmentPath))
                builder.Attachments.Add(attachmentPath);

            // Assigns the email body to the message
            message.Body = builder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                if (hostUsesLocalCertificate)
                    smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(user, password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }
        }

    }

    public class EmailToSend
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string PlainBody { get; set; }
        public string HtmlBody { get; set; }

    }

}
