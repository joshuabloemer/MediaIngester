using MediaIngesterCore.Parsing.SyntaxTree;

namespace MediaIngesterCore.Ingesting;

/// <summary>
///     Represents the data needed for an ingest
/// </summary>
public class IngestJob
{
    /// <summary>
    ///     The path of the destination root directory
    /// </summary>
    public readonly string DestinationPath;

    /// <summary>
    ///     The path to the directory to ingest
    /// </summary>
    public readonly string DirectoryPath;

    private readonly List<string> files = new();

    /// <summary>
    ///     The rules to match against while ingesting
    /// </summary>
    public readonly ProgramNode Rules;

    /// <summary>
    ///     Creates a new ingest job
    /// </summary>
    /// <param name="directoryPath">The path of the source directory</param>
    /// <param name="destinationPath">The path of the destination directory</param>
    /// <param name="rules">The first rule of the rule tree to use while ingesting</param>
    public IngestJob(string directoryPath, string destinationPath, ProgramNode rules)
    {
        this.DirectoryPath = directoryPath;
        this.DestinationPath = destinationPath;
        this.Rules = rules;
    }

    /// <summary>
    ///     Total size of all files in the job (in bytes)
    /// </summary>
    public ulong TotalSize { get; private set; }

    /// <summary>
    ///     The total number of files in the job
    /// </summary>
    public ulong TotalFiles { get; private set; }

    /// <summary>
    ///     The paths of all files to ingest
    /// </summary>
    public IReadOnlyCollection<string> Files => this.files.AsReadOnly();

    /// <summary>
    ///     Scams the source directory and populates the files list.
    /// </summary>
    /// <returns>A boolean indicating if the scan found any files</returns>
    public bool ScanDirectory()
    {
        try
        {
            this.files.Clear();
            foreach (string file in Directory.EnumerateFiles(this.DirectoryPath, "*", SearchOption.AllDirectories))
            {
                this.files.Add(file);
                this.TotalFiles++;
                this.TotalSize += (ulong)new FileInfo(file).Length;
            }

            return this.files.Count > 0;
        }
        catch
        {
            this.files.Clear();
            this.TotalFiles = 0;
            this.TotalSize = 0;
            throw;
        }
    }
}