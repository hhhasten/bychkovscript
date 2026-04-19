using BychkovScript.Core.AST;
using BychkovScript.Core.Lexing;

namespace BychkovScript.Core.Runtime;

public class Interpreter(Environment env)
{
    Environment Env { get; set; } = env;
    
    public Action<string>? OnImport { get; set; }
    
    public object? Evaluate(Node node)
    {
        return node switch
        {
            ProgramNode program => EvaluateProgram(program),
            
            NumberNode n => n.Value,
            StringNode s => s.Value,
            
            VariableNode v => Env.GetVariable(v.Name),
            
            BinaryNode b => EvaluateBinary(b),
            
            VariableDeclarationNode vDecl => EvaluateVariableDeclaration(vDecl),
            
            BlockNode b => EvaluateBlock(b),
            IfNode i => EvaluateIf(i),
            BooleanNode boolNode => boolNode.Value,
            
            WhileNode w => EvaluateWhile(w),
            ForNode f => EvaluateFor(f),
            
            FunctionDeclarationNode fDecl => EvaluateFunctionDeclaration(fDecl),
            FunctionCallNode fCall => EvaluateFunctionCall(fCall),
            MethodCallNode methodCall => EvaluateMethodCall(methodCall),
            ReturnNode ret => EvaluateReturn(ret),
            
            AssignmentNode a => EvaluateAssignment(a),
            
            ExpressionStatementNode exprStmt => EvaluateExpressionStatement(exprStmt),
            
            ImportNode imp => EvaluateImport(imp),
            
            ListLiteralNode listNode => EvaluateListLiteral(listNode),
            IndexAccessNode indexNode => EvaluateIndexAccess(indexNode),
            
            IndexAssignmentNode indexAssign => EvaluateIndexAssignment(indexAssign),
            
            UnaryOperationNode unary => EvaluateUnaryOperation(unary),
            
            _ => throw new Exception($"RuntimeError: Я поняття не маю, що це за вузол {node.GetType().Name}. " +
                                     $"Сходи подихай свіжим повітрям і перевір AST.")
        };
    }
    
    object? EvaluateProgram(ProgramNode program)
    {
        object? lastEvaluated = null;
        foreach (var statement in program.Statements)
        {
            lastEvaluated = Evaluate(statement);
        }
        return lastEvaluated;
    }
    
    object? EvaluateVariableDeclaration(VariableDeclarationNode node)
    {
        object? evalValue = null;
        
        if (node.Value != null)
        {
            evalValue = Evaluate(node.Value);
            
            if (node.DataType != null) 
            {
                ValidateType(node.DataType, evalValue);
            }
        }
        
        bool isConst = node.Modifier.Type == TokenType.Const;
        
        Env.DeclareVariable(node.Identifier.Value, evalValue, isConst);
        
        return null;
    }
    
    object EvaluateBinary(BinaryNode node)
    {
        object? left = Evaluate(node.Left);
        object? right = Evaluate(node.Right);
        
        if (node.Operator.Type is TokenType.And or TokenType.Or)
        {
            if (left is bool lBool && right is bool rBool)
            {
                return node.Operator.Type == TokenType.And ? lBool && rBool : lBool || rBool;
            }
            throw new Exception($"TypeError: Логічний оператор '{node.Operator.Value}' хоче boolean. Ти намагаєшся порівняти непорівнюване, як у своєму джаваскріпті");
        }
        
        if (left is double l && right is double r)
        {
            return node.Operator.Type switch
            {
                TokenType.Plus => l + r,
                TokenType.Minus => l - r,
                TokenType.Multiply => l * r,
                TokenType.Divide => l / r,
                TokenType.LessThan => l < r,
                TokenType.GreaterThan => l > r,
                TokenType.LessOrEquals => l <= r,
                TokenType.GreaterOrEquals => l >= r,
                TokenType.Equals => Math.Abs(l - r) < 1e-9,
                TokenType.NotEquals => Math.Abs(l - r) > 1e-9,
                _ => throw new Exception($"RuntimeError: Навіщо ти пхаєш оператор '{node.Operator.Value}' до чисел?")
            };
        }
        
        if (node.Operator.Type == TokenType.Plus && (left is string || right is string))
        {
            return left?.ToString() + right;
        }
        
