using UnityEngine;

public partial class MountainTeaGame
{
    class InteractionCue
    {
        public int category;public string action,detail;public Vector3 target;public bool ready;
    }
    static readonly string[] CueCategories={"採集","對話","設施","送餐","移動","線索"};
    static readonly Color[] CueColors={new Color(.37f,.7f,.43f),new Color(.38f,.66f,.83f),new Color(.92f,.74f,.39f),new Color(.97f,.55f,.26f),new Color(.68f,.52f,.83f),new Color(.86f,.81f,.54f)};
    LineRenderer cueRing;GameObject[] cueGlyphs=new GameObject[6];Material[] cueMaterials=new Material[6];int markerCategory=-1;
    Spot SelectNearestSpot()
    {
        Spot selected=null;float distance=2.2f;
        foreach(var s in spots)
        {
            if(!VisibleLivingSpot(s))continue;
            if((s.kind==23||s.kind==24)&&data.harvested.Contains(s.id))continue;
            if(data.night&&s.kind>=11&&s.kind<=13)continue;
            if(s.kind<3&&(data.night||data.harvested.Contains(s.id)))continue;
            float d=Vector3.Distance(player.position,s.pos);if(d<distance){selected=s;distance=d;}
        }
        return selected;
    }
    InteractionCue CurrentInteractionCue()
    {
        if(restSeat>=0)return new InteractionCue{category=2,target=restBenchSpots[restSeat].pos,ready=true,action="起身繼續散步",detail="E／Space／Esc 或移動起身；時間照常，不跨日、不消耗材料。"};
        if(data.holding)
        {
            if(data.heldGuest<0||data.heldGuest>=data.guests.Count)return null;
            var guest=data.guests[data.heldGuest];Vector3 seat=new Vector3(-16+data.heldGuest*3,0,-9.2f);float distance=Vector3.Distance(player.position,seat);
            if(guest.state!=1)return null;
            return new InteractionCue{category=3,target=seat,ready=distance<=2.6f,action="送出 "+DishNames[data.heldDish],detail="目標："+TableName(data.heldGuest)+" · "+guest.name+" · "+distance.ToString("F1")+" m / 2.6 m"};
        }
        var s=nearest;if(s==null||!VisibleLivingSpot(s)||Vector3.Distance(player.position,s.pos)>=2.2f)return null;
        if((s.kind<3||s.kind==23||s.kind==24)&&data.harvested.Contains(s.id)||s.kind<3&&data.night)return null;
        var cue=new InteractionCue{category=2,target=s.pos,ready=true,action="查看 "+s.name,detail="打開選項後再確認，不會自動購買或休息。"};
        if(s.kind<3||s.kind==23||s.kind==24){cue.category=0;cue.action="採集 "+s.name+" ＋"+(s.kind==0?3:2);cue.detail="這個採集點每天一次，下一天重新長出。";}
        else if(s.kind>=11&&s.kind<=13||s.kind==22||s.kind==26||s.kind==27){cue.category=1;cue.action="與 "+s.name+" 對話";cue.detail="開始對話；委託與故事選項會在對話內顯示。";}
        if(s.kind==14){cue.action="開始釣魚";cue.detail="開始後，浮標進入金色區域時按 Space 或 E。";}
        if(s.kind==16)cue.action="查看休息選項";
        if(s.kind==20||s.kind==21){cue.category=4;cue.action=s.kind==20?"前往瀑布山路":"返回河童溪谷";cue.ready=!data.night&&!trial;cue.detail=cue.ready?"按 E 切換地圖；沒有倒數或旅行費。":"先結束營業，再出發。";}
        if(s.kind==25){cue.category=2;cue.action="閱讀觀景記錄";cue.detail="查看山中風景，把發現留在旅行手帳。";}
        if(s.kind==31){cue.action="坐下小憩";cue.ready=CanTrailRest(s);cue.detail=data.holding||data.lunchState==2?"先放回托盤或送達／收回便當，再休息。":"坐下聽山風；E／Space／Esc 或移動起身，不會跨日。";}
        if(s.kind>=28&&s.kind<=30){cue.category=5;cue.action="查看巡山線索";cue.ready=data.onTrail&&!data.night;cue.detail=cue.ready?s.name+" · 查看後推進筆記故事。":"白天再查看線索。";}
        if(s.kind==22&&data.notebookStage==4){cue.category=5;cue.action="歸還巡山筆記";cue.ready=data.onTrail&&!data.night;cue.detail=cue.ready?"交給椛，領取原有的委託報酬。":"白天再歸還筆記。";}
        else if(s.kind==22&&data.lunchState==2){cue.category=3;cue.action="送達巡山便當";cue.ready=data.onTrail&&!data.night;cue.detail=cue.ready?"交付已打包的菇飯，領取原有報酬。":"白天到山路送達便當。";}
        return cue;
    }
    bool ShowInteractionCue()
    {
        return started&&!photoMode&&!AvatarMotion.Frozen&&!modal&&!paused&&!settingsOpen&&!notebook&&!travelBook&&!planning&&!relationships&&!result&&!trial&&!fishing&&!brewing;
    }
    LineRenderer CueLine(string name,Transform parent,Vector3[] points,bool loop,float width)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);var line=g.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=loop;line.positionCount=points.Length;line.SetPositions(points);line.startWidth=line.endWidth=width;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;return line;
    }
    void BuildInteractionFeedback()
    {
        interactionMarker=new GameObject("Context interaction ground marker");
        var ring=new Vector3[48];for(int i=0;i<ring.Length;i++){float a=i*Mathf.PI*2/ring.Length;ring[i]=new Vector3(Mathf.Sin(a)*.74f,0,Mathf.Cos(a)*.74f);}
        cueRing=CueLine("Target ring",interactionMarker.transform,ring,true,.05f);
        Vector3[][] icons={
            new[]{new Vector3(0,.01f,-.28f),new Vector3(-.2f,.01f,0),new Vector3(0,.01f,.28f),new Vector3(.2f,.01f,0)},
            new[]{new Vector3(-.25f,.01f,-.1f),new Vector3(-.25f,.01f,.22f),new Vector3(.25f,.01f,.22f),new Vector3(.25f,.01f,-.12f),new Vector3(0,.01f,-.12f),new Vector3(-.12f,.01f,-.28f),new Vector3(-.12f,.01f,-.12f)},
            new[]{new Vector3(-.27f,.01f,0),new Vector3(0,.01f,.27f),new Vector3(.27f,.01f,0),new Vector3(0,.01f,-.27f)},
            new[]{new Vector3(-.29f,.01f,.1f),new Vector3(-.2f,.01f,-.16f),new Vector3(.2f,.01f,-.16f),new Vector3(.29f,.01f,.1f)},
            new[]{new Vector3(-.3f,.01f,0),new Vector3(.28f,.01f,0),new Vector3(.04f,.01f,.24f),new Vector3(.28f,.01f,0),new Vector3(.04f,.01f,-.24f)},
            new[]{new Vector3(-.25f,.01f,-.23f),new Vector3(-.25f,.01f,.23f),new Vector3(.25f,.01f,.23f),new Vector3(.25f,.01f,-.23f)}
        };
        for(int i=0;i<6;i++)
        {
            cueMaterials[i]=new Material(Shader.Find("Sprites/Default")){name="Interaction "+CueCategories[i],color=CueColors[i]};
            cueGlyphs[i]=new GameObject(CueCategories[i]+" glyph");cueGlyphs[i].transform.SetParent(interactionMarker.transform,false);
            CueLine("Action symbol",cueGlyphs[i].transform,icons[i],i==0||i==2||i==5,.045f).sharedMaterial=cueMaterials[i];cueGlyphs[i].SetActive(false);
        }
        interactionMarker.SetActive(false);
    }
    void UpdateContextMarker()
    {
        var cue=ShowInteractionCue()?CurrentInteractionCue():null;interactionMarker.SetActive(cue!=null);if(cue==null)return;
        interactionMarker.transform.position=cue.target+Vector3.up*.2f;
        if(markerCategory!=cue.category){markerCategory=cue.category;cueRing.sharedMaterial=cueMaterials[cue.category];for(int i=0;i<6;i++)cueGlyphs[i].SetActive(i==cue.category);}
        cueRing.startWidth=cueRing.endWidth=cue.ready?.065f:.035f;
    }
    void DrawInteractionFeedback()
    {
        if(!ShowInteractionCue())return;var cue=CurrentInteractionCue();if(cue==null)return;
        Panel(new Rect(380,690,560,90));TeaTag(new Rect(399,702,76,28),CueCategories[cue.category],Color.Lerp(CueColors[cue.category],ink,.6f));
        Text(490,702,435,36,(cue.ready?"[E] ":cue.category==3&&data.holding?"走近目標 · ":"暫不可用 · ")+cue.action,heading);Text(400,746,520,29,cue.detail,small);
    }
    void TestInteractionFeedback()
    {
        NewGame();modal=false;player.position=spots.Find(s=>s.kind==0).pos;nearest=SelectNearestSpot();var cue=CurrentInteractionCue();
        Assert(cue.category==0&&cue.action.Contains("＋3")&&cue.ready,"harvest cue matches real amount and selected target");
        int money=data.money,leaves=data.leaves,harvest=data.harvested.Count;for(int i=0;i<10;i++){CurrentInteractionCue();UpdateContextMarker();}
        Assert(data.money==money&&data.leaves==leaves&&data.harvested.Count==harvest,"feedback queries cannot harvest or pay");
        Interact(nearest);nearest=SelectNearestSpot();Assert(CurrentInteractionCue()==null||CurrentInteractionCue().category!=0,"harvested point stops advertising interaction");
        player.position=spots[1].pos;nearest=SelectNearestSpot();Assert(CurrentInteractionCue().category==1&&CurrentInteractionCue().action.Contains("對話"),"friend cue promises dialogue not automatic quest completion");
        player.position=spots[6].pos;nearest=SelectNearestSpot();Assert(CurrentInteractionCue().action=="查看休息選項"&&data.day==1,"rest cue does not promise immediate next day");
        player.position=spots[4].pos;nearest=SelectNearestSpot();Assert(CurrentInteractionCue().action=="開始釣魚","fishing cue matches E action");
        player.position=ValleyGate;nearest=SelectNearestSpot();Assert(CurrentInteractionCue().category==4&&CurrentInteractionCue().ready,"travel cue at original gate");
        data.night=true;Assert(!CurrentInteractionCue().ready,"night travel cue explains blocked action");data.night=false;
        player.position=new Vector3(-12,0,-3);data.tea=3;OpenShop();modal=false;TickCafe(0,true);PickTray(0,0);nearest=spots[0];cue=CurrentInteractionCue();
        Assert(cue.category==3&&cue.target==new Vector3(-16,0,-9.2f)&&!cue.ready,"tray overrides nearby facility and targets correct guest remotely");
        player.position=cue.target+Vector3.right*2.6f;Assert(CurrentInteractionCue().ready==(Vector3.Distance(player.position,cue.target)<=2.6f),"delivery cue shares exact distance comparison with delivery action");
        player.position=cue.target+Vector3.right*2.59f;Assert(CurrentInteractionCue().ready,"inside delivery boundary shows ready E");
        player.position=cue.target+Vector3.right*2.61f;Assert(!CurrentInteractionCue().ready,"outside delivery range does not advertise ready E");
        player.position=cue.target;UpdateContextMarker();Assert(interactionMarker.activeSelf&&markerCategory==3,"delivery ring follows actual guest seat");
        Assert(interactionMarker.transform.position.y>cue.target.y+.15f,"interaction ring sits above tea garden paving");
        paused=true;UpdateContextMarker();Assert(!interactionMarker.activeSelf,"pause hides action marker");paused=false;modal=true;Assert(!ShowInteractionCue(),"dialogue hides action feedback");modal=false;photoMode=true;Assert(!ShowInteractionCue(),"photo mode hides action feedback");photoMode=false;fishing=true;Assert(!ShowInteractionCue(),"fishing minigame hides exploration feedback");fishing=false;
        ReturnTray();data.night=false;data.guests.Clear();ChangeRegion(true);data.notebookStage=1;SyncNotebook();var clue=spots.Find(s=>s.kind==28);player.position=clue.pos;nearest=SelectNearestSpot();Assert(CurrentInteractionCue().category==5&&CurrentInteractionCue().action.Contains("線索"),"ordered notebook clue gets distinct feedback");
        data.notebookStage=4;SyncNotebook();var momiji=spots.Find(s=>s.kind==22);player.position=momiji.pos;nearest=SelectNearestSpot();data.lunchState=2;Assert(CurrentInteractionCue().action=="歸還巡山筆記","notebook return has same priority as actual interaction over lunch");
        data.notebookStage=0;Assert(CurrentInteractionCue().category==3&&CurrentInteractionCue().action=="送達巡山便當","packed lunch uses delivery cue at Momiji");
        Assert(interactionMarker.GetComponentsInChildren<Collider>(true).Length==0&&cueGlyphs.Length==6,"six action glyphs add no collision");
        NewGame();modal=false;SyncWeatherPeople();var leaf=spots.Find(s=>s.kind==0);player.position=leaf.pos+Vector3.back*2.19f;nearest=SelectNearestSpot();Assert(nearest==leaf,"selection keeps original 2.2 metre radius");player.position=leaf.pos+Vector3.back*2.21f;nearest=SelectNearestSpot();Assert(nearest!=leaf,"selection does not extend interaction range");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.day==1&&data.money==40&&!data.holding,"feedback introduces no persisted progress fields");
        Debug.Log("QA FEEDBACK PASS: action categories, amounts, priority, delivery range, overlays, clue order, glyphs, distance, zero transactions and save compatibility.");
    }
}
