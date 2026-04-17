namespace BychkovScript.Core.Runtime;

public class Environment
{
    readonly Dictionary<string, object> _variables = new();

    public void DeclareVariable(string name, object value)
    {
        if (!_variables.TryAdd(name, value))
        {
            throw new Exception($"RuntimeError: Variable '{name}' already exists!");
        }
    }

    public object GetVariable(string name)
        => _variables.TryGetValue(name, out var value) ? 
            value : throw new Exception($"RuntimeError: Variable '{name}' already exists!");
}