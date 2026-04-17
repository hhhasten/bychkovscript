namespace BychkovScript.Core.Runtime;

public class Environment
{
    record Variable(object Value, bool IsConstant);

    readonly Dictionary<string, Variable> _variables = new();

    public void DeclareVariable(string name, object value, bool isConstant)
    {
        if (_variables.ContainsKey(name))
        {
            throw new Exception($"RuntimeError: Змінна '{name}' вже існує бляха! Вже не можна!");
        }

        _variables[name] = new Variable(value, isConstant);
    }
    
    public object GetVariable(string name)
        => _variables.TryGetValue(name, out var variable) ? 
            variable.Value : throw new Exception($"RuntimeError: Опа, а змінна '{name}' ще не визначена!");
    
    public void AssignVariable(string name, object value)
    {
        if (!_variables.TryGetValue(name, out var variable))
        {
            throw new Exception($"RuntimeError: Ти намагаєшся присвоїти значення не існуючій змінній '{name}'!");
        }

        if (variable.IsConstant)
        {
            throw new Exception($"RuntimeError: Ти сам оголосив константу '{name}' а тепер намагаєшся змінити їй значення. Браво, тормоз");
        }
        
        _variables[name] = variable with { Value = value };
    }
}