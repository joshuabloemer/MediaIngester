namespace MediaIngesterCore.Ingesting;

/// <summary>
///     Event that is raised when a file copy is completed
/// </summary>
public class FileIngestCompletedEventArgs : EventArgs
{
    public FileIngestCompletedEventArgs(int fileNumber, string filePath, string newPath, bool skipped, bool renamed,
        bool ruleMatched, bool ignored)
    {
        this.FileNumber = fileNumber;
        this.FilePath = filePath;
        this.NewPath = newPath;
        this.Skipped = skipped;
        this.Renamed = renamed;
        this.RuleMatched = ruleMatched;
        this.Ignored = ignored;
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
    ///     A boolean indicating if the file was skipped
    /// </summary>
    public bool Skipped { get; private set; }

    /// <summary>
    ///     A boolean indicating if the file was renamed
    /// </summary>
    public bool Renamed { get; private set; }

    /// <summary>
    ///     A boolean indicating if the file matched a rule
    /// </summary>
    public bool RuleMatched { get; private set; }

    /// <summary>
    ///     A boolean indicating if the file was ignored
    /// </summary>
    public bool Ignored { get; }
}