namespace BychkovScript.Core.Runtime;

public static class NativeLibrary
{
    public static void Register(Environment env)
    {
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
        
        env.DeclareVariable("toInt!", new Interpreter.NativeFunction(1, args => 
        {
            if (args[0] is string strVal && double.TryParse(strVal, out double parsedVal))
            {
                return parsedVal;
            }
            throw new Exception("RuntimeError: Не вдалося конвертувати строку в число.");
        }), isConstant: true);
    }
}