using System.ComponentModel.DataAnnotations;

namespace Cls.Shared.Contracts.Providers;
public class ProviderCreateRequest
{
    public required string Name { get; set; }
    [Phone]
    public string? Phone { get; set; }
    [Phone]
    public string? SecondPhone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
