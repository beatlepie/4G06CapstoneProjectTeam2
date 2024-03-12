using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

namespace Database
{
    public class DatabaseConnector : MonoBehaviour
    {
        public static DatabaseConnector Instance { get; private set; }

        public FirebaseAuth Auth { get; private set; }
        public DatabaseReference Root { get; private set; }

        public FirebaseUser CurrentUser { get; private set; }

        public PermissonLevel Perms { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
        
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        
            InitFirebase();
        }

        private void InitFirebase()
        {
            Debug.Log("[DatabaseConnector] Initializing Firebase Connection...");
        
            DependencyStatus status;
        
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                status = task.Result;
                if (status == DependencyStatus.Available)
                {
                    Debug.Log("[DatabaseConnector] Successfully established connection with Firebase");
                    Auth = FirebaseAuth.DefaultInstance;
                    Root = FirebaseDatabase.DefaultInstance.RootReference;
                    CurrentUser = Auth.CurrentUser;
                }
                else
                {
                    Debug.LogError("[DatabaseConnector] Could not resolve all Firebase dependencies: " + status);
                }
            });
        }
    }
}