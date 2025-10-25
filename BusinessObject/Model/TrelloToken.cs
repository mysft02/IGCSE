using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

[Table("trello_token")]
public class TrelloToken
{
    [Key]
    [Column("trello_id")]
    [StringLength(100)]
    public string TrelloId { get; set; } = null!;

    [Required]
    [Column("trello_api_token")]
    [StringLength(100)]
    public string TrelloApiToken { get; set; } = null!;

    [Key]
    [Column("user_id")]
    [StringLength(100)]
    public string UserId { get; set; } = null!;

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("is_sync")]
    public bool IsSync { get; set; } = false;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual Account User { get; set; } = null!;
}
