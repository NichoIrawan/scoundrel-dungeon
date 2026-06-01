using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class DungeonNode
    {
        public int NodeId;
        public string[] EncounterIds = new string[4];
        public int LeftNode = -1;
        public int RightNode = -1;
        public bool IsResolved;
        public bool IsEscaped;
        public string[] SelectedEncounterIds = Array.Empty<string>();
    }
}
