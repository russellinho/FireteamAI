using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.Shift
{
    public class PressKeyEvent : MonoBehaviour
    {
        public KeyMappingInput keyMappingInput;
        [Header("KEY")]
        [SerializeField]
        public KeyCode hotkey;
        public bool pressAnyKey;
		public bool invokeAtStart;

        [Header("KEY ACTION")]
        [SerializeField]
        public UnityEvent pressAction;
		
		void Start()
        {
            if (invokeAtStart == true)
                pressAction.Invoke();
        }

        void Update()
        {
            if (pressAnyKey == true)
            {
                if (Input.anyKeyDown) {
                    pressAction.Invoke();
                    if (keyMappingInput != null) {
                        keyMappingInput.HandleKeyChange();
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