using System;

namespace Assets.Scripts.SaveSystem
{
    [Serializable]
    public class AccountAuthMeta
    {
        public int Version = 1;
        public string Username;
        public string Salt;
        public string PasswordHash;
        public int Iterations;
        public string CreatedAtUtc;
        public string UpdatedAtUtc;
    }
}
