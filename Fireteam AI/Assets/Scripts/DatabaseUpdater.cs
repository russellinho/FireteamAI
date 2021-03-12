﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Firebase.Database;
using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

public class DatabaseUpdater : MonoBehaviour
{
    private const float NINETY_DAYS_MINS = 129600f;
	private const float THIRTY_DAYS_MINS = 43200;
	private const float SEVEN_DAYS_MINS = 10080f;
	private const float ONE_DAY_MINS = 1440f;
	private const float PERMANENT = -1f;
    public string itemToAdd;
    public float daysDuration;
    private IEnumerator<DataSnapshot> accounts;

    // void Update() {
    //     if (accounts != null) {
    //         while (accounts.MoveNext())
    //     }
    // }

    public void AddItemToAllAccounts() {
        float totalMins = (daysDuration == -1f) ? -1f : daysDuration * 24f * 60f;
        string category = null;
        if (InventoryScript.itemData.equipmentCatalog.ContainsKey(itemToAdd)) {
            Equipment e = InventoryScript.itemData.equipmentCatalog[itemToAdd];
            switch (e.category) {
                case "Top":
                    category = "tops";
                    break;
                case "Bottom":
                    category = "bottoms";
                    break;
                case "Footwear":
                    category = "footwear";
                    break;
                case "Headgear":
                    category = "headgear";
                    break;
                case "Facewear":
                    category = "facewear";
                    break;
            }
        } else if (InventoryScript.itemData.armorCatalog.ContainsKey(itemToAdd)) {
            category = "armor";
        } else if (InventoryScript.itemData.weaponCatalog.ContainsKey(itemToAdd)) {
            category = "weapons";
        } else if (InventoryScript.itemData.characterCatalog.ContainsKey(itemToAdd)) {
            category = "characters";
        } else if (InventoryScript.itemData.modCatalog.ContainsKey(itemToAdd)) {
            category = "mods";
        }

        if (category == null) {
            Debug.LogError("The item [" + itemToAdd + "] does not exist!");
            return;
        }

        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_inventory").GetValueAsync().ContinueWith(taskA => {
            Debug.Log("updated");
            IEnumerator<DataSnapshot> accountInventories = taskA.Result.Children.GetEnumerator();
            while (accountInventories.MoveNext()) {
                DataSnapshot account = accountInventories.Current;
                string accountId = account.Key.ToString();
                Debug.Log(accountId + " updated");
                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_inventory").Child(accountId).Child(category).Child(itemToAdd).Child("acquireDate").SetValueAsync(DateTime.Now.ToString());
                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_inventory").Child(accountId).Child(category).Child(itemToAdd).Child("duration").SetValueAsync(""+totalMins);
            }
        });
    }

    public void AddEquippedMeleeFieldToAllAccounts() {
        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
            IEnumerator<DataSnapshot> accountsSavedData = taskA.Result.Children.GetEnumerator();
            while (accountsSavedData.MoveNext()) {
                DataSnapshot account = accountsSavedData.Current;
                string accountId = account.Key.ToString();
                Debug.Log(accountId + " updated");
                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(accountId).Child("equipment").Child("equippedMelee").SetValueAsync("Recon Knife");
            }
        });
    }

    public void AddDefaultWeaponFieldToAllAccounts() {
        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
            IEnumerator<DataSnapshot> accountsSavedData = taskA.Result.Children.GetEnumerator();
            while (accountsSavedData.MoveNext()) {
                DataSnapshot account = accountsSavedData.Current;
                string accountId = account.Key.ToString();
                Debug.Log(accountId + " updated");
                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(accountId).Child("defaultWeapon").SetValueAsync("N4A1");
            }
        });
    }

    public void AddLoggedInFieldToAllAccounts() {
        DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").GetValueAsync().ContinueWith(taskA => {
            IEnumerator<DataSnapshot> accountsSavedData = taskA.Result.Children.GetEnumerator();
            while (accountsSavedData.MoveNext()) {
                DataSnapshot account = accountsSavedData.Current;
                string accountId = account.Key.ToString();
                Debug.Log(accountId + " updated");
                DAOScript.dao.dbRef.Child("fteam_ai").Child("fteam_ai_users").Child(accountId).Child("loggedIn").SetValueAsync("0");
            }
        });
    }

    public void AddKashFieldToAllAccounts() {
        Dictionary<string, object> inputData = new Dictionary<string, object>();        
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("kashFieldAdd");
		func.CallAsync(inputData);
    }

    public void AddSkillTableToAll()
    {
        Dictionary<string, object> inputData = new Dictionary<string, object>();        
		HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("addSkillTreeToAll");
		func.CallAsync(inputData);
    }
}