        if (node.Operator.Type == TokenType.Equals)
        {
            return Equals(left, right); 
        }
        
        if (node.Operator.Type == TokenType.NotEquals)
        {
            return !Equals(left, right);
        }
        
        string leftType = left?.GetType().Name ?? "null";
        string rightType = right?.GetType().Name ?? "null";
        
        throw new Exception($"JavaScriptFlashbackError: Спроба застосувати '{node.Operator.Value}' до {leftType} та {rightType}. Мені це нагадує псевдомову на букву J");
    }
    
    void ValidateType(TypeInfo typeInfo, object? value)
    {
        if (typeInfo.BaseType.Type == TokenType.TypeList)
        {
            if (value is not List<object?> list)
                throw new Exception($"TypeError: Очікувався list, а ти всунув '{value}'. В Rust за таку неповагу до контейнерів тебе б вигнали з IT.");
            
            if (typeInfo.ElementType is not null)
            {
                TypeInfo innerType = new TypeInfo(typeInfo.ElementType);
                foreach (var item in list)
                {
                    ValidateType(innerType, item);
                }
            }
            return;
        }
        
        switch (typeInfo.BaseType.Type)
        {
            case TokenType.TypeString:
                if (value is not string) throw new Exception($"TypeError: Змінна хоче 'str', а ти підсунув '{value}'. Рядок {typeInfo.BaseType.Line}. Ти що, пітоніст?");
                break;
            
            case TokenType.TypeInt32:
                if (value is not double dInt || dInt % 1 != 0) throw new Exception($"TypeError: Очікувався 'int', а прийшло якесь неподобство ({value}). Рядок {typeInfo.BaseType.Line}");
                break;
            
            case TokenType.TypeFloat32:
                if (value is not double) throw new Exception($"TypeError: Де ти тут бачиш 'float'? Те, що ти всунув ({value}), нікуди не лізе. Рядок {typeInfo.BaseType.Line}");
                break;
            
            case TokenType.TypeBoolean:
                if (value is not bool) throw new Exception($"TypeError: Булеве значення - це TRUE або FALSE. '{value}' це твоя хвора фантазія. Рядок {typeInfo.BaseType.Line}");
                break;
            
            case TokenType.TypeTrashcan:
                if (value is not List<object?>) throw new Exception($"TypeError: 'trashcan' - це список. Ти що, не знаєш як виглядає смітник? Рядок {typeInfo.BaseType.Line}");
                break;
            
            case TokenType.TypeVoid:
                if (value is not null) throw new Exception($"TypeError: Ти повернув щось із 'void' функції. Бляха нема слів. Браво.");
                break;
            
            default:
                throw new Exception($"RuntimeError: Де ти знайшов тип '{typeInfo.BaseType.Value}'? Ти його сам вигадав?");
        }
    }
    
    object? EvaluateBlock(BlockNode node)
    {
        foreach (var stmt in node.Statements)
        {
            Evaluate(stmt);
        }
        return null;
    }

    object? EvaluateIf(IfNode node)
    {
        object? condition = Evaluate(node.Condition);
        
        if (condition is not bool b)
        {
            throw new Exception($"TypeError: Умова в 'if' має бути тільки boolean! Досить тягнути звички зі свого смердючого пітону, де все підряд то правда.");
        }
        
        if (b)
        {
            Evaluate(node.TrueBlock);
        }
        else if (node.ElseBranch != null) 
        {
            Evaluate(node.ElseBranch);
        }

        return null;
    }
    
    object? EvaluateWhile(WhileNode node)
    {
        while (true)
        {
            object? condition = Evaluate(node.Condition);
            
            if (condition is not bool b)
                throw new Exception($"TypeError: Цикл 'while' хоче булеву умову, а не цей жах: {condition?.GetType().Name}");
                
            if (!b) break;
            
            Evaluate(node.Body);
        }
        return null;
    }

    object? EvaluateFor(ForNode node)
    {
        object? startObj = Evaluate(node.Start);
        object? endObj = Evaluate(node.End);

        if (startObj is not double startVal || endObj is not double endVal)
            throw new Exception("TypeError: Діапазон циклу 'for' має бути тільки int. Ти ще спробуй по ітератору зі стрінгами пройтися.");
        
