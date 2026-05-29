mod errors;
mod lexer;
mod ast;
//mod parser;
use lexer::Lexer;

fn main() {
    let source = "
    let x = 42 // some var\n
    let y = /* comment block */ 10\n
    /// doc comment";

    let mut lex = Lexer::new(source);
    let tokens = lex.tokenize();

    for tok in &tokens {
        println!("{:?}", tok);
    }

}
