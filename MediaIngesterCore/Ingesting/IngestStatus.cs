namespace MediaIngesterCore.Ingesting
{
    public enum IngestStatus
    {
        READY,
        INGESTING,
        COMPLETED,
        PAUSED,
        FAILED,
        CANCELED
    }
}
