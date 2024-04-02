using Auth;
using Firebase;
using Firebase.Database;
using UnityEngine;

namespace Database
{
    public class DatabaseConnector : MonoBehaviour
    {
        public static DatabaseConnector Instance { get; private set; }

        public DatabaseReference Root { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

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
                    Root = FirebaseDatabase.DefaultInstance.RootReference;
                    AuthConnector.Instance.InitAuth();
                }
                else
                {
                    Debug.LogError("[DatabaseConnector] Could not resolve all Firebase dependencies: " + status);
                }
            });
        }
    }
}