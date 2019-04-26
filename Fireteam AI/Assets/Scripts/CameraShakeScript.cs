using UnityEngine;
using System.Collections;
using Photon.Realtime;
using Photon.Pun;

public class CameraShakeScript : MonoBehaviour
{
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;
	public PlayerActionScript playerActionScript;
	
	// How long the object should shake for.
	public bool shake;

    // Amplitude of the shake. A larger value shakes the camera harder.
    private float shakeAmount = 0.001f;

	Vector3 originalPos;
	
	void Awake()
	{
		// TODO: Re-enable
		// if (!GetComponent<PhotonView> ().IsMine) {
		// 	this.enabled = false;
		// 	return;
		// }
		// if (camTransform == null)
		// {
		// 	camTransform = GetComponent(typeof(Transform)) as Transform;
		// }
	}

	public void SetShake(bool b) {
		shake = b;
	}
	
	void OnEnable()
	{
		originalPos = camTransform.localPosition;
	}

	void Update()
	{
		if (playerActionScript.health > 0) { 
			if (shake) {
				int r1 = Random.Range(1, 3);
				int r2 = Random.Range(1, 3);
				float f1 = (r1 == 1 ? 1f : -1f);
				float f2 = (r2 == 1 ? 1f : -1f);
				camTransform.Translate(transform.up * shakeAmount * r1);
				camTransform.Translate(transform.right * shakeAmount * r2);
			}
		}
	}
}