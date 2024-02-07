namespace MediaIngesterCore.Parsing.Exceptions;

public class MetadataNotFoundException : EvaluateException
{
    public MetadataNotFoundException(string message) : base(message)
    {
    }
}