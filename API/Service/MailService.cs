using System.Net;
using System.Net.Mail;
using System.Text;

namespace API.Service;

public class MailService
{
    // SMTP konfiguration fra appsettings.json
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public MailService(IConfiguration configuration, ILogger<MailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Læs SMTP konfiguration - først fra environment variabler, derefter fra appsettings.json
        _smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER")
                      ?? _configuration["MailSettings:SmtpServer"]
                      ?? "smtp.gmail.com";

        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")
                              ?? _configuration["MailSettings:SmtpPort"]
                              ?? "587");

        _smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME")
                        ?? _configuration["MailSettings:SmtpUsername"]
                        ?? "";

        _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                        ?? _configuration["MailSettings:SmtpPassword"]
                        ?? "";

        _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL")
                     ?? _configuration["MailSettings:FromEmail"]
                     ?? "";

        _fromName = Environment.GetEnvironmentVariable("FROM_NAME")
                    ?? _configuration["MailSettings:FromName"]
                    ?? "H2-MAGS System";

        // Valider at alle nødvendige settings er sat
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword) ||
            string.IsNullOrEmpty(_fromEmail))
        {
            _logger.LogWarning("⚠️ Mail konfiguration mangler - email funktionalitet vil ikke virke");
            _logger.LogWarning("SMTP_SERVER: {SmtpServer}", _smtpServer);
            _logger.LogWarning("SMTP_USERNAME: {SmtpUsername}",
                string.IsNullOrEmpty(_smtpUsername) ? "MISSING" : "SET");
            _logger.LogWarning("SMTP_PASSWORD: {SmtpPassword}",
                string.IsNullOrEmpty(_smtpPassword) ? "MISSING" : "SET");
            _logger.LogWarning("FROM_EMAIL: {FromEmail}", string.IsNullOrEmpty(_fromEmail) ? "MISSING" : _fromEmail);
        }
        else
        {
            _logger.LogInformation("✅ Mail konfiguration loaded successfully");
            _logger.LogInformation("SMTP_SERVER: {SmtpServer}", _smtpServer);
            _logger.LogInformation("FROM_EMAIL: {FromEmail}", _fromEmail);
        }
    }

    /// <summary>
    /// Generel metode til at sende emails via Gmail SMTP
    /// </summary>
    /// <param name="toEmail">Modtagerens email adresse</param>
    /// <param name="subject">Email emne</param>
    /// <param name="body">Email indhold (kan være HTML eller plain text)</param>
    /// <param name="isHtml">Angiver om body er HTML format</param>
    /// <returns>True hvis email blev sendt succesfuldt, ellers false</returns>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            // Valider konfiguration
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogError("❌ SMTP konfiguration mangler - kan ikke sende email");
                return false;
            }

            // Opret SMTP client med Gmail indstillinger
            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true, // Gmail kræver SSL
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds
            };

            // Opret email besked
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            // Tilføj modtager
            mailMessage.To.Add(toEmail);

            // Send email asynkront
            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("✅ Email sendt succesfuldt til: {Email}", toEmail);
            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "❌ SMTP fejl ved sending af email til: {Email}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Generel fejl ved sending af email til: {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Tester SMTP forbindelsen og konfigurationen
    /// </summary>
    /// <returns>True hvis forbindelsen virker, ellers false</returns>
    public async Task<bool> TestSmtpConnectionAsync()
    {
        try
        {
            _logger.LogInformation("🔧 Tester SMTP forbindelse...");

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                Timeout = 10000 // 10 sekunder timeout for test
            };

            // Prøv at oprette forbindelse
            await smtpClient.SendMailAsync(new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                To = { _fromEmail }, // Send til sig selv som test
                Subject = "SMTP Test",
                Body = "Dette er en test email for at verificere SMTP konfigurationen."
            });

            _logger.LogInformation("✅ SMTP forbindelse virker korrekt");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SMTP forbindelse fejler: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Sender en velkommen email til en ny bruger
    /// </summary>
    /// <param name="userEmail">Brugerens email adresse</param>
    /// <param name="username">Brugerens brugernavn</param>
    /// <param name="role">Brugerens rolle</param>
    /// <returns>True hvis email blev sendt succesfuldt, ellers false</returns>
    public async Task<bool> SendWelcomeEmailAsync(string userEmail, string username, string role = "User")
    {
        try
        {
            _logger.LogInformation("📧 Sender velkommen email til: {Email}", userEmail);

            // Opret HTML template for velkommen email
            var htmlBody = CreateWelcomeEmailTemplate(username, role);
            var subject = "Velkommen til H2-MAGS System! 🎉";

            // Send email
            var success = await SendEmailAsync(userEmail, subject, htmlBody, isHtml: true);

            if (success)
            {
                _logger.LogInformation("✅ Velkommen email sendt succesfuldt til: {Email}", userEmail);
            }
            else
            {
                _logger.LogWarning("❌ Kunne ikke sende velkommen email til: {Email}", userEmail);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fejl ved sending af velkommen email til: {Email}", userEmail);
            return false;
        }
    }

    /// <summary>
    /// Opretter HTML template for velkommen email
    /// </summary>
    /// <param name="username">Brugerens brugernavn</param>
    /// <param name="role">Brugerens rolle</param>
    /// <returns>HTML formateret email template</returns>
    private string CreateWelcomeEmailTemplate(string username, string role)
    {
        return $@"
<!DOCTYPE html>
<html lang='da'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Velkommen til H2-MAGS</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: white;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            border-bottom: 3px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .header h1 {{
            color: #007bff;
            margin: 0;
            font-size: 28px;
        }}
        .content {{
            margin-bottom: 30px;
        }}
        .highlight {{
            background-color: #e7f3ff;
            padding: 15px;
            border-left: 4px solid #007bff;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            color: #666;
            font-size: 14px;
            border-top: 1px solid #eee;
            padding-top: 20px;
        }}
        .button {{
            display: inline-block;
            background-color: #007bff;
            color: white;
            padding: 12px 25px;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Velkommen til H2-MAGS!</h1>
        </div>
        
        <div class='content'>
            <p>Hej <strong>{username}</strong>,</p>
            
            <p>Velkommen til H2-MAGS systemet! Din konto er nu oprettet og klar til brug.</p>
            
            <div class='highlight'>
                <h3>📋 Din konto information:</h3>
                <ul>
                    <li><strong>Brugernavn:</strong> {username}</li>
                    <li><strong>Rolle:</strong> {role}</li>
                    <li><strong>Oprettet:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</li>
                </ul>
            </div>
            
            <p>Du kan nu logge ind på systemet og begynde at bruge alle de tilgængelige funktioner.</p>
            
            <p style='text-align: center;'>
                <a href='https://25h2-mags.mercantec.tech/login' class='button'>Log ind på systemet</a>
            </p>
            
            <p><strong>💡 Tip:</strong> Hvis du har spørgsmål eller brug for hjælp, er du velkommen til at kontakte systemadministratoren.</p>
        </div>
        
        <div class='footer'>
            <p>Denne email er sendt automatisk fra H2-MAGS systemet.</p>
            <p>© 2025 H2-MAGS - Mercantec</p>
        </div>
    </div>
</body>
</html>";
    }
}