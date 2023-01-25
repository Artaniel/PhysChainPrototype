using UnityEngine;
using System.Collections;

public class HarponController : MonoBehaviour {
	
	public ChainController Chain;
	
	void OnCollisionEnter(Collision Col){
		Chain.HarponHitSomething(Col.gameObject);
	}
	
	void OnMouseDown(){
		Chain.HarpoonClicked();
	}
		
}
