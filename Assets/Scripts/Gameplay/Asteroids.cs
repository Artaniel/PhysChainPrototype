using UnityEngine;
using System.Collections;

public class Asteroids : MonoBehaviour {
	
	// ToDo list:
	// Добавить распределение по скоростям, отдельно нормы, отдельно флуктуации
	
	public GameObject AsteroidPrototype;//пока один прототип для всех астероидов. Потом усложню чтобы они были разнообразнее.
	public int MaxNumberOfAsteroids = 100;//сколько их может существовать вообще
	public float MaxDistFromPlayer = 1000;// Астероидов условно бесконечное количество. Дальше этого порога не симулируем.
	public Vector3 NormalSpeed; // Астероиды будут двигаться одним потоком с близкими скоростями. Это норма этой скорости.
	public float SpeedFluctuation;// Это отклонение от нормы скорости.
	private GameObject[] AsteroidBelt;
	public int CurrentNumberOfAsteroids = 0;
	public GameObject PlayerChar;
	public float VisualRange = 100; // Это радиус в котором нельзя спавнить чтобы игрок этого не видел
	public int SectorsNumber = 8; // Число отдельных секторов для подсчета плотности астероидного поля.
	public float DencityGrowMultiplier = 0.1f;//на сколько астероидов в секторе должно становится больше на каждый метр по Y
	public float BeltStartY = 0f;// Y начала пояса астероидов
	public float BeltStartDencity = 0f;//Опять в астероидах на сегмент. Номинальная плотность на BeltStartY
	public float MinSize = 0.1f;
	public float BeltStartMaxSize = 0.5f;
	public float MaxSizeGrowMultiplier = 0.01f;//скорость роста макс размера с Y
	
	void Start () {
		AsteroidBelt = new GameObject[MaxNumberOfAsteroids];
	}
	
	void Update () {
		CheckDistance();
	}
	
	void CheckDistance(){
		Vector3 PlayerPos = PlayerChar.transform.position;
		int[,] SectorContent = new int[SectorsNumber,CurrentNumberOfAsteroids+1];//сортируем по секторам. В 0 элеметне количество астероидов в секторе
		Vector3 CurrentAsteroidPosition;
		int CurrentSector;
		int i = 0;
		while(i<CurrentNumberOfAsteroids){			
			CurrentAsteroidPosition = AsteroidBelt[i].transform.position;
			if (Vector3.Distance(CurrentAsteroidPosition, PlayerPos)>MaxDistFromPlayer){
				KillAsteroid(i);
				i--;//мы убили астероид и поставили на его место в массиве другой, чтобы его тоже проверить надо откатиться на шаг
			}else{
				if (Vector3.Distance(CurrentAsteroidPosition, PlayerPos)>VisualRange){
					CurrentSector = (int)(Vector3.Angle(Vector3.up, CurrentAsteroidPosition-PlayerPos )/360*SectorsNumber);//в какой сектор отсчитывая от up попадает астероид
					//тут косяк. Угол между векторами от 0 до 180, а нам надо отсчитывать до 360 потому вводим доп чек на x. Если астероид левее игрока, то надо брать иначе
					if (CurrentAsteroidPosition.x<PlayerPos.x){
						CurrentSector = SectorsNumber - CurrentSector-1;
					}
					//Debug.Log(CurrentSector);
					SectorContent[CurrentSector,0]++;//В текущем секторе на 1 больше
					SectorContent[CurrentSector,SectorContent[CurrentSector,0]] = i;// Записываем номер чтобы знать кого сносить
				}
			}
			i++;
		}
		float DencityInCurrentSector;
		float SectorMidY;
		for (i=0; i<SectorsNumber; i++){
			// нужно определить плотность астероидов в зависимости от высоты сектора. Т.е. PlayerPos+модификатор зависящий от номера сектора
			// Пока возьму только от PlayerPos.y/10
			SectorMidY = (MaxDistFromPlayer+VisualRange)/2f*Mathf.Cos((i+0.5f)*2*Mathf.PI/SectorsNumber)+PlayerPos.y;
			if (SectorMidY>BeltStartY){
				DencityInCurrentSector = (SectorMidY-BeltStartY)*DencityGrowMultiplier+BeltStartDencity;
			}else{DencityInCurrentSector = 0;}
			if ((SectorContent[i,0]>DencityInCurrentSector)&&(SectorContent[i,0]!=0)){//+1 - допуск, т.к. PlayerPos.y/10 дробное и при увеличении/уменьшении на 1 может перескочить с > на <
				KillAsteroid(SectorContent[i,1]);
			}else{
				if (SectorContent[i,0]<(DencityInCurrentSector-1)){
					if (CurrentNumberOfAsteroids<MaxNumberOfAsteroids){
						MakeAsteroidInSector(VisualRange,MaxDistFromPlayer,i*360/SectorsNumber,(i+1)*360/SectorsNumber,CurrentNumberOfAsteroids);						
					}
				}
			}
		}		
	}
	
