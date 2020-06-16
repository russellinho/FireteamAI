using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BlackHawkScript : MonoBehaviour
{
    private const float NEXT_INSTRUCTION_DELAY = 2f;
    public GameControllerScript gameController;
    public enum ActionStates {Idle, Active, Pursue};
    public enum FlightMode {Ascend, Travel, Descend}
    public ActionStates actionState;
    public FlightMode flightMode;
    public Animator animator;
    public PhotonView pView;
    private Vector3 currentDestination;
    private float currentTimeLimit;
    private Vector3 originalPosBeforeTimeLimitSet;
    private Queue flightInstructions = new Queue();
    private float nextInstructionTimer;

    void Awake() {
        currentDestination = Vector3.negativeInfinity;
        currentTimeLimit = -1f;
    }

	void Update()
    {
        if (gameController.matchType == 'C')
        {
            UpdateForCampaign();
        } else if (gameController.matchType == 'V')
        {
            UpdateForVersus();
        }
    }

	void UpdateForCampaign() {
		if (!PhotonNetwork.IsMasterClient) {
			return;
		}
        ProcessFlightInstruction();
		DecideAction();
	}

	void UpdateForVersus() {
		if (!gameController.isVersusHostForThisTeam()) {
			return;
		}
        ProcessFlightInstruction();
		DecideAction();
	}

    void FixedUpdate() {
		if (gameController.matchType == 'C') {
			FixedUpdateForCampaign();
		} else if (gameController.matchType == 'V') {
			FixedUpdateForVersus();
		}
	}

	void FixedUpdateForCampaign() {
		DecideAnimation();
	}

	void FixedUpdateForVersus() {
		DecideAnimation();
	}

    void UpdateActionState(ActionStates action) {
		if (actionState != action) {
			pView.RPC("RpcUpdateActionState", RpcTarget.All, action, gameController.teamMap);
		}
	}

    void ProcessFlightInstruction() {
        // If queue isn't empty and the current destination isn't currently set
        if (nextInstructionTimer <= 0f && flightInstructions.Count != 0 && Vector3.Equals(currentDestination, Vector3.negativeInfinity)) {
            pView.RPC("RpcProcessFlightInstruction", RpcTarget.All, gameController.teamMap);
        }
        if (nextInstructionTimer > 0f) {
            nextInstructionTimer -= Time.deltaTime;
        }
    }

    [PunRPC]
    void RpcProcessFlightInstruction(string team) {
        if (team != gameController.teamMap) return;
        FlightInstruction f = (FlightInstruction)flightInstructions.Dequeue();
        flightMode = f.flightMode;
        currentTimeLimit = f.timeLimit;
        currentDestination = f.destination;
        if (f.timeLimit != -1f) {
            originalPosBeforeTimeLimitSet = transform.position;
        }
    }

	[PunRPC]
	private void RpcUpdateActionState(ActionStates action, string team) {
        if (team != gameController.teamMap) return;
		actionState = action;
	}

    void DecideAction() {
        if (!Vector3.Equals(currentDestination, Vector3.negativeInfinity)) {
            UpdateActionState(ActionStates.Pursue);
            PursueDestination();
        } else {
            UpdateActionState(ActionStates.Active);
        }
    }

    void DecideAnimation() {
        if (actionState == ActionStates.Idle) {
            animator.SetBool("isActive", false);
        } else {
            animator.SetBool("isActive", true);
        }
    }

    public void SetDestination(Vector3 d, bool clear, float timeLimit = -1f, FlightMode f = FlightMode.Ascend) {
        if (PhotonNetwork.IsMasterClient) {
            pView.RPC("RpcSetDestination", RpcTarget.All, d.x, d.y, d.z, clear, timeLimit, f, gameController.teamMap);
        }
    }

    [PunRPC]
    void RpcSetDestination(float x, float y, float z, bool clear, float timeLimit, FlightMode f, string team) {
        if (team != gameController.teamMap) return;
        if (clear) {
            currentDestination = Vector3.negativeInfinity;
        } else {
            FlightInstruction newFlightIns = new FlightInstruction(new Vector3(x, y, z), timeLimit, f);
        }
    }

    void PursueDestination() {
        Vector3 prevForward = transform.forward;
        transform.LookAt(currentDestination);
        Vector3 nextForward = transform.forward;
        transform.forward = prevForward;
        transform.forward = Vector3.Lerp(transform.forward, nextForward, Time.deltaTime * 2f);
        if (flightMode == FlightMode.Ascend) {
            Quaternion targetRot = Quaternion.Euler(8f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime);
        } else if (flightMode == FlightMode.Descend) {
            Quaternion targetRot = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime);
        } else if (flightMode == FlightMode.Travel) {
            Quaternion targetRot = Quaternion.Euler(8f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime);
        }
        if (currentTimeLimit == -1f) {
            transform.position = Vector3.Lerp(transform.position, currentDestination, Time.deltaTime * 5f);
        } else {
            transform.position = Vector3.Lerp(originalPosBeforeTimeLimitSet, currentDestination, Time.deltaTime / currentTimeLimit);
        }

        if (HasReachedDestination()) {
            nextInstructionTimer = NEXT_INSTRUCTION_DELAY;
            SetDestination(Vector3.negativeInfinity, true);
        }
    }

    bool HasReachedDestination() {
        if (Vector3.Distance(transform.position, currentDestination) <= 1f) {
            return true;
        }
        return false;
    }

    private struct FlightInstruction {
        public Vector3 destination;
        public FlightMode flightMode;
        public float timeLimit;
        public FlightInstruction(Vector3 destination, float timeLimit, FlightMode flightMode) {
            this.destination = destination;
            this.timeLimit = timeLimit;
            this.flightMode = flightMode;
        }
    }
}
