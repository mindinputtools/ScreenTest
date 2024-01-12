// See https://aka.ms/new-console-template for more information
public class ApiModel
{
    public string model { get; set; } = "llava";
    public string prompt { get; set; } = "Describe the attached image";
    public string[] images { get; set; } = new string[1];
    public bool stream { get; set; } = false;
}