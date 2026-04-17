namespace BychkovScript.Core.Lexing;

public record Token(TokenType Type, string Value, int Line, int Column)
{
    public override string ToString()
    {
        if (string.IsNullOrEmpty(Value))
        {
            return $"{Type} at {Line}:{Column}";
        }
        return $"{Type}: '{Value}' at {Line}:{Column}";
    }
}