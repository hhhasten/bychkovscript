namespace BychkovScript.Core.Lexing;

public class Lexer(string source)
{
    int _idx = 0;
    int _line = 1;
    int _column = 1;
    
    char Current => _idx < source.Length ? source[_idx] : '\0';
    
    char Peek => _idx + 1 < source.Length ? source[_idx + 1] : '\0';
    char PeekTwo => _idx + 2 < source.Length ? source[_idx + 2] : '\0';
    
    readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "fn", TokenType.Fn },
        { "mtd", TokenType.Mtd },
        { "let", TokenType.Let },
        { "const", TokenType.Const },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "while", TokenType.While },
        { "for", TokenType.For },
        { "and", TokenType.And },
        { "or", TokenType.Or },
        { "int", TokenType.TypeInt32 },
        { "float", TokenType.TypeFloat32 },
        { "str", TokenType.TypeString },
        { "boolean", TokenType.TypeBoolean},
        { "void", TokenType.TypeVoid},
        { "list", TokenType.TypeList},
        { "trashcan", TokenType.TypeTrashcan},
        { "TRUE", TokenType.BooleanLiteral },
        { "FALSE", TokenType.BooleanLiteral },
        { "in", TokenType.In },
        { "return", TokenType.Return },
        { "import", TokenType.Import },
    };

    /// <summary>
    /// Get next Token, main method for Parser.
    /// </summary>
    /// <returns>Token</returns>
    public Token GetNext()
    {
        SkipGarbage();

        if (Current == '\0')
        {
            return new Token(TokenType.EndOfFile, "\0", _line, _column);
        }

        int startLine = _line;
        int startColumn = _column;
        char c = Current;
        
        if (char.IsLetter(c) || c == '_')
        {
            return ReadIdentifier(c, startLine, startColumn);
        }
        
        if (char.IsDigit(c))
        {
            return ReadNumber(c, startLine, startColumn);
        }
        
        if (c == '"')
        {
            return ReadString(c, startLine, startColumn);
        }

        switch (c)
        {
            case '+': Move(); return new Token(TokenType.Plus, "+", startLine, startColumn);
            case '*': Move(); return new Token(TokenType.Multiply, "*", startLine, startColumn);
            case '/': Move(); return new Token(TokenType.Divide, "/", startLine, startColumn);
            
            case '(': Move(); return new Token(TokenType.OpenParen, "(", startLine, startColumn);
            case ')': Move(); return new Token(TokenType.CloseParen, ")", startLine, startColumn);
            case '{': Move(); return new Token(TokenType.OpenBrace, "{", startLine, startColumn);
            case '}': Move(); return new Token(TokenType.CloseBrace, "}", startLine, startColumn);
            
            case ';': Move(); return new Token(TokenType.SemiColon, ";", startLine, startColumn);
            case ':': Move(); return new Token(TokenType.Colon, ":", startLine, startColumn);
            case ',': Move(); return new Token(TokenType.Comma, ",", startLine, startColumn);
            
            case '[': Move(); return new Token(TokenType.OpenBracket, "[", startLine, startColumn);
            case ']': Move(); return new Token(TokenType.CloseBracket, "]", startLine, startColumn);
            case '|': Move(); return new Token(TokenType.Pipe, "|", startLine, startColumn);
            
            case '~': Move(); return new Token(TokenType.Tilde, "~", startLine, startColumn);
            
            case '-':
                if (Peek == '>')
                {
                    Move(); Move();
                    return new Token(TokenType.Arrow, "->", startLine, startColumn);
                }
                Move();
                return new Token(TokenType.Minus, "-", startLine, startColumn);
            
            case '=':
                if (Peek == '=' && PeekTwo == '=')
                {
                    Move(); Move(); Move();
                    return new Token(TokenType.Equals, "===", startLine, startColumn);
                }
                if (Peek == '>')
                {
                    Move(); Move();
                    return new Token(TokenType.Lambda, "=>", startLine, startColumn);
                }
                Move();
                return new Token(TokenType.Assign, "=", startLine, startColumn);
            
            case '<':
                if (Peek == '=')
                {
                    Move(); Move();
                    return new Token(TokenType.LessOrEquals, "<=", startLine, startColumn);
                }
                Move();
                return new Token(TokenType.LessThan, "<", startLine, startColumn);
            
            case '>':
                if (Peek == '=')
                {
                    Move(); Move();
                    return new Token(TokenType.GreaterOrEquals, ">=", startLine, startColumn);
                }
                Move();
                return new Token(TokenType.GreaterThan, ">", startLine, startColumn);
            
            case '.':
                if (Peek == '.')
                {
                    Move(); Move();
                    return new Token(TokenType.DotDot, "..", startLine, startColumn);
                }
                Move();
                return new Token(TokenType.Dot, ".", startLine, startColumn);
            
            case '!':
                if (Peek == '=' && PeekTwo == '=')
                {
                    Move(); Move(); Move();
                    return new Token(TokenType.NotEquals, "!==", startLine, startColumn);
                }
                Move();
                return new Token(TokenType.Bang, "!", startLine, startColumn);
        }
        
        Move();
        
        return new Token(TokenType.BadChar, c.ToString(), startLine, startColumn);
    }
    
    /// <summary>
    /// Skip whitespaces and ignore comments
    /// </summary>
    void SkipGarbage()
    {
        while (true)
        {
            if (char.IsWhiteSpace(Current))
            {
                Move();
            }
            else if (Current == '#')
            {
                while (Current != '\n' && Current != '\0')
                {
                    Move();
                }
            }
            
            else
            {
                break;
            }
        }
    }
    
    /// <summary>
    /// Move pointer to the next character,
    /// Update line and column
    /// </summary>
    void Move()
    {
        if (Current == '\n')
        {
            _line++;
            _column = 1;    
        }
        else
        {
            _column++;
        }
        
        _idx++;
    }
    
    /// <summary>
    /// Reading variable names and keywords
    /// </summary>
    Token ReadIdentifier(char c, int startLine, int startColumn)
    {
        string text = "";
        while (char.IsLetterOrDigit(Current) || Current == '_')
        {
            text += Current;
            Move();
        }

        // Check if text is keyword
        if (_keywords.TryGetValue(text, out TokenType type))
        {
            return new Token(type, text, startLine, startColumn);
        }
            
        return new Token(TokenType.Identifier, text, startLine, startColumn);
    }
    
    /// <summary>
    /// // Reading numbers
    /// </summary>
    Token ReadNumber(char c, int startLine, int startColumn)
    {
        string numStr = "";
        bool isFloat = false;

        while (char.IsDigit(Current) || Current == '.')
        {
            if (Current == '.')
            {
                if (!char.IsDigit(Peek))
                {
                    break;
                }

                if (isFloat)
                {
                    break;
                }
                
                isFloat = true;
            }
            numStr += Current;
            Move();
        }
            
        TokenType type = isFloat ? TokenType.FloatLiteral : TokenType.IntLiteral;
            
        return new Token(type, numStr, startLine, startColumn);
    }
    
    /// <summary>
    /// Reading strings
    /// </summary>
    Token ReadString(char c, int startLine, int startColumn)
    {
        Move();
        
        string str = "";

        while (Current != '"' && Current != '\0')
        {
            str += Current;
            Move();
        }

        if (Current == '"')
        {
            Move();
        }
            
        return new Token(TokenType.StringLiteral, str, startLine, startColumn);
    }
}