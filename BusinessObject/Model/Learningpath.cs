namespace BusinessObject.Model;

public class Learningpath
{
    public int LearningPathId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string ModulePath { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
