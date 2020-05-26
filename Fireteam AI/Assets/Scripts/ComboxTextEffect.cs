using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboxTextEffect : MonoBehaviour {

	[TextArea]
	public string m_text;
	[Range(0.01f, 0.1f)]
	public float m_characterInterval; //(in secs)

	private string m_partialText;
	private float m_cumulativeDeltaTime;
	private float disappearTime;

	private Text m_label;

	public GameObject parentRef;

	void Awake () {
		m_label = GetComponent<Text> ();
	}

	void OnEnable() {
		Reset ();
	}

	public void SetText(string message, string speaker) {
		Reset ();
		m_text = message;
		m_label.text = speaker;
		parentRef.GetComponent<AudioSource> ().Play ();
	}

	void Reset() {
		parentRef.GetComponent<AudioSource> ().Stop ();
		parentRef.GetComponent<AudioSource> ().time = 0f;
		m_partialText = "";
		m_cumulativeDeltaTime = 0;
		m_label.text = "";
		disappearTime = 4f;
	}

	void Update () {
		if (m_partialText.Length >= m_text.Length) {
			disappearTime -= Time.deltaTime;
			if (disappearTime <= 0f) {
				parentRef.SetActive (false);
			}
			return;
		}

		m_cumulativeDeltaTime += Time.deltaTime;
		while (m_cumulativeDeltaTime >= m_characterInterval && m_partialText.Length < m_text.Length) {
			m_partialText += m_text[m_partialText.Length];
			m_cumulativeDeltaTime -= m_characterInterval;
		}
		m_label.text = m_partialText;
	}

}