using BychkovScript.Core.Lexing;
using BychkovScript.Core.Parsing;
using BychkovScript.Core.Runtime;
using Environment = BychkovScript.Core.Runtime.Environment;

namespace BychkovScript.CLI;

public class ScriptRunner
{
    readonly Interpreter _interpreter;
    readonly Environment _globalEnv;

    public ScriptRunner()
    {
        _globalEnv = new Environment();
        NativeLibrary.Register(_globalEnv);
        _interpreter = new Interpreter(_globalEnv);
    }

    public void RunFromFiles(string mainScriptPath, string stdLibDirectory)
    {
        try
        {
            if (Directory.Exists(stdLibDirectory))
            {
                var stdFiles = Directory.GetFiles(stdLibDirectory, "*.bs");
                foreach (var file in stdFiles)
                {
                    ExecuteFile(file);
                }
            }
            
            if (File.Exists(mainScriptPath))
            {
                ExecuteFile(mainScriptPath);
            }
            else
            {
                Console.WriteLine($"Error: Файл {mainScriptPath} не знайдено.");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }

    void ExecuteFile(string path)
    {
        string code = File.ReadAllText(path);
        var lexer = new Lexer(code);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        
        _interpreter.Evaluate(program);
    }
}