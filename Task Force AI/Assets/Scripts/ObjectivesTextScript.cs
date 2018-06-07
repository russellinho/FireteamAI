using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivesTextScript {

	public Objective[] objectives;

    public ObjectivesTextScript()
    {
        objectives = null;
    }

    public string LoadObjectives(int map, int numberRemaining)
    {
        if (map == 1)
        {
			objectives = new Objective[2];
			objectives[0] = new Objective("Defuse bombs around the city. (" + numberRemaining + ")");
			objectives[1] = new Objective("Escape.");
			if (numberRemaining == 0)
				RemoveObjective (0);
            return "" + objectives[0].text + "\n" + objectives[1].text;
        }

        return "null objectives";
    }

	public void RemoveObjective(int index) {
		objectives [index].finished = true;
		string temp = "";
		for (int i = 0; i < objectives [index].text.Length; i++) {
			temp = temp + objectives[index].text[i] + '\u0336';
		}
		objectives [index].text = temp;
	}

	public class Objective {
		public string text;
		public bool finished;

		public Objective(string text) {
			finished = false;
			this.text = text;
		}
	}
}
