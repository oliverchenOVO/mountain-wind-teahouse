using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    readonly List<Material> streamMaterials=new List<Material>();float streamVisualTime;
    void BuildRiverScenery()
    {
        foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            bool valley=r.name=="Flowing jade stream",mountain=r.name=="Continuous mountain stream";if(!valley&&!mountain)continue;
            var material=r.sharedMaterial;material.SetFloat("_BankMin",valley?6.05f:96);material.SetFloat("_BankMax",valley?9.95f:100);streamMaterials.Add(material);
        }
        ResetRiverScenery();
    }
    void ResetRiverScenery()
    {
        streamVisualTime=0;foreach(var material in streamMaterials){material.SetFloat("_StreamTime",0);material.SetFloat("_RainAmount",0);material.SetFloat("_NightAmount",0);}
    }
    void TickRiverScenery(float dt)
    {
        if(started&&!AvatarMotion.Frozen)streamVisualTime+=Mathf.Max(0,dt);
        foreach(var material in streamMaterials){material.SetFloat("_StreamTime",streamVisualTime);material.SetFloat("_RainAmount",started?weatherBlend:0);material.SetFloat("_NightAmount",started?teaNightBlend:0);}
    }
    void TestRiverScenery()
    {
        NewGame();modal=false;AvatarMotion.Frozen=false;TickRainWeather(1);TickRiverScenery(.5f);
        Assert(streamMaterials.Count==2&&streamMaterials.TrueForAll(m=>m.shader.isSupported),"valley and mountain rivers use two supported controlled water materials");
        Assert(Mathf.Approximately(streamMaterials[0].GetFloat("_BankMax")-streamMaterials[0].GetFloat("_BankMin"),3.9f)||Mathf.Approximately(streamMaterials[1].GetFloat("_BankMax")-streamMaterials[1].GetFloat("_BankMin"),3.9f),"valley shallows follow original river width");
        Assert(streamMaterials.Exists(m=>m.GetFloat("_BankMin")==96&&m.GetFloat("_BankMax")==100),"mountain water receives its own bank coordinates");
        Assert(streamVisualTime==.5f&&streamMaterials.TrueForAll(m=>m.GetFloat("_StreamTime")==streamVisualTime),"both streams advance from one bounded visual clock");
        float time=streamVisualTime;AvatarMotion.Frozen=true;TickRiverScenery(2);Assert(streamVisualTime==time,"overlays freeze stream patterns without parking particle jobs");AvatarMotion.Frozen=false;
        string snapshot=JsonUtility.ToJson(data);int objects=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length,particles=FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length;
        for(int i=0;i<20;i++)TickRiverScenery(.02f);
        Assert(snapshot==JsonUtility.ToJson(data),"river visuals cannot spend harvest fish or advance gameplay time");
        Assert(objects==FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length&&particles==FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length,"stream updates allocate no scene objects or particle systems");
        Assert(Walkable(new Vector3(8,0,-4))&&Walkable(new Vector3(8,0,12))&&!Walkable(new Vector3(8,0,-11)),"river scenery preserves both original bridges and water blocking");
        Assert(Walkable(spots.Find(s=>s.kind==14).pos)&&spots.Find(s=>s.kind==14).pos==new Vector3(5,0,-11),"fishing ledge stays at original walkable interaction position");
        data.day=3;TickRainWeather(2);TickRiverScenery(.1f);Assert(streamMaterials.TrueForAll(m=>m.GetFloat("_RainAmount")>.99f),"rain enables shader ripples without adding emitters");
        data.tea=5;OpenShop();modal=false;TickRainWeather(2);TickTeaNightScene(2);TickRiverScenery(.1f);Assert(streamMaterials.TrueForAll(m=>m.GetFloat("_NightAmount")==1),"night glints use actual night blend not a permanent glow");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(streamVisualTime==0&&data.night&&data.day==3,"load resets cosmetic stream clock without changing night or weather");
        NextDay();modal=false;TickRainWeather(2);TickTeaNightScene(2);TickRiverScenery(.1f);Assert(streamMaterials.TrueForAll(m=>m.GetFloat("_RainAmount")<.01f&&m.GetFloat("_NightAmount")==0),"next sunny morning removes rain and night accents");
        ChangeRegion(true);TickRiverScenery(.1f);Assert(data.onTrail&&streamMaterials.Count==2&&Walkable(TP(98,2)),"same water shader supports original mountain bridge");ChangeRegion(false);
        ReturnMenu();Assert(streamVisualTime==0&&streamMaterials.TrueForAll(m=>m.GetFloat("_RainAmount")==0&&m.GetFloat("_NightAmount")==0),"title clears temporary water weather properties");
        NewGame();modal=false;
        Debug.Log("QA RIVER SCENERY PASS: material bounds, visual clock, overlays, readonly state, caches, bridges, fishing, weather, night, save, day and region.");
    }
}
