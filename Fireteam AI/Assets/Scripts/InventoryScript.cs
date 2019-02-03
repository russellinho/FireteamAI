using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    // Mapping for all items in the database - key is the item name and value is the
    // database path to load from
    public static Dictionary<string, string> inventoryCatalog = new Dictionary<string, string>();

    void Start() {
        inventoryCatalog.Add("", "");
    }
    
    public static ArrayList collectCharacters() {
        ArrayList ret = new ArrayList();

        //ret.Add();

        return ret;
    }

}
