namespace MediaIngesterCore.Ingesting;

/// <summary>
///     Event that is raised when a file copy is completed
/// </summary>
public class FileIngestCompletedEventArgs : EventArgs
{
    public FileIngestCompletedEventArgs(int fileNumber, string filePath, string newPath, FileIngestStatus status)
    {
        this.FileNumber = fileNumber;
        this.FilePath = filePath;
        this.NewPath = newPath;
        this.Status = status;
    }

    /// <summary>
    ///     The index of the file that was copied
    /// </summary>
    public int FileNumber { get; private set; }

    /// <summary>
    ///     The original path of the file that was copied
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    ///     The new path of the file that was copied
    /// </summary>
    public string NewPath { get; private set; }

    /// <summary>
    ///     The status of the file Evaluation
    /// </summary>
    public FileIngestStatus Status { get; private set; }
}