using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CoverSpotScript : MonoBehaviour
{
    public GameControllerScript gameController;
    private bool taken;
    // Start is called before the first frame update
    void Start()
    {
        taken = false;
        gameController.AddCoverSpot(this);
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
        taken = b;
    }

}
