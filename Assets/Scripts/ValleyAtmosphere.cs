using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class ValleyAtmosphere : MonoBehaviour
{
    public bool Evening;
    List<Light> lanterns=new List<Light>();ParticleSystem fireflies;static Material mist;
    public void Build()
    {
        foreach(var l in FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.type==LightType.Point)lanterns.Add(l);
        var m=new Material(Resources.Load<Shader>("Shaders/SoftParticle"));
        var tex=new Texture2D(32,32,TextureFormat.RGBA32,false);var pixels=new Color[1024];
        for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f;pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2));}
        tex.SetPixels(pixels);tex.Apply();m.mainTexture=tex;m.color=Color.white;m.SetFloat("_Glossiness",0);mist=m;
        Steam(new Vector3(-11.1f,1.94f,-.55f),.14f,5,2,new Color(.98f,.95f,.82f,.5f));
        Steam(new Vector3(-9.2f,1.43f,1.9f),.22f,6,2.5f,new Color(.98f,.95f,.83f,.6f));
        Steam(new Vector3(-9.4f,5.6f,5),.6f,3,4,new Color(.79f,.82f,.72f,.4f));
        Steam(new Vector3(8,.2f,20.7f),.7f,20,3,new Color(.83f,.95f,.88f,.7f),1.4f);
        Steam(new Vector3(8,.1f,20),.45f,10,2,new Color(.95f,1,.94f,.7f),.9f);
        var leaves=Emitter("Falling maple leaves",new Vector3(-12,6,-8),.15f,5,6,new Color(.84f,.56f,.26f,.9f),6);
        var leafTex=new Texture2D(32,32,TextureFormat.RGBA32,false);var leafColors=new Color[1024];
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
        {
            Vector2 q=new Vector2(x-15.5f,y-15.5f)/15.5f;float edge=.72f+.22f*Mathf.Cos(Mathf.Atan2(q.y,q.x)*5);
            leafColors[y*32+x]=new Color(1,1,1,q.magnitude<edge?1:0);
        }
        leafTex.SetPixels(leafColors);leafTex.Apply();var leafMaterial=new Material(mist);leafMaterial.mainTexture=leafTex;leaves.GetComponent<ParticleSystemRenderer>().sharedMaterial=leafMaterial;
        var rotation=leaves.rotationOverLifetime;rotation.enabled=true;rotation.z=.8f;
        var main=leaves.main;main.gravityModifier=.10f;main.startSpeed=.2f;var velocity=leaves.velocityOverLifetime;velocity.enabled=true;velocity.x=.28f;velocity.z=.16f;
        fireflies=Emitter("Tea garden fireflies",new Vector3(-12,.8f,-7),.14f,3,6,new Color(1,.87f,.38f,1),7);var fmain=fireflies.main;fmain.startSpeed=.08f;fmain.maxParticles=24;
        var renderer=fireflies.GetComponent<ParticleSystemRenderer>();var fm=new Material(mist);fm.EnableKeyword("_EMISSION");fm.SetColor("_EmissionColor",new Color(1,.67f,.22f));renderer.sharedMaterial=fm;
        fireflies.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    public ParticleSystem Steam(Vector3 p,float size,float rate,float life,Color color,float radius=.08f)
    {
        var ps=Emitter("Warm steam and waterfall spray",p,size,rate,life,color,radius);
        var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.x=.06f;velocity.y=.23f;
        return ps;
    }
    ParticleSystem Emitter(string name,Vector3 p,float size,float rate,float lifetime,Color color,float radius)
    {
        var g=new GameObject(name);g.transform.SetParent(transform);g.transform.position=p;g.transform.rotation=Quaternion.Euler(-90,0,0);
        var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;main.duration=8;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.7f,lifetime);main.startSize=new ParticleSystem.MinMaxCurve(size*.6f,size);main.startSpeed=.12f;main.startColor=color;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=100;
        var emission=ps.emission;emission.rateOverTime=rate;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=radius;shape.angle=12;
        var colorLife=ps.colorOverLifetime;colorLife.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.7f,.15f),new GradientAlphaKey(0,1)});colorLife.color=gradient;
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mist;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;ps.Play();return ps;
    }
    void Update()
    {
        foreach(var l in lanterns)if(l)l.intensity=(Evening?3.2f:.8f)+Mathf.Sin(Time.time*2+l.transform.position.x)*.07f;
        if(fireflies){if(Evening&&!fireflies.isPlaying)fireflies.Play();else if(!Evening&&fireflies.isPlaying)fireflies.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    }
    public static void Meal(int slot,int dish)
    {
        string name="Served plate "+slot;var old=GameObject.Find(name);if(old)Destroy(old);
        var root=new GameObject(name);Vector3 p=new Vector3(-16+slot*3,1.0f,-8.1f);root.transform.position=p;
        TeaHouseWorld.Shape("Ceramic plate",PrimitiveType.Cylinder,p,new Vector3(.60f,.035f,.60f),new Color(.90f,.86f,.70f),root.transform);
        if(dish==0||dish==4||dish==6)
        {
            TeaHouseWorld.Shape("Fresh tea cup",PrimitiveType.Cylinder,p+Vector3.up*.12f,new Vector3(.28f,.11f,.28f),new Color(.72f,.80f,.61f),root.transform);
            TeaHouseWorld.Shape("Tea",PrimitiveType.Cylinder,p+Vector3.up*.235f,new Vector3(.23f,.008f,.23f),new Color(.34f,.43f,.16f),root.transform);
            if(dish==4)TeaHouseWorld.Shape("Bamboo leaf garnish",PrimitiveType.Sphere,p+new Vector3(.06f,.25f,0),new Vector3(.13f,.02f,.04f),new Color(.55f,.73f,.25f),root.transform);
        }
        else if(dish==1||dish==3||dish==5)
        {
            TeaHouseWorld.Shape("Rice bowl",PrimitiveType.Sphere,p+Vector3.up*.12f,new Vector3(.50f,.24f,.50f),new Color(.91f,.88f,.72f),root.transform);
            if(dish==3)TeaHouseWorld.Shape("Tea mushroom broth",PrimitiveType.Cylinder,p+Vector3.up*.21f,new Vector3(.43f,.035f,.43f),new Color(.63f,.47f,.23f),root.transform);
            for(int i=0;i<3;i++)TeaHouseWorld.Shape("Mushroom topping",PrimitiveType.Sphere,p+new Vector3((i-1)*.10f,.25f,0),new Vector3(.12f,.06f,.14f),new Color(.56f,.32f,.18f),root.transform);
        }
        else
        {
            TeaHouseWorld.Shape("Grilled fish",PrimitiveType.Sphere,p+Vector3.up*.09f,new Vector3(.40f,.13f,.16f),new Color(.69f,.49f,.26f),root.transform);
            TeaHouseWorld.Shape("Fish tail",PrimitiveType.Cube,p+new Vector3(.24f,.10f,0),new Vector3(.12f,.025f,.20f),new Color(.45f,.34f,.21f),root.transform);
        }
        var atmosphere=FindFirstObjectByType<ValleyAtmosphere>();if(atmosphere){var ps=atmosphere.Steam(p+Vector3.up*.26f,.11f,3,1.5f,new Color(.98f,.95f,.83f,.5f));Destroy(ps.gameObject,7);}
    }
    public static void ClearMeals(){for(int i=0;i<3;i++){var g=GameObject.Find("Served plate "+i);if(g)Destroy(g);}}
}
public class NorenWind : MonoBehaviour
{
    public float phase;void Update(){transform.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*1.1f+phase)*5,0,Mathf.Sin(Time.time*.7f+phase)*1);}
}
public class WaterwheelSpin : MonoBehaviour
{void Update(){transform.Rotate(Time.deltaTime*13,0,0,Space.Self);}}
public class WaterRibbon : MonoBehaviour
{
    public float phase;Vector3 scale;void Start(){scale=transform.localScale;}void Update(){transform.localScale=new Vector3(scale.x*(1+Mathf.Sin(Time.time*2+phase)*.05f),scale.y,scale.z);}
}
