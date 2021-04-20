using System.Collections;
using UnityEngine;

namespace Michsky.UI.Shift
{
    public class LayoutGroupPositionFix : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(FixPosAfter(0.7f));
        }

        IEnumerator FixPosAfter(float secs)
        {
            yield return new WaitForSeconds(secs);
            FixPos();
        }

        public void FixPos()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}