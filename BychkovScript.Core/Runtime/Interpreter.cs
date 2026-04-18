using BychkovScript.Core.AST;
using BychkovScript.Core.Lexing;

namespace BychkovScript.Core.Runtime;

public class Interpreter(Environment env)
{
    public object? Evaluate(Node node)
    {
        return node switch
        {
            ProgramNode program => EvaluateProgram(program),
            
            NumberNode n => n.Value,
            StringNode s => s.Value,
            
            VariableNode v => env.GetVariable(v.Name),
            
            BinaryNode b => EvaluateBinary(b),
            
            VariableDeclarationNode vDecl => EvaluateVariableDeclaration(vDecl),
            PrintStatementNode pStmt => EvaluatePrint(pStmt),
            
            AssignmentNode a => EvaluateAssignment(a),
            
            BlockNode b => EvaluateBlock(b),
            IfNode i => EvaluateIf(i),
            BooleanNode boolNode => boolNode.Value,
            
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
        
        env.DeclareVariable(node.Identifier.Value, value!, isConst);
        
        return value;
    }

    object? EvaluatePrint(PrintStatementNode node)
    {
        object? value = Evaluate(node.Value);
        
        Console.WriteLine(value);
        
        return null;
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
    
    object? EvaluateAssignment(AssignmentNode node)
    {
        object? value = Evaluate(node.Value);
        
        env.AssignVariable(node.Identifier.Value, value!);
        
        return value;
    }
    
    void ValidateType(Token typeToken, object? value)
    {
        switch (typeToken.Type)
        {
            case TokenType.TypeString:
                if (value is not string)
                    throw new Exception($"TypeError: Змінна таки очікує тип 'string', але дурень-розробник умістив '{value}' у рядку {typeToken.Line}");
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
}