using UnityEngine;

public partial class MountainTeaGame
{
    static readonly Vector2[] TravelMainRoute={new Vector2(82,-13),new Vector2(90,-6),new Vector2(94,2),new Vector2(102,2),new Vector2(112,10),new Vector2(106,20)};
    static readonly Vector2[] TravelLookoutRoute={new Vector2(94,2),new Vector2(90,2),new Vector2(86,12),new Vector2(86,18.5f)};
    static readonly Vector2[] TravelActorOffsets={new Vector2(18,32),new Vector2(-22,20),new Vector2(-25,-22)};
    GUIStyle travelMapLabel,travelMapPin,travelPaperButton;
    int travelDestination=-1;
    static readonly string[] TravelDestinationNames={"觀景亭","哨所亭","① 長凳","② 長凳","溪谷出口"};
    Vector3 TravelDestinationPosition(int index)
    {return index==0?TP(86,21):index==1?TP(106,20):index==2?restBenchSpots[0].pos:index==3?restBenchSpots[1].pos:TrailStart;}
    bool SelectTravelDestination(int index)
    {if(index<0||index>=TravelDestinationNames.Length)return false;travelDestination=index;return true;}
    GuideStep DestinationGuide()
    {return GuideTo("destination","散步 · "+TravelDestinationNames[travelDestination],"沿山路與小橋前往，不必走直線。\n旅行手帳可更換或取消目的地。",TravelDestinationPosition(travelDestination),2);}
    string DestinationCaption()
    {return travelDestination<0?"點下方紙籤選目的地 · 不傳送、不自動尋路":TravelDestinationNames[travelDestination]+" · "+(data.onTrail?GuideDirection(DestinationGuide()):"先到溪谷東北入口按 E 上山");}
    static Vector2 TravelMapPoint(Rect plot,Vector3 world)
    {return new Vector2(plot.x+(world.x-77)/42*plot.width,plot.y+(25-world.z)/41*plot.height);}
    static bool TravelMapContains(Vector3 world)
    {return world.x>=77&&world.x<=119&&world.z>=-16&&world.z<=25;}
    void MapStroke(Vector2 a,Vector2 b,float width,Color color)
    {
        Matrix4x4 before=GUI.matrix;
        GUI.matrix=TravelStrokeMatrix(before,a,b);
        try{RoundFill(new Rect(0,-width*.5f,(b-a).magnitude,width),color);}finally{GUI.matrix=before;}
    }
    static Matrix4x4 TravelStrokeMatrix(Matrix4x4 parent,Vector2 a,Vector2 b)
    {return parent*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);}
    void MapRoute(Rect plot,Vector2[] route)
    {
        for(int i=1;i<route.Length;i++)
        {
            Vector2 a=TravelMapPoint(plot,new Vector3(route[i-1].x,0,route[i-1].y)),b=TravelMapPoint(plot,new Vector3(route[i].x,0,route[i].y));
            MapStroke(a,b,9,new Color(.53f,.47f,.32f));MapStroke(a,b,6,new Color(.86f,.77f,.53f));
        }
    }
    void MapDot(Vector2 p,float radius,Color color)
    {CircleFill(new Rect(p.x-radius,p.y-radius,radius*2,radius*2),color);}
    void MapLabel(Vector2 p,string text)
    {GUI.Label(new Rect(p.x,p.y,110,24),text,travelMapLabel);}
    void MapLandmark(Rect plot,Vector3 world,string label,Vector2 offset,bool bench=false)
    {
        Vector2 point=TravelMapPoint(plot,world);MapDot(point,7,cream);MapDot(point,5,bench?new Color(.66f,.38f,.24f):sage);
        MapLabel(point+offset,label);
    }
    void DrawTravelMap()
    {
        if(travelMapLabel==null){travelMapLabel=Style(16,FontStyle.Normal,ink);travelMapPin=Style(14,FontStyle.Bold,cream);travelMapPin.alignment=TextAnchor.MiddleCenter;travelPaperButton=new GUIStyle(button){fontSize=16,border=new RectOffset(12,12,12,12),padding=new RectOffset(4,4,0,0)};}
        Rect card=new Rect(210,250,490,415),plot=new Rect(230,290,450,310);
        RoundFill(card,new Color(.56f,.63f,.47f));RoundFill(new Rect(214,254,482,407),new Color(.88f,.88f,.73f));
        Text(230,260,365,27,"散步圖 · A 文  N 荷取  M 椛",travelMapLabel);Text(610,262,72,24,"↑ 北",small);
        RoundFill(plot,new Color(.70f,.77f,.59f));
        // Muted forest patches are schematic decoration, not collision or navigation data.
        for(int i=0;i<7;i++){MapDot(new Vector2(plot.x+14,plot.y+26+i*40),12,new Color(.47f,.60f,.43f,.25f));MapDot(new Vector2(plot.xMax-14,plot.y+16+i*40),12,new Color(.47f,.60f,.43f,.25f));}
        Vector2 riverTop=TravelMapPoint(plot,new Vector3(96,0,25)),riverBottom=TravelMapPoint(plot,new Vector3(100,0,-16));
        RoundFill(new Rect(riverTop.x,riverTop.y,riverBottom.x-riverTop.x,plot.height),new Color(.39f,.66f,.67f));
        MapRoute(plot,TravelMainRoute);MapRoute(plot,TravelLookoutRoute);
        Vector2 bridge=TravelMapPoint(plot,new Vector3(98,0,2));
        RoundFill(new Rect(bridge.x-28,bridge.y-6,56,12),new Color(.44f,.31f,.21f));
        for(int i=0;i<7;i++)Box(new Rect(bridge.x-25+i*8,bridge.y-5,5,10),new Color(.72f,.55f,.34f));
        MapLandmark(plot,TrailStart,"溪谷出口",new Vector2(13,-12));
        MapLandmark(plot,TP(86,21),"觀景亭",new Vector2(-66,-24));
        MapLandmark(plot,TP(106,20),"哨所",new Vector2(14,-25));
        MapLandmark(plot,TP(112,12),"果實小徑",new Vector2(-19,13));
        MapLabel(bridge+new Vector2(-74,-25),"小橋");
        MapLandmark(plot,restBenchSpots[0].pos,"① 長凳",new Vector2(13,-6),true);
        MapLandmark(plot,restBenchSpots[1].pos,"② 長凳",new Vector2(12,5),true);
        if(travelDestination>=0)
        {
            Vector2 target=TravelMapPoint(plot,TravelDestinationPosition(travelDestination));
            MapDot(target,13,gold);MapDot(target,9,cream);MapDot(target,5,sage);
        }
        for(int i=0;i<routineKinds.Length;i++)
        {
            var actor=spots.Find(s=>s.kind==routineKinds[i]);if(actor==null||!TravelMapContains(actor.pos))continue;
            Vector2 point=TravelMapPoint(plot,actor.pos),labelPoint=point+TravelActorOffsets[i];
            MapStroke(point,labelPoint,1.5f,new Color(.23f,.43f,.47f));MapDot(point,3,new Color(.23f,.43f,.47f));MapDot(labelPoint,10,new Color(.23f,.43f,.47f));
            GUI.Label(new Rect(labelPoint.x-10,labelPoint.y-10,20,20),i==0?"A":i==1?"N":"M",travelMapPin);
        }
        if(data.onTrail&&TravelMapContains(player.position))
        {
            Vector2 point=TravelMapPoint(plot,player.position);MapDot(point,10,cream);MapDot(point,7,gold);MapDot(point,3,ink);
        }
        MapDot(new Vector2(238,616),5,sage);Text(249,605,125,25,"亭／出口",travelMapLabel);
        MapDot(new Vector2(382,616),5,new Color(.66f,.38f,.24f));Text(393,605,85,25,"長凳",travelMapLabel);
        MapDot(new Vector2(495,616),7,gold);MapDot(new Vector2(495,616),3,ink);Text(508,605,170,25,"你的位置",travelMapLabel);
        Color paperBefore=GUI.backgroundColor;
        for(int i=0;i<TravelDestinationNames.Length;i++)
        {
            GUI.backgroundColor=travelDestination==i?gold:Color.Lerp(sage,cream,.45f);
            travelPaperButton.normal.textColor=travelDestination==i?ink:cream;
            travelPaperButton.hover.textColor=travelDestination==i?ink:cream;
            travelPaperButton.active.textColor=travelDestination==i?ink:cream;
            if(GUI.Button(new Rect(222+i*94,630,90,28),TravelDestinationNames[i],travelPaperButton))SelectTravelDestination(i);
        }
        GUI.backgroundColor=paperBefore;
    }
    string TravelMapLocation()
    {return data.onTrail?"目前在山路 · X "+player.position.x.ToString("F1")+" / Z "+player.position.z.ToString("F1"):"目前在溪谷 · 山路圖不顯示溪谷位置";}
    void TestTravelDestinations()
    {
        NewGame();modal=false;Assert(travelDestination==-1,"new journey has no temporary destination");
        string snapshot=JsonUtility.ToJson(data);Vector3 position=player.position;
        Assert(!SelectTravelDestination(-1)&&!SelectTravelDestination(5)&&travelDestination==-1,"invalid destination cannot change selection");
        for(int i=0;i<5;i++){Assert(SelectTravelDestination(i)&&TravelMapContains(TravelDestinationPosition(i)),"all destination papers target real mountain coordinates");Assert(DestinationCaption().Contains("先到溪谷"),"valley destination gives gate instructions instead of cross-region distance");}
        Assert(snapshot==JsonUtility.ToJson(data)&&position==player.position,"destination selection does not spend teleport or change saved progress");
        SelectTravelDestination(2);ChangeRegion(true);Assert(travelDestination==2&&DestinationGuide().target==restBenchSpots[0].pos,"entering mountain retains chosen bench and its actual interaction position");
        Assert(CurrentGuide().key=="destination","chosen walk overrides passive mountain guide");
        player.position=TravelDestinationPosition(2);Assert(DestinationCaption().Contains("附近")&&restSeat==-1,"arrival does not automatically sit or interact");
        trial=true;Assert(CurrentGuide().key=="trial","trial controls take priority over destination");trial=false;
        data.lunchState=2;Assert(CurrentGuide().key=="lunch","packed lunch guidance takes priority over optional walk");data.lunchState=0;
        travelDestination=-1;Assert(CurrentGuide().key!="destination","cancellation restores original guide");
        SelectTravelDestination(0);Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));modal=false;Assert(travelDestination==-1&&data.onTrail,"load clears paper without changing saved region");
        SelectTravelDestination(1);ChangeRegion(false);Assert(travelDestination==-1,"returning valley clears destination");
        SelectTravelDestination(4);NextDay();modal=false;Assert(travelDestination==-1,"next day clears destination");
        SelectTravelDestination(3);ReturnMenu();Assert(travelDestination==-1,"title clears temporary destination");NewGame();modal=false;
        Debug.Log("QA DESTINATION PASS: five targets, validation, readonly selection, valley entry, priority, arrival, cancel, save/load, return, days and title.");
    }
    void TestTravelMap()
    {
        NewGame();modal=false;var plot=new Rect(230,290,450,310);
        Assert(TravelMapPoint(plot,new Vector3(77,0,25))==new Vector2(plot.x,plot.y)&&TravelMapPoint(plot,new Vector3(119,0,-16))==new Vector2(plot.xMax,plot.yMax),"travel map corners share original walkable coordinate bounds");
        Assert(TravelMapPoint(plot,new Vector3(98,0,4)).y<TravelMapPoint(plot,new Vector3(98,0,2)).y,"map north follows increasing world z");
        Assert(TravelMapPoint(plot,new Vector3(100,0,2)).x>TravelMapPoint(plot,new Vector3(96,0,2)).x,"map east follows increasing world x");
        var parent=Matrix4x4.TRS(new Vector3(0,64,0),Quaternion.identity,new Vector3(.711f,.711f,1));var a=new Vector2(327,385);var b=new Vector2(370,464);var line=TravelStrokeMatrix(parent,a,b);
        Assert(Vector3.Distance(line.MultiplyPoint3x4(Vector3.zero),parent.MultiplyPoint3x4(a))<.001f,"scaled map line begins at parent-transformed map coordinate");
        Assert(Vector3.Distance(line.MultiplyPoint3x4(Vector3.right*(b-a).magnitude),parent.MultiplyPoint3x4(b))<.001f,"scaled diagonal map line ends within parent map instead of screen pivot");
        Assert(!TravelMapContains(ValleyGate)&&!TravelMapContains(TP(120,0))&&TravelMapContains(TP(98,2)),"map never clamps valley or outside coordinates to mountain edge");
        Assert(TravelMainRoute.Length==6&&TravelLookoutRoute.Length==4&&TravelLookoutRoute[0]==TravelMainRoute[2],"lookout spur joins actual main route at west bridge approach");
        foreach(var route in new[]{TravelMainRoute,TravelLookoutRoute})for(int i=1;i<route.Length;i++)for(int step=0;step<=10;step++)
        {Vector2 p=Vector2.Lerp(route[i-1],route[i],step/10f);Assert(TrailWalkable(TP(p.x,p.y)),"mapped path follows existing walkable mountain route");}
        Assert(restBenchSpots.Count==2&&restBenchSpots.TrueForAll(s=>TravelMapContains(s.pos)),"map benches use actual interaction positions");
        Assert(TravelMapLocation().Contains("溪谷"),"valley context explains absent mountain player pin");
        ChangeRegion(true);player.position=TP(88.52f,18.8f);Assert(TravelMapLocation().Contains("山路")&&TravelMapLocation().Contains("88.5"),"mountain caption reports actual current coordinates");
        string snapshot=JsonUtility.ToJson(data);Vector3 position=player.position;int count=spots.Count;
        for(int i=0;i<20;i++){TravelMapPoint(plot,player.position);TravelMapLocation();SelectTravelTab(i%2);}
        Assert(snapshot==JsonUtility.ToJson(data)&&player.position==position&&spots.Count==count,"map queries and tabs cannot teleport spend or advance progress");
        data.day=3;SyncWeatherPeople();SyncRoutines();player.position=TP(98,2);AvatarMotion.Frozen=false;TickLivingMountain(100);
        Assert(routineKinds.Length==3&&spots.Find(s=>s.kind==26).pos==TP(86,20)&&spots.Find(s=>s.kind==27).pos==TP(105,20)&&spots.Find(s=>s.kind==22).pos==TP(107,20),"map reads original rain refuge positions for all three friends");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.onTrail&&data.day==3&&restBenchSpots.Count==2,"map adds no saved fields or duplicate bench registration on load");
        NextDay();modal=false;Assert(!data.onTrail&&TravelMapLocation().Contains("溪谷"),"next day returns map context to valley");NewGame();modal=false;Assert(travelTab==0,"new journey retains original travel tab reset");
        Debug.Log("QA TRAVEL MAP PASS: bounds, compass, real main and spur geometry, benches, player context, readonly queries, rain friends, load and days.");
    }
}
