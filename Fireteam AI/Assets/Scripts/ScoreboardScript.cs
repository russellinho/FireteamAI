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
			playerIterator = GameControllerScript.playerList.Values.GetEnumerator();
			return;
		}
		// This mean that is has reached the end
		if (!playerIterator.MoveNext()) {
			if (index < 9) {
				names [index].text = "";
				kills [index].text = "";
				deaths [index].text = "";
				index++;
			} else {
				index = 1;
				playerIterator.Reset();
			}
		} else {
			PlayerStat curr = (PlayerStat)playerIterator.Current;
			if (curr.team == 'R') {
				names[index].color = Color.red;
				kills[index].color = Color.red;
				deaths[index].color = Color.red;
			} else if (curr.team == 'B') {
				names[index].color = Color.blue;
				kills[index].color = Color.blue;
				deaths[index].color = Color.blue;
			}
			names [index].text = curr.name;
			kills [index].text = "" + curr.kills;
			deaths [index].text = "" + curr.deaths;
			index++;
		}
	}
}
