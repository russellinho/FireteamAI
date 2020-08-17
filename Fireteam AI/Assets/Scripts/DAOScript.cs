using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Functions;
using Firebase.Unity.Editor;

public class DAOScript : MonoBehaviour
{
    public static DAOScript dao;
    public DatabaseReference dbRef;
    public FirebaseFunctions functions;
    public static string functionsCallHash = ".7^D{(7~=7ygT{9a";

    void Awake() {
        if (dao == null)
        {
            DontDestroyOnLoad(gameObject);
            dao = this;
        }
        else if (dao != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://koobando-web.firebaseio.com/");
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        functions = Firebase.Functions.FirebaseFunctions.DefaultInstance;
    }

}
