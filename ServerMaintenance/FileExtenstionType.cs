namespace ServerMaintenance;

public class FileExtenstionType
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
    public string Extension { get; set; }

    public FileExtenstionType(string name, string extension)
    {
        Name = name;
        Extension = extension;
        IsChecked = false; // Default value
    }
}