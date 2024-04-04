using Auth;
using Firebase;
using Firebase.Database;
using UnityEngine;

/// <summary>
/// This class includes a database singleton, all classes should use this proxy class instead of connecting to the database directly
/// Author: Michael Kim
/// Date: 2024-03-01
/// </summary>
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

        /// <summary>
        /// Initializes root to default instance of firebase Database and call auth connector to intialize
        /// </summary>
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