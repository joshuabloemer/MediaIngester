namespace MediaIngesterCore.Ingesting;
/// <summary>
/// Event that is raised when a file copy is started
/// </summary>
public class FileIngestStartedEventArgs : EventArgs
{ 
    /// <summary>
    /// The path of the file that is being copied
    /// </summary>
    public string FilePath { get; private set;}
    /// <summary>
    /// The index of the file that is being copied
    /// </summary>
    public int FileNumber { get; private set;}
    
    public FileIngestStartedEventArgs(string filePath, int fileNumber)
    {
        this.FilePath = filePath;
        this.FileNumber = fileNumber;
    }
}