using Database;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

namespace Auth
{
    public class AuthConnector : MonoBehaviour
    {
        public static AuthConnector Instance { get; private set; }
        public FirebaseAuth Auth { get; private set; }

        public FirebaseUser CurrentUser { get; private set; }
        public bool IsEmailVerified {get; set; }

        public PermissonLevel Perms { get; set; }

        /// <summary>
        /// Unity function that is called when application is started.
        /// Forces this to act as a static class with Instance.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Initializes auth to default instance of firebase Auth.
        /// </summary>
        public void InitAuth()
        {
            // This was chosen to be public as otherwise the dependency check must be called again separate from Database Connector.
            // It also conflicted with Database Connector initialization as it is threading the task, and had conflicts.
            Auth = FirebaseAuth.DefaultInstance;
            CurrentUser = Auth.CurrentUser;
        }
    }
}