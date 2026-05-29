namespace Cls.Shared.Contracts.Users;

public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}
