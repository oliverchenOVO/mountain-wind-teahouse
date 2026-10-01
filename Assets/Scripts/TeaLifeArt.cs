using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    Transform lifeRoot;GameObject rainyUmbrella,puddleRoot;ParticleSystem eaveDrops;
    List<LineRenderer> ripples=new List<LineRenderer>();float dripAccumulator,lifeTime,portraitTimer;
    class PortraitLife {public Camera camera;public AvatarExpression face;public bool wasSpeaking;}
    Dictionary<string,PortraitLife> portraitLife=new Dictionary<string,PortraitLife>();int dialogueMood;
    GameObject LifeShape(string name,PrimitiveType type,Vector3 p,Vector3 size,Color color,Transform parent=null)
    {return TeaHouseWorld.Shape(name,type,p,size,color,parent?parent:lifeRoot);}
    void LifeBeam(string name,Vector3 a,Vector3 b,float width,Color color,Transform parent=null)
    {var g=LifeShape(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(width,Vector3.Distance(a,b)*.5f,width),color,parent);g.transform.up=(b-a).normalized;}
    void BuildTeaLife()
    {
        lifeRoot=new GameObject("Tea house living details").transform;Color wood=new Color(.44f,.3f,.2f),clay=new Color(.65f,.36f,.24f);
        Vector3 rack=new Vector3(-6.7f,0,.2f);
        LifeShape("Umbrella rack base",PrimitiveType.Cube,rack+Vector3.up*.13f,new Vector3(1.2f,.22f,.65f),wood);
        for(int side=-1;side<=1;side+=2)LifeShape("Umbrella rack side",PrimitiveType.Cube,rack+new Vector3(side*.55f,.65f,0),new Vector3(.12f,1,.65f),wood);
        LifeShape("Umbrella rack rail",PrimitiveType.Cube,rack+new Vector3(0,1.03f,-.25f),new Vector3(1.2f,.13f,.12f),wood);
        for(int i=0;i<3;i++)
        {
            Vector3 p=rack+new Vector3((i-1)*.35f,.28f,0);Color cloth=i==0?sage:i==1?new Color(.7f,.3f,.24f):new Color(.33f,.5f,.62f);
            LifeBeam("Folded umbrella stem",p,p+Vector3.up*1.2f,.026f,wood);
            LifeShape("Folded umbrella cloth",PrimitiveType.Capsule,p+Vector3.up*.45f,new Vector3(.19f,.72f,.19f),cloth);
            LifeShape("Umbrella tie",PrimitiveType.Cylinder,p+Vector3.up*.62f,new Vector3(.205f,.02f,.205f),cream);
            LifeBeam("Umbrella handle",p+new Vector3(0,1.2f,0),p+new Vector3(.12f,1.12f,0),.032f,wood);
        }
        rainyUmbrella=new GameObject("Open rainy umbrella");rainyUmbrella.transform.SetParent(lifeRoot);
        Umbrella(new Vector3(-6.8f,0,-.8f),new Color(.65f,.28f,.22f),rainyUmbrella.transform);
        Vector3 shelf=new Vector3(-6.7f,0,1.4f);
        LifeShape("Small side shelf",PrimitiveType.Cube,shelf+Vector3.up*.65f,new Vector3(1.1f,.13f,.5f),wood);
        for(int side=-1;side<=1;side+=2)LifeShape("Side shelf leg",PrimitiveType.Cube,shelf+new Vector3(side*.43f,.32f,0),new Vector3(.10f,.64f,.35f),wood);
        for(int i=0;i<3;i++)TeaCanister(shelf+new Vector3((i-1)*.29f,.73f,0),.8f,i);
        for(int i=0;i<3;i++)TeaCanister(new Vector3(-12.8f+i*.48f,1.51f,-.18f),1,i);
        LifeShape("Pastry tray",PrimitiveType.Cube,new Vector3(-14.1f,1.54f,-.55f),new Vector3(.6f,.045f,.38f),wood);
        for(int i=0;i<4;i++){Vector3 p=new Vector3(-14.28f+(i%2)*.22f,1.61f,-.64f+(i/2)*.18f);LifeShape("Mountain rice sweet",PrimitiveType.Sphere,p,new Vector3(.17f,.13f,.17f),i%2==0?cream:new Color(.64f,.75f,.49f));}
        Vector3 tray=new Vector3(-10,1.53f,-.58f);LifeShape("Tea leaf bamboo tray",PrimitiveType.Cube,tray,new Vector3(.6f,.045f,.35f),gold);
        for(int i=0;i<9;i++)LifeShape("Fresh tea leaf display",PrimitiveType.Sphere,tray+new Vector3((i%3-1)*.15f,.07f,(i/3-1)*.09f),new Vector3(.16f,.025f,.07f),sage).transform.rotation=Quaternion.Euler(0,i*37,0);
        LifeShape("Folded linen towel",PrimitiveType.Cube,new Vector3(-9.7f,1.56f,-.22f),new Vector3(.28f,.055f,.25f),cream);
        for(int i=0;i<2;i++)
        {
            Vector3 p=new Vector3(-17.4f+i*11.1f,0,2.5f);LifeShape("Tea house herb pot",PrimitiveType.Cylinder,p+Vector3.up*.24f,new Vector3(.55f,.24f,.55f),clay);
            LifeShape("Herb pot soil",PrimitiveType.Cylinder,p+Vector3.up*.49f,new Vector3(.47f,.015f,.47f),wood);
            for(int leaf=0;leaf<5;leaf++)LifeShape("Potted mint leaf",PrimitiveType.Sphere,p+new Vector3(Mathf.Sin(leaf)*.15f,.65f+leaf*.05f,Mathf.Cos(leaf)*.15f),new Vector3(.27f,.055f,.12f),sage).transform.rotation=Quaternion.Euler(0,leaf*68,leaf*7);
        }
        BuildEaveWater();
        var batch=new List<GameObject>();foreach(var renderer in lifeRoot.GetComponentsInChildren<MeshRenderer>())if(!renderer.transform.IsChildOf(rainyUmbrella.transform)&&!renderer.transform.IsChildOf(puddleRoot.transform))batch.Add(renderer.gameObject);StaticBatchingUtility.Combine(batch.ToArray(),lifeRoot.gameObject);
    }
    void TeaCanister(Vector3 p,float size,int style)
    {
        Color c=style==0?sage:style==1?new Color(.72f,.4f,.27f):new Color(.4f,.55f,.63f);
        LifeShape("Tea canister",PrimitiveType.Cylinder,p+Vector3.up*.18f*size,new Vector3(.27f,.18f,.27f)*size,c);
        LifeShape("Tea canister lid",PrimitiveType.Cylinder,p+Vector3.up*.37f*size,new Vector3(.30f,.025f,.30f)*size,gold);
        LifeShape("Tea label",PrimitiveType.Cube,p+new Vector3(0,.19f,-.137f)*size,new Vector3(.13f,.17f,.008f)*size,cream);
        LifeShape("Tea label ink",PrimitiveType.Cube,p+new Vector3(0,.19f,-.145f)*size,new Vector3(.025f,.095f,.006f)*size,sage);
    }
    void Umbrella(Vector3 p,Color cloth,Transform parent)
    {
        LifeBeam("Open umbrella shaft",p+Vector3.up*.05f,p+Vector3.up*1.8f,.026f,gold,parent);
        var vertices=new List<Vector3>();var triangles=new List<int>();
        for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;vertices.Add(Vector3.up*.23f);vertices.Add(new Vector3(Mathf.Sin(a)*.58f,0,Mathf.Cos(a)*.58f));vertices.Add(new Vector3(Mathf.Sin(b)*.58f,0,Mathf.Cos(b)*.58f));int n=vertices.Count;triangles.AddRange(new[]{n-3,n-2,n-1});LifeBeam("Umbrella bamboo rib",p+Vector3.up*1.8f,p+new Vector3(Mathf.Sin(a)*.58f,1.57f,Mathf.Cos(a)*.58f),.012f,gold,parent);}
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();var canopy=new GameObject("Paper umbrella canopy");canopy.transform.SetParent(parent);canopy.transform.position=p+Vector3.up*1.57f;canopy.AddComponent<MeshFilter>().sharedMesh=mesh;canopy.AddComponent<MeshRenderer>().sharedMaterial=TeaHouseWorld.Mat("Paper umbrella red",cloth);
        LifeShape("Umbrella cap",PrimitiveType.Sphere,p+Vector3.up*1.81f,Vector3.one*.08f,gold,parent);
    }
    void BuildEaveWater()
    {
        var material=new Material(Resources.Load<Shader>("Shaders/SoftParticle"));material.color=new Color(.73f,.84f,.88f,.5f);
        var g=new GameObject("Rain eave drips");g.transform.SetParent(lifeRoot);eaveDrops=g.AddComponent<ParticleSystem>();eaveDrops.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=eaveDrops.main;main.maxParticles=100;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startSpeed=0;main.startSize=.035f;main.startLifetime=.55f;
        var emission=eaveDrops.emission;emission.enabled=false;var shape=eaveDrops.shape;shape.enabled=false;var renderer=eaveDrops.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=4;renderer.velocityScale=.03f;renderer.shadowCastingMode=ShadowCastingMode.Off;eaveDrops.Play();
        puddleRoot=new GameObject("Rain ground water marks");puddleRoot.transform.SetParent(lifeRoot);
        for(int i=0;i<5;i++)
        {
            Vector3 p=new Vector3(-17+i*2.25f,.055f,-5.65f);var puddle=LifeShape("Soft rain puddle",PrimitiveType.Cylinder,p,new Vector3(.85f,.012f,.44f),cream,puddleRoot.transform);puddle.GetComponent<Renderer>().sharedMaterial=material;
            var ring=new GameObject("Eave ripple");ring.transform.SetParent(puddleRoot.transform);ring.transform.position=p+Vector3.up*.025f;var line=ring.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=24;line.widthMultiplier=.018f;line.sharedMaterial=material;line.shadowCastingMode=ShadowCastingMode.Off;
            for(int point=0;point<24;point++){float angle=point*Mathf.PI/12;line.SetPosition(point,new Vector3(Mathf.Sin(angle)*.3f,0,Mathf.Cos(angle)*.3f));}ripples.Add(line);
        }
        puddleRoot.SetActive(false);rainyUmbrella.SetActive(false);
    }
    void SetDialogueMood(int mood){dialogueMood=Mathf.Clamp(mood,0,3);}
    void TickTeaLife(float dt)
    {
        if(!eaveDrops||closing)return;bool wet=started&&IsRainDay&&!data.onTrail;
        rainyUmbrella.SetActive(IsRainDay);puddleRoot.SetActive(wet);var main=eaveDrops.main;main.simulationSpeed=AvatarMotion.Frozen?0:1;
        if(!wet){eaveDrops.Clear();dripAccumulator=0;}
        else if(!AvatarMotion.Frozen)
        {
            lifeTime+=dt;dripAccumulator+=dt*32;
            while(dripAccumulator>=1){dripAccumulator--;float x=-17.5f+(lifeTime*17+dripAccumulator*.73f)%11;var drop=new ParticleSystem.EmitParams{position=new Vector3(x,3.27f,-5.32f),velocity=Vector3.down*6.3f,startLifetime=.51f,startSize=.035f,startColor=new Color(.8f,.91f,.96f,.65f)};eaveDrops.Emit(drop,1);}
            for(int i=0;i<ripples.Count;i++){float t=Mathf.Repeat(lifeTime*.9f+i*.21f,1);ripples[i].transform.localScale=Vector3.one*(.25f+t*1.4f);ripples[i].startColor=ripples[i].endColor=new Color(1,1,1,(1-t)*.65f);}
        }
        portraitTimer+=dt;
        foreach(var entry in portraitLife)
        {
            bool talking=modal&&entry.Key==speaker;var profile=entry.Value;profile.face.Speaking=talking;profile.face.Mood=talking?dialogueMood:0;profile.face.TickFace(0,AvatarMotion.Frozen);
            if(talking&&portraitTimer>.1f||talking!=profile.wasSpeaking)profile.camera.Render();profile.wasSpeaking=talking;
        }
        if(portraitTimer>.1f)portraitTimer=0;
        foreach(var s in spots)if(s.visual){var face=s.visual.GetComponent<AvatarExpression>();if(face){face.Speaking=modal&&FriendIndex(speaker)>=0&&(s.name.Contains(speaker)||s.kind==11&&speaker=="河城荷取"||s.kind==12&&speaker=="射命丸文"||s.kind==13&&speaker=="犬走椛"||s.kind==22&&speaker=="犬走椛"||s.kind==26&&speaker=="射命丸文"||s.kind==27&&speaker=="河城荷取");face.Mood=face.Speaking?dialogueMood:0;}}
        foreach(var guest in visitors)if(guest){var face=guest.GetComponent<AvatarExpression>();face.Speaking=modal&&guest.name.StartsWith(speaker=="河城荷取"?"Nitori":speaker=="射命丸文"?"Aya":speaker=="犬走椛"?"Momiji":"?");face.Mood=face.Speaking?dialogueMood:0;}
    }
    void TestTeaLife()
    {
        NewGame();modal=false;data.day=3;AvatarMotion.Frozen=false;TickTeaLife(.2f);
        Assert(rainyUmbrella.activeSelf&&puddleRoot.activeSelf&&eaveDrops.particleCount>0,"rain activates umbrella puddles and eave drops");
        AvatarMotion.Frozen=true;float before=lifeTime;TickTeaLife(1);Assert(eaveDrops.main.simulationSpeed==0&&lifeTime==before,"settings pause eave motion");AvatarMotion.Frozen=false;
        data.day=4;TickTeaLife(.1f);Assert(!rainyUmbrella.activeSelf&&!puddleRoot.activeSelf&&eaveDrops.particleCount==0,"sunny day hides dynamic rain props");
        foreach(var profile in portraitLife.Values){Assert(profile.face.EyeCount==2,"two blink pivots in each portrait");profile.face.Speaking=true;profile.face.Mood=1;profile.face.TickFace(.3f,true);Assert(profile.face.Smiling,"smile geometry activates while world is paused");profile.face.Mood=2;profile.face.TickFace(0,true);Assert(!profile.face.Smiling&&Mathf.Abs(profile.face.HeadTilt+5)<.1f,"curious pose tilts head and restores mouth");}
        var canopy=rainyUmbrella.transform.Find("Paper umbrella canopy").GetComponent<MeshFilter>().sharedMesh;Assert(canopy.normals[0].y>0,"umbrella canopy faces camera from above");
        Assert(Walkable(new Vector3(-12,0,-3))&&Walkable(new Vector3(-12,0,-6))&&Walkable(new Vector3(-16,0,-7.1f)),"counter approach and service lanes unchanged");
        Assert(lifeRoot.GetComponentsInChildren<Collider>(true).Length==0,"life decorations add no physical blockers");
        Debug.Log("QA LIFE PASS: rain details, freeze, sunny reset, face rigs, speaking expression, service access.");
    }
}
