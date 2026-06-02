using Assets.Scripts.Utilities;
using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class DungeonState
    {
        public SerializedDictionary<int, DungeonNode> Nodes = new();
        public int CurrentNode;
        public int LastResolvedNode = -1;
        public int[] UnresolvedNodes = Array.Empty<int>();
    }
}
