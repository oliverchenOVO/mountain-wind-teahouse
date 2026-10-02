using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    class PavilionRoofGroup
    {
        public Vector2 centre;public bool near;public float opacity=1;
        public readonly List<CanopyView> views=new List<CanopyView>();
    }
    readonly List<PavilionRoofGroup> pavilionRoofs=new List<PavilionRoofGroup>();
    bool qaOpaquePavilions;
    public static bool IsPavilionRoof(string name)
    {return name=="Pavilion solid roof underlay"||name=="Pavilion ceramic roof tile"||name=="Pavilion ridge";}
    void BuildPavilionRoofViews()
    {
        pavilionRoofs.Add(new PavilionRoofGroup{centre=new Vector2(86,21)});
        pavilionRoofs.Add(new PavilionRoofGroup{centre=new Vector2(106,20)});
        var materials=new Dictionary<Material,Material>();var shader=Resources.Load<Shader>("Shaders/FoliageFade");
        foreach(var renderer in trailRoot.GetComponentsInChildren<MeshRenderer>())if(IsPavilionRoof(renderer.name))
        {
            var original=renderer.sharedMaterial;
            if(!materials.TryGetValue(original,out var faded)){faded=new Material(shader){name=original.name+" / pavilion fade",color=original.color};materials.Add(original,faded);}
            var group=renderer.transform.position.x<96?pavilionRoofs[0]:pavilionRoofs[1];
            group.views.Add(new CanopyView{renderer=renderer,original=original,faded=faded,shadows=renderer.shadowCastingMode});
        }
    }
    void TickPavilionRoofViews(float dt)
    {
        bool active=started&&data.onTrail&&!closing&&!(qa&&qaOpaquePavilions);
        foreach(var group in pavilionRoofs)
        {
            if(active&&CookingFrozen)continue; // Preserve the current view while reading or paused.
            float distance=Vector2.Distance(new Vector2(player.position.x,player.position.z),group.centre);
            group.near=active&&distance<=(group.near?5.4f:4.2f);
            float opacity=Mathf.MoveTowards(group.opacity,group.near?.06f:1,Mathf.Max(0,dt)*2.8f);
            if(Mathf.Abs(opacity-group.opacity)<.00001f)continue;
            group.opacity=opacity;
            foreach(var view in group.views)if(view.renderer){view.opacity=opacity;ApplyCanopyView(view);}
        }
    }
    void RestorePavilionRoofViews()
    {
        foreach(var group in pavilionRoofs)
        {
            group.near=false;group.opacity=1;
            foreach(var view in group.views)if(view.renderer){view.opacity=1;ApplyCanopyView(view);}
        }
    }
    void TestPavilionRoofViews()
    {
        NewGame();modal=false;ChangeRegion(true);player.position=TP(86,21);
        Assert(pavilionRoofs.Count==2&&pavilionRoofs.TrueForAll(g=>g.views.Count==227),"both pavilion roof bases 224 tiles and ridge registered");
        Assert(pavilionRoofs.TrueForAll(g=>g.views.TrueForAll(v=>!v.renderer.isPartOfStaticBatch)),"all pavilion roof pieces remain outside static batch");
        Assert(pavilionRoofs.TrueForAll(g=>g.views.TrueForAll(v=>v.faded.shader.isSupported)),"pavilion fade uses supported cached shader");
        string snapshot=JsonUtility.ToJson(data);int objects=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length;
        var lookout=pavilionRoofs[0];var patrol=pavilionRoofs[1];Color color=lookout.views[0].original.color;
        TickPavilionRoofViews(.1f);Assert(lookout.opacity<1&&lookout.opacity>.06f&&patrol.opacity==1,"approach gradually fades only nearby pavilion");
        TickPavilionRoofViews(1);Assert(Mathf.Approximately(lookout.opacity,.06f)&&lookout.views.TrueForAll(v=>v.renderer.enabled&&v.renderer.shadowCastingMode==ShadowCastingMode.Off),"near pavilion keeps faint roof without obstructing shadow");
        var block=new MaterialPropertyBlock();lookout.views[0].renderer.GetPropertyBlock(block);Assert(Mathf.Approximately(block.GetFloat(OpacityId),.06f),"roof opacity reaches actual renderer property block");
        player.position=TP(86,16);TickPavilionRoofViews(1);Assert(lookout.near,"pavilion exit hysteresis prevents boundary flicker");
        player.position=TP(86,15);TickPavilionRoofViews(.1f);Assert(!lookout.near&&lookout.opacity>.06f&&lookout.opacity<1,"departing pavilion smoothly restores roof");TickPavilionRoofViews(1);
        Assert(lookout.views.TrueForAll(v=>v.renderer.sharedMaterial==v.original&&v.renderer.shadowCastingMode==v.shadows),"far pavilion restores original material and shadow");
        player.position=TP(106,20);TickPavilionRoofViews(1);Assert(patrol.near&&!lookout.near&&lookout.opacity==1,"patrol roof fades independently from lookout");
        photoMode=true;TickPavilionRoofViews(1);Assert(patrol.near,"photo mode retains pavilion visibility");photoMode=false;
        player.position=TP(106,14);paused=true;TickPavilionRoofViews(1);Assert(patrol.near&&Mathf.Approximately(patrol.opacity,.06f),"pause keeps current pavilion view");paused=false;
        modal=true;TickPavilionRoofViews(1);Assert(patrol.near,"dialogue keeps current pavilion view");modal=false;
        notebook=true;TickPavilionRoofViews(1);Assert(patrol.near,"notebook keeps current pavilion view");notebook=false;
        player.position=TP(106,20);for(int i=0;i<20;i++)TickPavilionRoofViews(.02f);
        Assert(JsonUtility.ToJson(data)==snapshot&&lookout.views[0].original.color==color&&objects==FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length,"pavilion view changes no progress original material or geometry");
        Assert(Walkable(TP(86,21))&&Walkable(TP(105,20))&&Walkable(TP(107,20))&&spots.Find(s=>s.kind==25).pos==TP(86,21),"pavilion fade preserves reading and rain refuge positions");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(pavilionRoofs.TrueForAll(g=>g.opacity==1&&!g.near)&&data.onTrail,"load clears transient roof state without changing region");modal=false;
        TickPavilionRoofViews(1);ChangeRegion(false);Assert(pavilionRoofs.TrueForAll(g=>g.opacity==1&&!g.near),"return to valley immediately restores both pavilion roofs");
        ChangeRegion(true);player.position=TP(86,21);data.day=3;SyncWeatherPeople();TickPavilionRoofViews(1);Assert(lookout.near&&IsRainDay,"rain does not prevent readable pavilion view");
        NextDay();modal=false;Assert(pavilionRoofs.TrueForAll(g=>g.opacity==1),"next day resets pavilion transient state");
        ChangeRegion(true);player.position=TP(86,21);TickPavilionRoofViews(1);ReturnMenu();Assert(pavilionRoofs.TrueForAll(g=>g.opacity==1&&!g.near),"title restores opaque pavilion roofs");
        NewGame();modal=false;Assert(pavilionRoofs.Count==2&&pavilionRoofs.TrueForAll(g=>g.views.Count==227&&g.opacity==1),"new journey reuses bounded pavilion caches");
        Debug.Log("QA PAVILION ROOF PASS: complete registration, gradual independent fade, hysteresis, renderer properties, shadows, overlays, readonly state, rain, load, travel, days and title.");
    }
}
