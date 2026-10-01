using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    bool travelBook;Transform trailRoot;List<Renderer> trailCanopies=new List<Renderer>();
    static readonly Vector3 TrailStart=new Vector3(82,0,-13),ValleyGate=new Vector3(18,0,16);
    static float TrailHeight(float x,float z){return Mathf.SmoothStep(0,1,Mathf.InverseLerp(-13,22,z))*3;}
    float GroundHeight(float x,float z){return x>60?TrailHeight(x,z):MountainArt.Height(x,z);}
    bool TrailWalkable(Vector3 p)
    {return p.x>=77&&p.x<=119&&p.z>=-16&&p.z<=25&&!(p.x>96&&p.x<100&&Mathf.Abs(p.z-2)>1.5f);}
    GameObject TrailShape(string n,PrimitiveType type,Vector3 p,Vector3 scale,Color c)
    {return TeaHouseWorld.Shape(n,type,p,scale,c,trailRoot);}
    Vector3 TP(float x,float z,float y=0){return new Vector3(x,TrailHeight(x,z)+y,z);}
    void BuildMountainTrip()
    {
        trailRoot=new GameObject("瀑布山路 · 休閒旅行").transform;
        var vertices=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
        for(int x=50;x<145;x+=2)for(int z=-40;z<66;z+=2)
        {
            Vector3[] cell={TP(x,z),TP(x,z+2),TP(x+2,z+2),TP(x+2,z)};
            foreach(int k in new[]{0,1,2,0,2,3}){vertices.Add(cell[k]);triangles.Add(triangles.Count);colors.Add(Color.Lerp(new Color(.27f,.43f,.31f),new Color(.56f,.64f,.4f),Mathf.PerlinNoise(cell[k].x*.14f,cell[k].z*.14f)));}
        }
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();
        var ground=new GameObject("Mountain trail meadow");ground.transform.SetParent(trailRoot);ground.AddComponent<MeshFilter>().sharedMesh=mesh;ground.AddComponent<MeshRenderer>().sharedMaterial=new Material(Resources.Load<Shader>("Shaders/Meadow"));
        var waterVertices=new List<Vector3>();var waterTriangles=new List<int>();
        for(int z=-20;z<30;z++){Vector3[] q={TP(96,z,.04f),TP(96,z+1,.04f),TP(100,z+1,.04f),TP(100,z,.04f)};foreach(int k in new[]{0,1,2,0,2,3}){waterVertices.Add(q[k]);waterTriangles.Add(waterTriangles.Count);}}
        var riverMesh=new Mesh();riverMesh.SetVertices(waterVertices);riverMesh.SetTriangles(waterTriangles,0);riverMesh.RecalculateNormals();
        var river=new GameObject("Continuous mountain stream");river.transform.SetParent(trailRoot);river.AddComponent<MeshFilter>().sharedMesh=riverMesh;river.AddComponent<MeshRenderer>().sharedMaterial=new Material(Resources.Load<Shader>("Shaders/Water"));
        Vector3[] route={TP(82,-13),TP(90,-6),TP(94,2),TP(102,2),TP(112,10),TP(106,20)};
        DetailedTrailPath(route);
        for(int i=0;i<16;i++)TrailShape("Trail bridge plank",PrimitiveType.Cube,TP(95.6f+i*.3f,2,.14f),new Vector3(.28f,.2f,2.8f),new Color(.49f,.33f,.21f));
        for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)TrailShape("Bridge marker",PrimitiveType.Cube,TP(96+i,2+side*1.3f,.6f),new Vector3(.12f,1.2f,.12f),sage);
        for(int i=0;i<10;i++)
        {
            for(int layer=0;layer<2;layer++)TrailRock("Layered waterfall cliff",TP(94+i*2.9f,28+layer*1.3f,1.7f+layer*2.1f),new Vector3(2.3f,2.7f+(i%3)*.5f,2.8f),new Color(.34f+layer*.05f,.4f+layer*.04f,.35f+layer*.025f),i+layer*19);
        }
        DetailedWaterfall();
        if(atmosphere)atmosphere.Steam(TP(98,24,.45f),.45f,14,2,new Color(.89f,.96f,.94f,.6f));
        for(int i=0;i<28;i++)
        {
            float x=i%2==0?78+(i%5):116+(i%3),z=-15+i*1.4f;
            TrailShape("Mountain tree trunk",PrimitiveType.Cylinder,TP(x,z,1.1f),new Vector3(.3f,1.1f,.3f),new Color(.38f,.25f,.17f));
            for(int layer=0;layer<2;layer++)
            {
                var crown=new GameObject("Faceted mountain canopy");crown.transform.SetParent(trailRoot);crown.transform.position=TP(x,z,1.5f+layer*1.3f);crown.transform.localScale=Vector3.one*(1.7f-layer*.3f);
                var v=new List<Vector3>();var t=new List<int>();
                for(int f=0;f<8;f++){float a=f*Mathf.PI/4,b=(f+1)*Mathf.PI/4;v.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));v.Add(Vector3.up*1.7f);v.Add(new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)));t.Add(v.Count-3);t.Add(v.Count-2);t.Add(v.Count-1);}
                var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();crown.AddComponent<MeshFilter>().sharedMesh=m;crown.AddComponent<MeshRenderer>().sharedMaterial=TeaHouseWorld.Mat("Trail crown "+i%3,i%3==0?new Color(.78f,.45f,.23f):new Color(.31f,.49f,.37f));trailCanopies.Add(crown.GetComponent<Renderer>());
            }
        }
        TrailDressing(route);
        Shelter(TP(106,20),"天狗哨所");Shelter(TP(86,21),"瀑布觀景亭");
        AddSpot("前往瀑布山路",ValleyGate,20);
        AddSpot("返回河童溪谷",TrailStart,21);
        AddSpot("巡山中的椛",TP(106,18),22,"Momiji");
        AddSpot("拍照中的文",TP(89,-5),26,"Aya");
        AddSpot("水路巡查的荷取",TP(103,5),27,"Nitori");
        AddSpot("瀑布觀景台",TP(86,21),25);
        for(int i=0;i<6;i++)
        {
            int kind=i%2==0?23:24;Vector3 p=TP(109+(i%3)*2,8+i*1.2f);
            AddSpot(kind==23?"山栗":"野莓",p,kind);
            var s=spots[spots.Count-1];s.visual=new GameObject(s.name);s.visual.transform.position=p;s.visual.transform.SetParent(trailRoot);
            for(int f=0;f<4;f++)TeaHouseWorld.Shape("Trail ingredient",PrimitiveType.Sphere,p+new Vector3((f%2)*.25f,.2f,(f/2)*.25f),Vector3.one*.23f,kind==23?new Color(.56f,.29f,.14f):new Color(.7f,.23f,.38f),s.visual.transform);
            DetailIngredients(s,kind);
        }
        MountainArt.WorldText("瀑布山路 →",ValleyGate+Vector3.up*1.6f,.16f,cream);
        var batch=new List<GameObject>();
        foreach(var renderer in trailRoot.GetComponentsInChildren<MeshRenderer>())
        {
            if(renderer.GetComponentInParent<AvatarMotion>()||renderer.GetComponent<WaterRibbon>()||renderer.name=="Faceted mountain canopy"||renderer.transform.parent.name=="山栗"||renderer.transform.parent.name=="野莓")continue;
            batch.Add(renderer.gameObject);
        }
        StaticBatchingUtility.Combine(batch.ToArray(),trailRoot.gameObject);
    }
    void Shelter(Vector3 p,string labelText)
    {
        DetailedShelter(p,labelText);
    }
    void SyncTrip()
    {
        SyncRoutines();SyncNotebook();
        foreach(var s in spots)if(s.kind==23||s.kind==24)if(s.visual)s.visual.SetActive(!data.harvested.Contains(s.id));
        if(playerMotion)playerMotion.Carrying=data.holding||data.lunchState==2;
    }
    bool ChangeRegion(bool mountain)
    {
        if(data.night||data.holding||trial)return false;
        data.onTrail=mountain;player.position=mountain?TP(82,-12):ValleyGate+Vector3.back;
        cam.transform.position=player.position+CameraOffset;modal=false;nearest=null;travelBook=false;
        if(mountain)data.trailVisited=true;SyncTrip();Save(false);Notify(mountain?"瀑布山路 · 山栗與野莓每天更新，便當沒有倒數限制。":"回到河童溪谷，可以備餐與營業了。",6);return true;
    }
    bool AcceptLunch()
    {
        if(data.night||data.lunchState!=0&&data.lunchState!=3||data.lunchDay==data.day)return false;
        data.lunchState=1;Save(false);return true;
    }
    bool PackLunch()
    {
        if(data.night||data.lunchState!=1||data.meal<1||!NearTea()||data.holding)return false;
        data.meal--;data.lunchPerfect=data.qualityMeal>0;if(data.lunchPerfect)data.qualityMeal--;
        data.lunchState=2;SyncTrip();Save(false);Notify("竹筍菇飯已打包，去瀑布山路的天狗哨所找椛。",6);return true;
    }
    bool UnpackLunch()
    {
        if(data.lunchState!=2||!NearTea())return false;
        data.meal++;if(data.lunchPerfect)data.qualityMeal++;data.lunchState=1;SyncTrip();Save(false);return true;
    }
    bool DeliverLunch()
    {
        if(!data.onTrail||data.night||data.lunchState!=2||Vector3.Distance(player.position,spots.Find(s=>s.kind==22).pos)>2.6f)return false;
        data.lunchState=3;data.lunchDay=data.day;data.deliveries++;data.money+=100;data.trailRecipes=true;
        data.chestnuts+=2;data.berries+=2;SyncTrip();
        data.journal.Add("第 "+data.day+" 天 · 巡山便當送達，椛分享了栗子飯與莓果茶食譜。");
        Say("犬走椛","熱便當送到啦，辛苦你走這趟山路。\n這是 100 文報酬，還有山栗與野莓各兩份。\n把栗子飯和莓果茶的做法帶回茶屋吧！");Play(chime);Save(false);return true;
    }
    void InteractTrip(Spot s)
    {
        if(s.kind>=28&&s.kind<=30){FindNotebookClue(s);return;}
        if(s.kind==22&&data.notebookStage==4){ReturnNotebook();return;}
        if(s.kind==20||s.kind==21){if(!ChangeRegion(s.kind==20))Notify("先結束營業或放回送餐托盤，再出發。");return;}
        if(s.kind==23||s.kind==24)
        {
            if(data.harvested.Contains(s.id))return;NormalizeWeek();data.weekHarvest++;data.harvested.Add(s.id);s.visual.SetActive(false);
            if(s.kind==23)data.chestnuts+=2;else data.berries+=2;Notify(s.name+" ＋2");Save(false);return;
        }
        if(s.kind==22){if(!DeliverLunch())Say("犬走椛",data.lunchState==1?"到茶屋料理台打包一份竹筍菇飯，再送到這個哨所吧。":"山路今天很平靜。打開旅行手帳，可以接下巡山便當委託。\n沒有時間限制，路上慢慢逛就好。");}
        if(s.kind==26){data.photoVisited=true;Say("射命丸文","瀑布在這個角度最好看！\n今天不追大新聞，拍一張山中日常就夠了。\n穿過小橋後，沿著山栗小徑就能到椛的哨所。 ");Save(false);}
        if(s.kind==27){data.waterVisited=true;Say("河城荷取","這條水路通向溪谷的水車。橋面濕，小心走。\n山栗與野莓就在前面，但別走進水裡喔。");Save(false);}
        if(s.kind==25){data.lookoutVisited=true;Say("瀑布觀景台","水聲從山壁落下，遠方的溪谷隱約可見。\n你把這個歇腳處記進旅行手帳。 ");Save(false);}
    }
    void LateUpdate()
    {
        TickRainWeather(Time.deltaTime);
        TickTeaLife(Time.deltaTime);
        TickLivingMountain(Time.deltaTime);UpdateMountainAudio(Time.deltaTime);UpdateInteractionMarker();
        foreach(var r in trailCanopies)if(r){Vector3 p=r.transform.position;r.enabled=!(data.onTrail&&Mathf.Abs(p.x-player.position.x)<3.5f&&p.z<player.position.z&&p.z>player.position.z-7);}
    }
    string LunchStatus(){return data.lunchState==2?"已打包 → 送到天狗哨所":data.lunchState==1?"已接取 → 茶屋打包菇飯 ×1":data.lunchState==3?"已完成，下一天可再接":"尚未接取";}
    void DrawTravelBook()
    {
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.4f));Panel(new Rect(175,130,1090,635));
        Text(210,155,850,40,"旅行手帳 · 瀑布山路",heading);if(Button(1060,150,170,"收起 [Esc]")){travelBook=false;notebook=false;}
        if(Button(210,198,225,"便當與地圖"))travelTab=0;if(Button(455,198,245,"巡山筆記事件"))travelTab=1;
        if(travelTab==1){DrawNotebookQuest();return;}
        Rect map=new Rect(210,250,490,380);Box(map,new Color(.66f,.76f,.6f));Box(new Rect(map.x+221,map.y,42,map.height),new Color(.31f,.65f,.69f));
        Box(new Rect(map.x+215,map.y+201,54,23),new Color(.58f,.4f,.24f));
        Vector3[] positions={new Vector3(82,0,-13),new Vector3(89,0,-5),new Vector3(103,0,5),new Vector3(112,0,12),new Vector3(106,0,18),new Vector3(86,0,21)};
        string[] names={"返回溪谷","文 · 拍照","荷取 · 水路","山栗／野莓","椛 · 哨所","觀景亭"};
        positions[1]=spots.Find(s=>s.kind==26).pos;positions[2]=spots.Find(s=>s.kind==27).pos;positions[4]=spots.Find(s=>s.kind==22).pos;
        for(int i=0;i<positions.Length;i++){float x=map.x+(positions[i].x-77)/42*map.width,y=map.y+(25-positions[i].z)/41*map.height;Box(new Rect(x-4,y-4,8,8),gold);Text(x+10,y-14,210,30,names[i],small);}
        if(data.onTrail){float x=map.x+(player.position.x-77)/42*map.width,y=map.y+(25-player.position.z)/41*map.height;Box(new Rect(x-5,y-5,10,10),ink);}
        Text(760,230,430,35,"巡山便當",heading);Text(760,270,435,32,LunchStatus(),small);
        Text(760,309,435,96,"指定竹筍菇飯 ×1，報酬 100 文。\n首次送達解鎖栗子飯與莓果茶。\n沒有倒數，便當可跨日保留。\n山栗 "+data.chestnuts+" · 野莓 "+data.berries,small);
        if(Button(760,413,435,"接下委託",!data.night&&(data.lunchState==0||data.lunchState==3)&&data.lunchDay!=data.day))AcceptLunch();
        if(Button(760,465,435,"在茶屋打包菇飯",data.lunchState==1&&NearTea()&&data.meal>0))PackLunch();
        if(Button(760,517,435,"拆開便當，退回料理",data.lunchState==2&&NearTea()))UnpackLunch();
        Text(760,582,440,95,"發現：\n"+(data.photoVisited?"✓ 文的拍照點  ":"□ 文的拍照點  ")+(data.waterVisited?"✓ 水路":"□ 水路")+"\n"+(data.lookoutVisited?"✓ 瀑布觀景亭":"□ 瀑布觀景亭"),small);
        Text(210,683,990,42,"溪谷東北出口進入山路 → 小橋 → 果實小徑 → 哨所。營業前請先返回茶屋。",small);
    }
    void TestMountainTrip()
    {
        NewGame();modal=false;NormalizePlanning();
        Assert(!Craft(5)&&!Craft(6),"trail recipes locked before delivery");
        Assert(AcceptLunch()&&!AcceptLunch(),"lunch request cannot duplicate");
        player.position=new Vector3(-12,0,-3);data.meal=1;data.qualityMeal=1;
        Assert(PackLunch()&&data.meal==0&&data.qualityMeal==0&&data.lunchPerfect,"lunch reserves premium meal once");
        Assert(!PackLunch()&&UnpackLunch()&&data.meal==1&&data.qualityMeal==1,"unpacking refunds meal and quality");
        Assert(PackLunch()&&!OpenShop(),"packed lunch blocks opening until delivered or unpacked");
        Assert(ChangeRegion(true)&&data.onTrail&&Walkable(TP(98,2))&&!Walkable(TP(98,10)),"trail entry bridge and river collision");
        Assert(!DeliverLunch(),"cannot deliver lunch remotely");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));
        Assert(data.onTrail&&data.lunchState==2&&data.lunchPerfect&&player.position.x>60,"mountain location and packed lunch survive loading");
        var chestnut=spots.Find(s=>s.kind==23);var berry=spots.Find(s=>s.kind==24);
        InteractTrip(chestnut);InteractTrip(chestnut);InteractTrip(berry);
        Assert(data.chestnuts==2&&data.berries==2,"trail ingredients harvest once per day");
        player.position=TP(106,18);int cash=data.money;
        Assert(DeliverLunch()&&data.money==cash+100&&data.trailRecipes&&data.deliveries==1,"lunch pays and unlocks both recipes");
        Assert(!DeliverLunch()&&!AcceptLunch()&&data.money==cash+100,"no duplicate delivery or same-day repeat");modal=false;
        data.mushrooms=1;data.leaves=1;
        Assert(Craft(5)&&Craft(6)&&data.chestnutRice==1&&data.berryTea==1&&data.chestnuts==2&&data.berries==2,"trail recipes consume new ingredients correctly");
        Assert(!OpenShop()&&ChangeRegion(false)&&!data.onTrail,"must return to valley for service");
        NextDay();Assert(!data.harvested.Contains(chestnut.id)&&AcceptLunch(),"next day renews harvests and delivery request");
        data.meal=1;player.position=new Vector3(-12,0,-3);PackLunch();NextDay();
        Assert(data.lunchState==2&&data.meal==0,"packed lunch does not expire across days");
        Assert(ChangeRegion(true),"repeat mountain entry");
        InteractTrip(spots.Find(s=>s.kind==26));modal=false;InteractTrip(spots.Find(s=>s.kind==27));modal=false;InteractTrip(spots.Find(s=>s.kind==25));modal=false;
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));
        Assert(data.trailRecipes&&data.photoVisited&&data.waterVisited&&data.lookoutVisited&&data.chestnutRice==1&&data.berryTea==1,"travel discoveries and new food persist");
        ChangeRegion(false);player.position=new Vector3(-12,0,-3);Assert(UnpackLunch(),"lunch must be unpacked at tea counter");SetMenu(5,true);SetMenu(6,true);
        data.guests.Clear();data.guests.Add(new Guest(Friends[0],5){state=1});
        Assert(Serve(0,5)&&data.guests[0].reward==77,"new mountain dish serves with correct price");
        Debug.Log("QA TRIP PASS: travel, collisions, premium lunch reservation/refund, delivery, daily harvest, recipes, persistence.");
    }
}
