using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BuildingsAll : MonoBehaviour { // not used now, was used to transfer to gameplay scene
	
	public WindowControl windowContlrol;
	
	void OnMouseDown(){
		if (name == "Port") {
			SceneManager.LoadScene("main");
		}
		if (name == "Labaratory") {
			windowContlrol.LabPressed();
		}
		if (name == "Workshop") {
			windowContlrol.WorkshopPressed();
		}
		if (name == "Lighthouse") {
			windowContlrol.LighthousePressed();
		}
		
	}
}
