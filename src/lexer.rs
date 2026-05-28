use crate::errors::LexerError;

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
    True,
    False,

    // Operators
    Plus,       // +
    Minus,      // -
    Star,       // *
    Slash,      // /
    Eq,         // =
    EqEqEq,     // ===
    Bang,       // !
    NotEqEq,    // !==
    Lt,         // <
    Gt,         // >

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

    // Eof
    Eof,
}

pub struct Lexer {
    input: Vec<char>, // src code
    pos: usize,       // idx
    line: usize,
    col: usize,
    indent_stack: Vec<usize>,
}
impl Lexer {
    pub fn new(source: &str) -> Self {
        Lexer {
            input: source.chars().collect(),
            pos: 0,
            line: 1,
            col: 1,
            indent_stack: vec![0],
        }
    }

    fn make_token(&self, kind: TokenKind) -> Token {
        Token { kind, line: self.line, col: self.col }
    }

    fn next_token(&mut self) -> Token {
        while let Some(c) = self.peek() {
            if c == ' ' || c == '\t' {
                self.advance();
            }
            else {
                break;
            }
        }

        match self.peek() {
            // one char tokens
            Some('+') => { self.advance(); self.make_token(TokenKind::Plus) }
            Some('*') => { self.advance(); self.make_token(TokenKind::Star) }
            Some('(') => { self.advance(); self.make_token(TokenKind::LParen) }
            Some(')') => { self.advance(); self.make_token(TokenKind::RParen) }
            Some(':') => { self.advance(); self.make_token(TokenKind::Colon) }
            Some(',') => { self.advance(); self.make_token(TokenKind::Comma) }

            // double char tokens
            Some('-') => {
                self.advance();
                if self.peek() == Some('>') {
                    self.advance();
                    self.make_token(TokenKind::Arrow) // ->
                } else {
                    self.make_token(TokenKind::Minus) // -
                }
            }
            Some('.') => {
                self.advance();
                if self.peek() == Some('.') {
                    self.advance();
                    self.make_token(TokenKind::DotDot) // ..
                } else {
                    self.make_token(TokenKind::Dot) // .
                }
            }

            // triple char tokens
            Some('=') => {
                self.advance();
                if self.peek() == Some('=') {
                    self.advance();
                    if self.peek() == Some('=') {
                        self.advance();
                        self.make_token(TokenKind::EqEqEq) // ===
                    } else {
                        panic!("{}", LexerError::double_eq(self.line, self.col))
                    }
                } else {
                    self.make_token(TokenKind::Eq)
                }
            }
            Some('!') => {
                self.advance();
                if self.peek() == Some('=') {
                    self.advance();
                    if self.peek() == Some('=') {
                        self.advance();
                        self.make_token(TokenKind::NotEqEq) // !==
                    } else {
                        panic!("{}", LexerError::double_not_eq(self.line, self.col))
                    }
                } else {
                    self.make_token(TokenKind::Bang)
                }
            }

            // indent
            Some('\n') => {
                self.advance();
                let tok = self.make_token(TokenKind::Newline);
                tok
            }

            // numbers
            Some(c) if c.is_ascii_digit() => self.read_number(),

            // identifiers/kwords
            Some(c) if c.is_alphabetic() || c == '_' => self.read_ident(),

            // eof
            None => self.make_token(TokenKind::Eof),

            // unknown
            Some(c) => {
                let c = c;
                self.advance();
                panic!("{}", LexerError::unknown_char(c, self.line, self.col))
            }
        }
    }

    pub fn tokenize(&mut self) -> Vec<Token> {
        let mut tokens = Vec::new();

        loop {
            let tok = self.next_token();
            let is_eof = tok.kind == TokenKind::Eof;
            tokens.push(tok);
            if is_eof { break; }
        }

        tokens
    }

    fn peek(&self) -> Option<char> {
        self.input.get(self.pos).copied()
    }

    fn peek_next(&self) -> Option<char> {
        self.input.get(self.pos + 1).copied()
    }

    fn advance(&mut self) -> Option<char> {
        let ch = self.input.get(self.pos).copied();
        if let Some(c) = ch {
            self.pos += 1;
            if c == '\n' {
                self.line += 1;
                self.col = 1;
            } else {
                self.col += 1;
            }
        }
        ch
    }

    fn read_number(&mut self) -> Token {
        let mut num = String::new();
        let mut is_float = false;

        while let Some(c) = self.peek() {
            if c.is_ascii_digit() {
                num.push(c);
                self.advance();
            } else if c == '.' && self.peek_next() != Some('.') {
                is_float = true;
                num.push(c);
                self.advance();
            } else {
                break;
            }
        }

        if is_float {
            let f: f64 = num.parse().unwrap();
            self.make_token(TokenKind::Float(f))
        } else {
            let i: i64 = num.parse().unwrap();
            self.make_token(TokenKind::Int(i))
        }
    }

    fn read_ident(&mut self) -> Token {
        let mut ident = String::new();

        while let Some(c) = self.peek() {
            if c.is_alphanumeric() || c == '_' {
                ident.push(c);
                self.advance();
            } else {
                break;
            }
        }

        // kword or identifier
        let kind = match ident.as_str() {
            "let"   => TokenKind::Let,
            "const" => TokenKind::Const,
            "fn"    => TokenKind::Fn,
            "ret"   => TokenKind::Ret,
            "if"    => TokenKind::If,
            "else"  => TokenKind::Else,
            "while" => TokenKind::While,
            "TRUE"  => TokenKind::Bool(true),
            "FALSE"  => TokenKind::Bool(false),
            _       => TokenKind::Ident(ident),
        };

        self.make_token(kind)
    }
}
