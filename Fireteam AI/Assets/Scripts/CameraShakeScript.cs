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
    private float shakeAmount = 0.00008f;
	public PhotonView pView;

	Vector3 originalPos;
	
	void Awake()
	{
		if (!pView.IsMine) {
			this.enabled = false;
			return;
		}
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
				Vector2 offset = Random.insideUnitCircle;
				camTransform.localPosition = originalPos + new Vector3 (0f, offset.y, offset.x) * shakeAmount;
			} else {
				camTransform.localPosition = originalPos;
			}
		}
	}
}