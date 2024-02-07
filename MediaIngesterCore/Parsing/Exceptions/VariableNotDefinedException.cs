namespace MediaIngesterCore.Parsing.Exceptions;

public class VariableNotDefinedException : EvaluateException
{
    public VariableNotDefinedException(string message) : base(message)
    {
    }
}