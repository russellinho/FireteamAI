using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;

public class DAOScript : MonoBehaviour
{
    public static DAOScript dao;
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
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://YOUR-FIREBASE-APP.firebaseio.com/");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
