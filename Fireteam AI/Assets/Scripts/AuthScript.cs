using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;

public class AuthScript : MonoBehaviour
{
    public static AuthScript authHandler;
    public FirebaseAuth auth;
    public Firebase.Auth.FirebaseUser user;

    void Awake() {
        if (authHandler == null)
        {
            DontDestroyOnLoad(gameObject);
            authHandler = this;
        }
        else if (authHandler != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }
    
}