	void KillAsteroid(int index){
		Destroy(AsteroidBelt[index]);
		AsteroidBelt[index] = AsteroidBelt[CurrentNumberOfAsteroids-1];
		AsteroidBelt[index].name = "Asteroid"+index.ToString();
		AsteroidBelt[CurrentNumberOfAsteroids-1] = null;
		CurrentNumberOfAsteroids--;
		//Debug.Log("-"+index.ToString());
	}
	
	public void KillAsteroidExternal(GameObject target){//Нужно для удаления астероида другими скриптами, например при переработке коллектором
		//Debug.Log(target.name.Substring(8));
		int index = int.Parse(target.name.Substring(8));//8 - позиция начала номера астероида в имени, после "Asteroid"		
		KillAsteroid(index);
	}
	
//	void MakeAsteroid(int index){//от тут должен быть хитрый алгоритм Спавна новых астероидов. Пока будет ... бесхитростный
//		Vector3 AsteroidPosition;
//		bool correct;
//		do{
//			correct = true;
//			AsteroidPosition = PlayerChar.transform.position;
//			AsteroidPosition += new Vector3(2*(Random.value-0.5f)*MaxDistFromPlayer,2*(Random.value-0.5f)*MaxDistFromPlayer,0);
//			if (Vector3.Distance(PlayerChar.transform.position, AsteroidPosition)<VisualRange){
//				correct = false;
//			}
//			if (AsteroidPosition.y<0){//тут будут косяки. Вплоть до зависания игры если игрок ушел слишком низко. Да и вообще так низя
//				correct = false;
//			}
//		}while(!correct);
//		AsteroidBelt[index] = Instantiate(AsteroidPrototype) as GameObject;
//		AsteroidBelt[index].transform.position = AsteroidPosition;
//		AsteroidBelt[index].rigidbody.velocity = NormalSpeed + new Vector3(2*(Random.value-0.5f)*SpeedFluctuation,2*(Random.value-0.5f)*SpeedFluctuation,0);		
//	}
	
	void MakeAsteroidInSector(float MinRange, float MaxRange, float MinAngle, float MaxAngle, int index){
		float Range = Random.value*(MaxRange-MinRange) + MinRange;
		float Angle = Random.value*(MaxAngle-MinAngle) + MinAngle;
		Vector3 PlayerPos = PlayerChar.transform.position;
		Vector3 NewAsteroidPosition = PlayerPos + new Vector3(Range*Mathf.Sin(Angle/180*Mathf.PI),Range*Mathf.Cos(Angle/180*Mathf.PI),0);
		float Size = Random.value*((NewAsteroidPosition.y-BeltStartY)*MaxSizeGrowMultiplier+BeltStartMaxSize);
		if (Size<MinSize) {Size  = MinSize;}
		if (Physics.OverlapSphere(NewAsteroidPosition,Size).Length == 0){//чек не занято ли место под новым астероидом. Если занято - просто не создаем
			AsteroidBelt[index] = Instantiate(AsteroidPrototype) as GameObject;
			AsteroidBelt[index].transform.position = NewAsteroidPosition;
			AsteroidBelt[index].transform.parent = transform;
			AsteroidBelt[index].GetComponent<Rigidbody>().velocity = NormalSpeed + new Vector3(2*(Random.value-0.5f)*SpeedFluctuation,2*(Random.value-0.5f)*SpeedFluctuation,0);//пока жестко, потом буду модифицировать
			AsteroidBelt[index].name = "Asteroid"+index.ToString();
			AsteroidBelt[index].transform.localScale = new Vector3(Size,Size,Size);
			AsteroidBelt[index].GetComponent<Rigidbody>().mass = Size*Size*Size*100;
			//Debug.Log("+"+index.ToString());
			CurrentNumberOfAsteroids++;
		}
	}
	
}
