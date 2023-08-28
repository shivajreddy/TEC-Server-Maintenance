
namespace ServerMaintenance;

public class FileType
{
    public string Name;
    public int Size;
    public string Path;
    public FileType(string name, int size, string path)
    {
        Name = name;
        Size = size;
        Path = path;
    }
}