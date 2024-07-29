using System.Reflection;
using OperatorCore;

namespace NodeImplementations;

public static class TypeRegistration
{
    public static void FindAndRegisterTypes()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            if (type.IsAssignableTo(typeof(GraphNodeLogic)))
            {
                GraphNodeTypes.RegisterNodeType(type);
            }
        }
    }
}