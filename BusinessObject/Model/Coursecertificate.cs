namespace BusinessObject.Model;

public class Coursecertificate
{
    public int CourseCertificateId { get; set; }

    public int CourseId { get; set; }

    public string UserId { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
