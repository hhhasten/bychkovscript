#[derive(Debug, Clone)]
pub struct Token {
    pub kind: TokenKind,

    // for debug
    pub line: usize,
    pub col: usize,
}

#[derive(Debug, Clone, PartialEq)]
pub enum TokenKind {
    // ------ Literals ------------------------------
    Int(i64),
    Float(f64),
    Str(String),
    Bool(bool),

    // Identifiers and keywords
    Ident(String),
    Let,
    Const,
    Fn,
    Ret,
    If,
    Else,
    While,
    For,

    // Operators
    Plus,       // +
    Minus,      // -
    Star,       // *
    Slash,      // /
    Eq,         // =
    ColonEq,    // :=
    EqEqEq,     // ===
    Bang,       // !
    NotEqEq,    // !==
    Lt,         // <
    Gt,         // >
    LtEq,       // <=
    GtEq,       // >=

    // Dividers
    LParen, // (
    RParen, // )
    Colon,  // :
    Comma,  // ,
    Dot,    // .
    DotDot, // ..
    Arrow,  // ->
    Newline,
    Indent,
    Dedent,

    // Comments
    Comment(String),        // //
    DocComment(String),     // /// doc comment

    // Eof
    Eof,
}