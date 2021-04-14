using System;
using UnityEngine;

[Serializable]
public class NpcLook
{
    public float MinimumX;
    public float MaximumX;
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    private Quaternion m_CharacterTargetRot;
    public Quaternion m_SpineTargetRot;
    private float currentSpineX;
    private float currentSpineY;

    public void ResetRot() {
        currentSpineX = 0f;
        currentSpineY = 0f;
        m_CharacterTargetRot = Quaternion.identity;
        m_SpineTargetRot = Quaternion.identity;
    }

    // TODO: Needs to be networked
    public void LookRotation(Transform character, Transform spineTransform, Vector3 target, float xRot, float yRot) {
        if (xRot > 180f) {
            xRot = -(360f - xRot);
        }

        if (yRot > 180f) {
            yRot = -(360f - yRot);
        }
        
        if (yRot < 0f) {
            // If max spine rotation has been reached to the left, rotate character instead
            if (currentSpineY > -40f) {
                currentSpineY += yRot;
            } else {
                // m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
                if (!Vector3.Equals(target, Vector3.negativeInfinity)) {
                    character.LookAt(target);
                    character.rotation = Quaternion.Euler(0f, character.rotation.eulerAngles.y, 0f);
                }
            }
        } else {
            // If turning right
            // If max spine rotation has been reached to the right, rotate character instead
            if (currentSpineY < 40f) {
                currentSpineY += yRot;
            } else {
                // m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
                if (!Vector3.Equals(target, Vector3.negativeInfinity)) {
                    character.LookAt(target);
                    character.rotation = Quaternion.Euler(0f, character.rotation.eulerAngles.y, 0f);
                }
            }
        }
        
        currentSpineX = Mathf.Clamp(currentSpineX + xRot, MinimumX, MaximumX);
        // currentSpineY = Mathf.Clamp(currentSpineY + yRot, -40f, 40f);

        m_SpineTargetRot = Quaternion.Euler(currentSpineX / 3f, currentSpineY / 3f, 0f);
        m_SpineTargetRot = ClampRotationAroundXAxis(m_SpineTargetRot);

        spineTransform.localRotation = m_SpineTargetRot;
        // m_CharacterTargetRot = Quaternion.Euler(0f, m_CharacterTargetRot.eulerAngles.y, 0f);
        // character.rotation = m_CharacterTargetRot;
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

        angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}
