namespace Lash.Users.Presentation.Requests;

public sealed record ChangePasswordRequest(string CurrentPassword, string Password, string ConfirmPassword);
