using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    readonly List<CanopyView> teaRoofViews=new List<CanopyView>();
    float teaRoofOpacity=1;bool teaRoofNear,qaOpaqueRoof;
    public static bool IsTeaRoof(string name)
    {return name=="Ceramic roof base"||name=="Individual ceramic tile"||name=="Roof ridge";}
    void BuildTeaRoofView()
    {
        var materials=new Dictionary<Material,Material>();var shader=Resources.Load<Shader>("Shaders/FoliageFade");
        foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(IsTeaRoof(renderer.name))
        {
            var original=renderer.sharedMaterial;
            if(!materials.TryGetValue(original,out var faded)){faded=new Material(shader){name=original.name+" / kitchen roof fade",color=original.color};materials.Add(original,faded);}
            teaRoofViews.Add(new CanopyView{renderer=renderer,original=original,faded=faded,shadows=renderer.shadowCastingMode});
        }
    }
    void TickTeaRoofView(float dt)
    {
        if(teaRoofViews.Count==0)return;
        bool active=started&&!data.onTrail&&!closing&&!(qa&&qaOpaqueRoof);
        float distance=new Vector2(player.position.x+12,player.position.z+2.2f).magnitude;
        teaRoofNear=active&&distance<=(teaRoofNear?5.4f:4.2f);
        float opacity=Mathf.MoveTowards(teaRoofOpacity,teaRoofNear?.055f:1,Mathf.Max(0,dt)*2.8f);
        if(Mathf.Abs(opacity-teaRoofOpacity)<.00001f)return;teaRoofOpacity=opacity;
        foreach(var view in teaRoofViews)if(view.renderer){view.opacity=opacity;ApplyCanopyView(view);}
    }
    void RestoreTeaRoofView()
    {
        teaRoofNear=false;teaRoofOpacity=1;
        foreach(var view in teaRoofViews)if(view.renderer){view.opacity=1;ApplyCanopyView(view);}
    }
    void TestTeaRoofView()
    {
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);RestoreTeaRoofView();
        Assert(teaRoofViews.Count==195&&teaRoofViews.TrueForAll(v=>!v.renderer.isPartOfStaticBatch),"all roof bases tiles and ridge registered outside static batch");
        int money=data.money,leaves=data.leaves,objects=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length;float clock=data.clock;
        TickTeaRoofView(.1f);Assert(teaRoofOpacity<1&&teaRoofOpacity>.055f,"kitchen roof smoothly fades when approaching");TickTeaRoofView(1);
        Assert(Mathf.Approximately(teaRoofOpacity,.055f)&&teaRoofViews.TrueForAll(v=>v.renderer.enabled&&v.renderer.shadowCastingMode==ShadowCastingMode.Off),"nearby roof keeps faint tiles but removes roof shadow");
        Color original=teaRoofViews[0].original.color;player.position=new Vector3(-12,0,-7.1f);TickTeaRoofView(1);Assert(teaRoofNear,"roof hysteresis prevents edge flicker");
        player.position=new Vector3(-12,0,-8);TickTeaRoofView(.1f);Assert(!teaRoofNear&&teaRoofOpacity>.055f&&teaRoofOpacity<1,"departing smoothly restores roof");TickTeaRoofView(1);
        Assert(teaRoofViews.TrueForAll(v=>v.renderer.sharedMaterial==v.original&&v.renderer.shadowCastingMode==v.shadows),"far view restores original roof material and shadow");
        player.position=new Vector3(-12,0,-3);photoMode=true;TickTeaRoofView(1);Assert(teaRoofNear,"photo mode retains kitchen visibility");photoMode=false;
        paused=true;TickTeaRoofView(1);Assert(teaRoofNear,"pause retains current readable roof state");paused=false;
        Assert(data.money==money&&data.leaves==leaves&&data.clock==clock&&teaRoofViews[0].original.color==original&&FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length==objects,"roof fade creates no geometry transactions clock changes or material mutations");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(teaRoofOpacity==1&&data.leaves==leaves,"load resets roof transient without changing save progress");
        TickTeaRoofView(1);ChangeRegion(true);TickTeaRoofView(1);Assert(teaRoofOpacity==1,"mountain travel restores valley roof");ChangeRegion(false);
        player.position=new Vector3(-12,0,-3);TickTeaRoofView(1);ReturnMenu();Assert(teaRoofOpacity==1&&!teaRoofNear,"title resets original roof");
        NewGame();modal=false;
        Debug.Log("QA ROOF PASS: complete registration, gradual fade, hysteresis, shadows, overlays, photo, no transactions, load, travel and title.");
    }
    void TestParticleCulling()
    {
        var systems=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        Assert(systems.Length>=8,"atmosphere rain and eave particle systems still present");
        foreach(var ps in systems)Assert(ps.main.cullingMode==ParticleSystemCullingMode.AlwaysSimulate,"particles do not park simulation jobs when offscreen: "+ps.name);
        Assert(rainDrops.main.maxParticles==500&&eaveDrops.main.maxParticles==100,"rain particle bounds retained");
        Debug.Log("QA PARTICLE PASS: bounded particle systems always simulate offscreen; warning absence checked separately in player logs.");
    }
}
