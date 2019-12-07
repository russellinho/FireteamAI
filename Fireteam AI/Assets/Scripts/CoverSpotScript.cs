using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CoverSpotScript : MonoBehaviour
{
    public GameControllerScript gameController;
    private bool taken;
    public short coverId;
    // Start is called before the first frame update
    void Start()
    {
        taken = false;
        gameController.AddCoverSpot(gameObject);
    }

    public bool IsTaken() {
        return taken;
    }

    public void TakeCoverSpot() {
        taken = true;
    }

    public void LeaveCoverSpot() {
        taken = false;
    }

    public void SetCoverSpot(bool b) {
        Debug.Log("entering");
        taken = b;
    }

}
