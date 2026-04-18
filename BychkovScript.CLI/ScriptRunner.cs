using BychkovScript.Core.Lexing;
using BychkovScript.Core.Parsing;
using BychkovScript.Core.Runtime;
using Environment = BychkovScript.Core.Runtime.Environment;

namespace BychkovScript.CLI;

public class ScriptRunner
{
    readonly Interpreter _interpreter;
    readonly Environment _globalEnv;
    readonly string _baseDirectory;

    public ScriptRunner()
    {
        _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _globalEnv = new Environment();
        NativeLibrary.Register(_globalEnv);
        
        _interpreter = new Interpreter(_globalEnv) 
        {
            OnImport = ResolveAndExecuteImport
        };

    }

    public void RunMain(string mainScriptPath)
    {
        try
        {
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
    
    void ResolveAndExecuteImport(string moduleName)
    {
        string localPath = Path.Combine(_baseDirectory, moduleName);
        if (File.Exists(localPath))
        {
            ExecuteFile(localPath);
            return;
        }
        
        string stdLibPath = Path.Combine(_baseDirectory, "stdlib", moduleName + ".bs");
        if (File.Exists(stdLibPath))
        {
            ExecuteFile(stdLibPath);
            return;
        }

        throw new Exception($"ImportError: Не вдалося знайти ніякого '{moduleName}'.");
    }
}