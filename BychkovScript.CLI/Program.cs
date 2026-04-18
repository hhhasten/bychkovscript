using BychkovScript.Core.Lexing;
using BychkovScript.Core.Parsing;
using BychkovScript.Core.Runtime;
using Environment = BychkovScript.Core.Runtime.Environment;

// Code
const string sourceCode = @"

#   NEW NATIVE FUNCTIONS: input!(), cls!(), wait!()
#   new void type
#   validation on function declaration
#

fn sayHello(name: str) -> int {
    print!(""Hello, "" + name + ""!"");
    return 1;
}
fn average(a: int, b: int) -> float {
    let result: float = (a + b) / 2;
    return result;
}

print!(""Who are you?"");
let name: str = input!();

print!(""wait 1 sec..."");
wait!(1000); 
cls!();

sayHello(name);

let status: int = sayHello(name);

print!(""Welcome to bychkovscript"");

let a: int = 5;
let b: int = 10;

print!(average(a, b));

print!(status);

";

try
{
    Lexer lexer = new Lexer(sourceCode);
    
    Parser parser = new Parser(lexer);
    var programNode = parser.ParseProgram();
    
    Environment env = new Environment();

    env.DeclareVariable("print!", new Interpreter.NativeFunction(1, args => 
    {
        Console.WriteLine(args[0]);
        return null;
    }), isConstant: true);

    env.DeclareVariable("input!", new Interpreter.NativeFunction(0, args => 
    {
        return Console.ReadLine() ?? "";
    }), isConstant: true);

    env.DeclareVariable("cls!", new Interpreter.NativeFunction(0, args => 
    {
        Console.Clear();
        return null;
    }), isConstant: true);

    env.DeclareVariable("wait!", new Interpreter.NativeFunction(1, args => 
    {
        if (args[0] is double ms) Thread.Sleep((int)ms);
        return null;
    }), isConstant: true);

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