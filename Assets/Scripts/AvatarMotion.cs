using UnityEngine;
using System.Collections.Generic;

// FK animation of separately modelled limbs; pivots are made from the Blender mesh parts.
public class AvatarMotion : MonoBehaviour
{
    Transform leftArm,rightArm,leftLeg,rightLeg;Quaternion la,ra,ll,rl;
    public bool Walking,Seated,Carrying,Dining;public float Gesture;
    public static bool Frozen;
    Transform cup,tray;
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
        tray=TeaHouseWorld.Shape("Serving tray",PrimitiveType.Cube,Vector3.zero,new Vector3(.7f,.045f,.45f),new Color(.38f,.24f,.13f)).transform;
        tray.SetParent(transform,false);tray.localPosition=new Vector3(0,.95f,.48f);tray.gameObject.SetActive(false);
        var bowl=TeaHouseWorld.Shape("Tray bowl",PrimitiveType.Sphere,Vector3.zero,new Vector3(.28f,.16f,.28f),new Color(.93f,.88f,.72f)).transform;bowl.SetParent(tray,false);bowl.localPosition=new Vector3(0,2,0);bowl.localScale=new Vector3(.4f,3,.6f);
    }
    Transform Pivot(string name,Vector3 pos){var g=new GameObject(name);g.transform.SetParent(transform,false);g.transform.localPosition=pos;return g.transform;}
    public void Arrive(Vector3 seat)
    {destination=seat;arriving=true;Seated=false;}
    public void DepartAfterMeal(float seconds,Vector3 door){eating=seconds;exit=door;}
    void Update()
    {
        cup.gameObject.SetActive(Dining);tray.gameObject.SetActive(Carrying);
        if(Frozen)return;
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
        if(Dining)rightArm.localRotation=ra*Quaternion.Euler(-70-Mathf.Sin(phase)*20,0,-12);
        leftLeg.localRotation=ll*Quaternion.Euler(Seated?-78:-swing,0,0);rightLeg.localRotation=rl*Quaternion.Euler(Seated?-78:swing,0,0);
    }
}
