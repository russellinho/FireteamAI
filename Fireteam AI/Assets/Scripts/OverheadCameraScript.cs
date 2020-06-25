using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OverheadCameraScript : MonoBehaviour
{
    public PhotonView pView;
    public Transform spineTransform;
    public Transform mainCamTrans;

    // Update is called once per frame
    void LateUpdate()
    {
        if (pView.IsMine) {
            transform.rotation = Quaternion.Euler(90f, 0f, -mainCamTrans.rotation.eulerAngles.y);
        } else {
            transform.rotation = Quaternion.Euler(90f, 0f, -spineTransform.rotation.eulerAngles.y + 32f);
        }
    }
}
