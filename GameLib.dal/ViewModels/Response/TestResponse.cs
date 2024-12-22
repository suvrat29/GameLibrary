namespace GameLib.dal.ViewModels.Response;

public class TestResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int ReadTime { get; set; }
    public DateTime CreatedAt { get; set; }
}