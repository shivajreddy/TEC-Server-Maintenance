using System;
using System.Diagnostics;
using System.IO;


namespace ServerMaintenance;


public class Logger
{
    private readonly string _fullFilePath;

    //! Constructor - create a logger instance with fixed file name
    public Logger(string logFileName)
    {
        var logFileName1 = logFileName;
        //! Location of Log files
        const string logFolder = @"T:\11 - SOFTWARE MANAGEMENT\TEC Server Maintenance";
        _fullFilePath = Path.Combine(logFolder, logFileName1);
        try
        {
            using (var writer = File.CreateText(_fullFilePath))
            {
                writer.WriteLine($"Log file created at: {DateTime.Now} | Create By: {Environment.UserName}");
                writer.WriteLine("========================================================================");
            }

            Debug.WriteLine("Log file created: " + logFileName1);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occurred: " + ex.Message);
        }
    }

    // Add line to log file 
    public void AddDataToLogFile(string data)
    {
        try
        {
            using (var writer = File.AppendText(_fullFilePath))
            {
                writer.WriteLine(data);
            }

            Debug.WriteLine("Added data to log file");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to add data to log file");
        }
    }
}