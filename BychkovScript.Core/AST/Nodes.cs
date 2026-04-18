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
    TypeInfo? DataType,     // type (int, float, string)
    Expression Value    // value
) : Statement(Modifier);

public record VariableNode(Token Token, string Name) : Expression(Token);

public record AssignmentNode(Token Identifier, Expression Value) : Statement(Identifier);

public record BlockNode(List<Statement> Statements) : Statement(new Token(TokenType.OpenBrace, "{", 0, 0));

public record IfNode(
    Token Token, 
    Expression Condition, 
    BlockNode TrueBlock, 
    Statement? ElseBranch
) : Statement(Token);

public record BooleanNode(Token Token, bool Value) : Expression(Token);

public record WhileNode(
    Token Token, 
    Expression Condition, 
    BlockNode Body
) : Statement(Token);

public record ForNode(
    Token Token, 
    Token Iterator,
    Expression Start,
    Expression End,
    BlockNode Body
) : Statement(Token);

public record Parameter(Token Name, TypeInfo Type);

public record FunctionDeclarationNode(
    Token Token, 
    Token Identifier, 
    List<Parameter> Parameters, 
    TypeInfo ReturnType, 
    BlockNode Body,
    bool IsMethod = false
) : Statement(Token);

public record ReturnNode(Token Token, Expression? Value) : Statement(Token);

public record FunctionCallNode(Token Identifier, List<Expression> Arguments) : Expression(Identifier);

public record ExpressionStatementNode(Expression Expression) : Statement(Expression.Token);

public record ImportNode(Token Token, string ModuleName) : Statement(Token);

public record TypeInfo(Token BaseType, Token? ElementType = null);

public record ListLiteralNode(Token Token, List<Expression> Elements) : Expression(Token);

public record IndexAccessNode(Expression Target, Expression Index) : Expression(Target.Token);

public record IndexAssignmentNode(IndexAccessNode IndexAccess, Expression Value) : Statement(IndexAccess.Target.Token);

public record UnaryOperationNode(Token Operator, Expression Right) : Expression(Operator);

public record MethodCallNode(Expression Target, Token Identifier, List<Expression> Arguments) : Expression(Target.Token);