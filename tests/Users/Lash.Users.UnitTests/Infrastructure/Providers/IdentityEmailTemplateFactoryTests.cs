using Lash.Users.Infrastructure.Providers;

namespace Lash.Users.UnitTests.Infrastructure.Providers;

public sealed class IdentityEmailTemplateFactoryTests
{
    [Fact]
    public void CreateEmailConfirmation_CreatesInformativePlainTextAndHtmlMessage()
    {
        var factory = new IdentityEmailTemplateFactory();

        var message = factory.CreateEmailConfirmation("user@example.com", "confirmation-code");

        Assert.Equal("user@example.com", message.To);
        Assert.Equal("Подтвердите электронную почту в Lash", message.Subject);
        Assert.Contains("confirmation-code", message.PlainTextBody);
        Assert.Contains("Если вы не создавали аккаунт", message.PlainTextBody);
        Assert.Contains("confirmation-code", message.HtmlBody);
    }

    [Fact]
    public void CreatePasswordReset_WhenCodeContainsHtml_EncodesItInHtmlMessage()
    {
        var factory = new IdentityEmailTemplateFactory();

        var message = factory.CreatePasswordReset("user@example.com", "<reset-code>");

        Assert.Equal("Сброс пароля в Lash", message.Subject);
        Assert.Contains("<reset-code>", message.PlainTextBody);
        Assert.Contains("&lt;reset-code&gt;", message.HtmlBody);
        Assert.DoesNotContain("<reset-code>", message.HtmlBody);
        Assert.Contains("Если вы не запрашивали сброс пароля", message.HtmlBody);
    }
}
