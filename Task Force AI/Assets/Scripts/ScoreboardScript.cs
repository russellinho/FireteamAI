using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardScript : MonoBehaviour {

	private int index;
	public GameObject namesCol;
	public GameObject killsCol;
	public GameObject deathsCol;

	private IEnumerator playerIterator;
	private Text[] names;
	private Text[] kills;
	private Text[] deaths;

	// Use this for initialization
	void Start () {
		index = 1;
		names = namesCol.GetComponentsInChildren<Text> ();
		kills = killsCol.GetComponentsInChildren<Text> ();
		deaths = deathsCol.GetComponentsInChildren<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (playerIterator == null) {
			playerIterator = GameControllerScript.totalKills.Keys.GetEnumerator ();
			return;
		}
		bool valid = playerIterator.MoveNext ();
		// This means it's reached the end
		if (index > 8) {
			index = 1;
			//playerIterator = GameControllerScript.totalKills.Keys.GetEnumerator ();
			playerIterator.Reset();
			return;
		}

		object c = (valid ? playerIterator.Current : null);
		if (c == null) {
			names [index].text = "";
			kills [index].text = "";
			deaths [index].text = "";
		} else {
			string name = (string)c;
			names [index].text = name;
			kills [index].text = "" + GameControllerScript.totalKills [name];
			deaths [index].text = "" + GameControllerScript.totalDeaths [name];
		}
		index++;
	}
}
