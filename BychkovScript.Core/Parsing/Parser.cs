using BychkovScript.Core.Lexing;
using BychkovScript.Core.AST;

namespace BychkovScript.Core.Parsing;

public class Parser
{
    readonly Lexer _lexer;
    Token _current = null!;

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
            
            case TokenType.If: 
                return ParseIfStatement();
            
            case TokenType.While: 
                return ParseWhileStatement();
            
            case TokenType.For: 
                return ParseForStatement();
            
            case TokenType.Fn or TokenType.Mtd:
                bool isMethod = _current.Type is TokenType.Mtd;
                return ParseFunctionDeclaration(isMethod);
            
            case TokenType.Return:
                return ParseReturnStatement();
            
            case TokenType.Import:
                Token importToken = Eat(TokenType.Import);
                Token moduleToken = Eat(TokenType.StringLiteral); 
                Eat(TokenType.SemiColon);
                return new ImportNode(importToken, moduleToken.Value);

            default:
                Expression expr = ParseExpression();
                
                if (_current.Type == TokenType.Assign)
                {
                    Move();
                    Expression value = ParseExpression();
                    Eat(TokenType.SemiColon);
                    
                    if (expr is VariableNode varNode)
                    {
                        return new AssignmentNode(varNode.Token, value);
                    }
                    
                    if (expr is IndexAccessNode indexAccess)
                    {
                        return new IndexAssignmentNode(indexAccess, value);
                    }

                    throw new Exception($"SyntaxError: Недопустима ціль для присвоєння у {_current.Line}");
                }

