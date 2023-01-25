using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GoHomeButton : MonoBehaviour { // not used right now, was used for returning to base (menu) scene from gameplay one	
	void OnMouseDown(){
		SceneManager.LoadScene("base");
	}
	
}
