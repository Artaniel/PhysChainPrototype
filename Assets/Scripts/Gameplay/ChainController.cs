using UnityEngine;
using System.Collections;

public class ChainController : MonoBehaviour {	
	public string status="start";//строка в которой храним в каком состоянии мы сейчас:
	//"start" - начальное состояние, гарпун в корабле, цепи нет.
	//"launched" - Гарпун запущен, но еще никуда не попал, цепь генерится за ним
	//"missed" - цепь достигла макс длины, а гарпун никуда не попал, просто болтается в пространстве, присоединится ни к чему уже не может. Или гарпун уже отцеплен вручную.
	//"connected" - Гарпун попал в астероид, связан с ним, цепь сформирована
	//"solid" - Цепь напряжена, орисовывается не отдельными физ телами, а тупым лерпом
	//"restarting" - цепь плавно возвращается в корабль
	public int maxChainLength = 50;// max number of chain links
	public int currentChainLength = 0;
	public float chainStep = 0.1f;//nominal distance between chain links
	public float lunchSpeedMultipier = 1;
	public GameObject harpoon;
	private GameObject[] chain;	
	public GameObject chainCellPrototype;
	public float solidationDistanceModifer = 1; // multiplier to distance, after wich chain goes solid
	public float desolidationDistanceModifer = 1; // same for reverse process, solid to flexible
	private int solidationStage = 0;
	public int solidationTimeFrames = 10; // how much time solidation process take in frames
	public GameObject chainContainer;
	private LineRenderer line; // for rope or chain line renderer
	private float restartingPhase = 0;
	public float restartTime = 0.5f;
	
	void Start () {
		chain = new GameObject[maxChainLength+1];// chain[0] is for harpoon, so number of indexes is +1 from number of chain links
		chain[0] = harpoon; //todo instantiate harpoon from prefab
		for(int i=1; i<=maxChainLength; i++){
			chain[i] = Instantiate(chainCellPrototype) as GameObject;
			chain[i].name = "Rope" + i.ToString(); // todo change to string builder
			chain[i].transform.parent = chainContainer.transform;
		}
		line = gameObject.GetComponent<LineRenderer>();
	}
	
	void Update () {
		if (status == "launched"){			//self made state machine
			float delta = Vector3.Distance(transform.position, chain[currentChainLength].transform.position);
			int cellsToAdd = (int)(delta / chainStep);
			if (cellsToAdd + currentChainLength <= maxChainLength){
				if (delta > chainStep){
					for (int i = 1; i<= cellsToAdd; i++){
						chain[currentChainLength+i].transform.position = Vector3.Lerp(chain[currentChainLength].transform.position, transform.position, i*(float)chainStep/delta);
						CreateCharJoint(chain[currentChainLength+i],chain[currentChainLength+i-1]);
						chain[currentChainLength+i].GetComponent<Rigidbody>().velocity = PlayerController.instance.gameObject.GetComponent<Rigidbody>().velocity;
					}
					currentChainLength += cellsToAdd;
				}
			}else{
				status = "missed";				
				CreateCharJoint(gameObject,chain[currentChainLength]);
			}
		}
		if (status == "connected"){
			if (Vector3.Distance(transform.position, harpoon.transform.position)> solidationDistanceModifer*chainStep*currentChainLength){
				status = "solid";
				solidationStage = 0;
			}
		}
		if (status == "solid"){
			if (solidationStage>solidationTimeFrames){
				for (int i = 1; i<= currentChainLength; i++){
					chain[i].transform.position = Vector3.Lerp(harpoon.transform.position, transform.position, (float)i/(float)currentChainLength);
				}
			}else{
				solidationStage++;
				for (int i = 1; i<= currentChainLength; i++)
					chain[i].transform.position = Vector3.Lerp( // this lerp for smothnes of movement to new position
						(Vector3.Lerp(harpoon.transform.position, transform.position, (float)i/(float)currentChainLength)) // this lerp for defining target position for specific chain link
						,chain[i].transform.position,0.5f);
			}
			if (Vector3.Distance(transform.position, harpoon.transform.position)< desolidationDistanceModifer*chainStep*currentChainLength){
				status = "connected";
				for (int i = 1; i<= currentChainLength; i++){
					chain[i].GetComponent<Rigidbody>().velocity = Vector3.Lerp(harpoon.GetComponent<Rigidbody>().velocity, PlayerController.instance.gameObject.GetComponent<Rigidbody>().velocity, (float)i/(float)currentChainLength);
				}
			}
		}
		if ((status == "missed")&&(Vector3.Distance(harpoon.transform.position,transform.position)<0.5f)){
			status = "restarting";
			restartingPhase = 0;			
		}
		if (status == "restarting"){
			RestartChainUpdate();
		}
		DrawRope();
	}
	
