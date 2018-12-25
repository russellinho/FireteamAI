using UnityEngine;
using System.Collections;
using Photon.Realtime;
using Photon.Pun;

public class CameraShakeScript : MonoBehaviour
{
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;
	
	// How long the object should shake for.
	public bool shake;

    // Amplitude of the shake. A larger value shakes the camera harder.
    private float shakeAmount = 0.1f;

	Vector3 originalPos;
	
	void Awake()
	{
		if (!GetComponent<PhotonView> ().IsMine) {
			this.enabled = false;
			return;
		}
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
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
		if (shake)
		{
			Vector2 offset = Random.insideUnitCircle;
			camTransform.localPosition = originalPos + new Vector3(offset.x, offset.y, 0f) * shakeAmount;
        }
		else
		{
			camTransform.localPosition = originalPos;
		}
	}
}