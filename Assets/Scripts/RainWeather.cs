using UnityEngine;
using UnityEngine.Rendering;
using System;

public partial class MountainTeaGame
{
    bool IsRainDay {get{return data.day%3==0;}}
    ParticleSystem rainDrops;ParticleSystem.Particle[] rainBuffer=new ParticleSystem.Particle[500];
    AudioSource rainAudio;float rainAccumulator;bool rainDialogue;System.Random rainRandom=new System.Random(815);
    Vector3[] valleyHomes;Transform rainAwning;Renderer rainRoof;float weatherBlend;
    static bool Sheltered(Vector3 p)
    {
        if(p.x<60)return p.x>=-18&&p.x<=-6&&p.z>=-5.3f&&p.z<=7;
        return (Mathf.Abs(p.x-86)<3.2f||Mathf.Abs(p.x-106)<3.2f)&&Mathf.Abs(p.z-20)<3.2f;
    }
    void BuildRainWeather()
    {
        valleyHomes=new[]{spots[1].pos,spots[2].pos,spots[3].pos};
        rainAwning=new GameObject("Tea house rain awning").transform;
        rainRoof=TeaHouseWorld.Shape("Rain awning cloth",PrimitiveType.Cube,new Vector3(-12,3.3f,-3.6f),new Vector3(12,.16f,3.4f),sage,rainAwning).GetComponent<Renderer>();
        for(int x=-18;x<=-6;x+=12)TeaHouseWorld.Shape("Rain awning post",PrimitiveType.Cylinder,new Vector3(x,1.65f,-5.1f),new Vector3(.15f,1.65f,.15f),new Color(.42f,.3f,.2f),rainAwning);
        var g=new GameObject("Mountain rain streaks");rainDrops=g.AddComponent<ParticleSystem>();rainDrops.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=rainDrops.main;main.loop=true;main.startLifetime=.9f;main.startSpeed=0;main.startSize=.045f;main.maxParticles=500;main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=rainDrops.emission;emission.enabled=false;var shape=rainDrops.shape;shape.enabled=false;
        var renderer=g.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=8;renderer.velocityScale=.035f;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        var material=new Material(Resources.Load<Shader>("Shaders/SoftParticle"));material.color=new Color(.74f,.85f,.89f,.55f);renderer.sharedMaterial=material;
        rainDrops.Play();rainAudio=Ambience("Rain on tea roof",NoiseLoop(48,.4f,.45f));SyncWeatherPeople();
    }
    void SyncWeatherPeople()
    {
        if(valleyHomes==null)return;bool shelter=IsRainDay&&!data.night;
        Vector3[] refuge={new Vector3(-16,0,-3.8f),new Vector3(-9,0,-3.8f),new Vector3(-7,0,-3.8f)};
        for(int i=0;i<3;i++)
        {
            var s=spots[i+1];Vector3 position=shelter?refuge[i]:valleyHomes[i];position.y=GroundHeight(position.x,position.z);s.pos=position;s.visual.transform.position=position;
            s.visual.transform.rotation=Quaternion.Euler(0,shelter?180:0,0);
        }
        rainAwning.gameObject.SetActive(IsRainDay);
        rainRoof.enabled=Vector3.Distance(player.position,new Vector3(-12,0,-3.6f))>12;
    }
    void TickRainWeather(float dt)
    {
        if(!rainDrops)return;SyncWeatherPeople();bool wet=started&&IsRainDay;
        float volume=wet?(Sheltered(player.position)?.22f:.42f):0;
        rainAudio.volume=Mathf.Lerp(rainAudio.volume,volume,1-Mathf.Exp(-dt*3));
        weatherBlend=Mathf.Lerp(weatherBlend,wet?1:0,1-Mathf.Exp(-dt*3));
        float evening=data.night?1:Mathf.Clamp01(data.clock/360)*.45f;
        sun.intensity=Mathf.Lerp(Mathf.Lerp(1.15f,.48f,evening),data.night?.4f:.68f,weatherBlend);
        sun.color=Color.Lerp(Color.Lerp(new Color(1,.96f,.82f),new Color(.9f,.67f,.5f),evening),new Color(.77f,.84f,.94f),weatherBlend);
        RenderSettings.ambientLight=Color.Lerp(Color.Lerp(new Color(.62f,.7f,.62f),new Color(.32f,.39f,.47f),evening),data.night?new Color(.3f,.36f,.43f):new Color(.48f,.55f,.61f),weatherBlend);
        cam.backgroundColor=Color.Lerp(new Color(.69f,.77f,.66f),new Color(.56f,.63f,.68f),weatherBlend);RenderSettings.fogColor=cam.backgroundColor;
        if(!wet){if(rainDrops.particleCount>0)rainDrops.Clear();rainAccumulator=0;return;}
        if(AvatarMotion.Frozen){if(!rainDrops.isPaused)rainDrops.Pause();return;}
        if(rainDrops.isPaused)rainDrops.Play();
        rainAccumulator+=dt*280;
        while(rainAccumulator>=1)
        {
            rainAccumulator--;
            Vector3 p=player.position+new Vector3((float)rainRandom.NextDouble()*30-15,0,(float)rainRandom.NextDouble()*30-15);
            if(Sheltered(p))continue;p.y=GroundHeight(p.x,p.z)+14;
            var drop=new ParticleSystem.EmitParams{position=p,velocity=new Vector3(.5f,-18,.3f),startLifetime=.85f,startSize=.055f,startColor=new Color(.75f,.86f,.92f,.55f)};rainDrops.Emit(drop,1);
        }
        int count=rainDrops.GetParticles(rainBuffer);
        for(int i=0;i<count;i++)if(rainBuffer[i].position.y<GroundHeight(rainBuffer[i].position.x,rainBuffer[i].position.z)+.1f)rainBuffer[i].remainingLifetime=0;
        rainDrops.SetParticles(rainBuffer,count);
    }
    bool CanServeRainTea()
    {return started&&IsRainDay&&!data.night&&!data.onTrail&&!modal&&!paused&&!notebook&&!travelBook&&!planning&&!relationships&&!trial&&!fishing&&!brewing&&!data.holding&&data.lunchState!=2&&NearTea()&&data.rainStoryStage>=0&&data.rainStoryStage<3&&(data.tea>0||data.leafTea>0);}
    bool ServeRainTea()
    {
        if(!CanServeRainTea())return false;int chapter=data.rainStoryStage,dish=data.leafTea>0?4:0;
        bool premium=Quality(dish)>=Stock(dish);ChangeStock(dish,-1);if(premium)ChangeQuality(dish,-1);
        data.rainStoryStage++;data.money+=20;
        string[] speakers={"河城荷取","射命丸文","犬走椛"};
        string[] lines={"雨一來，工坊的零件就得收好。\n你的屋簷剛好讓我停下來喘口氣。\n暖茶真舒服。等雨停了，再回去修水車吧。", "今天的新聞，是這間茶屋沒有趕走避雨的人。\n鏡頭沾了水，就先不拍了。\n有些山中日常，只用眼睛記住也很好。", "巡山時我會記下每一個能避雨的地方。\n現在，這間茶屋也在那份名單上。\n雨會停，燈還亮著。謝謝你的茶。"};
        data.journal.Add("第 "+data.day+" 天 · 避雨茶 "+(chapter+1)+"/3 · "+speakers[chapter]);
        Say(speakers[chapter],lines[chapter]+"\n\n暖茶 −1 · 謝禮 ＋20 文"+(chapter==2?"\n《山雨時分》完成，故事已記入手帳。":""));rainDialogue=true;Play(chime);Save(false);return true;
    }
    void DrawRainHUD()
    {
        Text(990,211,415,30,"天氣："+(IsRainDay?"山雨":"晴朗")+" · 明日："+((data.day+1)%3==0?"山雨":"晴朗"),small);
        if(!IsRainDay||data.night)return;
        Panel(new Rect(980,253,435,190));Text(1000,265,390,35,"茶屋避雨 · "+Mathf.Clamp(data.rainStoryStage,0,3)+" / 3",heading);
        Text(1000,307,390,68,data.rainStoryStage>=3?"三杯茶，三段山中日常。\n你可以繼續探索、備餐與營業。":"回茶屋料理台附近，備好清茶或竹葉茶。\n每段消耗一杯茶，謝禮 20 文。",small);
        if(Button(1000,384,395,data.rainStoryStage>=3?"避雨故事已完成":"替友人泡一杯避雨茶",CanServeRainTea()))ServeRainTea();
    }
    void TestRainWeather()
    {
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.tea=3;
        Assert(!IsRainDay&&!ServeRainTea(),"rain story unavailable on sunny days");NextDay();NextDay();modal=false;player.position=new Vector3(-12,0,-3);
        Assert(IsRainDay&&data.day==3,"forecast predicts deterministic third-day rain");SyncWeatherPeople();Assert(spots[1].pos.x<0&&Sheltered(spots[1].pos),"valley friends seek tea awning");
        ChangeRegion(true);player.position=TP(82,-12);for(int t=0;t<4;t++)TickLivingMountain(10);
        for(int i=0;i<3;i++)Assert(Sheltered(spots.Find(s=>s.kind==routineKinds[i]).pos),"mountain NPC reaches rain pavilion");ChangeRegion(false);player.position=new Vector3(-12,0,-3);
        int cash=data.money;data.night=true;Assert(!ServeRainTea(),"rain tea cannot bypass night service");data.night=false;
        player.position=new Vector3(0,0,-12);Assert(!ServeRainTea(),"rain tea cannot be served remotely");player.position=new Vector3(-12,0,-3);data.holding=true;Assert(!ServeRainTea(),"held tray blocks rain tea");data.holding=false;
        data.qualityTea=3;Assert(ServeRainTea()&&rainDialogue&&data.tea==2&&data.qualityTea==2&&data.money==cash+20,"premium rain tea consumed once in dedicated dialogue");Assert(!ServeRainTea(),"dialogue blocks duplicate rain tea");modal=false;
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(IsRainDay&&data.rainStoryStage==1&&data.tea==2,"weather and rain chapter survive loading");
        Assert(ServeRainTea(),"second rain chapter");modal=false;Assert(ServeRainTea(),"third rain chapter");modal=false;Assert(!ServeRainTea()&&data.money==cash+60&&data.tea==0&&data.qualityTea==0,"three chapters and bounded reward");
        TickRainWeather(.3f);Assert(rainDrops.particleCount>0&&rainAudio.clip.samples>0,"rain emitter and procedural audio configured");NextDay();TickRainWeather(.1f);Assert(!IsRainDay&&rainDrops.particleCount==0&&data.rainStoryStage==3&&spots[1].pos==valleyHomes[0],"sunny day clears rain restores actors and retains completion");
        Assert(Sheltered(TP(86,20))&&Sheltered(TP(106,20))&&!Sheltered(TP(90,-4)),"rain excludes pavilion and tea shelter columns");
        Debug.Log("QA RAIN PASS: forecast, shelter, service gates, stock and quality, three chapters, no duplicate reward, persistence, particles, audio.");
    }
}
