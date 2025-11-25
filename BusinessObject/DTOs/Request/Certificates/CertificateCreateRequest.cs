using Microsoft.AspNetCore.Http;

namespace BusinessObject.DTOs.Request.Certificates
{
    public class CertificateCreateRequest
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile ImageUrl { get; set; }
    }
}
