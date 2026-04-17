using BychkovScript.Core.Lexing;
using BychkovScript.Core.Parsing;
using BychkovScript.Core.Runtime;
using Environment = BychkovScript.Core.Runtime.Environment;

// Code
const string sourceCode = @"
let a: float = 10;
let b: int = 7;
const result: int = a + b * 2;

result = 1;

print!(""The result is:"");
print!(result);

const greeting: string = ""Hello from BychkovScript!"";
print!(greeting);
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