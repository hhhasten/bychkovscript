using BychkovScript.Core.Lexing;

namespace BychkovScript.Core.AST;

public abstract record Node(Token Token);

public abstract record Expression(Token Token) : Node(Token);
public abstract record Statement(Token Token) : Node(Token);


public record ProgramNode(List<Statement> Statements) : Node(new Token(TokenType.EndOfFile, "", 0, 0));

public record NumberNode(Token Token, double Value) : Expression(Token);

public record StringNode(Token Token, string Value) : Expression(Token);


public record BinaryNode(Expression Left, Token Operator, Expression Right) : Expression(Operator);

public record VariableDeclarationNode(
    Token Modifier,     // let / const
    Token Identifier,   // var name (x)
    Token DataType,     // type (int, float, string)
    Expression Value    // value
) : Statement(Modifier);

public record PrintStatementNode(
    Token Token,
    Expression Value
) : Statement(Token);

public record VariableNode(Token Token, string Name) : Expression(Token);

public record AssignmentNode(Token Identifier, Expression Value) : Statement(Identifier);