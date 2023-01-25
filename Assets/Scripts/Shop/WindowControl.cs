using UnityEngine;
using System.Collections;

public class WindowControl : MonoBehaviour {
	
	//ToDo list
	// Нужны Текстурки к окошками магазинов
	// Надо придумать как будет работать маяк, просто окошко - скучно
	// Довести отображение-расход баблоса до адекватных уровней
	
	public string status = "none";
	//"none" - всплывающих окон нет
	//"beltSelection" - окно маяка, выбор пояса астероидов
	//"workshop" - окно покупки движка и брони
	//"labaratory" - окно улучшения авионики, сканера и прочего разнообразного еще непридуманного
	public Messenger messenger;//ссылк на контейнер для переноса переменных в игровую сцену
	
	void OnGUI(){
		if (status == "beltSelection"){GUIBeltSelection();}
		if (status == "workshop"){GUIWorkshop();}
		if (status == "labaratory"){GUILab();}
	}

	void GUIBeltSelection(){
		GUI.Box(new Rect(Screen.width/2-150,Screen.height/2-50,300,100),"Location selection");		
		for (int i=0; i<3;i++){
			if (i == messenger.BeltNumber){
				GUI.Button(new Rect(Screen.width/2-150+20+i*30,Screen.height/2-50+30,20,20),"+");
			}else{
				if (GUI.Button(new Rect(Screen.width/2-150+20+i*30,Screen.height/2-50+30,20,20),(i+1).ToString())){					
					messenger.BeltNumber = i;
				}
			}
		}
		if(GUI.Button(new Rect(Screen.width/2-150+300-30,Screen.height/2-50+10,20,20),"x")){
			status = "none";
		}
	}
	
	void GUIWorkshop(){		
		GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-50,400,220),"");
		GUI.Label(new Rect(Screen.width/2-200+20,Screen.height/2-50+30,200,25),"Engine power level");
		for (int i=0; i<10;i++){
			if (i == messenger.EngineLvl){
				GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+60,20,20),"+");
			}else{				
				if (GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+60,20,20),(i+1).ToString())){					
					messenger.EngineLvl = i;
				}
			}
		}
		GUI.Label(new Rect(Screen.width/2-200+20,Screen.height/2-50+90,200,25),"Armor level");
		for (int i=0; i<10;i++){
			if (i == messenger.ArmorLvl){
				GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+120,20,20),"+");
			}else{				
				if (GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+120,20,20),(i+1).ToString())){					
					messenger.ArmorLvl = i;
				}
			}
		}
		GUI.Label(new Rect(Screen.width/2-200+20,Screen.height/2-50+150,200,25),"Avionics level");
		for (int i=0; i<10;i++){
			if (i == messenger.AvionicLvl){
				GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+180,20,20),"+");
			}else{				
				if (GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+180,20,20),(i+1).ToString())){					
					messenger.AvionicLvl = i;
				}
			}
		}
		if(GUI.Button(new Rect(Screen.width/2-200+400-30,Screen.height/2-50+10,20,20),"x")){
			status = "none";
		}		
	}
	
	void GUILab(){
		GUI.Box(new Rect(Screen.width/2-200,Screen.height/2-50,400,100),"Under Construction");
//		for (int i=0; i<10;i++){
//			if (i == messenger.AvionicLvl){
//				GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+30,20,20),"+");
//			}else{				
//				if (GUI.Button(new Rect(Screen.width/2-200+20+i*30,Screen.height/2-50+30,20,20),(i+1).ToString())){					
//					messenger.AvionicLvl = i;
//				}
//			}
//		}
		if(GUI.Button(new Rect(Screen.width/2-200+400-30,Screen.height/2-50+10,20,20),"x")){
			status = "none";
		}
	}
		
	public void LabPressed(){
		if (status == "none"){
			status = "labaratory";
		}
	}
	
	public void WorkshopPressed(){
		if (status == "none"){
			status = "workshop";
		}
	}
	
	public void LighthousePressed(){
		if (status == "none"){
			status = "beltSelection";
		}
	}
	
	
}
