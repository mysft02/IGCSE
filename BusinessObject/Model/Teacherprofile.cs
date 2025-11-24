namespace BusinessObject.Model;

public class Teacherprofile
{
    public int TeacherProfileId { get; set; }

    public string TeacherId { get; set; } = null!;

    public string TeacherName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string AvatarUrl { get; set; } = null!;

    public int? Experience { get; set; }

    public virtual ICollection<Certificate> Certificates { get; } = new List<Certificate>();
}
