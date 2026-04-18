namespace BychkovScript.Core.Lexing;

public enum TokenType
{
    // Literals and Identifiers ---------------------------------
    
    Identifier,     // Names of variables, functions etc
    IntLiteral,     // int values: 10, 20, 255
    StringLiteral,  // string "Hello world!"
    BooleanLiteral, // True/False
    FloatLiteral,   // 23.33, 125.50
    
    // Key Words -------------------------------------------------
    
    Fn, Let, Const,         // Declarations
    If, Else, While, For,   // Conditions
    And, Or,                // Logic operators 'and', 'or'
    In, DotDot, Dot,        // in, .., .
    Return,                 // return
    
    // Value types -----------------------------------------------
    
    TypeInt32,      // int
    TypeFloat32,    // float
    TypeString,     // str
    TypeBoolean,    // boolean
    TypeVoid,       // void
    
    // Operators -------------------------------------------------
    
    Assign,             // =
    Plus, Minus,        // +, -
    Multiply, Divide,   // '*', /
    Equals,             // ===
    LessThan,           // <
    GreaterThan,        // >
    LessOrEquals,       // <=
    GreaterOrEquals,    // >=
    
    // Syntax ----------------------------------------------------
    
    Arrow,                      // ->
    Lambda,                     // =>
    Bang,                       // '!'
    OpenParen, CloseParen,      // ( some shit )
    OpenBrace, CloseBrace,      // { some shit }
    Comma,                      // ,
    Colon,                      // :
    SemiColon,                  // ;
    
    // Special ---------------------------------------------------
    
    EndOfFile,      // yk
    BadChar,        // Forbidden chars for exceptions
}