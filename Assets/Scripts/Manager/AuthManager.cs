using Assets.Scripts.SaveSystem;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class AuthManager : MonoBehaviour
    {
        public AccountAuthMeta CreateAccount(string username, string password)
        {
            return DataPersistenceManager.Instance.CreateAccount(username, password);
        }

        public void Login(string username, string password)
        {
            DataPersistenceManager.Instance.Login(username, password);
        }
    }
}
