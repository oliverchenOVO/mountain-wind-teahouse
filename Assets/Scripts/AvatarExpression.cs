using UnityEngine;
using System.Collections.Generic;

// Small face/head gestures on the original separated meshes, not a skinning rig.
public class AvatarExpression : MonoBehaviour
{
    public bool Speaking;public int Mood;
    public int EyeCount {get{return eyes.Count;}}
    public bool Smiling {get{return smile&&smile.enabled;}}
    public float HeadTilt {get{return head?Mathf.DeltaAngle(0,head.localEulerAngles.z):0;}}
    public string MoodLabel {get{return Mood==1?"安心微笑":Mood==2?"好奇傾聽":Mood==3?"認真巡查":"山中日常";}}
    Transform head;GameObject originalMouth;LineRenderer smile;float phase;
    List<Transform> eyes=new List<Transform>();
    void Awake()
    {
        var meshes=GetComponentsInChildren<MeshRenderer>();MeshRenderer headMesh=null,mouth=null;
        foreach(var renderer in meshes){if(renderer.name=="Head")headMesh=renderer;if(renderer.name=="Mouth")mouth=renderer;}
        if(!headMesh)return;
        var rig=new GameObject("Gentle head gestures");head=rig.transform;head.SetParent(transform,false);head.position=headMesh.bounds.center;
        string[] names={"Head","Hair","Fringe","Eye","Iris","Pupil","Cheek","Mouth","Cap","Tokin","WolfEar","EarPink","StrawHat","HatTop","TwinTail"};
        foreach(var renderer in meshes)foreach(string prefix in names)if(renderer.name.StartsWith(prefix)){renderer.transform.SetParent(head,true);break;}
        foreach(var renderer in meshes)if(renderer.name.StartsWith("Eye")&&!renderer.name.StartsWith("EyeShine"))
        {
            var eye=new GameObject("Blink pivot").transform;eye.SetParent(head,false);eye.position=renderer.bounds.center;eyes.Add(eye);
            float side=Mathf.Sign(transform.InverseTransformPoint(renderer.bounds.center).x);
            foreach(var part in meshes)if(part.name.StartsWith("Eye")||part.name.StartsWith("Iris")||part.name.StartsWith("Pupil"))if(Mathf.Sign(transform.InverseTransformPoint(part.bounds.center).x)==side)part.transform.SetParent(eye,true);
        }
        if(mouth)
        {
            originalMouth=mouth.gameObject;var g=new GameObject("Warm smile curve");g.transform.SetParent(head,false);smile=g.AddComponent<LineRenderer>();smile.useWorldSpace=false;smile.positionCount=9;smile.widthMultiplier=.012f;
            smile.sharedMaterial=TeaHouseWorld.Mat("Face ink",new Color(.22f,.16f,.14f));smile.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            Vector3 p=head.InverseTransformPoint(mouth.bounds.center);float facing=Mathf.Sign(p.z);
            for(int i=0;i<9;i++){float x=(i-4)*.011f;smile.SetPosition(i,p+new Vector3(x,.012f*(x*x/.00194f),facing*.018f));}smile.enabled=false;
        }
        phase=Mathf.Abs(GetInstanceID()%29)*.11f;
    }
    public void TickFace(float dt,bool frozen)
    {
        if(!head)return;Mood=Mathf.Clamp(Mood,0,3);
        if(!frozen||Speaking)phase+=dt;
        float cycle=Mathf.Repeat(phase,4.8f),blink=cycle<.14f?Mathf.Lerp(.12f,1,Mathf.Abs(cycle-.07f)/.07f):1;
        foreach(var eye in eyes)eye.localScale=new Vector3(1,blink*(Mood==3?.88f:1),1);
        head.localRotation=Quaternion.Euler(Speaking?Mathf.Sin(phase*1.8f)*2.4f:0,0,Mood==2?-5f:Mood==1?Mathf.Sin(phase)*1.5f:0);
        if(smile)smile.enabled=Mood==1;if(originalMouth)originalMouth.SetActive(Mood!=1);
    }
    void Update(){TickFace(Time.unscaledDeltaTime,AvatarMotion.Frozen);}
}
