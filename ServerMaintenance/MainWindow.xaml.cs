using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;


namespace ServerMaintenance;


public sealed partial class MainWindow : Window
{
    private string _chosenPath = "";
    public ObservableCollection<FileExtenstionType> FileTypes { get; set; }
    private FileSearcher _searchResult;


    public MainWindow()
    {
        InitializeComponent();
        FileTypes = new ObservableCollection<FileExtenstionType>
        {
            new("Revit Project (.RVT)",".rvt"),
            new("Revit Template (.RTE)", ".rte"),
            new("Revit Family (.RFA)", ".rfa"),
            new("Revit Family Template (.RFT)", ".rft")
        };
    }

    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear previous returned file name, if it exists, between iterations of this scenario
        FilePathTextBlock.Text = "";

        // Create a folder picker
        var openPicker = new Windows.Storage.Pickers.FolderPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        // Initialize the folder picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your folder picker
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a folder
        var folder = await openPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);

            _chosenPath = folder.Path;

            // UI
            FileSearchContainer.Visibility = Visibility.Visible;
            FilePathTextBlock.Text = _chosenPath;
        }
        else
        {
            // UI
            FileSearchContainer.Visibility = Visibility.Collapsed;
            FilePathTextBlock.Text = "Operation cancelled.";
        }
    }

    private void FindFilesButton_Click(object sender, RoutedEventArgs e)
    {
        // Remove previous search
        TotalFilesCountTextBlock.Text = "0";
        TotalFilesSizeTextBlock.Text = "0 Bytes";

        // New Search
        _searchResult = new FileSearcher(_chosenPath, FileTypes.ToList());
        var searchResultAllFiles = _searchResult.AllFiles;
        var searchResultSize = _searchResult.TotalSize;


        // ! show results 
        FilesResultStackPanel.Visibility = Visibility.Visible;
        TotalFilesCountTextBlock.Text = searchResultAllFiles.Count.ToString();
        TotalFilesSizeTextBlock.Text = searchResultSize;
        AllFileNamesListBox.ItemsSource = _searchResult.AllFiles.Select(file => file.Name);
    }

    private void DeleteFilesButton_Click(object sender, RoutedEventArgs e)
    {
        // Delete 1 by 1, add to log 1 by 1
        var logFileName = $"ServerLog_{DateTime.Now:M-d}_{DateTime.Now:HH-mm-ss}.txt";
        var logger = new Logger(logFileName);

        var allFilesToBeDeleted = _searchResult.AllFiles;
        // Iterate over all files to be deleted
        foreach (var file in allFilesToBeDeleted)
        {
            // Delete it
            //DeleteFile(file.Path);

            // Log it
            var data = $"{file.Name} | {FormatSize((file.Size))} | {file.Path}";
            logger.AddDataToLogFile(data);
        }
        // Add Total delete files to logger
        logger.AddDataToLogFile("========================================================================");
        logger.AddDataToLogFile($"Finished at: {DateTime.Now} | Total files Deleted: {_searchResult.AllFiles.Count} | Total size deleted: {_searchResult.TotalSize}");

        Debug.WriteLine("Deleted all files");
    }

    //! Helper Methods -----
    //! Even handler when checkboxes are toggled
    private void CheckBoxIsToggled(object sender, RoutedEventArgs e)
    {
        bool noCheckBoxIsChecked = FileTypes.All(ft => !ft.IsChecked);
        FindFilesButton.IsEnabled = !noCheckBoxIsChecked;
    }

    //! Delete file at given path
    private void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            Console.WriteLine("File deleted successfully: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    //! convert bytes to MB, GB for logging
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
