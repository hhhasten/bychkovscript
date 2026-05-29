mod errors;
mod lexer;
use lexer::Lexer;

fn main() {
    let source = 
    "if x > 0:\n    
        let y = 1\n    
        let z = 2\n
        let w = 3";

    let mut lex = Lexer::new(source);
    let tokens = lex.tokenize();

    for tok in &tokens {
        println!("{:?}", tok);
    }
}
