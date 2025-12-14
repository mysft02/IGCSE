using Microsoft.AspNetCore.Http;

namespace BusinessObject.DTOs.Request.Media
{
    public class AddMediaRequest
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public IFormFile File { get; set; }
    }
}
