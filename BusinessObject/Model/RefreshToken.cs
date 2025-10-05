using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Model
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }
        public string AccountID { get; set; }
        [ForeignKey("AccountID")]
        public Account Account { get; set; }
        public string Token { get; set; }
        public string JwtID { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
