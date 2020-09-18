﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Functions;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;
using Koobando.UI.Console;

public class AdminCommands : MonoBehaviour
{
    [Command]
    public void BanPlayer(string playerId, float duration, string reason) {
        Dictionary<string, object> inputData = new Dictionary<string, object>();
        inputData["playerId"] = playerId;
        inputData["duration"] = ""+duration;
        inputData["reason"] = reason;

        HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("adminBanPlayerFireteam");
        func.CallAsync(inputData).ContinueWith((task) => {
            Dictionary<object, object> results = (Dictionary<object, object>)task.Result.Data;
            Debug.Log("[BanPlayer] returned successfully with status code [" + results["status"] + "]");
        });
    }
}
