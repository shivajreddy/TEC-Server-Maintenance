using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace ServerMaintenance;


class FileSearcher
{

    //! Properties
    private string _directory;
    private List<FileExtenstionType> _fileTypes;

    public List<FileType> AllFiles { get; }
    public string TotalSize;

    //! Constructor
    public FileSearcher(string directory, List<FileExtenstionType> fileTypes)
    {
        _directory = directory;
        _fileTypes = fileTypes;

        //! Perform search
        AllFiles = SearchFiles(_directory, fileTypes);

        //! update the TotalSize
        var totalSize = AllFiles.Aggregate<FileType, long>(0, (current, file) => current + file.Size);
        TotalSize = FormatSize(totalSize);
        Console.WriteLine("results generated");

    }

    //! Methods
    private List<FileType> SearchFiles(string directory, List<FileExtenstionType> fileTypes)
    {
        var finalResult = new List<FileType>();
        foreach (var fileType in fileTypes)
        {
            if (fileType.IsChecked)
            {
                var extenstionSearchResult = SearchFilesWithExtension(directory, fileType.Extension);
                finalResult.AddRange(extenstionSearchResult);
            }
        }
        return finalResult;
    }

    private List<FileType> SearchFilesWithExtension(string directory, string fileExtension)
    {
        var result = new List<FileType>();

        try
        {
            // Get all files with given fileExtension in the current directory. need "*" for Directory API
            var allFilePaths = Directory.GetFiles(directory, ("*"+fileExtension));

            // Process each .rvt file found
            foreach (var filePath in allFilePaths)
            {
                //! Filter for backups
                if (!ShouldIncludeFile(filePath)) continue;
                string fileName = Path.GetFileName(filePath);
                int fileSize = (int)new FileInfo(filePath).Length;

                result.Add(new FileType(fileName, fileSize, filePath));
            }

            // Recursively search in subdirectories
            var subDirectories = Directory.GetDirectories(directory);
            foreach (var subDirectory in subDirectories)
            {
                result.AddRange(SearchFilesWithExtension(subDirectory, fileExtension));
            }
        }
        catch (Exception ex) { Debug.WriteLine("Error: " + ex.Message); }



        return result;
    }

    // Logic for what a backup file is
    private bool ShouldIncludeFile(string filePath)
    {
        // Example: testProject.0012.rvt  - revit-2023 has 4 digits for backups
        var fileName = Path.GetFileName(filePath);
        return fileName.Length >= 9 && fileName[^9] == '.';
    }


    //! Helper methods
    //convert bytes to a human-readable size format
    private static string FormatSize(long bytes)
    {
        const long GB = 1024 * 1024 * 1024;
        const long MB = 1024 * 1024;

        return bytes switch
        {
            >= GB => $"{(double)bytes / GB:0.00} GB",
            >= MB => $"{(double)bytes / MB:0.00} MB",
            _ => $"{bytes} bytes"
        };
    }
}