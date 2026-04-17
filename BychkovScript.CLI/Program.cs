using BychkovScript.Core.Lexing;

// LEXER TEST
const string sourceCode = """

                          # COMMENT
                          fn calculate_sum(a: int, b: int) -> int {
                              let result = a + b;
                              return result;
                          }

                          const PI: float = 3.14;
                          let message: string = "Hello world!";

                          if (PI === 3.14 and TRUE) {
                              print!(message);
                          }

                          """;


Lexer lexer = new Lexer(sourceCode);

while (true)
{
    Token token = lexer.GetNext();
    
    Console.WriteLine(token);
    
    if (token.Type is TokenType.EndOfFile or TokenType.BadChar)
    {
        break;
    }
}