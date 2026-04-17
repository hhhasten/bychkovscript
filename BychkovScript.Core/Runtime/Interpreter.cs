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
        
        env.DeclareVariable(node.Identifier.Value, value!);
        
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
        
        if (left is double l && right is double r)
        {
            return node.Operator.Type switch
            {
                TokenType.Plus => l + r,
                TokenType.Minus => l - r,
                TokenType.Multiply => l * r,
                TokenType.Divide => l / r,
                _ => throw new Exception($"RuntimeError: Unsupported operator {node.Operator.Value}")
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
}