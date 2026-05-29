// types
#[derive(Debug, Clone, PartialEq)]
pub enum Type {
    // integers
    I8, I16, I32, I64,
    // integers unsigned
    U8, U16, U32, U64,
    // floating point
    F32, F64,
    // aliases 
    Int,    // = i64
    Uint,   // = u64
    Float,  // = f64
    // others
    Bool, Str, Char, Unit,  // unit = ()
}

// literals
#[derive(Debug, Clone, PartialEq)]
pub enum Literal {
    Int(i64),
    Float(f64),
    Str(String),
    Bool(bool),
    Char(char),
}

// operators
#[derive(Debug, Clone, PartialEq)]
pub enum BinOp {
    Add,    // +
    Sub,    // -
    Mul,    // *
    Div,    // /
    EqEqEq, // ===
    NotEqEq,// !==
    Lt,     // 
    Gt,     // >
    LtEq,   // <=
    GtEq,   // >=
    And,    // and
    Or,     // or
}
#[derive(Debug, Clone, PartialEq)]
pub enum UnaryOp {
    Neg,  // -x
    Not,  // !x
}

// expressions
#[derive(Debug, Clone, PartialEq)]
pub enum Expr {
    Literal(Literal),
    Ident(String),
    BinOp {
        left:   Box<Expr>,
        op:     BinOp,
        right:  Box<Expr>,
    },
    UnaryOp {
        op:     UnaryOp,
        expr:   Box<Expr>,
    },
    Call {
        name:   String,
        args:   Vec<Expr>,
        is_native: bool,
    },
    Range {
        from:   Box<Expr>,
        to:     Box<Expr>,
    },
    Cast {
        expr:   Box<Expr>,
        to:     Type,
    },
    Index {
        expr:   Box<Expr>,
        index:  Box<Expr>,
    },
}

// statements
#[derive(Debug, Clone, PartialEq)]
pub enum Stmt {
    VarDecl {
        name:    String,
        ty:      Option<Type>,
        value:   Option<Expr>,
        is_const: bool,
    },
    Assign {
        name:  String,
        value: Expr,
    },
    If {
        condition:  Expr,
        then_block: Block,
        else_block: Option<Block>,
    },
    ForIn {
        var:  String,
        iter: Expr,
        body: Block,
    },
    While {
        condition: Expr,
        body:      Block,
    },
    Ret(Option<Expr>),
    ExprStmt(Expr),
}

// block and function
pub type Block = Vec<Stmt>;

#[derive(Debug, Clone, PartialEq)]
pub struct Param {
    pub name: String,
    pub ty:   Type,
}

#[derive(Debug, Clone, PartialEq)]
pub struct FnDecl {
    pub name:    String,
    pub params:  Vec<Param>,
    pub ret_ty:  Type,
    pub body:    Block,
}

// program (root)
#[derive(Debug, Clone, PartialEq)]
pub struct Program {
    pub functions: Vec<FnDecl>,
}