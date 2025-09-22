using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Model
{
    public class UserProfile
    {
        [Key]
        public int ProfileID { get; set; }

        [Required]
        public string AccountID { get; set; }
        [ForeignKey("AccountID")]
        public Account Account { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }

        public string? Description { get; set; }
    }
}
