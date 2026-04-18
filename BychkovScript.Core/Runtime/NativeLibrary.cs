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
        
        env.DeclareVariable("len!", new Interpreter.NativeFunction(1, args => 
        {
            if (args[0] is List<object?> list)
            {
                return (double)list.Count;
            }
            if (args[0] is string str)
            {
                return (double)str.Length;
            }
        
            throw new Exception("RuntimeError: Метод len! можна викликати тільки для списків та рядків.");
        }, IsMethod: true), isConstant: true);
        
        env.DeclareVariable("push!", new Interpreter.NativeFunction(2, args => 
        {
            if (args[0] is List<object?> list)
            {
                list.Add(args[1]);
                return null; // void
            }
            throw new Exception("RuntimeError: Метод push! можна викликати тільки для списків.");
        }, IsMethod: true), isConstant: true);
        
        env.DeclareVariable("pop!", new Interpreter.NativeFunction(1, args => 
        {
            if (args[0] is List<object?> list)
            {
                if (list.Count == 0) 
                    throw new Exception("RuntimeError: Спроба викликати pop!() для порожнього списку.");
            
                int lastIndex = list.Count - 1;
                object? removedElement = list[lastIndex];
                list.RemoveAt(lastIndex);
                return removedElement;
            }
            throw new Exception("RuntimeError: Метод pop! можна викликати тільки для списків.");
        }, IsMethod: true), isConstant: true);
        
        env.DeclareVariable("removeAt!", new Interpreter.NativeFunction(2, args => 
        {
            if (args[0] is List<object?> list)
            {
                if (args[1] is double indexDouble)
                {
                    int index = (int)indexDouble;
                    if (index < 0 || index >= list.Count)
                        throw new Exception($"RuntimeError: Індекс {index} поза межами списку ({list.Count}).");
                
                    list.RemoveAt(index);
                    return null;
                }
                throw new Exception("TypeError: Індекс для removeAt! має бути числом.");
            }
            throw new Exception("RuntimeError: Метод removeAt! можна викликати тільки для списків.");
        }, IsMethod: true), isConstant: true);
        
        env.DeclareVariable("randInt!", new Interpreter.NativeFunction(2, args =>
        {
            if (args[0] is double min && args[1] is double max)
            {
                int result = Random.Shared.Next((int)min, (int)max);
                return (double)result;
            }
            throw new Exception("RuntimeError: Аргументами цієї функції повинні бути значення типу int.");
        }), isConstant: true);
        
        env.DeclareVariable("randSingle!", new Interpreter.NativeFunction(0, args 
            => Random.Shared.NextSingle()), isConstant: true);
        
        env.DeclareVariable("contains!", new Interpreter.NativeFunction(2, args => 
        {
            if (args[0] is List<object?> list)
            {
                return list.Contains(args[1]);
            }
            if (args[0] is string str && args[1] is string sub)
            {
                return str.Contains(sub);
            }
        
            throw new Exception("RuntimeError: contains! працює тільки зі списками або рядками.");
        }, IsMethod: true), isConstant: true);
    }
}