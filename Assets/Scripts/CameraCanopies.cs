using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    class CanopyView
    {
        public Renderer renderer;public Material original,faded;public Bounds bounds;
        public ShadowCastingMode shadows;public float opacity=1,hold;
    }
    readonly List<CanopyView> canopyViews=new List<CanopyView>();
    readonly Vector3[] canopyFocus=new Vector3[16];int canopyFocusCount;
    MaterialPropertyBlock canopyBlock;bool qaOpaqueCanopies;
    static readonly int OpacityId=Shader.PropertyToID("_Opacity");
    public static bool IsViewCanopy(string name)
    {
        return name=="Faceted cedar canopy"||name=="Autumn maple crown"||name=="Bamboo leaf spray"||name=="Faceted mountain canopy";
    }
    void BuildCanopyViews()
    {
        canopyBlock=new MaterialPropertyBlock();var materials=new Dictionary<Material,Material>();
        Shader shader=Resources.Load<Shader>("Shaders/FoliageFade");
        foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(IsViewCanopy(renderer.name))
        {
            Material original=renderer.sharedMaterial;
            if(!materials.TryGetValue(original,out var faded)){faded=new Material(shader){name=original.name+" / camera fade",color=original.color};materials.Add(original,faded);}
            canopyViews.Add(new CanopyView{renderer=renderer,original=original,faded=faded,bounds=renderer.bounds,shadows=renderer.shadowCastingMode});
        }
    }
    static bool CanopyBlocks(Bounds bounds,Vector3 target,Camera camera)
    {
        // Orthographic rays must be parallel, not aimed from the camera's center.
        Vector3 direction=camera.transform.forward;float distance=Vector3.Dot(target-camera.transform.position,direction)-camera.nearClipPlane;
        if(distance<=0)return false;
        bounds.Expand(.9f);Ray ray=new Ray(target-direction*distance,direction);
        return bounds.IntersectRay(ray,out float entry)&&entry<distance;
    }
    void AddCanopyFocus(Vector3 target,float range)
    {
        if(canopyFocusCount>=canopyFocus.Length||(target.x>60)!=data.onTrail)return;
        Vector2 delta=new Vector2(target.x-player.position.x,target.z-player.position.z);
        if(delta.sqrMagnitude<=range*range)canopyFocus[canopyFocusCount++]=target;
    }
    void GatherCanopyFocus()
    {
        canopyFocusCount=0;AddCanopyFocus(player.position+Vector3.up*1.3f,1);
        if(!data.onTrail)
        {
            AddCanopyFocus(new Vector3(-18.8f,1.45f,-3.5f),9);
            AddCanopyFocus(new Vector3(-12,1.5f,-.55f),9);
            AddCanopyFocus(new Vector3(-7.6f,1.5f,-1.5f),7);
            if(data.night)for(int i=0;i<3;i++)AddCanopyFocus(new Vector3(-16+i*3,1.3f,-9.2f),8);
        }
        foreach(var spot in spots)if((spot.kind>=11&&spot.kind<=13||spot.kind==22||spot.kind==26||spot.kind==27)&&VisibleLivingSpot(spot))AddCanopyFocus(spot.pos+Vector3.up*1.3f,5);
        if(nearest!=null&&VisibleLivingSpot(nearest))AddCanopyFocus(nearest.pos+Vector3.up*.7f,3);
    }
    void ApplyCanopyView(CanopyView view)
    {
        bool opaque=view.opacity>=.999f;view.renderer.sharedMaterial=opaque?view.original:view.faded;
        view.renderer.shadowCastingMode=opaque?view.shadows:ShadowCastingMode.Off;
        if(opaque)view.renderer.SetPropertyBlock(null);
        else{canopyBlock.Clear();canopyBlock.SetFloat(OpacityId,view.opacity);view.renderer.SetPropertyBlock(canopyBlock);}
    }
    void TickCanopyViews(float dt)
    {
        if(canopyBlock==null)return;GatherCanopyFocus();
        foreach(var view in canopyViews)
        {
            if(!view.renderer)continue;bool blocked=false;
            bool active=started&&!(qa&&qaOpaqueCanopies)&&(view.bounds.center.x>60)==data.onTrail;
            if(active)for(int i=0;i<canopyFocusCount;i++)if((view.bounds.center-canopyFocus[i]).sqrMagnitude<324&&CanopyBlocks(view.bounds,canopyFocus[i],cam)){blocked=true;break;}
            if(blocked)view.hold=.18f;else view.hold=Mathf.Max(0,view.hold-dt);
            float target=active&&(blocked||view.hold>0)?.14f:1;
            float opacity=Mathf.MoveTowards(view.opacity,target,Mathf.Max(0,dt)*3);
            if(Mathf.Abs(opacity-view.opacity)>.00001f){view.opacity=opacity;ApplyCanopyView(view);}
        }
    }
    int FadedCanopyCount(){int count=0;foreach(var view in canopyViews)if(view.opacity<.99f)count++;return count;}
    void RestoreCanopyViews()
    {
        foreach(var view in canopyViews)if(view.renderer){view.opacity=1;view.hold=0;ApplyCanopyView(view);}
    }
    void TestCanopyViews()
    {
        NewGame();modal=false;Assert(canopyViews.Count>100&&canopyViews.Exists(v=>v.renderer.name=="Autumn maple crown")&&canopyViews.Exists(v=>v.renderer.name=="Faceted mountain canopy"),"valley and mountain canopy renderers registered");
        Assert(canopyViews.TrueForAll(v=>!v.renderer.isPartOfStaticBatch),"fade foliage excluded from static geometry batch");
        Vector3 target=player.position+Vector3.up*1.3f;cam.transform.position=player.position+CameraOffset;Vector3 front=target-cam.transform.forward*4;
        Assert(CanopyBlocks(new Bounds(front,Vector3.one),target,cam),"orthographic sight ray finds foreground foliage");
        Assert(!CanopyBlocks(new Bounds(target+cam.transform.forward*4,Vector3.one),target,cam),"foliage behind target does not fade");
        Assert(!CanopyBlocks(new Bounds(front+cam.transform.right*5,Vector3.one),target,cam),"side foliage does not fade");
        Assert(!CanopyBlocks(new Bounds(cam.transform.position-cam.transform.forward*4,Vector3.one),cam.transform.position-cam.transform.forward*2,cam),"targets behind camera do not fade");
        player.position=new Vector3(-16,0,-4);cam.transform.position=player.position+CameraOffset;RestoreCanopyViews();int money=data.money,harvest=data.harvested.Count;float clock=data.clock;
        TickCanopyViews(.1f);Assert(FadedCanopyCount()>0&&canopyViews.Exists(v=>v.opacity>.14f&&v.opacity<1),"tea facilities trigger a gradual fade not disappearance");TickCanopyViews(1);
        Assert(canopyViews.TrueForAll(v=>v.opacity>=.14f&&v.renderer.enabled),"faded canopy retains silhouette and renderer");
        var faded=canopyViews.Find(v=>v.opacity<1);Color original=faded.original.color;int objects=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length;
        for(int i=0;i<20;i++)TickCanopyViews(.1f);
        Assert(faded.original.color==original&&FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length==objects,"fade preserves shared original materials and creates no per-frame geometry");
        Assert(data.money==money&&data.clock==clock&&data.harvested.Count==harvest,"camera polish has no gameplay transaction or clock effects");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.money==money&&data.harvested.Count==harvest,"canopy effects do not alter saved progress");
        ChangeRegion(true);TickCanopyViews(1);Assert(canopyViews.TrueForAll(v=>v.bounds.center.x>60||v.opacity==1),"valley foliage restores on mountain travel");
        var mountain=canopyViews.Find(v=>v.renderer.name=="Faceted mountain canopy");player.position=TP(mountain.bounds.center.x,mountain.bounds.center.z+2);cam.transform.position=player.position+CameraOffset;TickCanopyViews(1);
        Assert(FadedCanopyCount()>0&&trailCanopies.TrueForAll(r=>r.enabled),"mountain uses fading instead of hard hiding");
        started=false;TickCanopyViews(1);Assert(FadedCanopyCount()==0&&canopyViews.TrueForAll(v=>v.renderer.sharedMaterial==v.original&&v.renderer.shadowCastingMode==v.shadows),"title restores original materials and shadows");
        NewGame();modal=false;RestoreCanopyViews();
        Debug.Log("QA CANOPY PASS: registration, orthographic sight, smooth fade, material isolation, no gameplay changes, travel, title and persistence.");
    }
}
