using System.Text.Encodings.Web;

namespace Lash.Users.Infrastructure.Providers;

public interface IIdentityEmailTemplateFactory
{
    EmailMessage CreateEmailConfirmation(string email, string code);

    EmailMessage CreatePasswordReset(string email, string code);
}

public sealed class IdentityEmailTemplateFactory : IIdentityEmailTemplateFactory
{
    public EmailMessage CreateEmailConfirmation(string email, string code) =>
        Create(
            email,
            "Подтвердите электронную почту в Lash",
            "Подтверждение электронной почты",
            "Для завершения регистрации в Lash используйте код:",
            code,
            "Если вы не создавали аккаунт в Lash, просто проигнорируйте это письмо.");

    public EmailMessage CreatePasswordReset(string email, string code) =>
        Create(
            email,
            "Сброс пароля в Lash",
            "Сброс пароля",
            "Чтобы установить новый пароль в Lash, используйте код:",
            code,
            "Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.");

    private static EmailMessage Create(
        string email,
        string subject,
        string heading,
        string instruction,
        string code,
        string note)
    {
        var encodedCode = HtmlEncoder.Default.Encode(code);
        var plainTextBody = $"Здравствуйте!{Environment.NewLine}{Environment.NewLine}" +
                            $"{instruction}{Environment.NewLine}{Environment.NewLine}" +
                            $"{code}{Environment.NewLine}{Environment.NewLine}" +
                            note;
        var htmlBody = $"""
                        <!doctype html>
                        <html lang="ru">
                        <body style="font-family: Arial, sans-serif; color: #1f2937; line-height: 1.5;">
                          <h2>{heading}</h2>
                          <p>Здравствуйте!</p>
                          <p>{instruction}</p>
                          <p style="font-size: 24px; font-weight: 700; letter-spacing: 0.08em;">{encodedCode}</p>
                          <p>{note}</p>
                        </body>
                        </html>
                        """;

        return new EmailMessage(email, subject, plainTextBody, htmlBody);
    }
}
