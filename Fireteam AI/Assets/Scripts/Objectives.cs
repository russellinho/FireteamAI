﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Objectives {

	public string[] objectivesText;
	public int itemsRemaining;
	public int stepsLeftToCompletion;
	public int totalStepsToCompletion;
	public int escaperCount;
	public bool escapeAvailable;
	public float missionTimer1;
	public float missionTimer2;
	public float missionTimer3;
	public bool checkpoint1Passed;
	public bool checkpoint2Passed;
	public int selectedEvacIndex;

    public void LoadObjectives(int map)
    {
        if (map == 1)
        {
			itemsRemaining = 4;
			totalStepsToCompletion = 5;
			stepsLeftToCompletion = 5;
			objectivesText = new string[2];
			objectivesText[0] = "Defuse bombs around the city. (4)";
			objectivesText[1] = "Escape.";
        } else if (map == 2) {
			selectedEvacIndex = -2;
			totalStepsToCompletion = 3;
			stepsLeftToCompletion = 3;
			objectivesText = new string[3];
			objectivesText[0] = "Retrieve the pilot and secure a perimeter and defend yourselves until evac arrives.";
			objectivesText[1] = "Designate a landing zone for evac.";
			objectivesText[2] = "Escape with the pilot.";
			// Set the time before Cicadas start arriving - 1 min and 30 secs
			missionTimer1 = 90f;
		}
    }

	// Each mission has a unique set and number of objectives to get through
	public void UpdateObjectives(int map) {
		stepsLeftToCompletion--;

		if (map == 1) {
			if (stepsLeftToCompletion == 4) {
				objectivesText[0] = "Defuse bombs around the city. (3)";
				itemsRemaining = 3;
			} else if (stepsLeftToCompletion == 3) {
				objectivesText[0] = "Defuse bombs around the city. (2)";
				itemsRemaining = 2;
			} else if (stepsLeftToCompletion == 2) {
				objectivesText[0] = "Defuse bombs around the city. (1)";
				itemsRemaining = 1;
			} else if (stepsLeftToCompletion == 1) {
				objectivesText[0] = "Defuse bombs around the city. (0)";
				itemsRemaining = 0;
				RemoveObjective(0);
			} else if (stepsLeftToCompletion == 0) {
				RemoveObjective(1);
			}
		} else if (map == 2) {
			if (stepsLeftToCompletion == 2) {
				RemoveObjective(0);
			} else if (stepsLeftToCompletion == 1) {
				RemoveObjective(1);
			} else if (stepsLeftToCompletion == 0) {
				RemoveObjective(2);
			}
		}

	}

	public string GetObjectivesString() {
		string total = "";
		for (int i = 0; i < objectivesText.Length; i++) {
			if (i == objectivesText.Length - 1) {
				total += objectivesText[i];
			} else {
				total += objectivesText[i] + "\n";
			}
		}
		return total;
	}

	public void RemoveObjective(int index) {
		string temp = "";
		for (int i = 0; i < objectivesText [index].Length; i++) {
			temp = temp + objectivesText[index][i] + '\u0336';
		}
		objectivesText [index] = temp;
	}

	public float GetMissionProgress() {
		return (float)stepsLeftToCompletion / (float)totalStepsToCompletion;
	}
}
