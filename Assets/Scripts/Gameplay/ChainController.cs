using UnityEngine;
using System.Collections;

public class ChainController : MonoBehaviour {// Скрипт для поведения цепи с гарпуном
	
	//ToDo List:
	//Сматывание/разматывание цепи
	
	public string status="start";//строка в которой храним в каком состоянии мы сейчас:
	//"start" - начальное состояние, гарпун в корабле, цепи нет.
	//"launched" - Гарпун запущен, но еще никуда не попал, цепь генерится за ним
	//"missed" - цепь достигла макс длины, а гарпун никуда не попал, просто болтается в пространстве, присоединится ни к чему уже не может. Или гарпун уже отцеплен вручную.
	//"connected" - Гарпун попал в астероид, связан с ним, цепь сформирована
	//"solid" - Цепь напряжена, орисовывается не отдельными физ телами, а тупым лерпом
	//"restarting" - цепь плавно возвращается в корабль
	public int MaxChainLength = 50;// максимальное число звеньев цепи
	public int CurrentChainLength = 0;// сколько звеньев задейственно сейчас
	public float ChainStep = 0.1f;//Номинальное растояние между звениями цепи, если напряжена могут быть больше, но стремятся к этому
	public float LunchSpeedMultipier = 1;//коэффициент между натяжением мышкой при з-апуске и скоростью пуска
	public GameObject Harpoon;// Гарпун, он же конец цепи
	private GameObject[] Chain;	// цепь есть массив звеньев (ваш КО)
	public GameObject ChainCellPrototype;// образец звена
	public float SolidationDistanceModifer = 1; // множитель показывающий на какой дистанции цепь становится твердой.
	public float DesolidationDistanceModifer = 1; // Тоже для процсса перехода из твердой в гибкую
	private int SolidationStage = 0;//Нужно чтобы затвердение цепи было не мгновенным, конкретно эта переменная  -текущая стадия затвердения
	public int SolidationTimeFrames = 10;// Это всего сколько фреймов на затвердение
	public GameObject ChainContainer;//просто чтобы убрать объекты из корня дерева иерархии пакуем их сюда
	private LineRenderer Line;//тут ссылк на компонент для рисования линии веревки
	private float RestartingPhase = 0;
	public float RestartTime = 0.5f;
	
	void Start () {
		Chain = new GameObject[MaxChainLength+1];//нулевое место для гарпуна
		Chain[0] = Harpoon; 
		for(int i=1; i<=MaxChainLength; i++){
			Chain[i] = Instantiate(ChainCellPrototype) as GameObject;
			Chain[i].name = "Rope" + i.ToString();
			Chain[i].transform.parent = ChainContainer.transform;
		}
		Line = gameObject.GetComponent<LineRenderer>();
	}
	
