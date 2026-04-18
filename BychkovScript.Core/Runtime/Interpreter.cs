using BychkovScript.Core.AST;
using BychkovScript.Core.Lexing;

namespace BychkovScript.Core.Runtime;

public class Interpreter(Environment env)
{
    public Environment Env { get; private set; } = env;
    
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
            ReturnNode ret => EvaluateReturn(ret),
            
            AssignmentNode a => EvaluateAssignment(a),
            
            ExpressionStatementNode exprStmt => EvaluateExpressionStatement(exprStmt),
            
            ImportNode imp => EvaluateImport(imp),
            
            _ => throw new Exception($"RuntimeError: Unknown Node type {node.GetType().Name}")
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
        object? value = Evaluate(node.Value);
        
        ValidateType(node.DataType, value);
        
        bool isConst = node.Modifier.Type == TokenType.Const;
        
        Env.DeclareVariable(node.Identifier.Value, value!, isConst);
        
        return value;
    }
    
    object EvaluateBinary(BinaryNode node)
    {
        object? left = Evaluate(node.Left);
        object? right = Evaluate(node.Right);
        
        if (node.Operator.Type == TokenType.Equals)
        {
            return left!.Equals(right);
        }
        
        if (node.Operator.Type is TokenType.And or TokenType.Or)
        {
            if (left is bool lBool && right is bool rBool)
            {
                return node.Operator.Type == TokenType.And ? lBool && rBool : lBool || rBool;
            }
            throw new Exception($"TypeError: Логічні оператори потребують булевий тип");
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
                _ => throw new Exception($"RuntimeError: Оператор не підтримується: {node.Operator.Value}")
            };
        }

        if (left is not string && right is not string)
            throw new Exception($"RuntimeError: Operation type error {node.Operator.Value}");

        if (node.Operator.Type == TokenType.Plus)
        {
            return left!.ToString() + right!;
        }

        throw new Exception($"RuntimeError: Operation type error {node.Operator.Value}");
    }
    
    void ValidateType(Token typeToken, object? value)
    {
        switch (typeToken.Type)
        {
            case TokenType.TypeString:
                if (value is not string)
                    throw new Exception($"TypeError: Змінна таки очікує тип 'str', але дурень-розробник умістив '{value}' у рядку {typeToken.Line}");
                break;

            case TokenType.TypeInt32:
                if (value is not double dInt || dInt % 1 != 0)
                    throw new Exception($"TypeError: Змінна таки очікує тип 'int', але дурень-розробник умістив ({value}) у рядку {typeToken.Line}");
                break;

            case TokenType.TypeFloat32:
                if (value is not double)
                    throw new Exception($"TypeError: Змінна таки очікує тип 'float', але дурень-розробник умістив '{value}' у рядку {typeToken.Line}");
                break;
            
            case TokenType.TypeBoolean:
                if (value is not bool)
                    throw new Exception($"TypeError: Змінна таки очікує тип 'boolean', але дурень-розробник умістив '{value}' у рядку {typeToken.Line}");
                break;
            
            case TokenType.TypeVoid:
                if (value is not null)
                    throw new Exception($"TypeError: Функція з типом 'void' уж ніяк не повинна повертати значення");
                break;

            default:
                throw new Exception($"RuntimeError: Де ти знайшов тип даних {typeToken.Value}?");
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
            throw new Exception($"TypeError: Очікується булеве значення а тут {condition?.GetType().Name}");
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
                throw new Exception($"TypeError: Умова циклу while повинна бути логічною, а не оця срань {condition?.GetType().Name}");
                
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
            throw new Exception("TypeError: Діапазон циклу for повинний бути цілочисельним (int).");
        
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
        object funcObj = Env.GetVariable(node.Identifier.Value);
        
        if (funcObj is NativeFunction native)
        {
            if (native.Arity != -1 && node.Arguments.Count != native.Arity)
                throw new Exception($"RuntimeError: Функція '{node.Identifier.Value}' взагалі то очікує від вас {native.Arity} аргументів.");

            List<object?> nArgs = [];
            foreach (var arg in node.Arguments) nArgs.Add(Evaluate(arg));

            return native.Function(nArgs);
        }
        
        if (funcObj is not BychkovFunction func)
            throw new Exception($"TypeError: '{node.Identifier.Value}' не є функцією.");
        
        if (node.Arguments.Count != func.Declaration.Parameters.Count)
            throw new Exception($"RuntimeError: Функція '{node.Identifier.Value}' взагалі то очікує від вас {func.Declaration.Parameters.Count} аргументів.");
        
        List<object?> argValues = [];
        argValues.AddRange(node.Arguments.Select(Evaluate));

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
            
            if (func.Declaration.ReturnType != null && func.Declaration.ReturnType.Type != TokenType.TypeVoid)
            {
                throw new Exception($"RuntimeError: Функція '{node.Identifier.Value}' взагалі то повинна повертати '{func.Declaration.ReturnType.Value}', але ти скоріше всього не вдуплив і забув return");
            }
            
            return null;
        }
        catch (ReturnException r)
        {
            if (func.Declaration.ReturnType != null)
            {
                ValidateType(func.Declaration.ReturnType, r.Value);
            }
            else if (r.Value != null)
            {
                throw new Exception($"TypeError: Функция '{node.Identifier.Value}' не объявляла возвращаемый тип, но попыталась вернуть значение!");
            }
            
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

    record BychkovFunction(FunctionDeclarationNode Declaration, Environment Closure);
    
    public record NativeFunction(int Arity, Func<List<object?>, object?> Function);
}