using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    readonly List<Spot> restBenchSpots=new List<Spot>();int restSeat=-1;
    Vector3 restStandingPosition;Quaternion restStandingRotation;
    void BuildTrailRestActions()
    {
        AddSpot("觀景亭長凳",TP(88.52f,18.8f),31);restBenchSpots.Add(spots[spots.Count-1]);
        AddSpot("哨所長凳",TP(108.67f,17.9f),31);restBenchSpots.Add(spots[spots.Count-1]);
    }
    bool CanTrailRest(Spot spot)
    {
        return restBenchSpots.Contains(spot)&&restSeat<0&&started&&data.onTrail&&!data.night&&!data.holding&&data.lunchState!=2&&!trial&&!fishing&&!brewing&&!CookingFrozen&&Vector3.Distance(player.position,spot.pos)<2.2f;
    }
    bool BeginTrailRest(Spot spot)
    {
        if(!CanTrailRest(spot))return false;
        restStandingPosition=player.position;restStandingRotation=visual.rotation;
        restSeat=restBenchSpots.IndexOf(spot);player.position=spot.pos;visual.localPosition=Vector3.zero;visual.rotation=Quaternion.Euler(0,180,0);
        playerMotion.Seated=true;playerMotion.Walking=false;playerMotion.Gesture=0;nearest=spot;
        Notify("坐下聽聽山風 · E／Space／Esc 或移動即可起身，不會跨日或消耗材料。",5);return true;
    }
    void EndTrailRest()
    {
        if(restSeat<0)return;
        restSeat=-1;player.position=restStandingPosition;visual.localPosition=Vector3.zero;visual.rotation=restStandingRotation;
        playerMotion.Seated=false;playerMotion.Walking=false;nearest=null;
    }
    bool HandleTrailRestInput(Vector3 input,bool standKey)
    {
        if(restSeat<0)return false;
        if(input.sqrMagnitude>0||standKey)EndTrailRest();
        return true; // Consume the standing-up E so it cannot immediately sit again.
    }
    void TestTrailRestActions()
    {
        NewGame();modal=false;ChangeRegion(true);var bench=restBenchSpots[0];player.position=TP(bench.pos.x,bench.pos.z-1);
        Assert(restBenchSpots.Count==2&&restBenchSpots.TrueForAll(s=>s.kind==31&&Walkable(s.pos)),"two bounded rest benches registered on original walkable ground");
        nearest=SelectNearestSpot();Assert(nearest==bench&&CurrentInteractionCue().action=="坐下小憩","lookout bench selects distinct seated action instead of reading");
        string snapshot=JsonUtility.ToJson(data);Vector3 standing=player.position;Quaternion rotation=visual.rotation;
        Assert(BeginTrailRest(bench)&&restSeat==0&&playerMotion.Seated&&!playerMotion.Dining&&!playerMotion.Walking,"bench starts seated pose without dining or rewards");
        Assert(player.position==bench.pos&&visual.localPosition==Vector3.zero&&visual.rotation==Quaternion.Euler(0,180,0),"rest pose aligns hips with cushion and faces approach");
        Assert(CurrentInteractionCue().action=="起身繼續散步"&&CurrentGuide().key=="rest-bench","seated cue and guide clearly explain standing controls");
        Assert(JsonUtility.ToJson(data)==snapshot,"sitting changes no money ingredients bonds clock or daily progress");
        Assert(!BeginTrailRest(bench),"already seated cannot nest rest state");
        Assert(HandleTrailRestInput(Vector3.zero,false)&&restSeat==0,"idle rest retains seated state");
        paused=true;Assert(!CanTrailRest(bench)&&restSeat==0,"pause retains rest without accepting new action");paused=false;
        Assert(HandleTrailRestInput(Vector3.zero,true)&&restSeat==-1&&!playerMotion.Seated&&player.position==standing&&visual.rotation==rotation,"stand key consumes action and restores safe prior position and rotation");
        Assert(!HandleTrailRestInput(Vector3.zero,true),"standing controls do not consume normal interaction when not seated");
        BeginTrailRest(bench);Assert(HandleTrailRestInput(Vector3.right,false)&&restSeat==-1&&!playerMotion.Seated,"movement cancels rest before ordinary movement");
        player.position=bench.pos+Vector3.right*3;Assert(!BeginTrailRest(bench),"rest requires original interaction range");player.position=standing;
        data.holding=true;Assert(!BeginTrailRest(bench),"serving tray blocks sitting");data.holding=false;
        data.lunchState=2;Assert(!BeginTrailRest(bench),"packed lunch blocks sitting to preserve carrying pose");data.lunchState=0;
        modal=true;Assert(!BeginTrailRest(bench),"dialogue blocks sitting");modal=false;data.night=true;Assert(!BeginTrailRest(bench),"night blocks mountain rest");data.night=false;
        BeginTrailRest(bench);Save(false);var saved=JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(System.IO.Path.Combine(qaDir,"qa-save.json")));
        Assert(saved.x==standing.x&&saved.z==standing.z&&restSeat==0,"seated save uses safe standing position without ending local rest");
        Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(restSeat==-1&&!playerMotion.Seated&&Vector3.Distance(player.position,standing)<.001f&&data.onTrail,"loading resumes standing without adding save fields");modal=false;
        data.day=3;SyncWeatherPeople();player.position=TP(restBenchSpots[1].pos.x,restBenchSpots[1].pos.z-1);
        Assert(BeginTrailRest(restBenchSpots[1])&&restSeat==1&&IsRainDay,"patrol bench supports rainy day rest");EndTrailRest();
        player.position=standing;BeginTrailRest(bench);Assert(ChangeRegion(false)&&restSeat==-1&&!playerMotion.Seated&&!data.onTrail,"travel clears rest before changing region");
        ChangeRegion(true);player.position=standing;BeginTrailRest(bench);NextDay();Assert(restSeat==-1&&!playerMotion.Seated&&!data.onTrail,"next day clears rest pose");
        ChangeRegion(true);player.position=standing;BeginTrailRest(bench);ReturnMenu();Assert(restSeat==-1&&!playerMotion.Seated&&!started,"title clears rest pose");
        NewGame();modal=false;Assert(restSeat==-1&&restBenchSpots.Count==2&&!playerMotion.Seated,"new journey reuses original two benches");
        Debug.Log("QA TRAIL REST PASS: distance, pose, cue, guide, readonly state, cancellation, overlay and carrying guards, safe save position, rain, load, travel, days and title.");
    }
}
