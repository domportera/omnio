using System.Linq.Expressions;

namespace Utilities;

/// <summary>
/// A cache for default constructors of types descended from T.
/// </summary>
public class DynamicConstructorCache<T>
{
    public Func<T> GetDefaultConstructor(Type descendantType, bool skipCheck = false)
    {
        lock (_constructors)
        {
            if (_constructors.TryGetValue(descendantType, out var constructor))
                return constructor;
        }

        if(!skipCheck && !typeof(T).IsAssignableFrom(descendantType))
            throw new InvalidOperationException($"Type {descendantType} is not descended from {typeof(T)}");

        // compile this as a constructor expression
        var expression = Expression.Lambda<Func<T>>(Expression.New(descendantType)).Compile();
        lock (_constructors)
        {
            _constructors[descendantType] = expression;
        }
        return expression;
    }
    
    private readonly Dictionary<Type, Func<T>> _constructors = new();
}