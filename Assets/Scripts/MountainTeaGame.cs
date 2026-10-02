using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public partial class MountainTeaGame : MonoBehaviour
{
    [Serializable] public class SaveData
    {
        public int version=1, day=1, money=40, leaves, mushrooms, bamboo, fish, tea, meal, grilled;
        public bool questAccepted, questDone, trialDone, restored, night;
        public float clock, x=-6, z=-7;
        public int served, lost, wave;
        public int serviceRevision, heldGuest=-1, heldDish, qualityTea, qualityMeal, qualityFish;
        public bool holding, heldPerfect;
        public int soup,leafTea,qualitySoup,qualityLeafTea,nightIncome,nightTips,reportDay;
        public bool onTrail,trailVisited,trailRecipes,lunchPerfect,photoVisited,waterVisited,lookoutVisited;
        public int lunchState,lunchDay,deliveries,chestnuts,berries,chestnutRice,berryTea,qualityChestnutRice,qualityBerryTea;
        public int notebookStage,routineDay;public List<RoutineState> routines;
        public int rainStoryStage;
        public int weekStartDay,weekHarvest,weekCraft,weekServed;public bool[] weekClaimed;
        public int specialDay,specialDish,nightSpecialIncome;
        public bool comfortUpgrade,prepUpgrade;
        public bool guideHidden;
        public bool[] festivalShared;public bool festivalRewardClaimed;
        public int supplyDay,supplySpent;public int[] supplyBought;
        public bool ledgerStarted;public List<LedgerDay> ledger;
        public bool[] menu;public int[] prepTargets,gardenOwned,gardenStyle,sales;
        public List<string> journal=new List<string>();
        public List<Bond> bonds=new List<Bond>();
        public List<int> harvested=new List<int>();
        public List<Guest> guests=new List<Guest>();
    }
    [Serializable] public class Guest
    {
        public string name; public int order; public float patience=100; public bool done;
        public int state, dish, reward; public float stageTime; public bool paid;
        public bool perfect;
        public int dailyBonus;
        public Guest(string n,int o) {name=n;order=o;}
    }
    class Spot { public string name; public Vector3 pos; public int kind,id; public GameObject visual; }
    class Bullet { public GameObject visual; public Vector3 velocity; }
    public SaveData data=new SaveData();
    List<Spot> spots=new List<Spot>(); List<Bullet> bullets=new List<Bullet>();
    List<GameObject> visitors=new List<GameObject>();
    Transform player,visual; Camera cam; Light sun; AudioSource audioSource;
    AudioClip chime, pickupSound, bgm; Font font;
    GUIStyle title,heading,body,small,button,label;
    bool stylesReady, started, paused, notebook, modal, fishing, trial, result, credits;
    string speaker="",dialogue="",toast=""; float toastTimer, fishTimer, trialTimer, emitTimer, hurtTimer, walking;
    int hearts=3; Vector3 beforeTrial; Spot nearest;
    string savePath; float autosave;
    Color ink=new Color(.16f,.24f,.22f),cream=new Color(.96f,.94f,.86f),sage=new Color(.25f,.41f,.34f),gold=new Color(.83f,.65f,.34f);
    bool qa; string qaDir;
    bool closing;
    bool photoMode;
    static readonly Vector3 CameraOffset=new Vector3(0,21,-26.9f);
    AvatarMotion playerMotion;ValleyAtmosphere atmosphere;
    Transform visitorRoot;
    Dictionary<string,RenderTexture> portraits=new Dictionary<string,RenderTexture>();Texture2D paper;
    public static readonly string[] DishNames={"山風清茶","竹筍菇飯","溪魚鹽燒","山茶香菇湯","竹葉暖茶","山栗菇飯","野莓暖茶"};
    public static readonly int[] Prices={32,42,55,48,38,62,52};

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { if(!FindFirstObjectByType<MountainTeaGame>()) new GameObject("Mountain Tea / Game").AddComponent<MountainTeaGame>(); }
    void Awake()
    {
        Application.targetFrameRate=60; QualitySettings.vSyncCount=1;
        savePath=Path.Combine(Application.persistentDataPath,"mountain-tea-save.json");
        string[] args=Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length-1;i++) if(args[i]=="--qa") {qa=true;qaDir=args[i+1];}
        if(qa) Application.runInBackground=true;
        font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft JhengHei","Microsoft YaHei","Arial"},22);
        MountainArt.Build();atmosphere=FindFirstObjectByType<ValleyAtmosphere>();visitorRoot=new GameObject("Tea garden visitors").transform;
        var p=new GameObject("Player");player=p.transform;player.position=new Vector3(-6,0,-7);
        visual=TeaHouseWorld.Character("Traveler",player.position).transform;visual.SetParent(player,true);playerMotion=visual.GetComponent<AvatarMotion>();
        var cg=new GameObject("Valley Camera");cam=cg.AddComponent<Camera>();cam.tag="MainCamera";
        cam.orthographic=true;cam.orthographicSize=11.5f;cam.farClipPlane=150;cam.backgroundColor=new Color(.69f,.77f,.66f);cam.cullingMask=~(1<<30);
        cam.transform.rotation=Quaternion.Euler(38,0,0);cam.transform.position=player.position+CameraOffset;
        var sg=new GameObject("Mountain sunlight");sun=sg.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.shadows=LightShadows.Soft;sg.transform.rotation=Quaternion.Euler(48,-32,0);
        RenderSettings.ambientLight=new Color(.67f,.71f,.60f);RenderSettings.fog=true;RenderSettings.fogColor=cam.backgroundColor;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=60;RenderSettings.fogEndDistance=120;
        QualitySettings.shadowDistance=55;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;
        AddSpot("茶屋料理台",new Vector3(-12,0,-2.2f),10);
        AddSpot("河城荷取",new Vector3(13,0,-2),11,"Nitori");
        AddSpot("射命丸文",new Vector3(-3,0,6),12,"Aya");
        AddSpot("犬走椛",new Vector3(16,0,12),13,"Momiji");
        AddSpot("溪流釣點",new Vector3(5,0,-11),14);
        AddSpot("茶屋招牌",new Vector3(-8,0,-2.3f),15);
        AddSpot("回房休息",new Vector3(-17.5f,0,1),16);
        Vector3[] points={new Vector3(-2,0,-9),new Vector3(1,0,-11),new Vector3(-7,0,9),new Vector3(-12,0,12),new Vector3(2,0,9),new Vector3(-17,0,12),new Vector3(12,0,-10),new Vector3(17,0,-9),new Vector3(20,0,6),new Vector3(-19,0,-14),new Vector3(-11,0,-14),new Vector3(1,0,15),new Vector3(12,0,9),new Vector3(19,0,15),new Vector3(-19,0,5),new Vector3(-1,0,1),new Vector3(-9,0,10),new Vector3(18,0,-14),new Vector3(-2,0,-15),new Vector3(3,0,4),new Vector3(13,0,-14),new Vector3(-20,0,-8),new Vector3(-15,0,10),new Vector3(20,0,0)};
        for(int i=0;i<points.Length;i++) AddSpot(new[]{"山茶葉","野生香菇","嫩竹筍"}[i%3],points[i],i%3);
        BuildMountainTrip();
        audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=.28f;
        chime=Tone(660,.24f);pickupSound=Tone(880,.12f);bgm=Music();
        musicSource=gameObject.AddComponent<AudioSource>();musicSource.clip=bgm;musicSource.loop=true;musicSource.volume=.12f;musicSource.Play();
        BuildLivingMountain();
        BuildRainWeather();
        BuildTeaLife();
        BuildUpgradeArt();
        BuildFestival();
        BuildSupplyCart();
        BuildCanopyViews();
        BuildTeaRoofView();
        BuildCookingLife();
        CreatePortraits();
        LoadAudioPreferences();
        if(qa) StartCoroutine(QARun());
    }
    void AddSpot(string n,Vector3 p,int kind,string character=null)
    {
        p.y=GroundHeight(p.x,p.z);
        var s=new Spot{name=n,pos=p,kind=kind,id=spots.Count};
        if(character!=null) s.visual=TeaHouseWorld.Character(character,p);
        else if(kind<3)
        {
            s.visual=new GameObject(n);s.visual.transform.position=p;
            if(kind==0)
            {
                for(int i=0;i<3;i++) TeaHouseWorld.Shape("Tea leaf",PrimitiveType.Sphere,p+new Vector3((i-1)*.23f,.3f+i*.1f,0),new Vector3(.46f,.35f,.38f),new Color(.45f,.65f,.30f),s.visual.transform);
            }
            else if(kind==1)
            {
                TeaHouseWorld.Shape("Mushroom stem",PrimitiveType.Cylinder,p+Vector3.up*.2f,new Vector3(.17f,.2f,.17f),cream,s.visual.transform);
                TeaHouseWorld.Shape("Mushroom cap",PrimitiveType.Sphere,p+Vector3.up*.45f,new Vector3(.7f,.25f,.7f),new Color(.71f,.33f,.24f),s.visual.transform);
            }
            else for(int i=0;i<3;i++) TeaHouseWorld.Shape("Bamboo shoot",PrimitiveType.Capsule,p+new Vector3((i-1)*.19f,.35f,0),new Vector3(.20f,.45f,.20f),new Color(.66f,.70f,.35f),s.visual.transform);
        }
        else if(kind==14)
        {
            s.visual=TeaHouseWorld.Shape("Fishing marker",PrimitiveType.Cylinder,p+Vector3.up*.03f,new Vector3(1.7f,.03f,1.7f),new Color(.83f,.70f,.43f));
        }
        spots.Add(s);
    }
    void Update()
    {
        if(playerMotion)playerMotion.Walking=false;
        if(toastTimer>0) toastTimer-=Time.deltaTime;
        if(saveBadgeTimer>0)saveBadgeTimer-=Time.deltaTime;
        if(started&&Input.GetKeyDown(KeyCode.F8))photoMode=!photoMode;
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(settingsOpen){CloseAudioSettings();}
            else if(fishing){fishing=false;Notify("收起釣竿。");}
            else if(brewing){CancelBrew();}
            else if(modal){if(ambientDialogue)CloseAmbientChat(false);else if(storyGuest>=0)FinishStory();else {modal=false;speaker="";eventFriend=-1;ResetAmbientTalking();}}
            else if(relationships){relationships=false;notebook=true;}
            else if(planning){Save(false);planning=false;}
            else if(travelBook)travelBook=false;
            else if(notebook) notebook=false;
            else if(started&&!result) paused=!paused;
        }
        if(started&&!paused&&!modal&&!result&&!brewing&&Input.GetKeyDown(KeyCode.Tab)){if(travelBook){travelBook=false;notebook=true;}else if(planning){Save(false);planning=false;notebook=true;}else if(relationships){relationships=false;notebook=true;}else notebook=!notebook;return;}
        TickPlanningShortcuts();
        AvatarMotion.Frozen=!started||paused||notebook||modal||result||brewing||relationships||planning||travelBook||settingsOpen;
        if(brewing){if(!CookingFrozen){brewTimer+=Time.deltaTime;if(Input.GetKeyDown(KeyCode.Space))FinishBrew();}return;}
        if(!started||paused||notebook||modal||result||relationships||planning||travelBook||settingsOpen) return;
        if(Input.GetKeyDown(KeyCode.F5)) Save();
        if(fishing)
        {
            fishTimer+=Time.deltaTime;
            if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.E)) CatchFish();
            return;
        }
        Vector3 input=new Vector3((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),0,
            (Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
        float speed=Input.GetKey(KeyCode.LeftShift)?7:4.6f;
        Vector3 delta=input.normalized*speed*Time.deltaTime;
        Vector3 proposed=player.position+delta;
        if(trial) { proposed.x=Mathf.Clamp(proposed.x,11,21);proposed.z=Mathf.Clamp(proposed.z,7,17);player.position=proposed; }
        else
        {
            if(Walkable(proposed))player.position=proposed;
            else
            {
                Vector3 slide=player.position+new Vector3(delta.x,0,0);if(Walkable(slide))player.position=slide;
                slide=player.position+new Vector3(0,0,delta.z);if(Walkable(slide))player.position=slide;
            }
        }
        player.position=new Vector3(player.position.x,GroundHeight(player.position.x,player.position.z),player.position.z);
        if(playerMotion)playerMotion.Walking=input.sqrMagnitude>0;
        if(input.sqrMagnitude>0) {visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(input),Time.deltaTime*12);walking+=Time.deltaTime*12;}
        visual.localPosition=new Vector3(0,input.sqrMagnitude>0?Mathf.Sin(walking)*.055f:Mathf.Sin(Time.time*2)*.018f,0);
        Vector3 target=player.position+CameraOffset;
        cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-Time.deltaTime*5));
        if(Input.mouseScrollDelta.y!=0)cam.orthographicSize=Mathf.Clamp(cam.orthographicSize-Input.mouseScrollDelta.y,9,17);
        nearest=SelectNearestSpot();
        if(Input.GetKeyDown(KeyCode.E)&&!trial){if(data.holding)DeliverTray();else if(nearest!=null)Interact(nearest);}
        if(trial){UpdateTrial();return;}
        if(!data.night)
        {
            data.clock=Mathf.Min(360,data.clock+Time.deltaTime);
            if(data.clock>=360&&!modal) NotifyOnceDusk();
        }
        else UpdateGuests();
        float evening=data.night?1:Mathf.Clamp01(data.clock/360)*.45f;
        if(atmosphere)atmosphere.Evening=data.night;
        sun.color=Color.Lerp(new Color(1,.96f,.82f),new Color(.90f,.67f,.50f),evening);sun.intensity=Mathf.Lerp(1.15f,.48f,evening);
        RenderSettings.ambientLight=Color.Lerp(new Color(.62f,.70f,.62f),new Color(.32f,.39f,.47f),evening);
        autosave+=Time.deltaTime;if(autosave>20){autosave=0;Save(false);}
    }
    bool duskNotified;
    void NotifyOnceDusk(){if(!duskNotified){duskNotified=true;Notify("夕陽落在山肩。準備好料理後，可以提早或現在開店。",7);}}
    public bool Walkable(Vector3 p)
    {
        if(data.onTrail)return TrailWalkable(p);
        if(p.x<-23||p.x>23||p.z<-18||p.z>18)return false;
        if(p.x>5.9f&&p.x<10.1f&&Mathf.Abs(p.z+4)>1.1f&&Mathf.Abs(p.z-12)>1.1f)return false;
        if(p.x>-16.5f&&p.x<-7.5f&&p.z>.1f&&p.z<7)return false;
        if(p.x>12.2f&&p.x<17.8f&&p.z>.6f&&p.z<5.5f)return false;
        return true;
    }
    void Interact(Spot s)
    {
        if(s.kind>=20){InteractTrip(s);return;}
        if(s.kind<3)
        {
            if(data.harvested.Contains(s.id)||data.night)return;
            NormalizeWeek();data.weekHarvest++;data.harvested.Add(s.id); if(s.visual)s.visual.SetActive(false);
            if(playerMotion)playerMotion.Gesture=1;
            if(s.kind==0)data.leaves+=3;else if(s.kind==1)data.mushrooms+=2;else data.bamboo+=2;
            Notify(s.name+" ＋"+(s.kind==0?3:2));Play(pickupSound);Save(false);return;
        }
        ResetAmbientTalking();rainDialogue=false;festivalDialogue=false;modal=true;speaker=s.name;
        if(s.kind==10)dialogue="山風從門簾穿過，茶香留在屋裡。\n備好料理，客人才會在傍晚登門。";
        if(s.kind==11)dialogue=data.questDone?"水車又轉起來啦！茶屋那邊忙的話，我晚上過去捧場。":data.questAccepted?"修水車還缺 4 份嫩竹筍。不是拿來做機械啦，是給工人吃的！\n交給我，我付你 80 文工錢。":"你好，我是河城荷取。茶屋終於有人接手了？\n我正忙著修水車，能帶 4 份嫩竹筍給我嗎？報酬 80 文。";
        if(s.kind==12)dialogue="文文。新聞是：妖怪之山又有一間茶屋開張！\n我喜歡山風清茶，椛偏愛溪魚鹽燒；荷取來了記得準備竹筍菇飯。\n過橋沿著北邊山路走，就能找到椛。";
        if(s.kind==13)dialogue=data.trialDone?"身手不錯。今晚巡山結束，我會去茶屋。\n溪魚鹽燒配一杯茶，很適合休息。":"我是犬走椛。前面的山路需要巡查。\n來一場 25 秒的符卡練習吧：躲開紅色彈幕就好，撐過去有 50 文獎勵。\n這是練習，失敗隨時能再試。";
        if(s.kind>=11&&s.kind<=13){OfferAmbientChat(s);SetDialogueMood(s.kind==11?2:s.kind==13?3:1);}
        if(s.kind==14){modal=false;fishing=true;fishTimer=0;Notify("在浮標進入金色區域時按空白鍵。",4);}
        if(s.kind==15)dialogue=data.restored?"新招牌掛上去了。山風茶屋，今天也好好營業吧。":"舊招牌被風雨打壞了。\n花 180 文修繕，讓山風茶屋重新成為山中歇腳的地方。";
        if(s.kind==16)dialogue="收好工具，回房睡一晚。\n進入下一天會補滿山中的採集物，尚未結束的營業會先結算。";
    }
    public bool Craft(int dish)
    {
        if(dish<0||dish>=DishNames.Length||!RecipeUnlocked(dish)){Notify("這道食譜尚未解鎖。");return false;}
        if(dish==0&&data.leaves>=2){data.leaves-=2;data.tea++;}
        else if(dish==1&&data.mushrooms>=1&&data.bamboo>=1){data.mushrooms--;data.bamboo--;data.meal++;}
        else if(dish==2&&data.fish>=1){data.fish--;data.grilled++;}
        else if(dish==3&&Ingredients(3)){data.mushrooms-=2;data.leaves--;data.soup++;}
        else if(dish==4&&Ingredients(4)){data.leaves--;data.bamboo--;data.leafTea++;}
        else if(dish==5&&Ingredients(5)){data.chestnuts-=2;data.mushrooms--;data.chestnutRice++;}
        else if(dish==6&&Ingredients(6)){data.berries-=2;data.leaves--;data.berryTea++;}
        else {Notify("材料不足，白天去山中找找。",4);return false;}
        NormalizeWeek();data.weekCraft++;Notify("製作完成："+DishNames[dish]);Play(pickupSound);Save(false);return true;
    }
    public int Stock(int dish){return dish==0?data.tea:dish==1?data.meal:dish==2?data.grilled:dish==3?data.soup:dish==4?data.leafTea:dish==5?data.chestnutRice:dish==6?data.berryTea:0;}
    public bool CompleteQuest()
    {
        if(!data.questAccepted||data.questDone||data.bamboo<4)return false;
        RecordLedger(2,80);data.bamboo-=4;data.money+=80;data.questDone=true;Play(chime);Notify("荷取的委託完成 · ＋80 文");Save(false);return true;
    }
    void CatchFish()
    {
        float pos=(Mathf.Sin(fishTimer*2.3f)+1)*.5f;
        fishing=false;
        if(pos>=.60f&&pos<=.88f){data.fish++;Notify("釣到溪魚！＋1");Play(chime);Save(false);}
        else Notify("魚兒溜走了。再按 E 試試，沒有材料損失。",4);
    }
    public bool OpenShop()
    {
        if(data.night||data.onTrail||data.lunchState==2){Notify("先回到茶屋，或把便當拆開，再開始營業。");return false;}
        if(MenuStock()<3){Notify("今晚菜單上的料理至少要準備 3 份。",5);return false;}
        data.night=true;data.clock=360;data.served=0;data.lost=0;data.wave=0;modal=false;
        NormalizeSpecial();data.nightIncome=0;data.nightTips=0;data.nightSpecialIncome=0;data.sales=new int[DishNames.Length];data.reportDay=data.day;
        NewWave();Notify("門簾亮起。今晚的客人到了！",5);Save(false);return true;
    }
    void NewWave()
    {
        foreach(var g in visitors)if(g)Destroy(g);visitors.Clear();ValleyAtmosphere.ClearMeals();
        data.guests.Clear();
        if(data.wave==0){data.guests.Add(new Guest("射命丸文",0));data.guests.Add(new Guest("河城荷取",1));data.guests.Add(new Guest("犬走椛",2));}
        else{data.guests.Add(new Guest("河城荷取",0));data.guests.Add(new Guest("犬走椛",1));data.guests.Add(new Guest("射命丸文",0));}
        ChooseGuestOrders();ShowVisitors(true);
    }
    void ShowVisitors(bool animateArrival=false)
    {
        foreach(var s in spots)if(s.kind>=11&&s.kind<=13&&s.visual)s.visual.SetActive(!data.night);
        while(visitors.Count<data.guests.Count)visitors.Add(null);
        for(int i=0;i<data.guests.Count;i++)
        {
            if(data.guests[i].done)
            {
                if(visitors[i]){visitors[i].GetComponent<AvatarMotion>().DepartAfterMeal(2.5f,new Vector3(-5,0,-4));visitors[i]=null;}
                continue;
            }
            if(visitors[i])continue;
            Vector3 seat=new Vector3(-16+i*3,0,-9.2f);string n=data.guests[i].name;
            var g=TeaHouseWorld.Character(n=="河城荷取"?"Nitori":n=="射命丸文"?"Aya":"Momiji",animateArrival?new Vector3(-5+i*.3f,0,-5):seat);
            g.transform.SetParent(visitorRoot,true);
            g.transform.rotation=Quaternion.Euler(0,180,0);var motion=g.GetComponent<AvatarMotion>();
            if(animateArrival)motion.Arrive(seat);else motion.Seated=true;
            if(data.guests[i].state==2||data.guests[i].state==3)motion.SetMeal(data.guests[i].dish,false);
            if(data.guests[i].state==4)motion.DepartAfterMeal(0,new Vector3(-5,0,-4));visitors[i]=g;
        }
    }
    void UpdateGuests()
    {
        TickCafe(Time.deltaTime);
    }
    public bool Serve(int guest,int dish)
    {
        if(!PickTray(guest,dish))return false;
        return DeliverTray(true);
    }
    public bool Restore()
    {
        if(data.restored||data.money<180)return false;
        RecordLedger(3,180);data.money-=180;data.restored=true;UpdateSign();modal=false;result=true;Play(chime);Save(false);return true;
    }
    void UpdateSign()
    {
        var sign=GameObject.Find("Tea sign");if(sign)sign.GetComponent<Renderer>().sharedMaterial=TeaHouseWorld.Mat(data.restored?"RestoredSign":"OldSign",data.restored?gold:new Color(.42f,.39f,.31f));
    }
    public void NextDay()
    {
        ReturnTray();
        if(data.night)foreach(var g in data.guests)if(!g.done){if(g.state>=2)PayGuest(g);else data.lost++;}
        if(data.night){data.reportDay=data.day;}
        TodayLedger().closed=true;
        storyGuest=-1;brewing=false;ResetCookingLife();ResetAmbientTalking();
        data.day++;data.clock=0;data.night=false;data.harvested.Clear();data.guests.Clear();data.wave=0;duskNotified=false;
        data.onTrail=false;travelBook=false;SyncTrip();
        if(Friendship(2).stage==3)data.bamboo+=2;
        if(atmosphere)atmosphere.Evening=false;ValleyAtmosphere.ClearMeals();
        if(visitorRoot)foreach(Transform guest in visitorRoot)Destroy(guest.gameObject);
        sun.color=new Color(1,.96f,.82f);sun.intensity=1.15f;RenderSettings.ambientLight=new Color(.62f,.70f,.62f);
        foreach(var s in spots)if(s.kind<3&&s.visual)s.visual.SetActive(true);
        foreach(var g in visitors)Destroy(g);visitors.Clear();player.position=new Vector3(-6,0,-7);
        ShowVisitors();
        modal=false;Notify("第 "+data.day+" 天 · 山裡的採集物重新長好了。",5);Save(false);
    }
    void StartTrial()
    {
        if(data.night||data.onTrail){Notify("符卡練習在溪谷山路進行，白天回去再練習。",4);return;}
        beforeTrial=player.position;player.position=new Vector3(16,0,10);modal=false;trial=true;trialTimer=25;emitTimer=0;hearts=3;hurtTimer=0;
        Notify("符卡練習 · 在山路方場中閃避 25 秒！",4);
    }
    void UpdateTrial()
    {
        trialTimer-=Time.deltaTime;emitTimer-=Time.deltaTime;hurtTimer-=Time.deltaTime;
        if(emitTimer<=0)
        {
            emitTimer=.85f;float rotation=(25-trialTimer)*.25f;
            for(int i=0;i<10;i++)
            {
                float angle=i*Mathf.PI*2/10+rotation;var p=new Vector3(16,.6f,15.8f);
                var g=TeaHouseWorld.Shape("Spell practice bullet",PrimitiveType.Sphere,p,new Vector3(.28f,.28f,.28f),new Color(.93f,.30f,.31f));
                bullets.Add(new Bullet{visual=g,velocity=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*2.5f});
            }
        }
        for(int i=bullets.Count-1;i>=0;i--)
        {
            var b=bullets[i];b.visual.transform.position+=b.velocity*Time.deltaTime;Vector3 p=b.visual.transform.position;
            if(hurtTimer<=0&&Vector2.Distance(new Vector2(p.x,p.z),new Vector2(player.position.x,player.position.z))<.43f){hearts--;hurtTimer=1.4f;Play(pickupSound);Notify("擦傷！還有 "+hearts+" 次機會。");}
            if(p.x<10||p.x>22||p.z<6||p.z>18){Destroy(b.visual);bullets.RemoveAt(i);}
        }
        visual.gameObject.SetActive(hurtTimer<=0||Mathf.FloorToInt(Time.time*12)%2==0);
        if(hearts<=0||trialTimer<=0)EndTrial(hearts>0);
    }
    void EndTrial(bool success)
    {
        trial=false;foreach(var b in bullets)Destroy(b.visual);bullets.Clear();visual.gameObject.SetActive(true);player.position=beforeTrial;
        if(success&&!data.trialDone){RecordLedger(2,50);data.trialDone=true;data.money+=50;Notify("符卡練習通過 · ＋50 文！",6);Play(chime);Save(false);}
        else Notify(success?"再一次漂亮地避開了彈幕！":"練習結束。和椛說話就能再試，沒有懲罰。",5);
    }
    void Say(string n,string words){ResetAmbientTalking();rainDialogue=false;festivalDialogue=false;speaker=n;dialogue=words;modal=true;SetDialogueMood(n=="河城荷取"?2:n=="犬走椛"?3:1);}
    void Notify(string words,float time=3){toast=words;toastTimer=time;}
    void Play(AudioClip c){if(audioSource&&c)audioSource.PlayOneShot(c);}
    public void Save(bool feedback=true)
    {
        if(!started)return;
        NormalizeWeek();
        NormalizeSpecial();
        NormalizeFestival();
        NormalizeSupplies();
        NormalizeLedger();
        data.serviceRevision=3;
        data.x=player.position.x;data.z=player.position.z;
        if(trial){data.x=beforeTrial.x;data.z=beforeTrial.z;}
        try
        {
            string target=qa?Path.Combine(qaDir,"qa-save.json"):savePath;
            string temporary=target+".tmp";File.WriteAllText(temporary,JsonUtility.ToJson(data,true));
            if(File.Exists(target))File.Copy(target,target+".bak",true);
            File.Copy(temporary,target,true);File.Delete(temporary);saveBadgeTimer=2.8f;if(feedback)Notify("已儲存 · 第 "+data.day+" 天");
        }
        catch(Exception e){Debug.LogError("Save failed: "+e.Message);Notify("存檔失敗，請確認資料夾可寫入。",6);}
    }
    void Load(string source=null)
    {
        try
        {
            ResetTransient();
            data=JsonUtility.FromJson<SaveData>(File.ReadAllText(source??savePath));
            if(data==null||data.version!=1||data.day<1)throw new Exception("Invalid save version");
            if(data.harvested==null)data.harvested=new List<int>();if(data.guests==null)data.guests=new List<Guest>();
            MigrateCafe();
            NormalizePlanning();
            NormalizeWeek();
            NormalizeSpecial();
            NormalizeFestival();
            NormalizeSupplies();
            NormalizeLedger();
            if(!Walkable(new Vector3(data.x,0,data.z))){data.onTrail=false;data.x=-6;data.z=-7;}
            player.position=new Vector3(data.x,GroundHeight(data.x,data.z),data.z);SyncTrip();
            foreach(var s in spots)if(s.kind<3)s.visual.SetActive(!data.harvested.Contains(s.id));
            foreach(var g in visitors)if(g)Destroy(g);visitors.Clear();
            started=true;ShowVisitors();UpdateSign();UpdateFriendDecor();UpdateGarden();if(atmosphere)atmosphere.Evening=data.night;Notify("歡迎回來，茶屋主人。",4);
        }
        catch(Exception e){Debug.LogWarning("Load failed: "+e.Message);Say("存檔讀取失敗","存檔無法讀取。可以開始新旅程；舊存檔的備份仍保留在存檔資料夾。");}
    }
    void NewGame()
    {
        ResetConvenience();
        ResetTransient();
        data=new SaveData();started=true;paused=false;modal=false;result=false;notebook=false;
        StartLedger(false);ledgerOffset=0;
        foreach(var s in spots)if(s.kind<3)s.visual.SetActive(true);foreach(var g in visitors)Destroy(g);visitors.Clear();
        player.position=new Vector3(-6,0,-7);Save(false);
        ShowVisitors();UpdateSign();UpdateFriendDecor();UpdateGarden();
        SyncTrip();
        Say("山風茶屋 · 第一天","你接手了妖怪之山腳的一間舊茶屋。\n先按 WASD 探索，靠近茶葉、香菇和竹筍按 E 採集。\n過橋找荷取接委託；在溪邊釣魚；回料理台準備晚餐。\n第一個目標：賺到 180 文，修好茶屋招牌。\n按 Tab 隨時查看地圖與食譜，Esc 暫停。對話與手帳會暫停時間。");
    }
    void OnApplicationQuit(){Save(false);SaveAudioPreferences();}
    IEnumerator QuitAfterEffects()
    {
        if(closing)yield break;closing=true;Save(false);
        foreach(var ps in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)){ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);Destroy(ps.gameObject);}
        yield return new WaitForSeconds(.35f);Application.Quit(0);
    }
    void OnApplicationPause(bool pause){if(pause)Save(false);}
    void ResetTransient()
    {
        ResetAmbientTalking();
        RestoreTeaRoofView();
        ResetCookingLife();
        if(visitorRoot)foreach(Transform guest in visitorRoot)Destroy(guest.gameObject);visitors.Clear();ValleyAtmosphere.ClearMeals();
        trial=false;fishing=false;paused=false;notebook=false;modal=false;result=false;nearest=null;duskNotified=false;brewing=false;storyGuest=-1;relationships=false;eventFriend=-1;AvatarMotion.Frozen=false;if(playerMotion)playerMotion.Carrying=false;
        foreach(var b in bullets)Destroy(b.visual);bullets.Clear();
        if(visual)visual.gameObject.SetActive(true);
        planning=false;guestTarget=0;
        travelBook=false;
        photoMode=false;
        rainDialogue=false;
        festivalDialogue=false;
        settingsOpen=false;
    }
    void ReturnMenu()
    {
        Save(false);ResetTransient();started=false;
    }
    void CreatePortraits()
    {
        string[] names={"河城荷取","射命丸文","犬走椛"};string[] models={"Nitori","Aya","Momiji"};
        for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(100+i*6,0,100);var model=TeaHouseWorld.Character(models[i],p);model.transform.rotation=Quaternion.Euler(0,180,0);
            foreach(var tr in model.GetComponentsInChildren<Transform>())tr.gameObject.layer=30;
            var cameraObject=new GameObject("Portrait camera "+models[i]);var pc=cameraObject.AddComponent<Camera>();pc.enabled=false;pc.cullingMask=1<<30;pc.clearFlags=CameraClearFlags.SolidColor;pc.backgroundColor=new Color(.83f,.85f,.71f);pc.orthographic=true;pc.orthographicSize=.95f;
            pc.transform.position=p+new Vector3(0,1.48f,-5);pc.transform.LookAt(p+Vector3.up*1.48f);
            var texture=new RenderTexture(256,256,16);texture.Create();pc.targetTexture=texture;pc.Render();portraits[names[i]]=texture;
            portraitLife[names[i]]=new PortraitLife{camera=pc,face=model.GetComponent<AvatarExpression>()};
        }
    }
    void Portrait(string who,Rect rect)
    {
        if(portraits.TryGetValue(who,out var texture)){RoundFill(new Rect(rect.x-6,rect.y-6,rect.width+12,rect.height+12),gold);GUI.DrawTexture(rect,texture,ScaleMode.ScaleToFit);}
    }

    void InitStyles()
    {
        if(stylesReady)return;stylesReady=true;
        title=Style(48,FontStyle.Bold,ink);heading=Style(26,FontStyle.Bold,ink);body=Style(21,FontStyle.Normal,ink);small=Style(17,FontStyle.Normal,ink);label=Style(18,FontStyle.Bold,cream);
        label.alignment=TextAnchor.MiddleCenter;
        button=Style(19,FontStyle.Bold,cream);button.alignment=TextAnchor.MiddleCenter;
        button.hover.textColor=new Color(1,.88f,.53f);
        paper=new Texture2D(128,128);var colors=new Color[128*128];
        for(int y=0;y<128;y++)for(int x=0;x<128;x++){float n=Mathf.PerlinNoise(x*.47f,y*.47f)*.025f;colors[y*128+x]=new Color(cream.r-n,cream.g-n,cream.b-n,1);}
        paper.SetPixels(colors);paper.Apply();
        InitTeaTheme();
    }
    GUIStyle Style(int size,FontStyle weight,Color color){return new GUIStyle{font=font,fontSize=size,fontStyle=weight,normal={textColor=color},wordWrap=true};}
    void Box(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
    void Panel(Rect r)
    {
        TeaPanel(r);
    }
    void Text(float x,float y,float w,float h,string s,GUIStyle style=null){GUI.Label(new Rect(x,y,w,h),s,style??body);}
    bool Button(float x,float y,float w,string text,bool enabled=true,Color? color=null)
    {
        return TeaButton(new Rect(x,y,w,43),text,enabled,color??sage);
    }
    void OnGUI()
    {
        if(photoMode&&started&&!modal&&!paused&&!notebook&&!travelBook&&!planning&&!relationships&&!brewing&&!result)return;
        InitStyles();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);float ox=(Screen.width-1440*scale)/2,oy=(Screen.height-900*scale)/2;
        GUI.matrix=Matrix4x4.TRS(new Vector3(ox,oy,0),Quaternion.identity,new Vector3(scale,scale,1));
        if(!started)
        {
            GUI.enabled=!modal&&!settingsOpen;
            Panel(new Rect(70,80,565,740));Text(110,143,470,40,"妖怪之山  /  休閒冒險 DEMO 0.26",small);TeaSeal(new Rect(510,213,64,64));
            Text(105,205,495,90,"山風茶屋",title);Text(110,300,470,55,"妖怪之山的日常，從一杯茶開始。",heading);
            Text(110,383,455,100,"走進河童溪谷，採集、釣魚、準備晚餐。\n在傍晚的茶香裡，聽天狗說說山中的故事。",body);
            if(Button(110,515,455,"開始新旅程"))
            {
                if(File.Exists(savePath)){Say("開始新旅程","開始後會覆寫目前的進度。\n上一份存檔會保留為 .bak 備份。");}
                else NewGame();
            }
            if(Button(110,570,455,"繼續旅程",File.Exists(savePath)))Load();
            if(Button(110,625,220,"關於這個 Demo"))credits=!credits;
            if(Button(345,625,220,"離開遊戲"))StartCoroutine(QuitAfterEffects());
            if(Button(110,682,455,"聲音設定 · 調整山中的聲音"))settingsOpen=true;
            Text(110,745,460,45,"WASD 移動 · E 互動 · Tab 手帳 · Esc 暫停",small);
            if(credits){Panel(new Rect(700,220,660,390));Text(730,250,600,270,"東方 Project 非官方二次創作 Demo\n原作：上海アリス幻樂団 / ZUN\n\n人物：原創旅人、河城荷取、射命丸文、犬走椛\n本作角色模型由 Blender 程式建立，音樂為程式合成的原創旋律。\n目前包含一個溪谷、兩輪營業客人、委託、釣魚和符卡練習。\n本 Demo 的故事與對話是二次創作。",body);}
            GUI.enabled=true;
        }
        else
        {
            GUI.enabled=!modal&&!paused&&!notebook&&!result&&!brewing&&!relationships&&!planning&&!travelBook;DrawHUD();GUI.enabled=true;
            if(!modal&&!paused&&!notebook&&!result&&!trial&&!brewing&&!relationships&&!planning&&!travelBook){DrawWorldLabels(scale,ox,oy);DrawTableOrders();DrawInteractionFeedback();}
            if(data.night&&!paused&&!notebook&&!result&&!brewing&&!relationships&&!planning&&!travelBook)DrawCompactGuests();
            if(trial){Panel(new Rect(470,120,500,85));Text(490,134,460,35,"椛的符卡練習   "+Mathf.CeilToInt(trialTimer)+" 秒",heading);Text(490,171,460,26,"剩餘機會："+hearts+"    WASD 閃避紅色彈幕",small);}
            if(fishing)DrawFishing();
            if(notebook)DrawNotebook();
            if(relationships)DrawRelationships();
            if(planning)DrawPlanning();
            if(travelBook)DrawTravelBook();
            if(paused){GUI.enabled=!settingsOpen;DrawPause();GUI.enabled=true;}
            if(result)DrawResult();
            DrawCafeOverlay();
        }
        if(settingsOpen)DrawAudioSettings();
        if(modal)DrawDialog();
        if(toastTimer>0){RoundFill(new Rect(370,790,700,55),new Color(.12f,.22f,.19f,.95f));Text(390,801,660,38,toast,label);}
    }
    void DrawHUD()
    {
        Panel(new Rect(25,25,450,93));TeaSeal(new Rect(44,43,53,53));Text(111,37,344,35,"山風茶屋 · "+(IsRainDay?"山雨時分":"晴日山風"),heading);
        DrawRainHUD();
        DrawFestivalHUD();
        int minutes=9*60+Mathf.FloorToInt(data.clock/360*9*60);
        Text(111,77,340,28,"第 "+data.day+" 天 · "+(data.night?"18:00 營業中":(minutes/60).ToString("00")+":"+(minutes%60).ToString("00"))+" · "+data.money+" 文",small);
        Panel(new Rect(980,25,435,110));Text(1000,37,395,30,"材料  茶葉 "+data.leaves+" · 菇 "+data.mushrooms+" · 筍 "+data.bamboo+" · 魚 "+data.fish,small);
        Text(1000,75,395,55,"料理  清茶 "+data.tea+" · 菇飯 "+data.meal+" · 鹽燒 "+data.grilled+"\n香菇湯 "+data.soup+" · 竹葉茶 "+data.leafTea,small);
        if(Button(980,150,205,"手帳 [Tab]"))notebook=true;
        if(Button(1200,150,215,"茶屋計畫"))OpenPlanning();
        if(Button(25,290,285,"旅行手帳 · 山中委託")){travelBook=true;notebook=false;}
        if(Button(25,625,285,"七日手帖 · "+WeekStamps()+" / 7 茶印"))OpenPlanning(3);
        if(Button(25,681,285,"推薦備餐 · "+ShortDishes[DailyDish()]+" ＋6")){FocusRecipe(DailyDish());OpenPlanning(0);}
        if(data.onTrail)Text(35,345,300,95,"瀑布山路\n"+LunchStatus()+"\n山栗 "+data.chestnuts+" · 野莓 "+data.berries,small);
        if(data.notebookStage>0&&data.notebookStage<6){Panel(new Rect(25,455,330,135));Text(42,468,298,30,"巡山筆記 · 下一步",heading);Text(42,511,298,72,NotebookHint(),small);}
        DrawContextGuide();
        TeaTag(new Rect(25,840,540,36),"WASD 移動 · Shift 跑步 · E 互動 · 滾輪縮放",new Color(.12f,.22f,.19f,.92f));
        if(saveBadgeTimer>0)TeaTag(new Rect(1220,842,190,34),"進度已保存",sage);
    }
    void DrawWorldLabels(float scale,float ox,float oy)
    {
        var labelRects=new List<Rect>();var sorted=new List<Spot>(spots);sorted.Sort((a,b)=>Vector3.SqrMagnitude(player.position-a.pos).CompareTo(Vector3.SqrMagnitude(player.position-b.pos)));
        foreach(var s in sorted)
        {
            if(!VisibleLivingSpot(s)||(s.kind==23||s.kind==24)&&data.harvested.Contains(s.id))continue;
            if(data.night&&s.kind>=11&&s.kind<=13)continue;
            if(s.kind<3&&(data.harvested.Contains(s.id)||data.night))continue;
            if(Vector3.Distance(player.position,s.pos)>10&&s.kind<10)continue;
            Vector3 sp=cam.WorldToScreenPoint(s.pos+Vector3.up*(s.kind>=11&&s.kind<=13||s.kind==22||s.kind==26||s.kind==27?2.4f:s==nearest?1.8f:.85f));
            if(sp.z<0)continue;
            float x=(sp.x-ox)/scale,y=(Screen.height-sp.y-oy)/scale;
            if(x<80||x>1350||y<110||y>780)continue;
            if(x<495&&y<330)continue;
            if(x<375&&y>435&&y<610&&data.notebookStage>0&&data.notebookStage<6)continue;
            if(x>940&&y<(IsRainDay&&!data.night?470:245))continue;
            if(data.night&&x>860&&y>320)continue;
            bool actor=s.kind>=11&&s.kind<=13||s.kind==22||s.kind==26||s.kind==27;
            if(s!=nearest&&(!actor||Vector3.Distance(player.position,s.pos)>7))continue;
            Rect tag=new Rect(x-(s==nearest?105:82),y-15,s==nearest?210:164,34);bool overlap=false;
            foreach(var used in labelRects)if(new Rect(used.x-8,used.y-8,used.width+16,used.height+16).Overlaps(tag)){overlap=true;break;}if(overlap)continue;
            var cue=!data.holding&&s==nearest?CurrentInteractionCue():null;
            labelRects.Add(tag);TeaTag(tag,cue!=null?CueCategories[cue.category]+" · "+s.name:s.name,cue!=null?Color.Lerp(CueColors[cue.category],ink,.6f):new Color(.15f,.23f,.20f,.85f));
        }
    }
    void DrawGuests()
    {
        if(data.guests.Count==0)return;
        Panel(new Rect(960,350,455,398));Text(980,365,410,35,"今晚的客人 · 第 "+(data.wave+1)+" 輪",heading);
        bool near=!modal&&Vector3.Distance(player.position,new Vector3(-12,0,-2.2f))<4;
        for(int i=0;i<data.guests.Count;i++)
        {
            var g=data.guests[i];float y=417+i*103;
            Portrait(g.name,new Rect(980,y-2,48,48));
            Text(1042,y,345,27,g.name+" · "+(g.done?"已離席":g.state==0?"正在入座":g.state==1?"點了 "+DishNames[g.order]:g.state==2?"享用中":g.state==3?"想聊山中消息":"結帳離開"),small);
            if(g.done)continue;
            Box(new Rect(1042,y+29,348,4),new Color(.80f,.81f,.71f));Box(new Rect(1042,y+29,348*Mathf.Clamp01(g.patience/100),4),gold);
            if(g.state==1)for(int d=0;d<3;d++)if(Button(980+d*138,y+42,130,new[]{"端清茶","端菇飯","端鹽燒"}[d]+" ×"+Stock(d),near&&Stock(d)>0&&!data.holding))PickTray(i,d);
        }
    }
    void DrawDialog()
    {
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.28f));Panel(new Rect(335,320,770,390));
        Text(367,346,706,40,speaker,heading);Box(new Rect(367,395,706,2),gold);
        bool hasPortrait=portraits.ContainsKey(speaker);
        if(hasPortrait)Portrait(speaker,new Rect(368,418,124,142));
        if(hasPortrait)TeaTag(new Rect(925,349,150,30),ambientDialogue?FamiliarTopic():storyGuest>=0?FamiliarTopic(true):dialogueMood==1?"安心微笑":dialogueMood==2?"好奇傾聽":dialogueMood==3?"認真巡查":"山中日常",sage);
        Text(hasPortrait?515:367,415,hasPortrait?560:706,170,dialogue,body);
        if(ambientDialogue){DrawAmbientDialog();return;}
        if(rainDialogue){if(Button(730,633,345,"收好茶杯 [Esc]")){modal=false;speaker="";rainDialogue=false;}return;}
        if(festivalDialogue){if(Button(730,633,345,"記住今天的聚會 [Esc]")){modal=false;speaker="";festivalDialogue=false;}return;}
        if(storyGuest>=0){if(Button(365,633,350,"謝謝消息，結帳送客"))FinishStory();}
        else if(eventFriend>=0){int current=eventFriend;if(Button(365,633,350,Friendship(current).stage==1?"交付食材，繼續故事":"繼續故事",EventReady(current)&&EventMaterials(current)))CompleteFriendEvent(current);}
        else if(speaker=="茶屋料理台")
        {
            for(int d=0;d<3;d++) if(Button(365+d*240,535,226,"製作 "+DishNames[d]))BeginBrew(d);
            if(Button(365,583,710,"菜單、材料雜貨與庭院裝修"))OpenPlanning();
            if(Button(365,633,350,data.night?"正在營業":"開店，迎接傍晚",!data.night))OpenShop();
        }
        else if(speaker=="河城荷取"&&!data.questDone)
        {
            if(!data.questAccepted){if(Button(365,633,350,"接下委託")){data.questAccepted=true;modal=false;Notify("委託：交給荷取 4 份嫩竹筍");Save(false);}}
            else if(Button(365,633,350,"交付竹筍 ×4",data.bamboo>=4)){CompleteQuest();modal=false;}
        }
        else if(speaker=="犬走椛"){if(Button(365,633,350,"開始符卡練習",!data.night&&!data.onTrail)){StartTrial();}}
        else if(speaker=="茶屋招牌"&&!data.restored){if(Button(365,633,350,"修繕招牌 · 180 文",data.money>=180))Restore();}
        else if(speaker=="回房休息"||speaker=="今晚的營業紀錄"){if(Button(365,633,350,"休息，進入下一天"))NextDay();}
        else if(speaker=="開始新旅程"){if(Button(365,633,350,"確認開始新旅程"))NewGame();}
        int friend=FriendIndex(speaker);
        if(CanAmbientChat()&&Button(365,580,140,"聊聊近況")){OpenAmbientChat();return;}
        if((speaker=="茶屋招牌"||speaker=="今晚的營業紀錄")&&Button(365,580,710,speaker=="茶屋招牌"?"購買庭院布置":"查看詳細營業結算"))OpenPlanning(speaker=="茶屋招牌"?1:2);
        if(friend>=0&&eventFriend<0&&storyGuest<0&&!data.night&&Button(515,580,560,EventReady(friend)?"角色事件 · "+ChapterNames[friend]:EventRequirement(friend),EventReady(friend)))OpenFriendEvent(friend);
        if(Button(730,633,345,speaker=="山風茶屋 · 第一天"?"帶上工具出發":"關閉 [Esc]")){if(storyGuest>=0)FinishStory();else {modal=false;speaker="";eventFriend=-1;ResetAmbientTalking();}}
    }
    void DrawFishing()
    {
        Panel(new Rect(420,580,600,155));Text(450,598,540,37,"溪邊垂釣",heading);
        Rect track=new Rect(450,651,540,22);Box(track,new Color(.74f,.78f,.69f));Box(new Rect(track.x+track.width*.60f,track.y,track.width*.28f,track.height),gold);
        float marker=(Mathf.Sin(fishTimer*2.3f)+1)*.5f;Box(new Rect(track.x+marker*track.width-4,track.y-5,8,32),ink);
        Text(450,692,540,30,"金色區域按 Space 或 E 收竿 · Esc 取消",small);
    }
    void DrawNotebook()
    {
        if(data.onTrail){DrawTravelBook();return;}
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.35f));Panel(new Rect(175,130,1090,630));
        Text(205,156,850,44,"旅人的手帳",heading);if(Button(1070,150,160,"收起 [Tab]"))notebook=false;
        Text(210,224,470,40,"河童溪谷地圖",heading);
        Rect map=new Rect(210,275,490,365);Box(map,new Color(.78f,.83f,.69f));
        Box(new Rect(map.x+326,map.y,38,map.height),new Color(.36f,.64f,.66f));
        Box(new Rect(map.x,map.y+230,map.width,16),new Color(.70f,.59f,.40f));
        Box(new Rect(map.x+320,map.y+66,48,16),woodColor());
        MapPin(map,spots[0].pos,"茶屋");MapPin(map,spots[1].pos,"荷取");MapPin(map,spots[2].pos,"文");MapPin(map,spots[3].pos,"椛");MapPin(map,spots[4].pos,"釣點");MapPin(map,player.position,"你",true);
        Text(210,663,490,48,"北邊與中央都有橋。採集物每天更新。\n清茶、菇飯都能替代客人點的料理。",small);
        Text(760,224,430,40,"料理與委託",heading);
        if(Button(210,715,490,"山中友人 · 好感與故事 · 消息手帳")){notebook=false;relationships=true;}
        if(Button(760,715,435,"茶屋計畫 · 返回上次頁籤"))OpenPlanning();
        Text(760,279,435,315,"山風清茶　32 文\n茶葉 ×2\n\n竹筍菇飯　42 文\n香菇 ×1 ＋ 竹筍 ×1\n\n溪魚鹽燒　55 文\n溪魚 ×1\n\n喜歡的餐點額外 ＋15 文小費。",body);
        Text(760,598,435,96,"荷取委託："+(data.questDone?"完成 ✓":data.questAccepted?"竹筍 "+data.bamboo+" / 4":"尚未接取")+"\n椛的練習："+(data.trialDone?"通過 ✓":"白天可挑戰")+"\n招牌修繕："+(data.restored?"完成 ✓":data.money+" / 180 文"),small);
    }
    Color woodColor(){return new Color(.43f,.29f,.18f);}
    void MapPin(Rect map,Vector3 p,string n,bool isPlayer=false)
    {
        float x=map.x+(p.x+25)/50*map.width,y=map.y+(20-p.z)/40*map.height;
        Box(new Rect(x-5,y-5,10,10),isPlayer?new Color(.75f,.27f,.18f):ink);Text(x+8,y-13,100,30,n,small);
    }
    void DrawPause()
    {
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.4f));Panel(new Rect(475,195,490,525));TeaSeal(new Rect(830,218,60,60));Text(510,250,310,45,"稍作歇息",heading);
        if(Button(515,322,410,"繼續旅程 [Esc]"))paused=false;
        if(Button(515,380,410,"儲存進度 [F5]"))Save();
        if(Button(515,438,410,"聲音設定"))settingsOpen=true;
        if(Button(515,496,410,"儲存並回主選單"))ReturnMenu();
        if(Button(515,554,410,"儲存並離開"))StartCoroutine(QuitAfterEffects());
        Text(515,631,410,55,"每 20 秒、採集與交易後會自動存檔。\n右下紙籤會提示保存完成。",small);
    }
    void DrawResult()
    {
        Box(new Rect(0,0,1440,900),new Color(.07f,.17f,.13f,.5f));Panel(new Rect(360,230,720,430));
        Text(405,270,630,70,"茶屋，重新開張了。",title);
        Text(405,365,630,140,"新招牌在山風中輕輕晃動。\n荷取帶來水車的消息，文記下今晚的茶香，椛終於能安心歇腳。\n\n你完成了《山風茶屋》Demo 的主要目標。",body);
        if(Button(405,570,300,"繼續山中的生活"))result=false;
        if(Button(730,570,300,"儲存並回主選單"))ReturnMenu();
    }
    AudioClip Tone(float frequency,float duration)
    {
        int n=(int)(44100*duration);float[] samples=new float[n];for(int i=0;i<n;i++)samples[i]=Mathf.Sin(2*Mathf.PI*frequency*i/44100)*Mathf.Exp(-i/(44100f*.06f))*.3f;
        var clip=AudioClip.Create("Tea chime",n,1,44100,false);clip.SetData(samples,0);return clip;
    }
    AudioClip Music()
    {
        int rate=22050,n=rate*24;float[] samples=new float[n];int[] notes={60,64,67,69,67,64,62,55,60,64,67,72,69,67,64,62,57,60,64,67,64,60,59,55,60,62,64,67,64,62,60,55};
        for(int i=0;i<n;i++)
        {
            float time=i/(float)rate;int step=(int)(time/.75f);float t=time% .75f;
            float f=440*Mathf.Pow(2,(notes[step%notes.Length]-69)/12f);
            float tone=(Mathf.Sin(2*Mathf.PI*f*t)+.25f*Mathf.Sin(2*Mathf.PI*f*2*t))*Mathf.Exp(-t*5)*.14f;
            float bass=65.406f*Mathf.Pow(2,(step/8%2==0?0:5)/12f);tone+=Mathf.Sin(2*Mathf.PI*bass*time)*.025f;
            samples[i]=tone*Mathf.Min(1,time)*Mathf.Min(1,24-time);
        }
        var clip=AudioClip.Create("Original mountain afternoon",n,1,rate,false);clip.SetData(samples,0);return clip;
    }
    IEnumerator QARun()
    {
        Directory.CreateDirectory(qaDir);yield return new WaitForSeconds(2);
        int firstFrame=Time.frameCount;float firstTime=Time.realtimeSinceStartup;
        try
        {
            string legacy=Path.Combine(qaDir,"legacy-player-save.json");
            if(File.Exists(legacy))
            {
                var expected=JsonUtility.FromJson<SaveData>(File.ReadAllText(legacy));Load(legacy);
                Assert(data.day==expected.day&&data.money==expected.money&&data.restored==expected.restored&&data.tea==expected.tea&&data.meal==expected.meal&&data.grilled==expected.grilled,"existing v1 player save compatibility");
                Assert(Friendship(0).affection==0&&Friendship(1).stage==0&&Friendship(2).stage==0,"legacy save starts new stories without altering progress");
            }
            string v3=Path.Combine(qaDir,"v3-save.json");
            if(File.Exists(v3)){var expected=JsonUtility.FromJson<SaveData>(File.ReadAllText(v3));Load(v3);Assert(data.money==expected.money&&data.day==expected.day&&data.tea==expected.tea&&data.serviceRevision==3&&Friendship(0).stage==0,"v3 save compatibility");}
            string v4=Path.Combine(qaDir,"v4-save.json");
            if(File.Exists(v4)){var expected=JsonUtility.FromJson<SaveData>(File.ReadAllText(v4));Load(v4);Assert(data.money==expected.money&&data.day==expected.day&&data.tea==expected.tea,"v4 stock and money compatibility");for(int f=0;f<3;f++)Assert(Friendship(f).stage==expected.bonds[f].stage&&Friendship(f).affection==expected.bonds[f].affection,"v4 friendship and chapters retained");}
            string v5=Path.Combine(qaDir,"v5-save.json");
            if(File.Exists(v5)){var expected=JsonUtility.FromJson<SaveData>(File.ReadAllText(v5));Load(v5);Assert(data.money==expected.money&&data.day==expected.day&&data.soup==expected.soup&&data.menu.Length==7,"v5 stock retained and menu expanded");for(int d=0;d<5;d++)Assert(data.menu[d]==expected.menu[d]&&data.prepTargets[d]==expected.prepTargets[d]&&data.sales[d]==expected.sales[d],"v5 menu targets and report preserved");for(int s=0;s<4;s++)Assert(data.gardenOwned[s]==expected.gardenOwned[s]&&data.gardenStyle[s]==expected.gardenStyle[s],"v5 furnishing ownership preserved");}
            NewGame();modal=false;
            Assert(MountainArt.Height(-5,17)>1&&MountainArt.Height(8,0)==0,"continuous shrine elevation and river height");
            Assert(!Walkable(new Vector3(8,0,0)),"river collision");Assert(Walkable(new Vector3(8,0,-4)),"bridge crossing");
            data.questAccepted=true;
            foreach(var s in spots)if(s.kind<3)Interact(s);
            Assert(data.leaves==24&&data.mushrooms==16&&data.bamboo==16,"forage quantities");
            Interact(spots[6]);modal=false; // rest interaction does not mutate until confirmed
            Assert(CompleteQuest()&&data.money==120&&data.bamboo==12,"quest transaction");
            Assert(!CompleteQuest(),"no duplicate quest reward");
            Assert(Craft(0)&&Craft(1),"recipes");Assert(!Craft(2),"missing fish cannot craft");
            fishing=true;fishTimer=Mathf.Asin(.48f)/2.3f;CatchFish();Assert(data.fish==1,"fishing timing");Assert(Craft(2),"fish recipe");
            for(int i=0;i<3;i++)Craft(0);Craft(1);
            Assert(OpenShop(),"shop opens with prepared food");Assert(!OpenShop(),"no duplicate opening");
            TickCafe(0,true);
            int stockBefore=data.tea;data.qualityTea=1;
            Assert(PickTray(0,0)&&data.holding&&data.heldPerfect,"physical premium tray");
            Save(false);Load(Path.Combine(qaDir,"qa-save.json"));
            Assert(data.holding&&data.heldPerfect&&data.tea==stockBefore-1,"tray survives save and load");
            player.position=new Vector3(0,0,0);Assert(!DeliverTray(),"cannot deliver remotely");ReturnTray();
            Assert(data.tea==stockBefore&&data.qualityTea==1,"tray return refunds food and quality");
            int cashBefore=data.money;
            Assert(Serve(0,0)&&Serve(1,1)&&Serve(2,2),"first wave service");
            Assert(data.money==cashBefore,"payment waits until meal and conversation");
            Assert(!Serve(0,0),"no duplicate service");
            TestFinishWave();Assert(data.wave==1,"second wave");TickCafe(0,true);
            Assert(data.money==cashBefore+190&&data.journal.Count==3,"preferred dishes premium tip daily recommendation and journal");
            Assert(Serve(0,0)&&Serve(1,1)&&Serve(2,0),"second wave service");TestFinishWave();modal=false;
            Assert(data.served==6&&data.guests.Count==0,"night settlement");
            Assert(Restore()&&data.restored,"demo completion");Assert(!Restore(),"no duplicate restoration");result=false;
            NextDay();Assert(data.day==2&&!data.night&&data.harvested.Count==0,"day reset");
            Save(false);var roundtrip=JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(qaDir,"qa-save.json")));
            Assert(roundtrip.restored&&roundtrip.money==data.money&&roundtrip.day==2,"save roundtrip");
            StartTrial();hearts=0;EndTrial(false);Assert(!trial&&!data.trialDone,"trial failure is retryable");
            StartTrial();EndTrial(true);Assert(data.trialDone,"trial reward");
            int leavesBefore=data.leaves,teaBefore=data.tea,qualityBefore=data.qualityTea;
            BeginBrew(0);Assert(brewing&&data.leaves==leavesBefore,"preparation does not consume materials early");brewing=false;
            Assert(data.leaves==leavesBefore,"cancel preparation preserves materials");
            BeginBrew(0);brewTimer=1.5f;FinishBrew();
            Assert(data.leaves==leavesBefore-2&&data.tea==teaBefore+1&&data.qualityTea==qualityBefore+1,"perfect brewing recipe and quality");
            data.guests.Add(new Guest("射命丸文",0));data.night=true;data.guests[0].state=1;data.guests[0].patience=.01f;TickCafe(1,true);
            Assert(data.guests[0].done&&data.lost==1,"waiting guest can leave without payment");
            data.guests.Clear();data.night=false;
            TestFriendStories();
            TestTeaPlanning();
            TestMountainTrip();TestLivingMountain();TestRainWeather();TestAudioSettings();TestTeaLife();TestTeaWeek();TestDailySpecial();TestTeaUpgrades();TestContextGuide();TestUpgradeArt();TestLifeActions();TestFestival();TestSupplies();TestLedger();TestCanopyViews();TestInteractionFeedback();TestCookingLife();TestTeaRoofView();TestParticleCulling();TestFamiliarChats();TestPlanningConvenience();TestServiceReadability();
            Debug.Log("QA GAMEPLAY PASS: forage, collisions, quest, fishing, crafting, two service waves, restoration, day reset, save, trial.");
        }
        catch(Exception e){Debug.LogError("QA FAIL: "+e);File.WriteAllText(Path.Combine(qaDir,"FAILED.txt"),e.ToString());Application.Quit(1);yield break;}
        data=new SaveData();data.leaves=8;data.mushrooms=4;data.bamboo=6;data.tea=4;data.meal=2;data.grilled=1;
        foreach(var s in spots)if(s.kind<3)s.visual.SetActive(true);
        player.position=new Vector3(-6,0,-6);cam.transform.position=player.position+CameraOffset;toastTimer=0;UpdateSign();UpdateFriendDecor();UpdateGarden();
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"01-valley.png"));
        yield return new WaitForSeconds(1);notebook=true;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"02-notebook.png"));
        yield return new WaitForSeconds(.5f);notebook=false;player.position=new Vector3(-12,0,-3);OpenShop();toastTimer=0;
        yield return new WaitForSeconds(4);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"03-evening.png"));
        yield return new WaitForSeconds(1);BeginBrew(0);brewTimer=1.5f;
        ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"08-brewing.png"));yield return new WaitForSeconds(.5f);FinishBrew();
        PickTray(0,0);player.position=new Vector3(-16,0,-7.1f);cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"09-tray.png"));
        yield return new WaitForSeconds(.5f);DeliverTray();
        yield return new WaitForSeconds(3);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"10-dining.png"));
        yield return new WaitForSeconds(4);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"11-news.png"));
        yield return new WaitForSeconds(.5f);FinishStory();
        yield return new WaitForSeconds(.5f);NextDay();Friendship(0).affection=4;Friendship(1).affection=4;Friendship(2).affection=4;
        player.position=spots[2].pos+new Vector3(0,0,-1);relationships=true;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"12-friendships.png"));
        yield return new WaitForSeconds(.5f);OpenFriendEvent(0);
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"13-aya-event.png"));
        yield return new WaitForSeconds(.5f);CompleteFriendEvent(0);
        data.money=600;player.position=new Vector3(-12,0,-4);cam.transform.position=player.position+CameraOffset;
        BuyGarden(0,1,true);BuyGarden(1,2,true);BuyGarden(2,2,true);BuyGarden(3,1,true);OpenPlanning(1);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"14-garden-shop.png"));
        yield return new WaitForSeconds(.5f);planning=false;
        data.meal=1;AcceptLunch();player.position=new Vector3(-12,0,-3);PackLunch();ChangeRegion(true);toastTimer=0;
        yield return new WaitForSeconds(.5f);travelBook=true;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"18-travel-book.png"));
        yield return new WaitForSeconds(.5f);travelBook=false;player.position=TP(104,15);cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"19-mountain-trail.png"));
        yield return new WaitForSeconds(.5f);photoMode=true;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"21-detailed-trail.png"));
        yield return new WaitForSeconds(.5f);player.position=TP(88,19);cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"22-waterfall-pavilion.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;
        yield return new WaitForSeconds(.5f);player.position=spots.Find(s=>s.kind==22).pos;DeliverLunch();
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"20-lunch-delivery.png"));
        yield return new WaitForSeconds(.5f);modal=false;AcceptNotebook();travelTab=1;travelBook=true;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"23-notebook-quest.png"));
        yield return new WaitForSeconds(.5f);travelBook=false;player.position=TP(90,-3);cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"24-quest-clue.png"));
        yield return new WaitForSeconds(.5f);travelBook=false;ChangeRegion(false);player.position=new Vector3(-12,0,-4);cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"15-custom-garden.png"));
        yield return new WaitForSeconds(.5f);OpenPlanning(0);
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"16-menu.png"));
        yield return new WaitForSeconds(.5f);planningTab=2;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"17-report.png"));
        yield return new WaitForSeconds(.5f);planning=false;
        yield return new WaitForSeconds(1);NextDay();player.position=new Vector3(16,0,10);StartTrial();toastTimer=0;
        yield return new WaitForSeconds(3);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"04-spell-practice.png"));
        yield return new WaitForSeconds(.5f);if(trial)EndTrial(false);
        player.position=new Vector3(3,0,17);cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"06-waterfall.png"));
        yield return new WaitForSeconds(.5f);Say("河城荷取","水車轉起來啦！\n山風茶屋今天也好好營業吧。");
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"07-character-dialogue.png"));
        yield return new WaitForSeconds(.5f);modal=false;result=false;data.day=3;data.night=false;data.clock=0;data.tea=3;data.rainStoryStage=0;player.position=new Vector3(-12,0,-6);cam.transform.position=player.position+CameraOffset;SyncWeatherPeople();toastTimer=0;
        yield return new WaitForSeconds(2);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"25-rain-teahouse.png"));
        yield return new WaitForSeconds(.5f);player.position=new Vector3(-12,0,-3);ServeRainTea();
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"26-rain-story.png"));
        yield return new WaitForSeconds(.5f);modal=false;photoMode=true;player.position=new Vector3(-14.5f,0,-4.6f);cam.transform.position=player.position+CameraOffset;cam.orthographicSize=9;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"29-tea-house-life.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;cam.orthographicSize=11.5f;Say("射命丸文","雨聲剛好蓋過山路的喧鬧。\n今天就留在茶屋，慢慢喝完這杯茶吧。");SetDialogueMood(1);
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"30-aya-expression.png"));
        yield return new WaitForSeconds(.5f);modal=false;data.weekStartDay=1;data.day=7;data.weekHarvest=3;data.weekCraft=5;data.weekServed=9;data.questDone=true;data.deliveries=1;data.lookoutVisited=true;data.notebookStage=6;data.restored=true;data.rainStoryStage=3;Friendship(0).stage=1;NormalizePlanning();data.gardenOwned[0]=2;ClaimWeek(0);ClaimWeek(1);OpenPlanning(3);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"31-seven-day-journal.png"));
        yield return new WaitForSeconds(.5f);planning=false;
        yield return new WaitForSeconds(.5f);data.day=3;data.specialDay=0;NormalizeSpecial();OpenPlanning(0);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"32-daily-recommendation.png"));
        yield return new WaitForSeconds(.5f);planning=false;data.day=1;data.specialDay=0;data.onTrail=false;data.tea=4;data.qualityTea=1;data.meal=3;data.grilled=2;SetMenu(0,true);SetMenu(1,true);SetMenu(2,true);OpenShop();guestTarget=0;toastTimer=0;
        yield return new WaitForSeconds(4);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"33-guest-taste-hints.png"));
        yield return new WaitForSeconds(.5f);Serve(0,0);PayGuest(data.guests[0]);OpenPlanning(2);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"34-special-report.png"));
        yield return new WaitForSeconds(.5f);planning=false;data.guests.Clear();data.night=false;
        yield return new WaitForSeconds(.5f);modal=false;result=false;player.position=new Vector3(-12,0,-3);data.money=600;OpenPlanning(4);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"35-tea-house-upgrades.png"));
        yield return new WaitForSeconds(.5f);BuyUpgrade(0);BuyUpgrade(1);data.prepTargets[0]=8;data.leaves=14;OpenPlanning(0);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"36-batch-preparation.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);OpenPlanning(4);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"37-upgrades-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);planning=false;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;data.guideHidden=false;toastTimer=0;cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"38-context-foraging.png"));
        yield return new WaitForSeconds(.5f);data.tea=4;data.meal=3;data.grilled=2;player.position=new Vector3(-12,0,-3);OpenShop();
        yield return new WaitForSeconds(4);PickTray(0,0);player.position=new Vector3(-12,0,-6);cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"39-context-delivery.png"));
        yield return new WaitForSeconds(.5f);ReturnTray();NextDay();modal=false;data.meal=1;player.position=new Vector3(-12,0,-3);AcceptLunch();PackLunch();ChangeRegion(true);toastTimer=0;cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"40-context-small-window.png"));
        yield return new WaitForSeconds(.5f);data.guideHidden=true;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"41-guide-collapsed.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);data.guideHidden=false;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;result=false;player.position=new Vector3(-14,0,-5.5f);cam.orthographicSize=9;cam.transform.position=player.position+CameraOffset;photoMode=true;toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"42-teahouse-before-upgrades.png"));
        yield return new WaitForSeconds(.5f);data.money=600;BuyUpgrade(0);BuyUpgrade(1);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"43-visible-teahouse-upgrades.png"));
        yield return new WaitForSeconds(.5f);BuyGarden(0,1,true);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"44-bamboo-upgrade-palette.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.money=180;OpenPlanning(6);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"50-material-supplies.png"));
        yield return new WaitForSeconds(.5f);BuySupply(0,3);BuySupply(0,3);data.trailRecipes=true;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"51-supplies-purchased.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"52-supplies-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);planning=false;photoMode=true;player.position=new Vector3(-16,0,-4);cam.transform.position=player.position+CameraOffset;cam.orthographicSize=9;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"53-teahouse-supply-cart.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;data.tea=6;data.meal=3;data.grilled=3;player.position=new Vector3(-12,0,-5);OpenShop();
        yield return new WaitForSeconds(4);Serve(0,0);Serve(1,1);Serve(2,2);player.position=new Vector3(-12,0,-6);cam.transform.position=new Vector3(-13,0,-8)+CameraOffset;cam.orthographicSize=6;photoMode=true;toastTimer=0;
        yield return new WaitForSeconds(1.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"45-tea-and-meal-actions.png"));
        yield return new WaitForSeconds(.5f);paused=true;photoMode=false;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"46-paused-life-actions.png"));
        yield return new WaitForSeconds(.5f);paused=false;photoMode=false;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;data.day=7;data.tea=data.meal=data.grilled=3;player.position=new Vector3(-12,0,-3);cam.transform.position=new Vector3(-12,0,-6)+CameraOffset;SyncFestivalArt();OpenPlanning(5);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"47-festival-invitations.png"));
        yield return new WaitForSeconds(.5f);planning=false;ShareFestival(0);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"48-festival-friend-dialogue.png"));
        yield return new WaitForSeconds(.5f);modal=false;ShareFestival(1);modal=false;ShareFestival(2);modal=false;photoMode=true;toastTimer=0;cam.orthographicSize=9;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"49-festival-teahouse.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.money=500;BuySupply(0,3);BuySupply(1,1);data.questAccepted=true;data.bamboo=4;CompleteQuest();BuyUpgrade(1);data.tea=3;OpenShop();TickCafe(0,true);Serve(0,0);PayGuest(data.guests[0]);OpenPlanning(7);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"54-ledger-today.png"));
        yield return new WaitForSeconds(.5f);NextDay();modal=false;OpenPlanning(7);ledgerOffset=1;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"55-ledger-history.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"56-ledger-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);Load(Path.Combine(qaDir,"legacy-player-save.json"));OpenPlanning(7);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"57-ledger-legacy-partial.png"));
        yield return new WaitForSeconds(.5f);planning=false;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;player.position=new Vector3(-16,0,-4);cam.transform.position=player.position+CameraOffset;cam.orthographicSize=9;photoMode=true;toastTimer=0;qaOpaqueCanopies=true;RestoreCanopyViews();
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"58-canopies-before.png"));
        yield return new WaitForSeconds(.5f);qaOpaqueCanopies=false;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"59-canopies-clear-teahouse.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;Screen.SetResolution(1024,768,FullScreenMode.Windowed);toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"60-clear-view-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ChangeRegion(true);var focusCrown=canopyViews.Find(v=>v.renderer.name=="Faceted mountain canopy");player.position=TP(focusCrown.bounds.center.x,focusCrown.bounds.center.z+2);cam.transform.position=player.position+CameraOffset;photoMode=true;toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"61-clear-view-mountain.png"));
        yield return new WaitForSeconds(.5f);ChangeRegion(false);photoMode=false;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;player.position=spots.Find(s=>s.kind==0).pos+Vector3.back*.7f;cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"62-action-harvest.png"));
        yield return new WaitForSeconds(.5f);player.position=spots[1].pos+Vector3.back;cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"63-action-dialogue.png"));
        yield return new WaitForSeconds(.5f);player.position=spots[0].pos+Vector3.back;cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"64-action-facility.png"));
        yield return new WaitForSeconds(.5f);data.tea=3;OpenShop();modal=false;
        yield return new WaitForSeconds(4);PickTray(0,0);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"65-action-delivery-distant.png"));
        yield return new WaitForSeconds(.5f);player.position=new Vector3(-16,0,-10.2f);cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"66-action-delivery-ready.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"67-action-small-window.png"));
        yield return new WaitForSeconds(.5f);ReturnTray();NewGame();modal=false;Screen.SetResolution(1440,900,FullScreenMode.Windowed);ChangeRegion(true);data.notebookStage=1;SyncNotebook();player.position=spots.Find(s=>s.kind==28).pos+Vector3.back*.7f;cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"68-action-notebook-clue.png"));
        yield return new WaitForSeconds(.5f);ChangeRegion(false);player.position=ValleyGate;cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"69-action-travel.png"));
        yield return new WaitForSeconds(.5f);NewGame();modal=false;data.leaves=20;data.mushrooms=20;data.bamboo=20;player.position=new Vector3(-12,0,-3);cam.transform.position=player.position+CameraOffset;toastTimer=0;BeginBrew(0);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"70-cooking-tea.png"));
        yield return new WaitForSeconds(.5f);CancelBrew();BeginBrew(1);toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"71-cooking-meal.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"72-cooking-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);brewTimer=(PerfectStart+PerfectEnd)*.5f/.42f;FinishBrew();
        yield return new WaitForSeconds(.7f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"73-cooking-perfect.png"));
        yield return new WaitForSeconds(3);BeginBrew(0);CancelBrew();toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"74-cooking-cancelled.png"));
        yield return new WaitForSeconds(.5f);NewGame();modal=false;player.position=new Vector3(-12,0,-3);cam.transform.position=player.position+CameraOffset;cam.orthographicSize=9;photoMode=true;toastTimer=0;qaOpaqueRoof=true;RestoreTeaRoofView();
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"75-kitchen-before.png"));
        yield return new WaitForSeconds(.5f);qaOpaqueRoof=false;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"76-kitchen-clear.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;data.mushrooms=10;data.bamboo=10;BeginBrew(1);toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"77-kitchen-cooking.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"78-kitchen-small-window.png"));
        yield return new WaitForSeconds(.5f);CancelBrew();Screen.SetResolution(1440,900,FullScreenMode.Windowed);data.day=3;photoMode=true;toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"79-kitchen-rain.png"));
        yield return new WaitForSeconds(.5f);data.day=1;player.position=new Vector3(-6.2f,0,-2.2f);cam.transform.position=player.position+CameraOffset;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"80-kitchen-roof-restored.png"));
        yield return new WaitForSeconds(.5f);photoMode=false;cam.orthographicSize=11.5f;
        yield return new WaitForSeconds(.5f);NewGame();modal=false;data.questDone=true;player.position=spots[1].pos+Vector3.back;cam.transform.position=player.position+CameraOffset;Interact(spots[1]);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"81-familiar-choice.png"));
        yield return new WaitForSeconds(.5f);OpenAmbientChat();
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"82-familiar-nitori-sun.png"));
        yield return new WaitForSeconds(.5f);CloseAmbientChat(false);data.day=3;SyncWeatherPeople();player.position=spots[2].pos+Vector3.back;cam.transform.position=player.position+CameraOffset;Interact(spots[2]);OpenAmbientChat();
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"83-familiar-aya-rain.png"));
        yield return new WaitForSeconds(.5f);CloseAmbientChat(false);data.day=2;data.clock=320;SyncWeatherPeople();player.position=spots[3].pos+Vector3.back;cam.transform.position=player.position+CameraOffset;Interact(spots[3]);OpenAmbientChat();
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"84-familiar-momiji-dusk.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"85-familiar-small-window.png"));
        yield return new WaitForSeconds(.5f);CloseAmbientChat(false);Screen.SetResolution(1440,900,FullScreenMode.Windowed);NewGame();modal=false;data.questDone=true;data.day=3;data.tea=5;player.position=new Vector3(-12,0,-3);cam.transform.position=player.position+CameraOffset;OpenShop();modal=false;
        yield return new WaitForSeconds(5);int familiarGuest=data.guests.FindIndex(g=>g.name==Friends[1]);PickTray(familiarGuest,0);DeliverTray(true);toastTimer=0;
        yield return new WaitForSeconds(7);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"86-familiar-rain-dinner.png"));
        yield return new WaitForSeconds(.5f);FinishStory();
        yield return new WaitForSeconds(.5f);NewGame();modal=false;player.position=new Vector3(-12,0,-3);cam.transform.position=player.position+CameraOffset;data.money=180;OpenPlanning(0);FocusRecipe(1);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"87-convenience-menu-focus.png"));
        yield return new WaitForSeconds(.5f);PlanningShortcut(KeyCode.F2);
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"88-convenience-supply-gaps.png"));
        yield return new WaitForSeconds(.5f);BuySupply(1,1);BuySupply(2,1);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"89-convenience-ready.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"90-convenience-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);PlanningShortcut(KeyCode.F3);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"91-convenience-original-brew.png"));
        yield return new WaitForSeconds(.5f);CancelBrew();OpenPlanning();toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"92-convenience-resumed-page.png"));
        yield return new WaitForSeconds(.5f);planning=false;SelectTravelTab(1);travelBook=true;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"93-convenience-travel-tab.png"));
        yield return new WaitForSeconds(.5f);travelBook=false;
        NewGame();modal=false;data.tea=8;data.meal=5;data.grilled=4;player.position=new Vector3(-12,0,-3);cam.transform.position=player.position+CameraOffset;OpenShop();toastTimer=0;
        yield return new WaitForSeconds(5);data.guests[0].patience=80;data.guests[1].patience=45;data.guests[2].patience=20;ChooseServiceGuest(2);toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"94-service-waiting.png"));
        yield return new WaitForSeconds(.5f);PickTray(2,data.guests[2].order);player.position=new Vector3(-13,0,-6);cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"95-service-tray-target.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"96-service-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);player.position=new Vector3(-10,0,-9.2f);cam.transform.position=player.position+CameraOffset;toastTimer=0;
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"97-service-delivery-ready.png"));
        yield return new WaitForSeconds(.5f);DeliverTray();toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"98-service-dining.png"));
        yield return new WaitForSeconds(.5f);ReturnMenu();NewGame();modal=false;
        yield return new WaitForSeconds(.5f);modal=false;paused=true;settingsOpen=true;toastTimer=0;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"27-audio-settings.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1024,768,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"28-settings-small-window.png"));
        yield return new WaitForSeconds(.5f);Screen.SetResolution(1440,900,FullScreenMode.Windowed);
        yield return new WaitForSeconds(1);
        yield return new WaitForSeconds(.5f);CloseAudioSettings();paused=false;data.day=1;started=false;toastTimer=0;player.position=new Vector3(-10,0,-4);cam.transform.position=player.position+CameraOffset;
        yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"05-title.png"));
        yield return new WaitForSeconds(1);
        float fps=(Time.frameCount-firstFrame)/(Time.realtimeSinceStartup-firstTime);Debug.Log("QA render average (includes captures): "+fps.ToString("F1")+" fps");
        File.WriteAllText(Path.Combine(qaDir,"PASSED.txt"),"All gameplay assertions passed. Screenshots captured from the Windows player. Average render rate including captures: "+fps.ToString("F1")+" fps.");yield return QuitAfterEffects();
    }
    void Assert(bool condition,string test){if(!condition)throw new Exception(test);Debug.Log("PASS: "+test);}
}