        int start = (int)startVal;
        int end = (int)endVal;
        string iteratorName = node.Iterator.Value;
        
        try {
            Env.DeclareVariable(iteratorName, (double)start, isConstant: false);
        } catch {
            Env.AssignVariable(iteratorName, (double)start);
        }
        
        for (int i = start; i < end; i++)
        {
            Env.AssignVariable(iteratorName, (double)i);
            Evaluate(node.Body);
        }

        return null;
    }
    
    object? EvaluateFunctionDeclaration(FunctionDeclarationNode node)
    {
        var function = new BychkovFunction(node, Env);

        Env.DeclareVariable(node.Identifier.Value, function, isConstant: true);
        return null;
    }

    object EvaluateReturn(ReturnNode node)
    {
        object? value = node.Value != null ? Evaluate(node.Value) : null;
        throw new ReturnException(value);
    }
    
    object? EvaluateAssignment(AssignmentNode node)
    {
        object? value = Evaluate(node.Value);
        
        Env.AssignVariable(node.Identifier.Value, value!);
    
        return value;
    }

    object? EvaluateFunctionCall(FunctionCallNode node)
    {
        object? funcObj = Env.GetVariable(node.Identifier.Value);
        
        if (funcObj is NativeFunction native)
        {
            if (native.Arity != -1 && node.Arguments.Count != native.Arity)
                throw new Exception($"RuntimeError: Нативна функція '{node.Identifier.Value}' очікує {native.Arity} аргументів, а не твій хаос із {node.Arguments.Count} штук.");

            List<object?> nArgs = [];
            foreach (var arg in node.Arguments) nArgs.Add(Evaluate(arg));

            return native.Function(nArgs);
        }
        
        if (funcObj is not BychkovFunction func)
            throw new Exception($"TypeError: '{node.Identifier.Value}' - це, шановний, не функція. Не намагайся викликати те, що не викликається.");
        
        if (node.Arguments.Count != func.Declaration.Parameters.Count)
            throw new Exception($"RuntimeError: Функція '{node.Identifier.Value}' хоче {func.Declaration.Parameters.Count} аргументів. Іди помийся");
        
        List<object?> argValues = [];
        argValues.AddRange(node.Arguments.Select(Evaluate));

        Environment callEnv = new Environment(func.Closure);
        for (int i = 0; i < argValues.Count; i++)
        {
            ValidateType(func.Declaration.Parameters[i].Type, argValues[i]);
            callEnv.DeclareVariable(func.Declaration.Parameters[i].Name.Value, argValues[i]!, false);
        }
        
        Environment previousEnv = Env;
        try
        {
            Env = callEnv;
            Evaluate(func.Declaration.Body);
            
            if (func.Declaration.ReturnType is not null && func.Declaration.ReturnType.BaseType.Type is not TokenType.TypeVoid)
            {
                throw new Exception($"RuntimeError: Функція '{node.Identifier.Value}' обіцяла повернути '{func.Declaration.ReturnType.BaseType.Value}', але ти забив і нічого не повернув. В расті за таку брехню тебе б спалили заживо.");
            }
            
            return null;
        }
        catch (ReturnException r)
        {
            ValidateType(func.Declaration.ReturnType, r.Value);
            return r.Value;
        }
        finally
        {
            Env = previousEnv; 
        }
    }
    
    object? EvaluateMethodCall(MethodCallNode node)
    {
        object? target = Evaluate(node.Target);
        
        object? funcObj = Env.GetVariable(node.Identifier.Value);
        
        List<object?> argValues = [target];
        argValues.AddRange(node.Arguments.Select(Evaluate));
        
        if (funcObj is NativeFunction native)
        {
            if (!native.IsMethod)
                throw new Exception($"TypeError: '{node.Identifier.Value}' не є методом. Використовуй її як звичайний виклик.");
            
            if (native.Arity != -1 && argValues.Count != native.Arity)
                throw new Exception($"RuntimeError: Метод '{node.Identifier.Value}' очікує {native.Arity} аргументів.");

            return native.Function(argValues);
        }
        
        if (funcObj is not BychkovFunction func)
            throw new Exception($"TypeError: '{node.Identifier.Value}' не є методом.");
        
