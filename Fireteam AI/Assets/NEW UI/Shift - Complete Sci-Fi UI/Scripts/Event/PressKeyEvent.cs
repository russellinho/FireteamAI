using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.Shift
{
    public class PressKeyEvent : MonoBehaviour
    {
        private const float KEY_PRESS_DELAY = 0.75f;
        public KeyMappingInput keyMappingInput;
        [Header("KEY")]
        [SerializeField]
        public KeyCode hotkey;
        public bool pressAnyKey;
		public bool invokeAtStart;

        [Header("KEY ACTION")]
        [SerializeField]
        public UnityEvent pressAction;
        private float keyPressDelay;
		
		void Start()
        {
            keyPressDelay = 0f;
            if (invokeAtStart == true)
                pressAction.Invoke();
        }

        void Update()
        {
            if (pressAnyKey == true)
            {
                if (keyMappingInput != null) {
                    keyPressDelay += Time.deltaTime;
                    if (Input.anyKeyDown) {
                        if (keyPressDelay >= KEY_PRESS_DELAY && GetComponent<ModalWindowManager>().isOn) {
                            keyMappingInput.HandleKeyChange();
                            pressAction.Invoke();
                            keyPressDelay = 0f;
                            keyMappingInput = null;
                        }
                    }
                }
            }

            else
            {
                if (Input.GetKeyDown(hotkey))
                    pressAction.Invoke();
            }
        }
    }
}