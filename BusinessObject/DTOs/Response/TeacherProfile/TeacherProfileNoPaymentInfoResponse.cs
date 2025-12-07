namespace BusinessObject.DTOs.Response.TeacherProfile
{
    public class TeacherProfileNoPaymentInfoResponse
    {
        public int TeacherProfileId { get; set; }

        public string TeacherId { get; set; }

        public string TeacherName { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }

        public string Experience { get; set; }

        public List<CertificateResponse> Certificates { get; set; } = new List<CertificateResponse>();
    }
}
