using BychkovScript.Core.Lexing;
using BychkovScript.Core.Parsing;
using BychkovScript.Core.Runtime;
using Environment = BychkovScript.Core.Runtime.Environment;

namespace BychkovScript.CLI;

public class ScriptRunner
{
    public static void ExecuteFile(string filePath)
    {
        try
        {
            string sourceCode = File.ReadAllText(filePath);
            
            var lexer = new Lexer(sourceCode);
            var parser = new Parser(lexer);
            var ast = parser.ParseProgram();
            
            var globalEnv = new Environment();
            var interpreter = new Interpreter(globalEnv);
            
            interpreter.OnImport = (moduleName) => 
            {
                string dir = Path.GetDirectoryName(filePath) ?? "";
                string importPath = Path.Combine(dir, moduleName + ".bs");
                if (!File.Exists(importPath))
                    throw new Exception($"ImportError: Модуль '{moduleName}' не знайдено!");
                
                string importSource = File.ReadAllText(importPath);
                var importLexer = new Lexer(importSource);
                var importParser = new Parser(importLexer);
                interpreter.Evaluate(importParser.ParseProgram());
            };
            
            interpreter.Evaluate(ast);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }
}