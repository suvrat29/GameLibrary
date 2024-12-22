namespace GameLib.dal.ViewModels.Request;

public class CreateTestRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int ReadTime { get; set; }
}