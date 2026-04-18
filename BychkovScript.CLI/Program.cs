using BychkovScript.Core.Lexing;
using BychkovScript.Core.Parsing;
using BychkovScript.Core.Runtime;
using Environment = BychkovScript.Core.Runtime.Environment;

// Code
const string sourceCode = @"

let count: int = 0;

if (count < 10) {
    print!(""bad"");
} else if (count >= 10 and count < 20) {
    print!(""ok"");
} else {
    print!(""excellent!"");
}
";

try
{
    Lexer lexer = new Lexer(sourceCode);
    
    Parser parser = new Parser(lexer);
    var programNode = parser.ParseProgram();
    
    Environment env = new Environment();
    Interpreter interpreter = new Interpreter(env);
    
    Console.WriteLine("Script start\n");
    
    interpreter.Evaluate(programNode);
    
    Console.WriteLine("\nScript end");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.Message);
    Console.ResetColor();
}