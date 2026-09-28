using System.Net.Mail;
using ErrorsFlow.Errors;
using Lash.Users.Application.Errors;
using Lash.Users.Infrastructure.Identity;
using Lash.Users.Infrastructure.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Providers;

public sealed class EmailSendersTests
{
    [Fact]
    public async Task SendAsync_WhenConfirmationUserIsMissing_CompletesWithoutSending()
    {
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUserEntity?)null);
        var templateFactory = new Mock<IIdentityEmailTemplateFactory>();
        var transport = new Mock<IEmailTransport>();
        var sender = new EmailConfirmationSender(
            userManager.Object,
            templateFactory.Object,
            transport.Object,
            NullLogger<EmailConfirmationSender>.Instance);

        var result = await sender.SendAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(result.IsSuccess);
        templateFactory.Verify(factory => factory.CreateEmailConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        transport.Verify(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_WhenConfirmationEmailIsAlreadyConfirmed_CompletesWithoutSending()
    {
        var userManager = CreateUserManager();
        var user = IdentityUserEntity.Create("user@example.com");
        user.EmailConfirmed = true;
        userManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        var templateFactory = new Mock<IIdentityEmailTemplateFactory>();
        var transport = new Mock<IEmailTransport>();
        var sender = new EmailConfirmationSender(
            userManager.Object,
            templateFactory.Object,
            transport.Object,
            NullLogger<EmailConfirmationSender>.Instance);

        var result = await sender.SendAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(result.IsSuccess);
        userManager.Verify(manager => manager.GenerateEmailConfirmationTokenAsync(user), Times.Never);
        transport.Verify(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_WhenConfirmationCanBeDelivered_GeneratesCodeAndSendsTemplate()
    {
        var user = IdentityUserEntity.Create("user@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManager.Setup(manager => manager.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("confirmation-code");
        var message = new EmailMessage(user.Email!, "subject", "text", "html");
        var templateFactory = new Mock<IIdentityEmailTemplateFactory>();
        templateFactory.Setup(factory => factory.CreateEmailConfirmation(user.Email!, "confirmation-code"))
            .Returns(message);
        var transport = new Mock<IEmailTransport>();
        var sender = new EmailConfirmationSender(
            userManager.Object,
            templateFactory.Object,
            transport.Object,
            NullLogger<EmailConfirmationSender>.Instance);

        var eventId = Guid.NewGuid();
        var result = await sender.SendAsync(user.Id, eventId);

        Assert.True(result.IsSuccess);
        transport.Verify(sender => sender.SendAsync(
            It.Is<EmailMessage>(sent =>
                sent.To == message.To &&
                sent.Subject == message.Subject &&
                sent.PlainTextBody == message.PlainTextBody &&
                sent.HtmlBody == message.HtmlBody &&
                sent.EventId == eventId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenPasswordResetDeliveryFails_ReturnsDeliveryError()
    {
        var user = IdentityUserEntity.Create("user@example.com");
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManager.Setup(manager => manager.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-code");
        var templateFactory = new Mock<IIdentityEmailTemplateFactory>();
        templateFactory.Setup(factory => factory.CreatePasswordReset(user.Email!, "reset-code"))
            .Returns(new EmailMessage(user.Email!, "subject", "text", "html"));
        var transport = new Mock<IEmailTransport>();
        transport.Setup(sender => sender.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmtpException(SmtpStatusCode.GeneralFailure));
        var sender = new PasswordResetSender(
            userManager.Object,
            templateFactory.Object,
            transport.Object,
            NullLogger<PasswordResetSender>.Instance);

        var result = await sender.SendAsync(user.Id, Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(UsersApplicationErrorCodes.EmailDeliveryFailed, result.Error.Code);
    }

    private static Mock<UserManager<IdentityUserEntity>> CreateUserManager() =>
        new(
            Mock.Of<IUserStore<IdentityUserEntity>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            Mock.Of<ILogger<UserManager<IdentityUserEntity>>>());
}
