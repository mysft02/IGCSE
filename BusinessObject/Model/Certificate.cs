namespace BusinessObject.Model;

public class Certificate
{
    public int CertificateId { get; set; }

    public int TeacherProfileId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
