namespace BychkovScript.Core.Runtime;

public class ReturnException(object? value) : Exception
{
    public object? Value { get; } = value;
}