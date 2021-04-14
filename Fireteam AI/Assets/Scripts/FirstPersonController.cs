using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;
using Koobando.AntiCheat;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        private const float MAX_JETPACK_BOOST_TIME = 1.5f;
        private const float JETPACK_DELAY = 0.2f;
        [SerializeField] public bool m_IsWalking;
        [SerializeField] public bool m_IsCrouching;
    	[SerializeField] public bool m_IsRunning;
        [SerializeField] public bool m_IsMoving;
        private bool m_IsIncapacitated;
        private bool m_IsSwimming;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private EncryptedFloat m_JumpSpeed;
        private float m_SwimGravity = 3f;
        private float m_SwimSpeed = 2f;
        private float m_SwimUpSpeed = 4f;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] public MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        private Terrain terrainUnderneath;
        public TerrainSounds terrainSounds;

        private Camera m_Camera;
        private EncryptedBool m_Jump;
        private float m_YRotation;
        public Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private Vector3 m_DashDir = Vector3.zero;
        public CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private EncryptedBool m_Jumping;
        public AudioSource m_AudioSource;
        public bool sprintLock;

        public bool canMove;
        public WeaponActionScript weaponActionScript;
        public EquipmentScript equipmentScript;
        public PlayerScript playerScript;
        public PlayerActionScript playerActionScript;
        public PhotonView photonView;

        public Transform charTransform;
        public Transform spineTransform;
        public Transform fpcTransformSpine;
        public Transform fpcTransformBody;
        public Transform headTransform;
        public Animator animator;
        public Animator fpcAnimator;

        private int networkDelay = 5;
        private int networkDelayCount = 0;
        private bool meleeSlowActive;
        // Time after initial jump you must wait before using the booster
        private float jetpackBoostDelay;
        // Holds the time you've been using the boost for. Can't go past max time
        private float jetpackBoostTimer;

        private bool initialized;

        // Use this for initialization
        public void Initialize()
        {
            if (animator.gameObject.activeInHierarchy) {
                animator.SetBool("onTitle", false);
            }
            m_MouseLook.Init(charTransform, spineTransform, fpcTransformSpine, fpcTransformBody);
            if (photonView != null && !photonView.IsMine) {
				//this.enabled = false;
                initialized = true;
                return;
            }
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_IsCrouching = false;
            m_JumpSpeed = 10f;
			canMove = true;
			sprintLock = false;
            initialized = true;
            StartCoroutine("StartGetTerrainWalkingOn");
        }


        // Update is called once per frame
        private void Update()
        {
            if (!initialized) {
                return;
            }
            if (photonView != null && !photonView.IsMine)
            {
                return;
            }
            //RotateView();
            // the jump state needs to read here to make sure it is not missed
			if (!m_Jump && canMove && !playerActionScript.hud.container.pauseMenuGUI.pauseActive && IsFullyMobile() && playerActionScript.lastStandTimer <= 0f)
            {
                m_Jump = PlayerPreferences.playerPreferences.KeyWasPressed("Jump");
            }

            // Handle character landing
            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
                fpcAnimator.SetBool("Jumping", false);
                // Calculate fall damage
                playerActionScript.DetermineFallDamage();
                // Reset vertical velocity before landing
                playerActionScript.ResetVerticalVelocityBeforeLanding();
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

        }

        void LateUpdate() {
            if (!initialized) {
                return;
            }
            if (photonView != null && !photonView.IsMine)
            {
                ClientRotateView();
                return;
            }
            if (playerActionScript.health > 0) {
                Rotations rotAngles = RotateView();
                if (networkDelayCount < 3)
                {
                    networkDelayCount++;
                }
                if (!Vector3.Equals(rotAngles.spineRot, Vector3.negativeInfinity) && networkDelayCount == 3)
                {
                    networkDelayCount = 0;
                    photonView.RPC("RpcUpdateSpineRotation", RpcTarget.Others, rotAngles.spineRot.x, rotAngles.spineRot.y, rotAngles.spineRot.z,
                    rotAngles.charRot.x, rotAngles.charRot.y, rotAngles.charRot.z);
                }
            }
        }


        private void PlayLandingSound()
        {
            GetTerrainWalkingOn();
            int terrainType = GetTerrainTypeFromCurrentTerrain();
            if (terrainType != -1) {
                m_AudioSource.clip = terrainSounds.landSounds[terrainType];
                m_AudioSource.Play();
                m_NextStep = m_StepCycle + .5f;
            }
        }


        private void FixedUpdate()
        {
            if (!initialized) {
                return;
            }
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

            Vector3 desiredMove = fpcTransformBody.forward*m_Input.y + fpcTransformBody.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            if (m_DashDir.Equals(Vector3.zero)) {
                m_MoveDir.x = desiredMove.x*speed;
                m_MoveDir.z = desiredMove.z*speed;
            } else {
                m_MoveDir = m_DashDir;
            }

            // Water physics
            if (m_IsSwimming) {
                if (m_Jump) {
                    m_MoveDir.y += (m_SwimUpSpeed);
                    m_MoveDir.y = Math.Min(m_MoveDir.y, m_SwimUpSpeed);
                    m_Jump = false;
                }

                m_MoveDir.y -= (Time.fixedDeltaTime * m_SwimGravity);
                m_MoveDir.y = Math.Max(m_MoveDir.y, -m_SwimGravity);
            } else {
                // Ground physics
                if (m_CharacterController.isGrounded)
                {
                    playerActionScript.ToggleJetpackParticleEffect(false);
                    playerActionScript.hud.RemoveActiveSkill("213");
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {
                        if (m_IsCrouching)
                        {
                            m_IsCrouching = false;
                            playerActionScript.HandleJumpAfterCrouch();
                        }
                        else
                        {
                            m_MoveDir.y = m_JumpSpeed * (1f + playerActionScript.skillController.GetJumpBoost());
                            PlayJumpSound();
                            m_Jumping = true;
                            // animator.SetTrigger("Jump");
                            TriggerJumpInAnimator();
                            jetpackBoostTimer = MAX_JETPACK_BOOST_TIME;
                            jetpackBoostDelay = JETPACK_DELAY;
                        }
                        m_Jump = false;
                    }
                }
                else
                {
                    jetpackBoostDelay -= Time.fixedDeltaTime;
                    if (playerActionScript.skillController.HasJetpackBoost() && jetpackBoostDelay <= 0f && jetpackBoostTimer > 0f && PlayerPreferences.playerPreferences.KeyWasPressed("Jump", true)) {
                        playerActionScript.ToggleJetpackParticleEffect(true);
                        playerActionScript.hud.AddActiveSkill("213", 0f);
                        m_MoveDir -= Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime/2f;
                        jetpackBoostTimer -= Time.fixedDeltaTime;
                    } else {
                        playerActionScript.ToggleJetpackParticleEffect(false);
                        playerActionScript.hud.RemoveActiveSkill("213");
                        m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
                    }
                }
            }
            
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
           // UpdateCameraPosition(speed);

            //m_MouseLook.UpdateaLock();
        }


        private void PlayJumpSound()
        {
            GetTerrainWalkingOn();
            int terrainType = GetTerrainTypeFromCurrentTerrain();
            if (terrainType != -1) {
                m_AudioSource.clip = terrainSounds.jumpSounds[terrainType];
                m_AudioSource.Play();
            }
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

            if (m_IsSwimming) {
                PlaySwimAudio();
            } else {
                if (playerActionScript.IsInWater()) {
                    PlayWaterFootstepAudio();
                } else {
                    PlayFootStepAudio();
                }
            }
        }

        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            int terrainType = GetTerrainTypeFromCurrentTerrain();
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            if (terrainType != -1) {
                AudioClip[] chosenTerrain = terrainSounds.GetTerrainFootsteps(terrainType);
                int n = Random.Range(1, chosenTerrain.Length);
                m_AudioSource.clip = chosenTerrain[n];
                m_AudioSource.PlayOneShot(m_AudioSource.clip);
                // move picked sound to index 0 so it's not picked next time
                chosenTerrain[n] = chosenTerrain[0];
                chosenTerrain[0] = m_AudioSource.clip;
            }
        }

        private void PlaySwimAudio()
        {
            if (m_AudioSource.clip == playerActionScript.audioController.swimSound1) {
                m_AudioSource.clip = playerActionScript.audioController.swimSound2;
            } else {
                m_AudioSource.clip = playerActionScript.audioController.swimSound1;
            }

            m_AudioSource.PlayOneShot(m_AudioSource.clip);
        }

        private void PlayWaterFootstepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }

            int r = Random.Range(0, 3);
            if (r == 0) {
                m_AudioSource.clip = playerActionScript.audioController.waterFootstep1;
            } else if (r == 1) {
                m_AudioSource.clip = playerActionScript.audioController.waterFootstep2;
            } else {
                m_AudioSource.clip = playerActionScript.audioController.waterFootstep3;
            }

            m_AudioSource.PlayOneShot(m_AudioSource.clip);
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

        public void DashMove(Vector3 dashDir) {
            m_DashDir = dashDir;
        }

        public void EndDash() {
            m_DashDir = Vector3.zero;
        }

        public bool IsFullyMobile() {
            NpcScript n = null;
            if (playerActionScript.objectCarrying != null) {
                n = playerActionScript.objectCarrying.GetComponent<NpcScript>();
                if (n != null) {
                    return !n.immobileWhileCarrying;
                }
            }
            return true;
        }

        private void GetInput(out float speed)
		{
            bool enableRunFlag = IsFullyMobile();

			// Read input
            float horizontal = 0f;
            if (PlayerPreferences.playerPreferences.KeyWasPressed("Left", true)) {
                horizontal = -1f;
            } else if (PlayerPreferences.playerPreferences.KeyWasPressed("Right", true)) {
                horizontal = 1f;
            }
            float vertical = 0f;
            if (PlayerPreferences.playerPreferences.KeyWasPressed("Backward", true)) {
                vertical = -1f;
            } else if (PlayerPreferences.playerPreferences.KeyWasPressed("Forward", true)) {
                vertical = 1f;
            }

            // Nullify movement if the user is paused
            if (playerActionScript.hud.container.pauseMenuGUI.pauseActive) {
                horizontal = 0f;
                vertical = 0f;
            }

			bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
			// On standalone builds, walk/run speed is modified by a key press.
			// keep track of whether or not the character is walking or running
            // if (weaponActionScript == null) {
            //     weaponActionScript = GetComponent<WeaponActionScript>();
            // }
			if (weaponActionScript != null && (weaponActionScript.isAiming && weaponActionScript.weaponMetaData.steadyAim)) {
				if (!m_IsCrouching && !m_IsSwimming && !m_IsIncapacitated) {
					m_IsWalking = true;
					m_IsRunning = false;
				} else {
					m_IsWalking = false;
					m_IsRunning = false;
				}
			} else {
				if (!m_IsCrouching && m_CharacterController.isGrounded) {
					if (!m_IsSwimming && !m_IsIncapacitated && PlayerPreferences.playerPreferences.KeyWasPressed("Walk", true)) {
						m_IsWalking = true;
						m_IsRunning = false;
					} else if (!m_IsSwimming && !m_IsIncapacitated && PlayerPreferences.playerPreferences.KeyWasPressed("Sprint", true) && vertical > 0f && playerActionScript.sprintTime > 0f && !sprintLock && enableRunFlag) {
						m_IsWalking = false;
						m_IsRunning = true;
                        // if (weaponActionScript.isReloading || weaponActionScript.isCocking) {
                        //     SwitchToSprintingInAnimator();
                        // }
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
            if (m_IsSwimming) {
                speed = m_SwimSpeed;
            } else if (m_IsIncapacitated) {
                speed = playerActionScript.totalSpeedBoost / 4f;
            } else if (m_IsRunning) {
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
            if (meleeSlowActive) {
                speed = 1f;
            }
			if (!canMove)
				speed = 0f;

            if (!enableRunFlag) {
                NpcScript n = playerActionScript.objectCarrying.GetComponent<NpcScript>();
                if (n != null) {
                    speed *= n.weightSpeedReduction;
                }
            }
        }

        public void SetMouseDynamicsForMelee(bool b) {
            meleeSlowActive = b;
            if (b) {
                m_MouseLook.XSensitivity = 0.1f;
                m_MouseLook.YSensitivity = 0.1f;
            } else {
                m_MouseLook.XSensitivity = m_MouseLook.originalXSensitivity;
                m_MouseLook.YSensitivity = m_MouseLook.originalYSensitivity;
            }
        }

        private void ClientRotateView() {
            m_MouseLook.LookRotationClient (spineTransform, charTransform);
        }

        private Rotations RotateView()
        {
            return m_MouseLook.LookRotation (charTransform, spineTransform, fpcTransformSpine, fpcTransformBody, playerActionScript.hud.container.pauseMenuGUI.pauseActive);
        }

        [PunRPC]
        private void RpcUpdateSpineRotation(float xSpineRot, float ySpineRot, float zSpineRot, float xCharRot, float yCharRot, float zCharRot)
        {
            m_MouseLook.NetworkedLookRotation(spineTransform, xSpineRot, ySpineRot, zSpineRot, charTransform, xCharRot, yCharRot, zCharRot);
        }

        public void SetMovingInAnimator(int x) {
            fpcAnimator.SetFloat("MoveSpeed", 2f);
            fpcAnimator.SetBool("Moving", (x == 0 ? false : true));
            if (fpcAnimator.GetInteger("MovingDir") == x || !canMove) return;
            fpcAnimator.SetInteger("MovingDir", x);
            photonView.RPC("RpcSetMovingInAnimator", RpcTarget.Others, x);
        }

        [PunRPC]
        private void RpcSetMovingInAnimator(int x) {
            animator.SetInteger("Moving", x);
        }

        public void SetIncapacitatedInAnimator(bool x)
        {
            if (animator.GetBool("Incapacitated") == x) return;
            photonView.RPC("RpcSetIncapacitatedInAnimator", RpcTarget.All, x);
        }

        [PunRPC]
        void RpcSetIncapacitatedInAnimator(bool x)
        {
            animator.SetBool("Incapacitated", x);
            if (x) {
                animator.ResetTrigger("EnteredWater");
                animator.SetInteger("Moving", 0);
                animator.SetBool("Crouching", false);
                animator.SetBool("isSprinting", false);
                animator.SetBool("isDead", false);
                animator.SetBool("isWalking", false);
                animator.SetBool("Swimming", false);
            }
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
            fpcAnimator.SetBool("Crouching", x);
            photonView.RPC("RpcSetCrouchingInAnimator", RpcTarget.Others, x);
            if (x) {
                fpcAnimator.SetFloat("MoveSpeed", 0.5f);
            } else {
                fpcAnimator.SetFloat("MoveSpeed", 2f);
            }
        }

        public void PlayFiringInFPCAnimator() {
            fpcAnimator.Play("Firing");
        }

        [PunRPC]
        private void RpcSetCrouchingInAnimator(bool x) {
            animator.SetBool("Crouching", x);
        }

        public void SetSprintingInAnimator(bool x) {
            if (playerActionScript.skillController.HasRunNGun()) {
                if (x) {
                    fpcAnimator.SetFloat("MoveSpeed", 4f);
                } else {
                    fpcAnimator.SetFloat("MoveSpeed", 2f);
                }
                return;
            }
            if (fpcAnimator.GetBool("Sprinting") == x) return;
            fpcAnimator.SetBool("Sprinting", x);
            photonView.RPC("RpcSetSprintingInAnimator", RpcTarget.Others, x);
        }

        void SwitchToSprintingInAnimator() {
            fpcAnimator.CrossFade("Sprinting", 0.01f);
        }

        [PunRPC]
        private void RpcSetSprintingInAnimator(bool x) {
            animator.SetBool("isSprinting", x);
        }

        public void SetIsDeadInAnimator(bool x) {
            if (fpcAnimator.GetBool("isDead") == x) return;
            photonView.RPC("RpcSetIsDeadInAnimator", RpcTarget.All, x);
        }

        [PunRPC]
        private void RpcSetIsDeadInAnimator(bool x) {
            animator.SetBool("isDead", x);
            animator.Play("Death", 0);
            animator.Play("Death", 1);
        }

        public void SetWalkingInAnimator(bool x) {
            if (fpcAnimator.GetBool("isWalking") == x) return;
            fpcAnimator.SetBool("isWalking", x);
            photonView.RPC("RpcSetWalkingInAnimator", RpcTarget.Others, x);
            if (x) {
                fpcAnimator.SetFloat("MoveSpeed", 0.5f);
            } else {
                fpcAnimator.SetFloat("MoveSpeed", 2f);
            }
        }

        public void SetSwimmingInAnimator(bool x)
        {
            photonView.RPC("RpcSetSwimmingInAnimator", RpcTarget.All, x);
        }

        [PunRPC]
        private void RpcSetWalkingInAnimator(bool x) {
            animator.SetBool("isWalking", x);
        }

        [PunRPC]
        private void RpcSetSwimmingInAnimator(bool x)
        {
            if (x) {
                animator.SetTrigger("EnteredWater");
            }
            animator.SetBool("Swimming", x);
        }

        public void TriggerJumpInAnimator() {
            fpcAnimator.SetBool("Jumping", true);
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
            animator.SetTrigger("Reload");
        }

        public void SyncAnimatorValues(int weaponType, int moving, bool weaponReady, bool crouching, bool sprinting, bool dead, bool walking, bool swimming) {
            photonView.RPC("RpcSyncAnimatorValues", RpcTarget.Others, weaponType, moving, weaponReady, crouching, sprinting, dead, walking, swimming);
        }

        [PunRPC]
        private void RpcSyncAnimatorValues(int weaponType, int moving, bool weaponReady, bool crouching, bool sprinting, bool dead, bool walking, bool swimming) {
            animator.SetInteger("WeaponType", weaponType);
            animator.SetInteger("Moving", moving);
            animator.SetBool("weaponReady", weaponReady);
            animator.SetBool("Crouching", crouching);
            animator.SetBool("isSprinting", sprinting);
            animator.SetBool("isDead", dead);
            animator.SetBool("isWalking", walking);
            animator.SetBool("Swimming", swimming);
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
            animator.SetBool("Incapacitated", false);
            animator.ResetTrigger("EnteredWater");
            animator.SetInteger("WeaponType", 1);
            animator.SetInteger("Moving", 0);
            animator.SetBool("weaponReady", false);
            animator.SetBool("Crouching", false);
            animator.SetBool("isSprinting", false);
            animator.SetBool("isDead", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("Swimming", false);
            animator.Play("IdleAssaultRifle", 0);
            animator.Play("Idle", 1);
        }

        public void SetAiminginFPCAnimator(bool x) {
            fpcAnimator.SetBool("Aiming", x);
        }

        public void ResetFPCAnimator(int currentlyEquippedType) {
            fpcAnimator.SetBool("Aiming", false);
            fpcAnimator.SetBool("weaponReady", false);
            fpcAnimator.SetBool("Moving", false);
            fpcAnimator.SetBool("Sprinting", false);
            fpcAnimator.SetBool("Crouching", false);
            fpcAnimator.SetBool("isDead", false);
            fpcAnimator.SetBool("isWalking", false);
            fpcAnimator.SetBool("Jumping", false);
            fpcAnimator.SetBool("Swimming", false);
            fpcAnimator.SetInteger("WeaponType", currentlyEquippedType);
            fpcAnimator.SetInteger("MovingDir", 0);
            fpcAnimator.ResetTrigger("Reload");
            fpcAnimator.ResetTrigger("CockShotgun");
            fpcAnimator.ResetTrigger("CockBoltAction");
            fpcAnimator.ResetTrigger("isCockingGrenade");
            fpcAnimator.ResetTrigger("ThrowGrenade");
            fpcAnimator.ResetTrigger("UseBooster");
            fpcAnimator.ResetTrigger("HolsterWeapon");
            fpcAnimator.ResetTrigger("EnteredWater");
        }

        public void SetSwimmingInFPCAnimator(bool b)
        {
            if (b) {
                fpcAnimator.SetTrigger("EnteredWater");
            }
            fpcAnimator.SetBool("Swimming", b);
        }

        void GetTerrainWalkingOn()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 3f)) {
                terrainUnderneath = hit.transform.gameObject.GetComponent<Terrain>();
            } else {
                terrainUnderneath = null;
            }
        }

        IEnumerator StartGetTerrainWalkingOn()
        {
            if (this.enabled) {
                GetTerrainWalkingOn();
            }
            yield return new WaitForSeconds(0.6f);
            if (this.enabled) {
                GetTerrainWalkingOn();
            }
            StartCoroutine("StartGetTerrainWalkingOn");
        }

        int GetTerrainTypeFromCurrentTerrain()
        {
            if (terrainUnderneath == null) return -1;
            return (int)terrainUnderneath.terrainType;
        }

        public void SetIsSwimming(bool b)
        {
            if (m_IsIncapacitated) {
                b = false;
            }
            m_IsSwimming = b;
            if (b) {
                m_IsWalking = false;
                m_IsCrouching = false;
    	        m_IsRunning = false;
                m_IsMoving = false;
                m_Jump = false;
            }
        }

        public bool GetIsSwimming()
        {
            return m_IsSwimming;
        }

        public void SetIsIncapacitated(bool b)
        {
            m_IsIncapacitated = b;
            if (b) {
                m_IsSwimming = false;
                m_IsWalking = false;
                m_IsCrouching = false;
                m_IsRunning = false;
                m_IsMoving = false;
                m_Jump = false;
            }
        }

        public bool GetIsIncapacitated()
        {
            return m_IsIncapacitated;
        }

    }
}
