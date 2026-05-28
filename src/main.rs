mod lexer;
mod errors;
use lexer::{Lexer};

fn main() {
    let source = "let x = 42";
    let mut lex = Lexer::new(source);
    let tokens = lex.tokenize();

    for tok in &tokens {
        println!("{:?}", tok);
    }
}