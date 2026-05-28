mod lexer;
mod errors;
use lexer::{Lexer};

fn main() {
    let source = r#"let x = "bychkov""#;
    let mut lex = Lexer::new(source);
    let tokens = lex.tokenize();

    for tok in &tokens {
        println!("{:?}", tok);
    }
}