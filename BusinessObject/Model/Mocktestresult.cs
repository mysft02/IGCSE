namespace BusinessObject.Model;

public class Mocktestresult
{
    public int MockTestResultId { get; set; }

    public int MockTestId { get; set; }

    public decimal Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Mocktest MockTest { get; set; } = null!;
    
    public virtual ICollection<Mocktestquestion> MockTestQuestions { get; set; }

    public virtual ICollection<Mocktestuseranswer> MockTestUserAnswer { get; set; }
}
