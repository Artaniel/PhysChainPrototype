using UnityEngine;
using System.Collections;

public class ScoreController : MonoBehaviour {
	
	public float PlayerScore = 0;
	
	void OnGUI(){
		GUI.Label(new Rect(5,15,100,25),((int)(PlayerScore*10)).ToString());
	}
	
	public void AddScore(float score){
		PlayerScore += score;
	}
}