using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _smtpUser = "khaled.yousef.333@gmail.com"; 
    private readonly string _smtpPass = "yzmu nzxb ptgd jjvm"; 

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress(_smtpUser, "Orderaty");
        mail.To.Add(email);
        mail.Subject = subject;
        mail.Body = htmlMessage;
        mail.IsBodyHtml = true;

        using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
        {
            smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

        return Task.CompletedTask;
    }
}
