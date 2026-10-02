using UnityEngine;
using System.Collections.Generic;

// FK animation of separately modelled limbs; pivots are made from the Blender mesh parts.
public class AvatarMotion : MonoBehaviour
{
    Transform leftArm,rightArm,leftLeg,rightLeg;Quaternion la,ra,ll,rl;
    public bool Walking,Seated,Carrying,Dining;public float Gesture;
    public static bool Frozen;
    Transform cup,tray;
    Transform mealBowl,chopsticks;Renderer bowlFood;float mealClock,reception;
    public int MealDish=-1;
    public float MealClock {get{return mealClock;}}
    public bool CupVisible {get{return cup.gameObject.activeSelf;}}
    public bool FoodVisible {get{return mealBowl.gameObject.activeSelf&&chopsticks.gameObject.activeSelf;}}
    public Quaternion MealArmPose {get{return rightArm.localRotation;}}
    public string LifeAction {get{return !Dining?"歇息":reception>0?"收到餐點":TeaMeal?"喝茶":"用餐";}}
    bool TeaMeal {get{return MealDish==0||MealDish==4||MealDish==6;}}
    bool arriving,departing;Vector3 destination,exit;float eating=-1,phase;
    void Awake()
    {
        leftArm=Pivot("Shoulder.L",new Vector3(-.31f,1.28f,0));rightArm=Pivot("Shoulder.R",new Vector3(.31f,1.28f,0));
        leftLeg=Pivot("Hip.L",new Vector3(-.15f,.56f,0));rightLeg=Pivot("Hip.R",new Vector3(.15f,.56f,0));
        foreach(var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            string n=renderer.name;Vector3 p=transform.InverseTransformPoint(renderer.bounds.center);
            if(n.StartsWith("Sleeve")||n.StartsWith("Hand"))renderer.transform.SetParent(p.x<0?leftArm:rightArm,true);
            else if(n.StartsWith("Leg")||n.StartsWith("Shoe"))renderer.transform.SetParent(p.x<0?leftLeg:rightLeg,true);
        }
        la=leftArm.localRotation;ra=rightArm.localRotation;ll=leftLeg.localRotation;rl=rightLeg.localRotation;
        cup=TeaHouseWorld.Shape("Held tea cup",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.18f,.12f,.18f),new Color(.93f,.88f,.72f)).transform;
        cup.SetParent(rightArm,false);cup.localPosition=new Vector3(0,-.45f,.12f);cup.gameObject.SetActive(false);
        mealBowl=new GameObject("Held meal bowl").transform;mealBowl.SetParent(leftArm,false);mealBowl.localPosition=new Vector3(.12f,-.38f,.24f);
        var bowlBody=TeaHouseWorld.Shape("Dining bowl",PrimitiveType.Sphere,Vector3.zero,new Vector3(.27f,.14f,.27f),new Color(.93f,.88f,.72f)).transform;bowlBody.SetParent(mealBowl,false);
        var food=TeaHouseWorld.Shape("Dining food",PrimitiveType.Sphere,Vector3.zero,new Vector3(.22f,.07f,.22f),new Color(.9f,.85f,.67f)).transform;food.SetParent(mealBowl,false);food.localPosition=Vector3.up*.055f;bowlFood=food.GetComponent<Renderer>();
        chopsticks=new GameObject("Held dining utensils").transform;chopsticks.SetParent(rightArm,false);chopsticks.localPosition=new Vector3(0,-.45f,.12f);chopsticks.localRotation=Quaternion.Euler(25,0,-15);
        for(int side=-1;side<=1;side+=2){var stick=TeaHouseWorld.Shape("Dining chopstick",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.015f,.16f,.015f),new Color(.46f,.29f,.18f)).transform;stick.SetParent(chopsticks,false);stick.localPosition=new Vector3(side*.025f,0,0);stick.localRotation=Quaternion.Euler(90,0,0);}
        mealBowl.gameObject.SetActive(false);chopsticks.gameObject.SetActive(false);
        tray=TeaHouseWorld.Shape("Serving tray",PrimitiveType.Cube,Vector3.zero,new Vector3(.7f,.045f,.45f),new Color(.38f,.24f,.13f)).transform;
        tray.SetParent(transform,false);tray.localPosition=new Vector3(0,.95f,.48f);tray.gameObject.SetActive(false);
        var bowl=TeaHouseWorld.Shape("Tray bowl",PrimitiveType.Sphere,Vector3.zero,new Vector3(.28f,.16f,.28f),new Color(.93f,.88f,.72f)).transform;bowl.SetParent(tray,false);bowl.localPosition=new Vector3(0,2,0);bowl.localScale=new Vector3(.4f,3,.6f);
    }
    Transform Pivot(string name,Vector3 pos){var g=new GameObject(name);g.transform.SetParent(transform,false);g.transform.localPosition=pos;return g.transform;}
    public void Arrive(Vector3 seat)
    {destination=seat;arriving=true;Seated=false;}
    public void DepartAfterMeal(float seconds,Vector3 door){eating=seconds;exit=door;}
    public void SetMeal(int dish,bool received)
    {
        MealDish=Mathf.Clamp(dish,0,6);Dining=true;mealClock=0;reception=received?1.2f:0;
        Color color=MealDish==2?new Color(.65f,.38f,.2f):MealDish==3?new Color(.69f,.57f,.28f):new Color(.9f,.85f,.67f);
        bowlFood.sharedMaterial=TeaHouseWorld.Mat("Dining food "+MealDish,color);TickMeal(0,true);
    }
    public void TickMeal(float dt,bool frozen)
    {
        cup.gameObject.SetActive(Dining&&TeaMeal);mealBowl.gameObject.SetActive(Dining&&!TeaMeal);chopsticks.gameObject.SetActive(Dining&&!TeaMeal);
        if(Dining&&!frozen){mealClock+=Mathf.Max(0,dt);reception=Mathf.Max(0,reception-dt);}
        var face=GetComponent<AvatarExpression>();if(face){face.Contented=Dining;face.MealNod=Dining&&reception>0?Mathf.Sin((1.2f-reception)/1.2f*Mathf.PI)*6:0;}
        if(!Dining){reception=0;return;}if(frozen)return;
        float lift=Mathf.Sin(Mathf.Clamp01((Mathf.Repeat(mealClock,4.2f)-.65f)/1.8f)*Mathf.PI);
        if(TeaMeal){rightArm.localRotation=ra*Quaternion.Euler(Mathf.Lerp(-60,-128,lift),0,-14);leftArm.localRotation=la*Quaternion.Euler(-12,0,-4);}
        else{leftArm.localRotation=la*Quaternion.Euler(-60,0,8);rightArm.localRotation=ra*Quaternion.Euler(Mathf.Lerp(-56,-112,lift),0,-12);}
    }
    void Update()
    {
        TickMeal(0,true);tray.gameObject.SetActive(Carrying);
        if(Frozen)
        {
            var face=GetComponent<AvatarExpression>();
            if(face&&face.Speaking&&!Dining&&!Carrying){float talk=Mathf.Sin(Time.unscaledTime*2)*3;leftArm.localRotation=la*Quaternion.Euler(-8+talk,0,-4);rightArm.localRotation=ra*Quaternion.Euler(-14-talk,0,6);}
            return;
        }
        if(arriving||departing)
        {
            Vector3 delta=destination-transform.position;
            if(delta.magnitude<.08f){arriving=false;Walking=false;Seated=!departing;if(departing){Destroy(gameObject);return;}transform.rotation=Quaternion.Euler(0,180,0);}
            else{Walking=true;transform.position=Vector3.MoveTowards(transform.position,destination,Time.deltaTime*3.5f);transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(new Vector3(delta.x,0,delta.z)),Time.deltaTime*8);}
        }
        if(eating>=0&&!arriving){eating-=Time.deltaTime;if(eating<=0){departing=true;Seated=false;destination=exit;}}
        phase+=Time.deltaTime*(Walking?10:2);float swing=Walking?Mathf.Sin(phase)*27:Mathf.Sin(phase)*3;
        Gesture=Mathf.Max(0,Gesture-Time.deltaTime);
        float reaching=Gesture>0?Mathf.Sin((1-Gesture)*Mathf.PI)*-58:0;
        leftArm.localRotation=la*Quaternion.Euler(swing+reaching,0,Walking?-5:0);rightArm.localRotation=ra*Quaternion.Euler(-swing+reaching,0,Walking?5:0);
        if(Carrying){leftArm.localRotation=la*Quaternion.Euler(-65,0,-10);rightArm.localRotation=ra*Quaternion.Euler(-65,0,10);}
        if(Dining)TickMeal(Time.deltaTime,false);
        leftLeg.localRotation=ll*Quaternion.Euler(Seated?-78:-swing,0,0);rightLeg.localRotation=rl*Quaternion.Euler(Seated?-78:swing,0,0);
    }
}
