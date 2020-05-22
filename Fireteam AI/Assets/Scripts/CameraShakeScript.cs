using UnityEngine;
using System.Collections;
using Photon.Realtime;
using Photon.Pun;

public class CameraShakeScript : MonoBehaviour
{
	private const float MELEE_LERP_TIME = 0.3f;
	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;
	public PlayerActionScript playerActionScript;
	private Vector3 initialCamRot;
	private Vector3 swingCamRot;
	private Vector3 lungeCamRot;
	private const float LUNGE_X_ROT = -30f;
	private const float SWING_X_ROT = -1f;
	private const float SWING_Y_ROT = 40f;
	private const float SWING_Z_ROT = 3f;
	private bool wasASwing;
	private bool wasALunge;
	
	// How long the object should shake for.
	public bool shake;

    // Amplitude of the shake. A larger value shakes the camera harder.
    private float shakeAmount = 0.00008f;
	private float meleeShakeTimer = 0f;
	private bool meleeShook;
	public PhotonView pView;

	Vector3 originalPos;
	
	void Awake()
	{
		if (!pView.IsMine) {
			this.enabled = false;
			return;
		}
		initialCamRot = camTransform.localRotation.eulerAngles;
		swingCamRot = new Vector3(initialCamRot.x + SWING_X_ROT, initialCamRot.y + SWING_Y_ROT, initialCamRot.z + SWING_Z_ROT);
		lungeCamRot = new Vector3(initialCamRot.x + LUNGE_X_ROT, initialCamRot.y, initialCamRot.z);
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
			// Used for shaking the camera during melee
			if (playerActionScript.wepActionScript.isMeleeing) {
				if (playerActionScript.wepActionScript.isLunging) {
					wasALunge = true;
					wasASwing = false;
					if (!meleeShook && meleeShakeTimer < MELEE_LERP_TIME) {
						camTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(initialCamRot), Quaternion.Euler(lungeCamRot), meleeShakeTimer);
						meleeShakeTimer += (Time.deltaTime * (1f / MELEE_LERP_TIME));
					} else {
						meleeShook = true;
						DecreaseMeleeCamLunge();
					}
				} else {
					wasALunge = false;
					wasASwing = true;
					if (!meleeShook && meleeShakeTimer < MELEE_LERP_TIME) {
						camTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(initialCamRot), Quaternion.Euler(swingCamRot), meleeShakeTimer);
						meleeShakeTimer += (Time.deltaTime * (1f / MELEE_LERP_TIME));
					} else {
						meleeShook = true;
						DecreaseMeleeCamSwing();
					}
				}
			} else {
				// Used for shaking the camera during melee
				meleeShook = false;
				if (wasASwing) {
					DecreaseMeleeCamSwing();
				} else if (wasALunge) {
					DecreaseMeleeCamLunge();
				}
				// Used for shaking camera during gunfire
				if (shake) {
					Vector2 offset = Random.insideUnitCircle;
					camTransform.localPosition = originalPos + new Vector3 (0f, offset.y, offset.x) * shakeAmount;
				} else {
					camTransform.localPosition = originalPos;
				}
			}
		} else {
			meleeShook = false;
			if (wasASwing) {
				DecreaseMeleeCamSwing();
			} else if (wasALunge) {
				DecreaseMeleeCamLunge();
			}
		}
	}

	void DecreaseMeleeCamSwing() {
		if (meleeShakeTimer > 0f) {
			meleeShakeTimer -= Time.deltaTime;
			camTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(initialCamRot), Quaternion.Euler(swingCamRot), meleeShakeTimer);
		} else {
			meleeShakeTimer = 0f;
		}
	}

	void DecreaseMeleeCamLunge() {
		if (meleeShakeTimer > 0f) {
			meleeShakeTimer -= Time.deltaTime;
			camTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(initialCamRot), Quaternion.Euler(lungeCamRot), meleeShakeTimer);
		} else {
			meleeShakeTimer = 0f;
		}
	}
}