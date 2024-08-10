using System.Diagnostics.CodeAnalysis;

namespace OperatorCore;

public partial class SubGraph
{
    [NonSerialized] private static readonly Dictionary<Guid, SubgraphDefinition> SubGraphsByType = new();

    private static SubgraphDefinition GetSubgraphDefinition(Guid typeId)
    {
        lock (TypeLock.GetLock(typeId))
        {
            if (SubGraphsByType.TryGetValue(typeId, out var subgraph))
                return subgraph;

            if (!TryLoadSubgraphDefinitionFromFile(typeId, out subgraph))
            {
                subgraph = new SubgraphDefinition(typeId);
            }

            SubGraphsByType.Add(typeId, subgraph);
            return subgraph;
        }
    }

    private static bool TryLoadSubgraphDefinitionFromFile(Guid typeId,
        [NotNullWhen(true)] out SubgraphDefinition? subgraph)
    {
        subgraph = null;
        return false;

        return true;
    }

    private static bool TryLoadInstanceInfo(Guid typeId, Guid instanceId,
        [NotNullWhen(true)] out InstanceInfo? instanceInfo)
    {
        instanceInfo = null;
        return false;
    }

    private static class TypeLock
    {
        private static readonly object GlobalLock = new();
        private static readonly Dictionary<Guid, object> FileLocksByType = new();

        public static object GetLock(Guid type)
        {
            object? lockObj;
            lock (GlobalLock)
            {
                if (!FileLocksByType.TryGetValue(type, out lockObj))
                {
                    lockObj = new object();
                    FileLocksByType.Add(type, lockObj);
                }
            }

            return lockObj;
        }
    }
}