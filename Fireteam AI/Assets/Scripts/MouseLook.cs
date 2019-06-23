using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using Photon.Pun;
using Photon.Realtime;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX;
        public float MaximumX;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        public Quaternion m_SpineTargetRot;
        public Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;
        private float spineRotationRange;

        public PhotonView pView;

        public void Init(Transform character, Transform spineTransform)
        {
            spineRotationRange = 0f;
            m_CharacterTargetRot = character.localRotation;
            m_SpineTargetRot = spineTransform.localRotation;
            //m_CameraTargetRot = camera.localRotation;
        }

        public void ResetRot() {
            spineRotationRange = 0f;
            m_CharacterTargetRot = Quaternion.identity;
            m_SpineTargetRot = Quaternion.identity;
        }

        public void ResetSpineRotationRange()
        {
            spineRotationRange = 0f;
        }

        public void RealignHipsToSpine(Transform spineRotation) {
            Quaternion nextRotation = spineRotation.rotation;
            m_CharacterTargetRot = Quaternion.identity;
            m_SpineTargetRot = Quaternion.identity;
            //m_CharacterTargetRot = nextRotation;
        }

        public Vector3 LookRotation(Transform character, Transform spineTransform, Transform camera)
        {
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (m_SpineTargetRot.x);

            // If turning left
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
            m_SpineTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
            //m_CameraTargetRot *= Quaternion.Euler (-xRot, yRot, 0f);

            if (clampVerticalRotation)
                //m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);
                m_SpineTargetRot = ClampRotationAroundXAxis(m_SpineTargetRot);

            if(smooth)
            {
                spineTransform.localRotation = Quaternion.Slerp(spineTransform.localRotation, m_SpineTargetRot, smoothTime * Time.deltaTime);
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
                //camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
            }
            else
            {
                // If the player has turned his hips maximum to the right/left and is still rotating right/left, rotate the body instead
                // Else, rotate the hips to the right/left
                m_SpineTargetRot = Quaternion.Euler(m_SpineTargetRot.eulerAngles.x, m_SpineTargetRot.eulerAngles.y, m_SpineTargetRot.eulerAngles.x);
                spineTransform.localRotation = m_SpineTargetRot;
                m_CharacterTargetRot = Quaternion.Euler(0f, m_CharacterTargetRot.eulerAngles.y, 0f);
                character.localRotation = m_CharacterTargetRot;
                //camera.localRotation = m_CameraTargetRot;
                //camera.localRotation = Quaternion.Euler(spineTransform.localRotation.eulerAngles.x, spineTransform.localRotation.eulerAngles.y, 0f);
            }

            if (xRot != 0f || yRot != 0f)
            {
                return new Vector3(m_SpineTargetRot.eulerAngles.x, m_SpineTargetRot.eulerAngles.y, m_SpineTargetRot.eulerAngles.x);
            }
            return Vector3.negativeInfinity;

            //UpdateCursorLock();
        }

        public void NetworkedLookRotation(Transform spineTransform, float spineXRot, float spineYRot, float spineZRot)
        {
            m_SpineTargetRot = Quaternion.Euler(spineXRot, spineYRot, spineZRot);
            spineTransform.localRotation = m_SpineTargetRot;
        }

        public void LookRotationClient(Transform spineTransform) {
            //m_SpineTargetRot = Quaternion.Euler(m_SpineTargetRot.eulerAngles.x, m_SpineTargetRot.eulerAngles.y, m_SpineTargetRot.eulerAngles.x);
            spineTransform.localRotation = m_SpineTargetRot;
        }

        /**public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if(!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }*/

        /**public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }*/

        /**private void InternalLockUpdate()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
				m_cursorIsLocked = !m_cursorIsLocked;
				Cursor.visible = !Cursor.visible;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
            }
        }*/

        Quaternion ClampRotationAroundXAxis(Quaternion q)
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
}
