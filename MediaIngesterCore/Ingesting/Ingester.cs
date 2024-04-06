using MediaIngesterCore.Parsing;
using static MediaIngesterCore.Utils;

namespace MediaIngesterCore.Ingesting;

/// <summary>
///     Represents a media ingest
/// </summary>
public class Ingester
{
    /// <summary>
    ///     The job this ingester is working on
    /// </summary>
    public readonly IngestJob Job;

    public Ingester(IngestJob job)
    {
        this.Job = job;
    }

    /// <summary>
    ///     The status of the ingest
    /// </summary>
    public IngestStatus Status { get; private set; } = IngestStatus.READY;

    /// <summary>
    ///     Raised when a file copy is started
    /// </summary>
    public event EventHandler<FileIngestStartedEventArgs>? FileIngestStarted;

    /// <summary>
    ///     Raised when a file copy is completed
    /// </summary>
    public event EventHandler<FileIngestCompletedEventArgs>? FileIngestCompleted;

    public Task Ingest()
    {
        return this.Ingest(new CancellationToken(), new ManualResetEvent(true));
    }

    /// <summary>
    ///     Starts the ingest
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the ingest.</param>
    /// <param name="resetEvent">A ManualResetEvent that can be used to pause and resume the ingest</param>
    /// <param name="progress">A progress object that can be used to report progress</param>
    /// <returns>A reference to the ingest task</returns>
    /// <exception cref="InvalidOperationException">If the ingest is already in progress</exception>
    public Task Ingest(CancellationToken cancellationToken, ManualResetEvent resetEvent,
        IProgress<double>? progress = null)
    {
        if (this.Status != IngestStatus.READY)
            throw new InvalidOperationException("Cannot start ingest while ingest is in progress");
        return Task.Run(() =>
        {
            try
            {
                this.Status = IngestStatus.INGESTING;
                for (int i = 0; i < this.Job.Files.Count; i++)
                {
                    if (!resetEvent.WaitOne(0)) this.Status = IngestStatus.PAUSED;
                    resetEvent.WaitOne();
                    this.Status = IngestStatus.INGESTING;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.Status = IngestStatus.CANCELED;
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    string filePath = this.Job.Files.ElementAt(i);
                    this.FileIngestStarted?.Invoke(this, new FileIngestStartedEventArgs(filePath, i));
                    FileIngestCompletedEventArgs args = this.IngestFile(filePath, i);
                    this.FileIngestCompleted?.Invoke(this, args);
                    progress?.Report((i + 1d) / this.Job.Files.Count);
                }

                this.Status = IngestStatus.COMPLETED;
            }
            catch
            {
                this.Status = IngestStatus.FAILED;
                throw;
            }
        }, cancellationToken);
    }

    private FileIngestCompletedEventArgs IngestFile(string filePath, int i)
    {
        Evaluator evaluator = new Evaluator(filePath);
        string? destination = evaluator.Evaluate(this.Job.Rules);

        if (evaluator.Ignore)
            return new FileIngestCompletedEventArgs(i, filePath, "", FileIngestStatus.IGNORED);

        if (evaluator.RuleMatched)
            destination = Path.Join(this.Job.DestinationPath, destination);
        else
            destination = Path.Join(this.Job.DestinationPath, "Unsorted");
        Directory.CreateDirectory(destination);
        string fileName = Path.GetFileName(filePath);
        int duplicates = 0;

        bool renamed = false;
        while (File.Exists(Path.Join(destination, fileName)))
        {
            if (IsSameFile(filePath, Path.Join(destination, fileName)))
                return new FileIngestCompletedEventArgs(i, filePath, Path.Join(destination, fileName),
                    FileIngestStatus.SKIPPED);

            renamed = true;
            duplicates++;
            fileName = $"{Path.GetFileNameWithoutExtension(filePath)} ({duplicates}){Path.GetExtension(filePath)}";
        }

        File.Copy(filePath, Path.Join(destination, fileName));

        return new FileIngestCompletedEventArgs(i, filePath, Path.Join(destination, fileName),
            renamed ? FileIngestStatus.RENAMED : FileIngestStatus.COMPLETED);
    }
}