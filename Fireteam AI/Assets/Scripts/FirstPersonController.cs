using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] public bool m_IsWalking;
        [SerializeField] public bool m_IsCrouching;
    	  [SerializeField] public bool m_IsRunning;
        [SerializeField] public bool m_IsMoving;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] public MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        public Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;
    		public bool sprintLock;

    		public bool canMove;
        public WeaponActionScript weaponActionScript;
        public EquipmentScript equipmentScript;
        public PlayerScript playerScript;
        public PlayerActionScript playerActionScript;
        public PhotonView photonView;

        public Transform charTransform;
        public Transform spineTransform;
        public Transform fpcTransform;
        public Transform headTransform;
        public Animator animator;
        public Animator fpcAnimator;

        private int networkDelay = 5;
        private int networkDelayCount = 0;

        // Use this for initialization
        private void Start()
        {
            if (animator.gameObject.activeInHierarchy) {
                animator.SetBool("onTitle", false);
            }
            m_MouseLook.Init(charTransform, spineTransform, fpcTransform);
            if (photonView != null && !photonView.IsMine) {
				//this.enabled = false;
                return;
            }
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_IsCrouching = false;
            m_AudioSource = GetComponent<AudioSource>();
			canMove = true;
			sprintLock = false;

        }


        // Update is called once per frame
        private void Update()
        {
            if (photonView != null && !photonView.IsMine)
            {
                return;
            }
            //RotateView();
            // the jump state needs to read here to make sure it is not missed
			if (!m_Jump && canMove)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

        }

        void LateUpdate() {
            if (photonView != null && !photonView.IsMine)
            {
                ClientRotateView();
                return;
            }
            if (playerActionScript.health > 0) {
                Vector3 spineRotAngles = RotateView();
                if (networkDelayCount < 5)
                {
                    networkDelayCount++;
                }
                if (!Vector3.Equals(spineRotAngles, Vector3.negativeInfinity) && networkDelayCount == 5)
                {
                    networkDelayCount = 0;
                    photonView.RPC("RpcUpdateSpineRotation", RpcTarget.Others, spineRotAngles.x, spineRotAngles.y, spineRotAngles.z);
                }
            }
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            if (photonView != null && !photonView.IsMine)
            {
                return;
            }
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            // reorient the body to the camera forward
            //m_Camera.transform.forward = new Vector3(transform.forward.x, m_Camera.transform.forward.y, transform.forward.z);
            //transform.forward = m_Camera.transform.forward;
            if (m_Input.x < 0f && m_Input.y > 0f)
            {
                // Move forward-left
                //animator.SetInteger("Moving", 5);
                SetMovingInAnimator(5);
                if (m_IsWalking) {
                    //animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.x > 0f && m_Input.y > 0f)
            {
                // Move forward-right
                //animator.SetInteger("Moving", 6);
                SetMovingInAnimator(6);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.x < 0f && m_Input.y < 0f)
            {
                // Move backwards-left
                // animator.SetInteger("Moving", 7);
                SetMovingInAnimator(7);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.x > 0f && m_Input.y < 0f)
            {
                // Move backwards-right
                // animator.SetInteger("Moving", 8);
                SetMovingInAnimator(8);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.x > 0f)
            {
                // Move right
                // animator.SetInteger("Moving", 3);
                SetMovingInAnimator(3);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.x < 0f)
            {
                // Move left
                // animator.SetInteger("Moving", 2);
                SetMovingInAnimator(2);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.y > 0f)
            {
                // Move forward
                // animator.SetInteger("Moving", 1);
                SetMovingInAnimator(1);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else if (m_Input.y < 0f)
            {
                // Move backwards
                // animator.SetInteger("Moving", 4);
                SetMovingInAnimator(4);
                if (m_IsWalking) {
                    // animator.SetBool("isWalking", true);
                    SetWalkingInAnimator(true);
                }
            }
            else
            {
                // animator.SetInteger("Moving", 0);
                SetMovingInAnimator(0);
                // animator.SetBool("isWalking", false);
                SetWalkingInAnimator(false);
            }

            Vector3 desiredMove = m_Camera.transform.forward*m_Input.y + m_Camera.transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    if (m_IsCrouching)
                    {
                        // animator.SetBool("Crouching", false);
                        SetCrouchingInAnimator(false);
                        m_IsCrouching = false;
                    }
                    else
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        PlayJumpSound();
                        m_Jumping = true;
                        // animator.SetTrigger("Jump");
                        TriggerJumpInAnimator();
                    }
                    m_Jump = false;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
           // UpdateCameraPosition(speed);

            //m_MouseLook.UpdateaLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
		{

			// Read input
			float horizontal = CrossPlatformInputManager.GetAxis ("Horizontal");
			float vertical = CrossPlatformInputManager.GetAxis ("Vertical");

			bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
			// On standalone builds, walk/run speed is modified by a key press.
			// keep track of whether or not the character is walking or running
            // if (weaponActionScript == null) {
            //     weaponActionScript = GetComponent<WeaponActionScript>();
            // }
			if (weaponActionScript != null && weaponActionScript.isAiming) {
				if (!m_IsCrouching) {
					m_IsWalking = true;
					m_IsRunning = false;
				} else {
					m_IsWalking = false;
					m_IsRunning = false;
				}
			} else {
				if (!m_IsCrouching && m_CharacterController.isGrounded) {
					if (Input.GetKey(KeyCode.C)) {
						m_IsWalking = true;
						m_IsRunning = false;
					} else if (Input.GetKey(KeyCode.LeftShift) && vertical > 0f && playerActionScript.sprintTime > 0f && !sprintLock) {
						m_IsWalking = false;
						m_IsRunning = true;
					} else {
						m_IsWalking = false;
						m_IsRunning = false;
					}
				}
			}
#endif
			// set the desired speed to be walking or running
			//speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
			speed = playerActionScript.totalSpeedBoost;
			if (m_IsRunning) {
				speed = playerActionScript.totalSpeedBoost * 2f;
			} else if (m_IsCrouching || m_IsWalking) {
                speed = playerActionScript.totalSpeedBoost / 3f;
            }

            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
			if (!canMove)
				speed = 0f;
        }

        private void ClientRotateView() {
            m_MouseLook.LookRotationClient (spineTransform);
        }

        private Vector3 RotateView()
        {
            return m_MouseLook.LookRotation (charTransform, spineTransform, fpcTransform);
        }

        [PunRPC]
        private void RpcUpdateSpineRotation(float xSpineRot, float ySpineRot, float zSpineRot)
        {
            m_MouseLook.NetworkedLookRotation(spineTransform, xSpineRot, ySpineRot, zSpineRot);
        }

        public void SetMovingInAnimator(int x) {
            if (fpcAnimator.GetInteger("MovingDir") == x || !canMove) return;
            fpcAnimator.SetBool("Moving", (x == 0 ? false : true));
            fpcAnimator.SetInteger("MovingDir", x);
            photonView.RPC("RpcSetMovingInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetMovingInAnimator(int x) {
            animator.SetInteger("Moving", x);
        }

        public void SetWeaponTypeInAnimator(int x) {
            if (fpcAnimator.GetInteger("WeaponType") == x) return;
            photonView.RPC("RpcSetWeaponTypeInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetWeaponTypeInAnimator(int x) {
            animator.SetInteger("WeaponType", x);
        }

        public void SetWeaponReadyInAnimator(bool x) {
            if (fpcAnimator.GetBool("weaponReady") == x) return;
            fpcAnimator.SetBool("weaponReady", x);
            photonView.RPC("RpcSetWeaponReadyInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetWeaponReadyInAnimator(bool x) {
            animator.SetBool("weaponReady", x);
        }

        public void SetCrouchingInAnimator(bool x) {
            if (fpcAnimator.GetBool("Crouching") == x) return;
            photonView.RPC("RpcSetCrouchingInAnimator", RpcTarget.Others, x);
        }

        public void PlayFiringInFPCAnimator() {
            fpcAnimator.Play("Firing");
        }

        [PunRPC]
        private void RpcSetCrouchingInAnimator(bool x) {
            animator.SetBool("Crouching", x);
        }

        public void SetSprintingInAnimator(bool x) {
            if (fpcAnimator.GetBool("Sprinting") == x) return;
            fpcAnimator.SetBool("Sprinting", x);
            photonView.RPC("RpcSetSprintingInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetSprintingInAnimator(bool x) {
            animator.SetBool("isSprinting", x);
        }

        public void SetIsDeadInAnimator(bool x) {
            if (fpcAnimator.GetBool("isDead") == x) return;
            photonView.RPC("RpcSetIsDeadInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetIsDeadInAnimator(bool x) {
            animator.SetBool("isDead", x);
            animator.Play("Death", 0);
            animator.Play("Death", 1);
        }

        public void SetWalkingInAnimator(bool x) {
            if (fpcAnimator.GetBool("isWalking") == x) return;
            photonView.RPC("RpcSetWalkingInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetWalkingInAnimator(bool x) {
            animator.SetBool("isWalking", x);
        }

        public void TriggerJumpInAnimator() {
            photonView.RPC("RpcTriggerJumpInAnimator", RpcTarget.Others);
        }

        [PunRPC]
        private void RpcTriggerJumpInAnimator() {
            animator.SetTrigger("Jump");
        }

        public void TriggerReloadingInAnimator() {
            photonView.RPC("RpcTriggerReloadingInAnimator", RpcTarget.Others);
        }

        [PunRPC]
        private void RpcTriggerReloadingInAnimator() {
            animator.SetTrigger("Reloading");
        }

        public void SyncAnimatorValues(int weaponType, int moving, bool weaponReady, bool crouching, bool sprinting, bool dead, bool walking) {
            photonView.RPC("RpcSyncAnimatorValues", RpcTarget.Others, weaponType, moving, weaponReady, crouching, sprinting, dead, walking);
        }

        [PunRPC]
        private void RpcSyncAnimatorValues(int weaponType, int moving, bool weaponReady, bool crouching, bool sprinting, bool dead, bool walking) {
            animator.SetInteger("WeaponType", weaponType);
            animator.SetInteger("Moving", moving);
            animator.SetBool("weaponReady", weaponReady);
            animator.SetBool("Crouching", crouching);
            animator.SetBool("isSprinting", sprinting);
            animator.SetBool("isDead", dead);
            animator.SetBool("isWalking", walking);
        }

        // private void OnControllerColliderHit(ControllerColliderHit hit)
        // {
        //     Rigidbody body = hit.collider.attachedRigidbody;
        //     //dont move the rigidbody if the character is on top of it
        //     if (m_CollisionFlags == CollisionFlags.Below)
        //     {
        //         return;
        //     }

        //     if (body == null || body.isKinematic)
        //     {
        //         return;
        //     }
        //     body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        // }

        public void ResetAnimationState() {
            photonView.RPC("RpcResetAnimationState", RpcTarget.Others);
        }

        [PunRPC]
        void RpcResetAnimationState()
        {
            animator.SetInteger("WeaponType", 1);
            animator.SetInteger("Moving", 0);
            animator.SetBool("weaponReady", false);
            animator.SetBool("Crouching", false);
            animator.SetBool("isSprinting", false);
            animator.SetBool("isDead", false);
            animator.SetBool("isWalking", false);
            animator.Play("IdleAssaultRifle", 0);
            animator.Play("IdleAssaultRifle", 1);
        }

        public void SetAiminginFPCAnimator(bool x) {
            fpcAnimator.SetBool("Aiming", x);
        }

        public void ResetFPCAnimator() {
            fpcAnimator.SetBool("Aiming", false);
            fpcAnimator.SetBool("weaponReady", false);
            fpcAnimator.SetBool("Moving", false);
            fpcAnimator.SetBool("Sprinting", false);
            fpcAnimator.SetBool("Crouching", false);
            fpcAnimator.SetBool("isSprinting", false);
            fpcAnimator.SetBool("isDead", false);
            fpcAnimator.SetBool("isWalking", false);
            fpcAnimator.SetInteger("WeaponType", 1);
            fpcAnimator.SetInteger("MovingDir", 0);
        }

    }
}
