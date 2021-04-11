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
    private float spineRotationRange;
    public void Init(Transform character, Transform spineTransform)
    {
        spineRotationRange = 0f;
        m_CharacterTargetRot = character.localRotation;
        m_SpineTargetRot = spineTransform.localRotation;
    }

    public void ResetRot() {
        spineRotationRange = 0f;
        m_CharacterTargetRot = Quaternion.identity;
        m_SpineTargetRot = Quaternion.identity;
    }

    // TODO: Needs to be networked
    public void LookRotation(Transform character, Transform spineTransform, float xRot, float yRot) {
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (m_SpineTargetRot.x);

        if (yRot < 0f) {
            // If max spine rotation has been reached to the left, rotate character instead
            if (spineRotationRange > -50f && angleX < 10f && angleX > -20f) {
                spineRotationRange += yRot;
                m_SpineTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            } else {
                m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            }
        } else {
            // If turning right
            // If max spine rotation has been reached to the right, rotate character instead
            if (spineRotationRange < 20f && angleX < 10f && angleX > -20f) {
                spineRotationRange += yRot;
                m_SpineTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            } else {
                m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            }
        }

        m_SpineTargetRot *= Quaternion.Euler(xRot, 0f, 0f);
        m_SpineTargetRot = ClampRotationAroundXAxis(m_SpineTargetRot);

        spineTransform.localRotation = m_SpineTargetRot;
        m_CharacterTargetRot = Quaternion.Euler(0f, m_CharacterTargetRot.eulerAngles.y, 0f);
        character.localRotation = m_CharacterTargetRot;

        if (xRot != 0f || yRot != 0f)
        {
            Vector3 spineRotRet = new Vector3(m_SpineTargetRot.eulerAngles.x, m_SpineTargetRot.eulerAngles.y, m_SpineTargetRot.eulerAngles.x);
            Vector3 charRotRet = new Vector3(m_CharacterTargetRot.eulerAngles.x, m_CharacterTargetRot.eulerAngles.y, m_CharacterTargetRot.eulerAngles.z);
        }
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
