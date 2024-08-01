using OperatorCore;

namespace NodeImplementations;

public static class TypeRegistration
{
    public static void FindAndRegisterTypes()
    {
        GraphNodeTypes.RegisterCurrentAssembly();
    }
}