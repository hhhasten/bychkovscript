using BychkovScript.Core.Lexing;
using BychkovScript.Core.AST;

namespace BychkovScript.Core.Parsing;

public class Parser
{
    readonly Lexer _lexer;
    Token _current;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        Move();
    }

    void Move() => _current = _lexer.GetNext();

    Token Eat(TokenType expectedType)
    {
        if (_current.Type == expectedType)
        {
            Token consumedToken = _current;
            Move();
            return consumedToken;
        }
        
        throw new Exception(
            $"SyntaxError: Expected token {expectedType}, but got {_current.Type} '{_current.Value}' " +
            $"{_current.Line}::{_current.Column}");
    }

    public ProgramNode ParseProgram()
    {
        List<Statement> statements = [];

        while (_current.Type != TokenType.EndOfFile)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }
    
    Statement ParseStatement()
    {
        switch (_current.Type)
        {
            case TokenType.Let:
            case TokenType.Const:
                return ParseVariableDeclaration();
            
            case TokenType.Print:
                return ParsePrintStatement();
            
            case TokenType.Identifier:
                Token name = Eat(TokenType.Identifier);
                Eat(TokenType.Assign);
                Expression value = ParseExpression();
                Eat(TokenType.SemiColon);
                return new AssignmentNode(name, value);

            default:
                throw new Exception($"SyntaxError: Unexpected statement starting with {_current.Type} '{_current.Value}' at {_current.Line}::{_current.Column}");
        }
    }
    
    Statement ParseVariableDeclaration()
    {
        Token modifier = _current;
        Move();
        
        Token identifier = Eat(TokenType.Identifier);
        
        Eat(TokenType.Colon);
        
        Token dataType = _current;
        if (dataType.Type is not (TokenType.TypeInt32 or TokenType.TypeFloat32 or TokenType.TypeString))
        {
            throw new Exception($"SyntaxError: Expected type but got {dataType.Type} '{dataType.Value}' {_current.Line}::{_current.Column}");
        }
        Move();
        
        Eat(TokenType.Assign);
        
        Expression value = ParseExpression();
        
        Eat(TokenType.SemiColon);

        return new VariableDeclarationNode(modifier, identifier, dataType, value);
    }
    
    Statement ParsePrintStatement()
    {
        Token printToken = Eat(TokenType.Print);
        
        Eat(TokenType.Bang); 
        
        Eat(TokenType.OpenParen);
        
        Expression value = ParseExpression(); 
        
        Eat(TokenType.CloseParen);
        
        Eat(TokenType.SemiColon);

        return new PrintStatementNode(printToken, value);
    }

    Expression ParseExpression()
    {
        return ParseAddictive();
    }

    Expression ParseAddictive()
    {
        Expression left = ParseMultiplicative();

        while (_current.Type is TokenType.Plus or TokenType.Minus)
        {
            Token operatorToken = _current;
            Move();

            Expression right = ParseMultiplicative();
            left = new BinaryNode(left, operatorToken, right);
        }

        return left;
    }

    Expression ParseMultiplicative()
    {
        Expression left = ParsePrimary();
        
        while (_current.Type is TokenType.Multiply or TokenType.Divide)
        {
            Token operatorToken = _current;
            Move();
            
            Expression right = ParsePrimary();
            
            left = new BinaryNode(left, operatorToken, right);
        }

        return left;
    }

    Expression ParsePrimary()
    {
        Token token = _current;

        switch (token.Type)
        {
            case TokenType.Identifier:
                Move();
                return new VariableNode(token, token.Value);
            
            case TokenType.IntLiteral:
            case TokenType.FloatLiteral:
                Move();
                double value = double.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture);
                return new NumberNode(token, value);
            
            case TokenType.StringLiteral:
                Move();
                return new StringNode(token, token.Value);
            
            case TokenType.OpenParen:
                Move();
                Expression expression = ParseExpression();
                Eat(TokenType.CloseParen);
                return expression;

            default:
                throw new Exception($"SyntaxError: Unexpected token {_current.Type} at line {_current.Line}. Awaited expression.");
        }
    }
}