using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class LoggerScript : MonoBehaviour
{

    public static LoggerScript logger;

    public void LogDebug(string message, string context = "Debug") {
        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("logInGameMessage");
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["context"] = context;
        inputData["message"] = message;
        func.CallAsync(inputData);
    }

    public void LogError(string message, string context = "Error") {
        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("logInGameMessage");
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["callHash"] = DAOScript.functionsCallHash;
        inputData["context"] = context;
        inputData["message"] = message;
        func.CallAsync(inputData);
    }

}
