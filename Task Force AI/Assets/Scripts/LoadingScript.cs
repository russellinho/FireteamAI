using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class LoadingScript : MonoBehaviour {

	private string scene;
	private bool loadScene;
	private AsyncOperation async;
	private Slider loadingBar;

	// Use this for initialization
	void Start() {
		loadScene = false;
		loadingBar = GetComponentInChildren<Slider>();
		scene = PlayerPrefs.GetString("newScene", "mainMenu");
		PlayerPrefs.DeleteKey("newScene");
	}

	// Updates once per frame
	void Update() {

		if (loadScene == false) {

			loadScene = true;

			StartCoroutine(LoadNewScene());

		}


		if (loadScene == true) {
			loadingBar.value = async.progress;
		}

	}

	IEnumerator LoadNewScene() {
		try
		{
			async = SceneManager.LoadSceneAsync(scene);
		}
		catch (Exception e)
		{
			Debug.LogException(e, this);
			async = SceneManager.LoadSceneAsync("mainMenu");
		}


		while (!async.isDone) {
			yield return null;
		}
	}
}