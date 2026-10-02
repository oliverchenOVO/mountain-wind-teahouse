using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    readonly List<Light> teaNightLights=new List<Light>();
    readonly List<Renderer> teaNightPaper=new List<Renderer>(),teaNightHalos=new List<Renderer>();
    GameObject teaNightRoot,teaHaloRoot;Material teaLampMaterial;
    MaterialPropertyBlock teaHaloTint=new MaterialPropertyBlock();
    AmbientMode teaDayAmbientMode;float teaNightBlend,teaLightClock;
    GameObject NightShape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)
    {
        var shape=TeaHouseWorld.Shape(name,type,p,scale,color,teaNightRoot.transform);
        var collider=shape.GetComponent<Collider>();if(collider)collider.enabled=false;
        return shape;
    }
    Light NightLight(string name,Vector3 p,float range)
    {
        var g=new GameObject(name);g.transform.SetParent(teaNightRoot.transform);g.transform.position=p;
        var light=g.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.72f,.40f);light.range=range;
        light.shadows=LightShadows.None;light.renderMode=LightRenderMode.ForcePixel;teaNightLights.Add(light);return light;
    }
    void NightHalo(Vector3 p,float scale,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Quad);g.name="Cached tea lantern halo";g.transform.SetParent(teaHaloRoot.transform);g.transform.position=p;g.transform.localScale=Vector3.one*scale;
        var collider=g.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
        var r=g.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;teaNightHalos.Add(r);
    }
    void BuildTeaNightScene()
    {
        teaDayAmbientMode=RenderSettings.ambientMode;
        teaNightRoot=new GameObject("Tea house night fixtures");teaHaloRoot=new GameObject("Soft lantern halos");
        teaLampMaterial=new Material(Resources.Load<Shader>("Shaders/LanternPaper")){name="Warm tea lantern paper",color=new Color(.92f,.82f,.61f)};
        teaLampMaterial.SetFloat("_Glossiness",.06f);
        var texture=new Texture2D(48,48,TextureFormat.RGBA32,false);
        for(int y=0;y<48;y++)for(int x=0;x<48;x++){float distance=new Vector2((x-23.5f)/23.5f,(y-23.5f)/23.5f).magnitude;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-distance),3)));}texture.Apply();
        var haloMaterial=new Material(Resources.Load<Shader>("Shaders/SoftParticle")){name="Warm lantern soft halo",mainTexture=texture};
        foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(r.name=="Paper lantern")
        {
            r.sharedMaterial=teaLampMaterial;teaNightPaper.Add(r);var light=r.GetComponent<Light>();
            if(light){light.shadows=LightShadows.None;light.renderMode=LightRenderMode.ForcePixel;light.color=new Color(1,.72f,.40f);teaNightLights.Add(light);}
            NightHalo(r.transform.position,1.65f,haloMaterial);
        }
        for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(-16+i*3-.48f,1.17f,-7.85f);
            NightShape("Table lantern cedar base",PrimitiveType.Cylinder,p-Vector3.up*.13f,new Vector3(.30f,.04f,.30f),new Color(.30f,.22f,.14f));
            var paperLamp=NightShape("Table lantern paper",PrimitiveType.Capsule,p+Vector3.up*.10f,new Vector3(.23f,.23f,.23f),cream).GetComponent<Renderer>();paperLamp.sharedMaterial=teaLampMaterial;teaNightPaper.Add(paperLamp);
            NightShape("Table lantern bamboo cap",PrimitiveType.Cylinder,p+Vector3.up*.32f,new Vector3(.26f,.025f,.26f),new Color(.40f,.27f,.16f));
            for(int hoop=0;hoop<3;hoop++)NightShape("Table lantern fine hoop",PrimitiveType.Cylinder,p+Vector3.up*(-.06f+hoop*.16f),new Vector3(.25f,.009f,.25f),new Color(.60f,.42f,.24f));
            NightLight("Table warmth "+(i+1),p+Vector3.up*.35f,3.5f);NightHalo(p+Vector3.up*.10f,.88f,haloMaterial);
        }
        NightLight("Counter soft fill",new Vector3(-12,2.0f,-.9f),4.5f);
        ResetTeaNightScene();
    }
    void ApplyTeaNightFixtures()
    {
        bool valley=started&&!data.onTrail;
        for(int i=0;i<teaNightLights.Count;i++)if(teaNightLights[i])
        {
            float night=i<2?2.0f:i<5?1.45f:.95f;
            float wobble=1+Mathf.Sin(teaLightClock*1.1f+i*1.9f)*.025f;
            teaNightLights[i].intensity=valley?Mathf.Lerp(i<2?.8f:0,night*wobble,teaNightBlend):i<2?.8f:0;
        }
        teaLampMaterial.SetColor("_EmissionColor",Color.Lerp(new Color(.06f,.035f,.012f),new Color(.64f,.34f,.09f),teaNightBlend));
        teaHaloRoot.SetActive(valley&&teaNightBlend>.001f);
        for(int i=0;i<teaNightHalos.Count;i++)
        {
            teaNightHalos[i].transform.rotation=cam.transform.rotation;
            teaHaloTint.SetColor("_Color",new Color(1,.73f,.36f,teaNightBlend*(i<2?.30f:.23f)));teaNightHalos[i].SetPropertyBlock(teaHaloTint);
        }
    }
    void ResetTeaNightScene()
    {
        if(!teaNightRoot)return;teaNightBlend=0;teaLightClock=0;RenderSettings.ambientMode=teaDayAmbientMode;ApplyTeaNightFixtures();
    }
    void TickTeaNightScene(float dt)
    {
        if(!teaNightRoot)return;
        teaNightBlend=Mathf.MoveTowards(teaNightBlend,started&&data.night?1:0,Mathf.Max(0,dt)*.9f);
        if(!AvatarMotion.Frozen)teaLightClock+=Mathf.Max(0,dt);
        ApplyTeaNightFixtures();
        RenderSettings.ambientMode=teaNightBlend>.001f?AmbientMode.Flat:teaDayAmbientMode;
        if(teaNightBlend<=0)return;
        // Rain has already calculated the daytime base. This is the final night palette.
        sun.intensity=Mathf.Lerp(sun.intensity,Mathf.Lerp(.25f,.21f,weatherBlend),teaNightBlend);
        sun.color=Color.Lerp(sun.color,new Color(.68f,.77f,.96f),teaNightBlend);
        RenderSettings.ambientLight=Color.Lerp(RenderSettings.ambientLight,Color.Lerp(new Color(.31f,.36f,.46f),new Color(.29f,.34f,.43f),weatherBlend),teaNightBlend);
        cam.backgroundColor=Color.Lerp(cam.backgroundColor,Color.Lerp(new Color(.18f,.24f,.34f),new Color(.20f,.26f,.35f),weatherBlend),teaNightBlend);RenderSettings.fogColor=cam.backgroundColor;
    }
    void TestTeaNightScene()
    {
        NewGame();modal=false;TickRainWeather(1);TickTeaNightScene(2);
        Assert(teaNightLights.Count==6&&teaNightPaper.Count==5&&teaNightHalos.Count==5,"night fixtures have bounded six lights and five reusable paper lamps and halos");
        Assert(teaNightPaper.TrueForAll(r=>!r.isPartOfStaticBatch),"dynamic lamp paper stays outside static batching");
        Assert(teaLampMaterial.shader.name=="MountainTea/LanternPaper"&&teaLampMaterial.shader.isSupported,"lamp emission uses an included shader without runtime keyword stripping");
        Assert(teaNightBlend==0&&!teaHaloRoot.activeSelf&&teaNightLights[2].intensity==0,"daytime hides halos and switches off table lighting");
        Assert(RenderSettings.ambientMode==teaDayAmbientMode,"daytime restores original ambient mode");
        data.tea=8;OpenShop();modal=false;string snapshot=JsonUtility.ToJson(data);
        TickRainWeather(.1f);TickTeaNightScene(.1f);Assert(teaNightBlend>0&&teaNightBlend<1,"night lighting gradually transitions instead of flashing");
        TickRainWeather(2);TickTeaNightScene(2);
        Assert(teaNightBlend==1&&teaHaloRoot.activeSelf&&sun.intensity<=.26f&&RenderSettings.ambientMode==AmbientMode.Flat,"night palette is cooler with active warm lamps");
        Assert(teaNightLights.TrueForAll(l=>l.shadows==LightShadows.None&&l.range<=7),"night lights bounded and shadowless");
        Assert(teaNightLights[2].intensity>1.3f&&teaNightLights[2].intensity<1.6f,"table warmth remains gently bounded");
        Assert(teaNightHalos.TrueForAll(r=>r.shadowCastingMode==ShadowCastingMode.Off),"soft halos cannot cast blocking shadows");
        for(int i=0;i<3;i++)Assert(Vector3.Distance(teaNightLights[i+2].transform.position,new Vector3(-16+i*3-.48f,1.52f,-7.85f))<.001f,"table lamps follow original tables without moving guest seats: "+teaNightLights[i+2].transform.position);
        int renderers=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length,particles=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length;
        for(int i=0;i<30;i++){TickRainWeather(.01f);TickTeaNightScene(.01f);}
        Assert(snapshot==JsonUtility.ToJson(data),"night visual queries do not transact advance gameplay or change save fields");
        Assert(renderers==FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length&&particles==FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length,"night ticks create no meshes or particle systems");
        float time=teaLightClock;AvatarMotion.Frozen=true;TickTeaNightScene(2);Assert(teaLightClock==time,"overlays freeze tiny lantern shimmer");AvatarMotion.Frozen=false;
        data.day=3;TickRainWeather(2);TickTeaNightScene(2);Assert(teaHaloRoot.activeSelf&&sun.intensity>=.20f&&sun.intensity<=.26f,"rain night retains readable light level");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));TickRainWeather(2);TickTeaNightScene(2);Assert(data.night&&teaNightBlend==1,"saved night restores visual palette without extra save fields");
        NextDay();modal=false;TickRainWeather(2);TickTeaNightScene(2);Assert(teaNightBlend==0&&!teaHaloRoot.activeSelf&&teaNightLights[2].intensity==0,"next morning returns to unlit table fixtures");
        ChangeRegion(true);TickTeaNightScene(2);Assert(!teaHaloRoot.activeSelf&&teaNightLights[2].intensity==0,"valley night fixtures do not follow player onto mountain trail");ChangeRegion(false);
        data.night=true;TickTeaNightScene(2);ReturnMenu();Assert(teaNightBlend==0&&!teaHaloRoot.activeSelf&&RenderSettings.ambientMode==teaDayAmbientMode,"title clears transient night palette and restores ambient mode");
        NewGame();modal=false;
        Debug.Log("QA NIGHT SCENE PASS: cached fixtures, light bounds, blending, palette, weather, overlays, readonly state, save, day, region and title.");
    }
}
