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
    public static string functionsCallHash;

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
        HttpsCallableReference func = functions.GetHttpsCallable("getFunctionsCallHash");
		func.CallAsync().ContinueWith((task) => {
            Dictionary<object, object> results = (Dictionary<object, object>)task.Result.Data;
            if (results["status"].ToString() == "200") {
                Debug.Log("Functions call hash retrieved successfully.");
                functionsCallHash = results["message"].ToString();
            } else {
                Debug.LogWarning("Functions call hash failed to load.");
            }
        });
    }

}
