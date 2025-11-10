namespace BusinessObject.Model;

public class Finalquiz
{
    public int FinalQuizId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