	void FixedUpdate(){
		if (status == "solid"){
			PullBackSolid();
		}
	}
	
	void PullBackSolid(){ // much physics and geometry here. It equals momentums in projection on axis of chain to prevent its extending
		Vector3 deltaPos = harpoon.transform.position - transform.position;
		Vector3 deltaV = harpoon.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity;		
		float alpha = Vector3.Angle(deltaPos, deltaV);
		Vector3 deltaVNormal =  deltaV.magnitude* Mathf.Cos(alpha/180*Mathf.PI)* deltaPos / deltaPos.magnitude; // diference in velociti in projection of axis of chain
		float hMass = harpoon.GetComponent<Rigidbody>().mass + harpoon.GetComponent<CharacterJoint>().connectedBody.GetComponent<Rigidbody>().mass;//mass of Harpoon + mass of asteroid connected to it
		float pMass = GetComponent<Rigidbody>().mass;
		if (Vector3.Angle(deltaVNormal,deltaPos)<90){
			GetComponent<Rigidbody>().velocity += deltaVNormal * hMass / (hMass + pMass);
			harpoon.GetComponent<Rigidbody>().velocity += - deltaVNormal * pMass / (hMass + pMass);
			harpoon.GetComponent<CharacterJoint>().connectedBody.GetComponent<Rigidbody>().velocity += - deltaVNormal * pMass / (hMass + pMass);
		}
	}
	
	public void LaunchChain(Vector3 target){ //target is target position set by input
		harpoon.transform.position = transform.position;
		harpoon.GetComponent<Rigidbody>().velocity = lunchSpeedMultipier * target + PlayerController.instance.GetComponent<Rigidbody>().velocity;
		harpoon.transform.LookAt(transform.position + target);
		harpoon.transform.Rotate(90,0,0);
		status = "launched";
	}
	
	public void HarpoonClicked(){
		DisconnectHarpoon();
	}
	
	public void HarponHitSomething(GameObject target){
		if ((status == "launched")&&(target.name.Contains("Asteroid"))) {
			ConnectChain(target);
			status = "connected";
			CreateCharJoint(gameObject,chain[currentChainLength]);
		}		
	}
	
	public void DisconnectHarpoon(){
		Destroy(harpoon.GetComponent<CharacterJoint>());
		status = "missed";
	}
	
	void ConnectChain(GameObject target){
		CreateCharJoint(harpoon,target);
	}
	
	void CreateCharJoint(GameObject hostOfConnection, GameObject connectedObject){
		CharacterJoint jointConnection = hostOfConnection.AddComponent<CharacterJoint>();		
		jointConnection.anchor = Vector3.zero;
		jointConnection.connectedBody = connectedObject.GetComponent<Rigidbody>();
		jointConnection.axis = new Vector3(0,0,1);	//rotation available only in plane of screen
	}
	
	void RestartChainUpdate(){
		if (restartingPhase == 0){
			for (int i = 1; i<=currentChainLength; i++){
				Destroy(chain[currentChainLength-i+1].GetComponent<CharacterJoint>());
				chain[currentChainLength-i+1].GetComponent<Rigidbody>().velocity = Vector3.zero;
				chain[currentChainLength-i+1].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			}
			harpoon.GetComponent<Rigidbody>().velocity = Vector3.zero;
			harpoon.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			Destroy(gameObject.GetComponent<CharacterJoint>());
		}
		restartingPhase+= Time.deltaTime;
		if (restartingPhase<restartTime){
			for (int i = 1; i<=currentChainLength; i++){
			chain[currentChainLength-i+1].transform.position = Vector3.Lerp(chain[currentChainLength-i+1].transform.position, transform.position, restartingPhase/restartTime );// вообще это криво, но работать будет
			harpoon.transform.position = transform.position;
		}
		}else{
			for (int i = 1; i<=currentChainLength; i++){
				chain[currentChainLength-i+1].transform.Translate(0,0,-100);
			}
			harpoon.transform.position = new Vector3(0,0,-100);
			currentChainLength = 0;
			status = "start";	
		}		
	}
	
	void DrawRope(){
		if (status != "start") {
			line.positionCount = currentChainLength + 2;
			line.SetPosition(0,harpoon.transform.position);
			for (int i = 1; i<= currentChainLength; i++){
				line.SetPosition(i,chain[i].transform.position);
			}
			line.SetPosition(currentChainLength+1,transform.position);
		}
		else line.positionCount = 0;
	}
}
