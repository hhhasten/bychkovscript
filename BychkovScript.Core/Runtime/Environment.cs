namespace BychkovScript.Core.Runtime;

public class Environment(Environment? parent = null)
{
    record Variable(object? Value, bool IsConstant);

    readonly Dictionary<string, Variable> _variables = new();

    public void DeclareVariable(string name, object? value, bool isConstant)
    {
        if (_variables.ContainsKey(name))
        {
            throw new Exception($"RuntimeError: Змінна '{name}' вже існує в цьому скоупі! " +
                                $"Тобі написав код діпсік, чи ти звик до var у JavaScript, де можна смітити в пам'яті і переоголошувати все підряд? " +
                                $"Май повагу до пам'яті, вона зараз дорога");
        }

        _variables[name] = new Variable(value, isConstant);
    }
    
    public object? GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out var variable))
            return variable.Value;

        if (parent != null)
            return parent.GetVariable(name);

        throw new Exception($"RuntimeError: Змінну '{name}' не знайдено! Чекав 'undefined', як у своєму улюбленому джаваскріпті? " +
                            $"У нас тут строгий лексичний скоуп, а не прохідний двір. Оголоси її спочатку і не грай на нервах");
    }
    
    public void AssignVariable(string name, object value)
    {
        if (_variables.TryGetValue(name, out var variable))
        {
            if (variable.IsConstant)
            {
                throw new Exception($"RuntimeError: Ти намагаєшся змінити КОНСТАНТУ '{name}'. " +
                                    $"Куди ти взагалі лізеш?. Не смій мутувати те, що обіцяв не чіпати!");
            }
            _variables[name] = variable with { Value = value };
            return;
        }
        
        if (parent != null)
        {
            parent.AssignVariable(name, value);
            return;
        }

        throw new Exception($"RuntimeError: Присвоєння в нікуди! Змінної '{name}' не існує. Це прекраний BychkovScript, а не смітник (джаваскріпт)");
    }
}