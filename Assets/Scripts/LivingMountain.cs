using UnityEngine;
using System;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    [Serializable] public class RoutineState {public float x,z,wait=8;public int node;}
    int travelTab;GameObject interactionMarker;
    AudioSource windAudio,streamAudio,fallsAudio;AudioClip footstep;float stepClock;
    readonly int[] routineKinds={26,27,22};
    readonly Vector2[][] paths={
        new[]{new Vector2(89,-5),new Vector2(90,2),new Vector2(86,12),new Vector2(86,21),new Vector2(86,12),new Vector2(90,2)},
        new[]{new Vector2(103,5),new Vector2(102,8),new Vector2(103,12),new Vector2(105,8)},
        new[]{new Vector2(106,18),new Vector2(112,14),new Vector2(110,9),new Vector2(106,14)}};
    void BuildLivingMountain()
    {
        AddSpot("白色羽毛",TP(90,-2),28);AddSpot("橋邊的紅繩",TP(102,2),29);AddSpot("落在石旁的筆記",TP(111,16),30);
        for(int i=0;i<3;i++)
        {
            var s=spots.Find(t=>t.kind==28+i);
            s.visual=TeaHouseWorld.Shape(s.name,PrimitiveType.Cube,s.pos+Vector3.up*.12f,new Vector3(.5f,.08f,.35f),i==1?new Color(.74f,.25f,.22f):cream);
            s.visual.transform.rotation=Quaternion.Euler(0,20+i*23,0);
        }
        interactionMarker=TeaHouseWorld.Shape("Interaction ground marker",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.3f,.012f,1.3f),gold);
        interactionMarker.SetActive(false);
        if(!FindFirstObjectByType<AudioListener>())player.gameObject.AddComponent<AudioListener>();
        windAudio=Ambience("Mountain wind",NoiseLoop(11,.09f,.015f));
        streamAudio=Ambience("Nearby stream",NoiseLoop(22,.32f,.13f));
        fallsAudio=Ambience("Waterfall spray",NoiseLoop(33,.44f,.37f));
        footstep=Tone(95,.055f);SyncRoutines();SyncNotebook();
    }
    AudioSource Ambience(string name,AudioClip clip)
    {
        var source=new GameObject(name).AddComponent<AudioSource>();source.transform.SetParent(transform);source.clip=clip;source.loop=true;source.volume=0;source.Play();return source;
    }
    AudioClip NoiseLoop(int seed,float amplitude,float smoothing)
    {
        const int rate=22050;var samples=new float[rate*4];var random=new System.Random(seed);float low=0;
        for(int i=0;i<samples.Length;i++){low=Mathf.Lerp(low,(float)random.NextDouble()*2-1,smoothing);samples[i]=low*amplitude;}
        // Crossfade the join to avoid a click when the loop repeats.
        for(int i=0;i<400;i++)samples[samples.Length-400+i]=Mathf.Lerp(samples[samples.Length-400+i],samples[i],i/399f);
        var clip=AudioClip.Create("Generated environmental texture",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    static float AmbientGain(float distance,float radius){float t=Mathf.Clamp01(1-distance/radius);return t*t;}
    void UpdateMountainAudio(float dt)
    {
        if(!windAudio)return;float active=started?1:0;
        windAudio.volume=Mathf.Lerp(windAudio.volume,active*.2f,dt*3);
        float riverDistance=data.onTrail?Mathf.Abs(player.position.x-98):Mathf.Abs(player.position.x-8);
        streamAudio.volume=Mathf.Lerp(streamAudio.volume,active*.55f*AmbientGain(riverDistance,13),dt*3);
        Vector3 waterfall=data.onTrail?TP(98,25):new Vector3(8,0,19);
        fallsAudio.volume=Mathf.Lerp(fallsAudio.volume,active*.7f*AmbientGain(Vector3.Distance(player.position,waterfall),23),dt*3);
        if(!AvatarMotion.Frozen&&playerMotion.Walking&&!fishing&&!trial){stepClock+=dt;if(stepClock>.38f){stepClock=0;audioSource.PlayOneShot(footstep,.2f);}}else stepClock=0;
    }
    void SyncRoutines()
    {
        if(data.routines==null||data.routines.Count!=3||data.routineDay!=data.day)
        {
            data.routines=new List<RoutineState>();data.routineDay=data.day;
            for(int i=0;i<3;i++)data.routines.Add(new RoutineState{x=paths[i][0].x,z=paths[i][0].y});
        }
        for(int i=0;i<3;i++)
        {
            var state=data.routines[i];if(state==null||float.IsNaN(state.x)||float.IsNaN(state.z)||!TrailWalkable(TP(state.x,state.z))){state=new RoutineState{x=paths[i][0].x,z=paths[i][0].y};data.routines[i]=state;}
            state.node=Mathf.Clamp(state.node,0,paths[i].Length-1);ApplyRoutine(i);
        }
    }
    void ApplyRoutine(int i)
    {var s=spots.Find(t=>t.kind==routineKinds[i]);var r=data.routines[i];s.pos=TP(r.x,r.z);s.visual.transform.position=s.pos;}
    void TickLivingMountain(float dt)
    {
        if(!started||AvatarMotion.Frozen||!data.onTrail||data.night||fishing||trial)return;
        if(data.routines==null||data.routineDay!=data.day)SyncRoutines();
        for(int i=0;i<3;i++)
        {
            var s=spots.Find(t=>t.kind==routineKinds[i]);var r=data.routines[i];var motion=s.visual.GetComponent<AvatarMotion>();motion.Walking=false;
            Vector3 facing=player.position-s.pos;facing.y=0;
            if(facing.sqrMagnitude<9){if(facing.sqrMagnitude>.01f)s.visual.transform.rotation=Quaternion.Slerp(s.visual.transform.rotation,Quaternion.LookRotation(facing),dt*4);continue;}
            if(r.wait>0){r.wait-=dt;if(r.wait<=0)r.node=(r.node+1)%paths[i].Length;continue;}
            Vector2 current=new Vector2(r.x,r.z),next=Vector2.MoveTowards(current,paths[i][r.node],dt*1.15f);
            if(TrailWalkable(TP(next.x,next.y))){r.x=next.x;r.z=next.y;ApplyRoutine(i);motion.Walking=next!=current;}
            Vector3 direction=new Vector3(next.x-current.x,0,next.y-current.y);if(direction.sqrMagnitude>.00001f)s.visual.transform.rotation=Quaternion.LookRotation(direction);
            if(Vector2.Distance(next,paths[i][r.node])<.05f)r.wait=8;
        }
    }
    bool AcceptNotebook(){if(data.notebookStage!=0||data.night)return false;data.notebookStage=1;SyncNotebook();Save(false);return true;}
    void SyncNotebook(){foreach(var s in spots)if(s.kind>=28&&s.kind<=30&&s.visual)s.visual.SetActive(data.notebookStage==s.kind-27);}
    bool FindNotebookClue(Spot s)
    {
        if(!data.onTrail||data.night||data.notebookStage!=s.kind-27||Vector3.Distance(player.position,s.pos)>2.6f)return false;
        data.notebookStage++;SyncNotebook();Play(pickupSound);
        Say("巡山筆記",s.kind==28?"石縫裡有一根白色羽毛。\n紅繩留下的痕跡往小橋另一端延伸。":s.kind==29?"這是綁在筆記封面上的紅繩。\n荷取提到，哨所下方的苔石旁有一本冊子。":"找到了！紙頁夾著巡查路線與幾片山茶葉。\n沿著小徑去找椛，把筆記還給她吧。");Save(false);return true;
    }
    bool ReturnNotebook()
    {
        if(data.notebookStage!=4||!data.onTrail||data.night||Vector3.Distance(player.position,spots.Find(s=>s.kind==22).pos)>2.6f)return false;
        data.notebookStage=5;data.money+=60;data.leaves+=3;data.journal.Add("第 "+data.day+" 天 · 找回椛的巡山筆記，約好回茶屋喝茶。");
        Say("犬走椛","你找回了我的巡山筆記！\n除了路線，裡面還記著山裡適合歇腳的地方。\n收下 60 文和三份茶葉吧。回茶屋後，泡杯茶再讀最後一頁。");Play(chime);Save(false);return true;
    }
    bool NotebookEpilogue()
    {
        if(data.notebookStage!=5||data.onTrail||data.night||!NearTea())return false;
        data.notebookStage=6;travelBook=false;data.journal.Add("巡山筆記末頁 · 山风茶屋也是值得守護的歇腳處。");
        Say("筆記的最後一頁","「巡查結束，溪谷的茶香又飄上來了。\n若有人走累了，就告訴他：山風茶屋一直亮著燈。」\n你把筆記的故事收進手帳。明天也好好開店吧。");Save(false);return true;
    }
    string NotebookHint(){return new[]{"椛遺失了巡山筆記，沿山路替她找回吧。","去文的拍照點以北找白色羽毛（90，−2）。","過小橋，找橋東側的紅繩（102，2）。","前往哨所下方的苔石旁（111，16）。","靠近巡山中的椛，按 E 歸還筆記。","返回茶屋料理台附近，閱讀後日談。","已完成 · 山風茶屋，也是值得守護的地方。"}[Mathf.Clamp(data.notebookStage,0,6)];}
    void DrawNotebookQuest()
    {
        Text(215,265,970,40,"椛的遺失巡山筆記",heading);
        Text(215,325,970,100,"沒有倒數，也不需要戰鬥。\n線索依序出現，任務進度會存檔，可分幾天完成。\n報酬：60 文、山茶葉 ×3；完成後回茶屋閱讀後日談。",body);
        Text(215,440,960,55,"下一步："+NotebookHint(),body);
        if(Button(215,518,430,"接下尋找筆記的委託",data.notebookStage==0&&!data.night))AcceptNotebook();
        if(Button(675,518,430,"在茶屋閱讀最後一頁",data.notebookStage==5&&!data.onTrail&&!data.night&&NearTea()))NotebookEpilogue();
        Text(215,585,960,100,"山中日常：文沿西岸散步拍照；荷取巡查東岸水路；椛繞行哨所與果實小徑。\n靠近角色時，她們會停下來。地圖上的角色標記會隨位置更新。\n金色腳下標記表示目前能互動的目標，按 E 即可。",small);
    }
    bool VisibleLivingSpot(Spot s){return s.kind<28||s.kind>30||data.notebookStage==s.kind-27;}
    void UpdateInteractionMarker()
    {
        if(!interactionMarker)return;bool show=started&&!AvatarMotion.Frozen&&!photoMode&&!trial&&nearest!=null&&VisibleLivingSpot(nearest);
        interactionMarker.SetActive(show);if(show)interactionMarker.transform.position=nearest.pos+Vector3.up*.055f;
    }
    void TestLivingMountain()
    {
        string oldSave=System.IO.Path.Combine(qaDir,"v61-save.json");
        if(System.IO.File.Exists(oldSave))
        {
            var old=JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(oldSave));Load(oldSave);
            Assert(data.money==old.money&&data.lunchState==old.lunchState&&data.deliveries==old.deliveries&&data.trailRecipes==old.trailRecipes&&data.chestnuts==old.chestnuts&&data.berries==old.berries&&data.chestnutRice==old.chestnutRice&&data.berryTea==old.berryTea&&data.notebookStage==0&&data.routines.Count==3,"v61 mountain progress retained and living fields initialized");
        }
        NewGame();modal=false;ChangeRegion(true);AvatarMotion.Frozen=false;
        foreach(var path in paths)for(int n=0;n<path.Length;n++)for(int k=0;k<=20;k++){Vector2 p=Vector2.Lerp(path[n],path[(n+1)%path.Length],k/20f);Assert(TrailWalkable(TP(p.x,p.y)),"NPC route stays on walkable bank");}
        player.position=TP(82,-12);TickLivingMountain(9);Vector3 before=spots.Find(s=>s.kind==26).pos;TickLivingMountain(1);Assert(Vector3.Distance(before,spots.Find(s=>s.kind==26).pos)>.5f,"NPC actually moves");
        player.position=spots.Find(s=>s.kind==26).pos;before=player.position;TickLivingMountain(1);Assert(spots.Find(s=>s.kind==26).pos==before,"NPC stops near player");
        AvatarMotion.Frozen=true;before=spots.Find(s=>s.kind==22).pos;TickLivingMountain(10);Assert(spots.Find(s=>s.kind==22).pos==before,"menus pause routine");AvatarMotion.Frozen=false;
        Assert(AcceptNotebook()&&!AcceptNotebook(),"quest accepts once");var clue=spots.Find(s=>s.kind==28);player.position=TP(82,-12);Assert(!FindNotebookClue(clue),"clue cannot be found remotely");
        for(int kind=28;kind<=30;kind++){clue=spots.Find(s=>s.kind==kind);player.position=clue.pos;Assert(FindNotebookClue(clue)&&!FindNotebookClue(clue),"ordered clue collected once");modal=false;}
        Assert(!ReturnNotebook(),"notebook return requires nearby Momiji");player.position=spots.Find(s=>s.kind==22).pos;int cash=data.money;Assert(ReturnNotebook()&&!ReturnNotebook()&&data.money==cash+60&&data.leaves==3,"notebook reward exactly once");modal=false;
        Vector3 savedPosition=spots.Find(s=>s.kind==26).pos;Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.notebookStage==5&&data.routines.Count==3&&spots.Find(s=>s.kind==26).pos==savedPosition,"quest and routine positions survive save");
        NextDay();Assert(data.notebookStage==5&&data.routineDay==data.day,"quest survives next day while routines reset");ChangeRegion(true);
        Assert(!NotebookEpilogue(),"epilogue needs tea house");ChangeRegion(false);player.position=new Vector3(-12,0,-3);Assert(NotebookEpilogue()&&!NotebookEpilogue(),"tea house epilogue once");modal=false;
        Assert(AmbientGain(2,23)>AmbientGain(20,23)&&AmbientGain(30,23)==0&&fallsAudio.clip.samples>0,"distance ambience falloff and generated clip");
        Debug.Log("QA LIVING PASS: patrol paths, movement, nearby stop, pause, ordered quest, reward, persistence, epilogue, ambience falloff.");
    }
}
