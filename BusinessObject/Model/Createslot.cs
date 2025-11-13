namespace BusinessObject.Model;

public class Createslot
{
    public int CreateSlotId { get; set; }

    public int Slot { get; set; }

    public int AvailableSlot { get; set; }

    public string TeacherId { get; set; } = null!;

    public int PackageId { get; set; }

    public virtual Package Package { get; set; }
}
