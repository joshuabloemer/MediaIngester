namespace MediaIngesterCore.Ingesting
{
    public enum IngestStatus
    {
        Ready,
        Ingesting,
        Completed,
        Paused,
        Failed,
        Canceled
    }
}
