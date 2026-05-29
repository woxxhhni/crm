namespace Cls.Shared.Contracts.Steps;

public class StepResponse
{
    public int Id { get; set; }
    public int StageId { get; set; }
    public string Name { get; set; } = default!;
    public int OrderPosition { get; set; }
    public string? Description { get; set; }
    public bool IsFinalStep { get; set; }
    public bool IsActive { get; set; }
    public string StageName { get; set; } = default!;
}
