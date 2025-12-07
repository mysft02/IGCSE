namespace BusinessObject.Model;

public class Studentenrollment
{
    public int EnrollmentId { get; set; }

    public string StudentId { get; set; } = null!;

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string? ParentId { get; set; }

    public virtual Account Student { get; set; } = null!;

    public virtual Account? Parent { get; set; }

    public virtual Course Course { get; set; }
}