                Eat(TokenType.SemiColon);
                return new ExpressionStatementNode(expr);
        }
    }
    
    BlockNode ParseBlock()
    {
        Eat(TokenType.OpenBrace);
        List<Statement> statements = [];
        
        while (_current.Type != TokenType.CloseBrace && _current.Type != TokenType.EndOfFile)
        {
            statements.Add(ParseStatement());
        }

        Eat(TokenType.CloseBrace);
        return new BlockNode(statements);
    }
    
    Statement ParseWhileStatement()
    {
        Token whileToken = Eat(TokenType.While);
        
        Eat(TokenType.OpenParen);
        Expression condition = ParseExpression();
        Eat(TokenType.CloseParen);
        
        BlockNode body = ParseBlock();
        
        return new WhileNode(whileToken, condition, body);
    }

    // for i in 0..10 { ... }
    Statement ParseForStatement()
    {
        Token forToken = Eat(TokenType.For);
        
        Token iterator = Eat(TokenType.Identifier);
        
        Eat(TokenType.In);
        
        Expression start = ParseExpression();
        
        Eat(TokenType.DotDot);
        
        Expression end = ParseExpression();
        
        BlockNode body = ParseBlock();
        
        return new ForNode(forToken, iterator, start, end, body);
    }
    
    Statement ParseIfStatement()
    {
        Token ifToken = Eat(TokenType.If);
        
        Eat(TokenType.OpenParen);
        Expression condition = ParseExpression();
        Eat(TokenType.CloseParen);

        BlockNode trueBlock = ParseBlock();
        Statement? elseBranch = null;
        
        if (_current.Type == TokenType.Else)
        {
            Eat(TokenType.Else);
            
            if (_current.Type == TokenType.If)
            {
                elseBranch = ParseIfStatement(); 
            }
            else
            {
                elseBranch = ParseBlock();
            }
        }

        return new IfNode(ifToken, condition, trueBlock, elseBranch);
    }
    
    Statement ParseVariableDeclaration()
    {
        Token modifier = _current; Move();
        Token name = Eat(TokenType.Identifier);
        
        TypeInfo? typeInfo = null; 
        Expression? value = null;
        
        if (_current.Type == TokenType.ColonAssign) 
        {
            Move();
            value = ParseExpression();
        }
        else
        {
            Eat(TokenType.Colon);
            typeInfo = ParseType(); 

            if (_current.Type == TokenType.Assign) 
            {
                Move();
                value = ParseExpression();
            }
        }
        
        if (modifier.Type == TokenType.Const)
        {
            throw new Exception($"SyntaxError: Константа '{name.Value}' так то має бути ініціалізована одразу.");
        }
        
        Eat(TokenType.SemiColon);
        
        return new VariableDeclarationNode(modifier, name, typeInfo, value);
    }

    Expression ParseExpression()
    {
        return ParseLogicalOr();
    }
    
    Expression ParseLogicalOr()
    {
        Expression left = ParseLogicalAnd();
        while (_current.Type == TokenType.Or)
        {
            Token op = _current; Move();
            Expression right = ParseLogicalAnd();
            left = new BinaryNode(left, op, right);
        }
        return left;
    }

    Expression ParseLogicalAnd()
    {
        Expression left = ParseEquality();
        while (_current.Type == TokenType.And)
        {
            Token op = _current; Move();
            Expression right = ParseEquality();
            left = new BinaryNode(left, op, right);
        }
        return left;
    }

    Expression ParseEquality()
    {
        Expression left = ParseRelational();
        while (_current.Type is TokenType.Equals or TokenType.NotEquals) 
        {
            Token op = _current; Move();
            Expression right = ParseRelational();
            left = new BinaryNode(left, op, right);
        }
        return left;
    }
    
    Expression ParseRelational()
    {
        Expression left = ParseAddictive(); 
        
        while (_current.Type is TokenType.LessThan or TokenType.GreaterThan or 
               TokenType.LessOrEquals or TokenType.GreaterOrEquals)
        {
            Token op = _current; Move();
            Expression right = ParseAddictive();
            left = new BinaryNode(left, op, right);
        }
        return left;
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
        Expression left = ParseUnary();
        
        while (_current.Type is TokenType.Multiply or TokenType.Divide)
        {
            Token operatorToken = _current;
            Move();
            
            Expression right = ParseUnary();
            
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
                Token id = _current; Move();
                
                if (_current.Type == TokenType.Bang)
                {
                    id = new Token(TokenType.Identifier, id.Value + "!", id.Line, id.Column);
                    Move(); // Съедаем '!'
                }
                
                if (_current.Type == TokenType.OpenParen)
                {
                    Move(); // Съедаем '('
                    List<Expression> args = [];
                    if (_current.Type != TokenType.CloseParen)
                    {
                        while (true)
                        {
                            args.Add(ParseExpression());
                            if (_current.Type == TokenType.Comma) Move();
                            else break;
                        }
                    }
                    Eat(TokenType.CloseParen);
                    
                    return new FunctionCallNode(id, args);
                }
                
                return new VariableNode(id, id.Value);
            
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
            
            case TokenType.BooleanLiteral:
                Move();
                bool boolValue = token.Value.ToUpper() == "TRUE";
                return new BooleanNode(token, boolValue);
            
            case TokenType.OpenBracket:
                Token bracketToken = Eat(TokenType.OpenBracket);
                List<Expression> elements = [];
                
                if (_current.Type != TokenType.CloseBracket)
                {
                    while (true)
                    {
                        elements.Add(ParseExpression());
                        if (_current.Type == TokenType.Comma) Move();
                        else break;
                    }
                }
                Eat(TokenType.CloseBracket);
                return new ListLiteralNode(bracketToken, elements);

            default:
                throw new Exception($"SyntaxError: Unexpected token {_current.Type} at line {_current.Line}. Awaited expression.");
        }
    }
    
    Statement ParseFunctionDeclaration(bool isMethod = false)
    {
        Token keywordToken = _current;
        Move();
        Token name = Eat(TokenType.Identifier);
        
        Eat(TokenType.OpenParen);
        List<Parameter> parameters = [];
        
        if (_current.Type != TokenType.CloseParen)
        {
            while (true)
            {
                Token paramName = Eat(TokenType.Identifier);
                Eat(TokenType.Colon);
                
                var paramType = ParseType();
                
                parameters.Add(new Parameter(paramName, paramType));
                
                if (_current.Type == TokenType.Comma) Move();
                else break;
            }
        }
        Eat(TokenType.CloseParen);
        
        TypeInfo? returnType = null;
        if (_current.Type == TokenType.Arrow) // ->
        {
            Eat(TokenType.Arrow);
            returnType = ParseType();
        }

        BlockNode body = ParseBlock();
        return new FunctionDeclarationNode(keywordToken, name, parameters, returnType!, body, isMethod);
    }

    Statement ParseReturnStatement()
    {
        Token returnToken = Eat(TokenType.Return);
        Expression? value = null;
        
        if (_current.Type != TokenType.SemiColon)
        {
            value = ParseExpression();
        }
        
        Eat(TokenType.SemiColon);
        return new ReturnNode(returnToken, value);
    }
    
    TypeInfo ParseType()
    {
        Token baseType = _current; Move();
        
        if (baseType.Type == TokenType.TypeList)
        {
            Eat(TokenType.Pipe);
            Token elementType = _current; Move();
            return new TypeInfo(baseType, elementType);
        }
        
        return new TypeInfo(baseType);
    }
    
    Expression ParsePostfix()
    {
        Expression expr = ParsePrimary();
        
        while (true)
        {
            if (_current.Type == TokenType.OpenBracket)
            {
                Move();
                Expression index = ParseExpression();
                Eat(TokenType.CloseBracket);
                
                expr = new IndexAccessNode(expr, index);
            }
            else if (_current.Type == TokenType.Dot)
            {
                Move();
                
                Token methodName = Eat(TokenType.Identifier);
                string methodNameString = methodName.Value;
                
                if (_current.Type == TokenType.Bang)
                {
                    Move();
                    methodNameString += "!";
                }
                
                Eat(TokenType.OpenParen);
                
                List<Expression> arguments = [];
                if (_current.Type != TokenType.CloseParen)
                {
                    while (true)
                    {
                        arguments.Add(ParseExpression());
                        if (_current.Type == TokenType.Comma) Move();
                        else break;
                    }
                }
                Eat(TokenType.CloseParen);
                
                Token finalNameToken = methodName with { Value = methodNameString };
                expr = new MethodCallNode(expr, finalNameToken, arguments);
            }
            else
            {
                break;
            }
        }

        return expr;
    }
    
    Expression ParseUnary()
    {
        if (_current.Type == TokenType.Tilde)
        {
            Token op = _current; 
            Move();
            
            Expression right = ParseUnary(); 
            return new UnaryOperationNode(op, right);
        }
        
        return ParsePostfix(); 
    }
}