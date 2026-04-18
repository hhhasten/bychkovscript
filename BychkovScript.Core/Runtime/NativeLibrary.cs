namespace BychkovScript.Core.Runtime;

public static class NativeLibrary
{
    public static void Register(Environment env)
    {
        env.DeclareVariable("print!", new Interpreter.NativeFunction(1, args => 
        {
            Console.Write(args[0]);
            return null;
        }), isConstant: true);
        
        env.DeclareVariable("println!", new Interpreter.NativeFunction(1, args => 
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
            if (args[0] is string strVal && int.TryParse(strVal, out int parsedVal))
            {
                return parsedVal;
            }
            throw new Exception("RuntimeError: Не вдалося конвертувати str в int.");
        }), isConstant: true);
        env.DeclareVariable("toFloat!", new Interpreter.NativeFunction(1, args => 
        {
            if (args[0] is string strVal && float.TryParse(strVal, out float parsedVal))
            {
                return parsedVal;
            }
            throw new Exception("RuntimeError: Не вдалося конвертувати str в float.");
        }), isConstant: true);
    }
}