        if (!func.Declaration.IsMethod)
            throw new Exception($"TypeError: '{node.Identifier.Value}' це звичайна функція fn. Використовуй її як звичайний виклик, а не через крапку.");

        if (argValues.Count != func.Declaration.Parameters.Count)
            throw new Exception($"RuntimeError: Метод '{node.Identifier.Value}' очікує {func.Declaration.Parameters.Count} аргументів (разом з об'єктом), але отримав {argValues.Count}.");
        
        Environment callEnv = new Environment(func.Closure);
        for (int i = 0; i < argValues.Count; i++)
        {
            callEnv.DeclareVariable(func.Declaration.Parameters[i].Name.Value, argValues[i]!, false);
        }
        
        Environment previousEnv = Env;
        try
        {
            Env = callEnv;
            Evaluate(func.Declaration.Body);
            
            if (func.Declaration.ReturnType != null && func.Declaration.ReturnType.BaseType.Type != TokenType.TypeVoid)
            {
                throw new Exception($"RuntimeError: Метод '{node.Identifier.Value}' повинен повертати '{func.Declaration.ReturnType.BaseType.Value}', але ти скоріше всього не вдуплив і забув return");
            }
            
            return null;
        }
        catch (ReturnException r)
        {
            ValidateType(func.Declaration.ReturnType, r.Value);
            return r.Value;
        }
        finally 
        { 
            Env = previousEnv; 
        }
    }
    
    object? EvaluateExpressionStatement(ExpressionStatementNode node)
    {
        Evaluate(node.Expression); 
        
        return null; 
    }
    
    object? EvaluateImport(ImportNode node)
    {
        if (OnImport == null)
            throw new Exception("RuntimeError: Срєда виполнєнія не підтримує імпорт файлів");
        
        OnImport.Invoke(node.ModuleName); 
        
        return null;
    }
    
    object EvaluateListLiteral(ListLiteralNode node)
    {
        List<object?> list = new();
        foreach (var element in node.Elements)
        {
            list.Add(Evaluate(element));
        }
        return list;
    }

    object? EvaluateIndexAccess(IndexAccessNode node)
    {
        object? target = Evaluate(node.Target);
        
        object? indexObj = Evaluate(node.Index);

        if (target is not List<object?> list)
            throw new Exception("TypeError: Ти намагаєшся взяти індекс не зі списку, а чорт знає звідки");

        if (indexObj is not double dIndex || dIndex % 1 != 0)
            throw new Exception("TypeError: Індекс масиву повинен бути цілим числом, не вигадуй велосипед.");

        int index = (int)dIndex;
        
        if (index < 0 || index >= list.Count)
            throw new Exception($"RuntimeError: Індекс {index} іс аут оф розмів масиву ({list.Count})");

        return list[index];
    }
    
    object? EvaluateIndexAssignment(IndexAssignmentNode node)
    {
        object? target = Evaluate(node.IndexAccess.Target);
        
        object? indexValue = Evaluate(node.IndexAccess.Index);
        object? newValue = Evaluate(node.Value);

        if (target is not List<object?> list)
        {
            throw new Exception("RuntimeError: Ти намагаєшся взяти індекс не зі списку, а чорт знає звідки");
        }

        if (indexValue is not double dIndex || dIndex % 1 != 0)
        {
            throw new Exception("RuntimeError: Індекс масиву повинен бути цілим числом, не вигадуй велосипед.");
        }

        int index = (int)dIndex;

        if (index < 0 || index >= list.Count)
        {
            throw new Exception($"RuntimeError: Індекс {index} іс аут оф розмір масиву ({list.Count})");
        }
        
        list[index] = newValue;
    
        return newValue;
    }
    
    object? EvaluateUnaryOperation(UnaryOperationNode node)
    {
        object? right = Evaluate(node.Right);

        if (node.Operator.Type == TokenType.Tilde)
        {
            if (right is not bool b)
                throw new Exception($"TypeError: Оператор '~' можна використовувати тіки для boolean, а не для {right}");
            
            return !b;
        }

        throw new Exception($"RuntimeError: Невідомий унарный оператор {node.Operator.Value}");
    }

    record BychkovFunction(FunctionDeclarationNode Declaration, Environment Closure);
    
    public record NativeFunction(int Arity, Func<List<object?>, object?> Function, bool IsMethod = false);
}