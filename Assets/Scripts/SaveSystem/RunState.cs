using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class RunState
    {
        public PlayerState Player = new();
    }
}