	void Update () {
		if (status == "launched"){			
			float Delta = Vector3.Distance(transform.position, Chain[CurrentChainLength].transform.position);
			int CellsToAdd = (int)(Delta / ChainStep);
			if (CellsToAdd + CurrentChainLength <= MaxChainLength){
				if (Delta > ChainStep){
					for (int i = 1; i<= CellsToAdd; i++){
						Chain[CurrentChainLength+i].transform.position = Vector3.Lerp(Chain[CurrentChainLength].transform.position, transform.position, i*(float)ChainStep/Delta);
						CreateCharJoint(Chain[CurrentChainLength+i],Chain[CurrentChainLength+i-1]);
						Chain[CurrentChainLength+i].GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
					}
					CurrentChainLength += CellsToAdd;
				}
			}else{
				status = "missed";				
				CreateCharJoint(gameObject,Chain[CurrentChainLength]);
			}
		}
		if (status == "connected"){
			if (Vector3.Distance(transform.position, Harpoon.transform.position)> SolidationDistanceModifer*ChainStep*CurrentChainLength){
				status = "solid";
				SolidationStage = 0;
			}
		}
		if (status == "solid"){
			if (SolidationStage>SolidationTimeFrames){
				for (int i = 1; i<= CurrentChainLength; i++){
					Chain[i].transform.position = Vector3.Lerp(Harpoon.transform.position, transform.position, (float)i/(float)CurrentChainLength);
				}
			}else{
				SolidationStage++;
				for (int i = 1; i<= CurrentChainLength; i++){
					Chain[i].transform.position = Vector3.Lerp((Vector3.Lerp(Harpoon.transform.position, transform.position, (float)i/(float)CurrentChainLength)),Chain[i].transform.position,0.5f);
					// ищем позицию посредине между старым положением и местом куда должна стать точка после натяжения цепи, делаем так в SolidationTimeFrames фреймах подряд.
					//Сделано для менее внезапного перехода на жесткую позицию
				}
			}
			if (Vector3.Distance(transform.position, Harpoon.transform.position)< DesolidationDistanceModifer*ChainStep*CurrentChainLength){
				status = "connected";
				for (int i = 1; i<= CurrentChainLength; i++){
					Chain[i].GetComponent<Rigidbody>().velocity = Vector3.Lerp(Harpoon.GetComponent<Rigidbody>().velocity, GetComponent<Rigidbody>().velocity, (float)i/(float)CurrentChainLength);
				}
			}
		}
		if ((status == "missed")&&(Vector3.Distance(Harpoon.transform.position,transform.position)<0.5f)){
			status = "restarting";
			RestartingPhase = 0;			
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
	
	void PullBackSolid(){//В случае максимального натяжения цепи должно выровнять импульсы так чтобы цепь прекратила растягиваться, а вращение сохранилось
		Vector3 DeltaPos = Harpoon.transform.position - transform.position;
		Vector3 DeltaV = Harpoon.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity;		
		float alpha = Vector3.Angle(DeltaPos, DeltaV);
		//Debug.Log(Mathf.Cos(alpha/180*Mathf.PI));
		Vector3 DeltaVNormal =  DeltaV.magnitude* Mathf.Cos(alpha/180*Mathf.PI)* DeltaPos / DeltaPos.magnitude;		
		float HMass = Harpoon.GetComponent<Rigidbody>().mass + Harpoon.GetComponent<CharacterJoint>().connectedBody.GetComponent<Rigidbody>().mass;//маса гарпуна + того к кому он присоединен
		float PMass = GetComponent<Rigidbody>().mass;
		if (Vector3.Angle(DeltaVNormal,DeltaPos)<90){
			GetComponent<Rigidbody>().velocity += DeltaVNormal * HMass / (HMass + PMass);
			Harpoon.GetComponent<Rigidbody>().velocity += - DeltaVNormal * PMass / (HMass + PMass);
			Harpoon.GetComponent<CharacterJoint>().connectedBody.GetComponent<Rigidbody>().velocity += - DeltaVNormal * PMass / (HMass + PMass);
		}
	}
	
	public void LaunchChain(Vector3 target){//target тут - позиция в которую запускаем гарпун, высщитанная из координат мышки, уже относительно корабля
		Harpoon.transform.position = transform.position;
		Harpoon.GetComponent<Rigidbody>().velocity = LunchSpeedMultipier * target + GetComponent<Rigidbody>().velocity;
		Harpoon.transform.LookAt(transform.position + target);
		Harpoon.transform.Rotate(90,0,0);
		status = "launched";
	}
	
	public void HarpoonClicked(){
		DisconnectHarpoon();
	}
	
	public void HarponHitSomething(GameObject target){
		if ((status == "launched")&&(target.name.Contains("Asteroid"))) {
			ConnectChain(target);
			status = "connected";
			CreateCharJoint(gameObject,Chain[CurrentChainLength]);
		}		
	}
	
	public void DisconnectHarpoon(){
		Destroy(Harpoon.GetComponent<CharacterJoint>());
		status = "missed";
	}
	
	void ConnectChain(GameObject target){
		CreateCharJoint(Harpoon,target);
		//Потом сюда добавлю какие то довороты гарпуна. Возможно визуальные эффекты.
	}
	
	void CreateCharJoint(GameObject OnWho, GameObject ConnectedToWho){
		CharacterJoint JointConnection = OnWho.AddComponent<CharacterJoint>();		
		JointConnection.anchor = Vector3.zero;
		JointConnection.connectedBody = ConnectedToWho.GetComponent<Rigidbody>();
		JointConnection.axis = new Vector3(0,0,1);	//значит что связь может вразщаться только в плоскости экрана	
	}
	
	void RestartChainUpdate(){
		if (RestartingPhase == 0){
			for (int i = 1; i<=CurrentChainLength; i++){
				Destroy(Chain[CurrentChainLength-i+1].GetComponent<CharacterJoint>());
				Chain[CurrentChainLength-i+1].GetComponent<Rigidbody>().velocity = Vector3.zero;
				Chain[CurrentChainLength-i+1].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			}
			Harpoon.GetComponent<Rigidbody>().velocity = Vector3.zero;
			Harpoon.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			Destroy(gameObject.GetComponent<CharacterJoint>());
		}
		RestartingPhase+= Time.deltaTime;
		if (RestartingPhase<RestartTime){
			for (int i = 1; i<=CurrentChainLength; i++){
			Chain[CurrentChainLength-i+1].transform.position = Vector3.Lerp(Chain[CurrentChainLength-i+1].transform.position, transform.position, RestartingPhase/RestartTime );// вообще это криво, но работать будет
			Harpoon.transform.position = transform.position;
		}
		}else{
			for (int i = 1; i<=CurrentChainLength; i++){
				Chain[CurrentChainLength-i+1].transform.Translate(0,0,-100);
			}
			Harpoon.transform.position = new Vector3(0,0,-100);
			CurrentChainLength = 0;
			status = "start";	
		}		
	}
	
	void DrawRope(){
		if (status != "start") {		
			Line.SetVertexCount(CurrentChainLength+2);	
			Line.SetPosition(0,Harpoon.transform.position);
			for (int i = 1; i<= CurrentChainLength; i++){
				Line.SetPosition(i,Chain[i].transform.position);
			}
			Line.SetPosition(CurrentChainLength+1,transform.position);
		}
		else Line.SetVertexCount(0);
	}
}
