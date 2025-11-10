using System;
namespace BusinessObject.Model;

public class Mocktestuseranswer
{
    public int MockTestUserAnswerId { get; set; }

    public int MockTestResultId { get; set; }

    public int MockTestQuestionId { get; set; }

    public string? Answer { get; set; }

    public decimal Score { get; set; }
}
