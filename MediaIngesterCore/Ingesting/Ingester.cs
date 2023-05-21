using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaIngesterCore.Parsing;
using static MediaIngesterCore.Utils;

namespace MediaIngesterCore.Ingesting
{
    /// <summary>
    /// Represents a media ingest
    /// </summary>
    internal class Ingester
    {
        /// <summary>
        /// The job this ingester is working on
        /// </summary>
        public readonly IngestJob Job;
        /// <summary>
        /// The status of the ingest
        /// </summary>
        public IngestStatus Status { get; private set; } = IngestStatus.Ready;
        /// <summary>
        /// Raised when a file copy is started
        /// </summary>
        public event EventHandler<FileIngestStartedEventArgs>? FileIngestStarted;
        /// <summary>
        /// Raised when a file copy is completed
        /// </summary>
        public event EventHandler<FileIngestCompletedEventArgs>? FileIngestCompleted;
        
        public Ingester(IngestJob job)
        {
            this.Job = job;
        }
        
        /// <summary>
        /// Starts the ingest
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ingest.</param>
        /// <param name="resetEvent">A ManualResetEvent that can be used to pause and resume the ingest</param>
        /// <returns>A reference to the ingest task</returns>
        /// <exception cref="InvalidOperationException">If the ingest is already in progress</exception>
        public Task Ingest(CancellationToken cancellationToken, ManualResetEvent resetEvent)
        {
            if (this.Status != IngestStatus.Ready)
            {
                throw new InvalidOperationException("Cannot start ingest while ingest is in progress");
            }
            return Task.Run(() =>
            {
                try
                {
                    this.Status = IngestStatus.Ingesting;
                    for (int i = 0; i < this.Job.Files.Count; i++)
                    {
                        if (!resetEvent.WaitOne(0))
                        {
                            this.Status = IngestStatus.Paused;
                        }
                        resetEvent.WaitOne();
                        this.Status = IngestStatus.Ingesting;
                        if (cancellationToken.IsCancellationRequested)
                        {
                            this.Status = IngestStatus.Canceled;
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        string filePath = this.Job.Files.ElementAt(i);
                        this.FileIngestStarted?.Invoke(this, new FileIngestStartedEventArgs(filePath, i));
                        this.FileIngestCompleted?.Invoke(this,IngestFile(filePath, i));
                    }
                    this.Status = IngestStatus.Completed;
                }
                catch
                {
                    this.Status = IngestStatus.Failed;
                    throw;
                }
            }, cancellationToken);
        }

        private FileIngestCompletedEventArgs IngestFile(string filePath, int i)
        {
            Evaluator evaluator = new Evaluator(filePath);
            string destination = (string)evaluator.Evaluate(this.Job.Rules);
            if (evaluator.RuleMatched)
            {
                destination = Path.Join(this.Job.DestinationPath, destination);
            }
            else
            {
                destination = Path.Join(this.Job.DestinationPath, "Unsorted");
            }
            Directory.CreateDirectory(destination);
            string fileName = Path.GetFileName(filePath);
            int duplicates = 0;
            bool skipped = false;
            bool renamed = false;
            while (File.Exists(Path.Join(destination, fileName)))
            {
                if (IsSameFile(filePath, Path.Join(destination, fileName)))
                {
                    skipped = true;
                    return new FileIngestCompletedEventArgs(i,filePath, Path.Join(destination, fileName), skipped, renamed, evaluator.RuleMatched);
                }
                renamed = true;
                duplicates++;
                fileName = $"{Path.GetFileNameWithoutExtension(filePath)} ({duplicates}){Path.GetExtension(filePath)}";
            }
            return new FileIngestCompletedEventArgs(i,filePath, Path.Join(destination, fileName), skipped, renamed, evaluator.RuleMatched);
        }
    }
}
