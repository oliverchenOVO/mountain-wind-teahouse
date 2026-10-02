using UnityEngine;

public partial class MountainTeaGame
{
    class GuideStep
    {
        public string key,title,text;public Vector3 target;public bool hasTarget;public int page;
        public GuideStep(string k,string t,string message,int action=0){key=k;title=t;text=message;page=action;}
    }
    GuideStep GuideTo(string key,string titleText,string message,Vector3 pos,int page=0)
    {return new GuideStep(key,titleText,message,page){target=pos,hasTarget=true};}
    Spot GuideSpot(int kind)
    {
        Spot found=null;float best=float.MaxValue;
        foreach(var s in spots)if(s.kind==kind&&VisibleLivingSpot(s)&&!((kind<3||kind==23||kind==24)&&data.harvested.Contains(s.id)))
        {float distance=(s.pos-player.position).sqrMagnitude;if(distance<best){best=distance;found=s;}}
        return found;
    }
    int MissingIngredient(int dish)
    {
        if(dish==0)return data.leaves<2?0:-1;
        if(dish==1)return data.mushrooms<1?1:data.bamboo<1?2:-1;
        if(dish==2)return data.fish<1?14:-1;
        if(dish==3)return data.mushrooms<2?1:data.leaves<1?0:-1;
        if(dish==4)return data.leaves<1?0:data.bamboo<1?2:-1;
        if(dish==5)return data.chestnuts<2?23:data.mushrooms<1?1:-1;
        return data.berries<2?24:data.leaves<1?0:-1;
    }
    GuideStep CurrentGuide()
    {
        if(trial)return new GuideStep("trial","符卡練習","WASD 移動，避開紅色彈幕。\n撐過 25 秒；失敗可以再試。");
        if(fishing)return new GuideStep("fish","看準浮標","金色區域按 Space 或 E。\nEsc 收竿；失敗不損失材料。");
        if(brewing)return new GuideStep("brew","掌握火候","金色區域按 Space，製作精品。\nEsc 取消，不消耗材料。");
        if(data.holding&&data.heldGuest>=0&&data.heldGuest<data.guests.Count)
            return GuideTo("tray","把餐點送上桌","送給 "+data.guests[data.heldGuest].name+"。\n到桌邊按 E；料理台可放回。",new Vector3(-16+data.heldGuest*3,0,-9.2f));
        if(data.lunchState==2)
        {
            var s=GuideSpot(data.onTrail?22:20);
            return GuideTo("lunch","便當不會冷掉",data.onTrail?"找到巡山中的椛，靠近按 E。\n沒有倒數，可以慢慢走。":"去溪谷東北入口，按 E 上山。\n沿小徑過橋，找到椛本人。",s.pos,2);
        }
        if(data.night)
        {
            foreach(var guest in data.guests)if(!guest.done&&guest.state==1)
                return GuideTo("service","先端盤，再送餐","在茶屋附近選右側客人與料理。\n拿好托盤後，到桌邊按 E。",spots[0].pos);
            return new GuideStep("waiting","客人正在歇息","等待入座或享用；聊天後點繼續。\n想提早結算，可回房休息。",1);
        }
        if(data.onTrail)
        {
            if(data.notebookStage>=1&&data.notebookStage<=4)
            {var s=GuideSpot(data.notebookStage<4?27+data.notebookStage:22);return GuideTo("clue","巡山筆記",NotebookHint(),s.pos,2);}
            var spot=GuideSpot(data.notebookStage==5||data.lunchState==1?21:25);
            return GuideTo("trail","山路慢慢逛",data.notebookStage==5?"回溪谷茶屋，從旅行手帳讀後日談。":data.lunchState==1?"回茶屋打包一份菇飯，再出發。":"觀景亭可以歇腳，山栗與野莓可採集。\n旅行手帳能接便當與筆記委託。",spot.pos,2);
        }
        if(IsRainDay&&data.rainStoryStage<3&&NearTea()&&(data.tea>0||data.leafTea>0))
            return new GuideStep("rain","屋簷下的暖茶","右側「替友人泡一杯避雨茶」。\n每段用一杯茶，也可以晚點再聊。",1);
        int dish=DailyDish();if(!OnMenu(dish)){dish=-1;for(int d=0;d<DishNames.Length;d++)if(OnMenu(d)){dish=d;break;}}
        if(dish<0)return new GuideStep("menu","先決定今晚菜單","茶屋計畫 → 每日菜單，選料理上架。\n不用供應所有餐點。",1);
        if(MenuStock()>=3)return GuideTo("open","晚餐已備好","回料理台附近，茶屋計畫點「開店」。\n至少三份上架料理就能營業。",spots[0].pos,1);
        int kind=MissingIngredient(dish);
        if(kind<0)return GuideTo("prepare","材料已經足夠","準備 "+DishNames[dish]+"。\n茶屋計畫點製作／火候；手冊可批次備餐。",spots[0].pos,1);
        if(kind==23||kind==24)return GuideTo("mountain","去山路找食材","需要"+(kind==23?"山栗":"野莓")+"；東北入口按 E 上山。\n備好料理後再回茶屋。",GuideSpot(20).pos,2);
        var ingredient=GuideSpot(kind);
        if(ingredient==null)return GuideTo("rest","今天已採完","回房休息，明天採集物會重新長好。\n材料與料理保留，也能改做別的餐點。",GuideSpot(16).pos,1);
        return GuideTo("forage",kind==14?"到溪邊釣魚":"先找一點食材","準備 "+DishNames[dish]+"：需要"+ingredient.name+"。\n靠近金色互動標記，按 E。",ingredient.pos);
    }
    string GuideDirection(GuideStep step)
    {
        if(!step.hasTarget)return "可自由探索，不必照順序完成。";
        Vector2 delta=new Vector2(step.target.x-player.position.x,step.target.z-player.position.z);float distance=delta.magnitude;
        if(distance<2.2f)return "就在附近 · 依上方提示操作";
        int direction=Mathf.RoundToInt(Mathf.Atan2(delta.x,delta.y)*Mathf.Rad2Deg/45);direction=(direction+8)%8;
        return "直線："+new[]{"北","東北","東","東南","南","西南","西","西北"}[direction]+" · "+distance.ToString("F0")+" m（不是尋路）";
    }
    void DrawContextGuide()
    {
        var step=CurrentGuide();Panel(new Rect(25,125,330,data.guideHidden?63:155));
        Text(43,140,215,34,data.guideHidden?"操作引導已收起":step.title,heading);
        if(Button(269,135,68,data.guideHidden?"展開":"收起")){data.guideHidden=!data.guideHidden;Save(false);}
        if(data.guideHidden)return;
        Text(43,184,294,66,step.text,small);Text(43,252,294,24,GuideDirection(step),small);
        if(Button(25,737,285,step.page==1?"引導 · 打開茶屋計畫":step.page==2?"引導 · 打開旅行手帳":"引導 · 查看地圖"))
        {if(step.page==1)OpenPlanning();else if(step.page==2){travelBook=true;notebook=false;}else notebook=true;}
    }
    void TestContextGuide()
    {
        NewGame();modal=false;Assert(CurrentGuide().key=="forage","new player guide points to available tea leaves");
        int cash=data.money,count=data.harvested.Count;CurrentGuide();CurrentGuide();Assert(data.money==cash&&data.harvested.Count==count,"guide inspection does not perform gameplay transactions");
        data.leaves=4;Assert(CurrentGuide().key=="prepare","materials redirect guide to cooking");
        NormalizePlanning();for(int d=0;d<7;d++)data.menu[d]=false;Assert(CurrentGuide().key=="menu","empty menu guide explains supply selection");data.menu[0]=true;
        data.tea=3;Assert(CurrentGuide().key=="open","prepared menu guide explains opening");OpenShop();TickCafe(0,true);Assert(CurrentGuide().key=="service","waiting order guide explains tray selection");
        PickTray(0,0);var step=CurrentGuide();Assert(step.key=="tray"&&step.hasTarget&&step.target.x==-16,"held tray takes priority and targets assigned guest table");
        player.position=step.target;Assert(GuideDirection(step).Contains("附近"),"arrival uses nearby guidance");ReturnTray();data.guests.Clear();Assert(CurrentGuide().key=="waiting","dining or empty night does not suggest gathering");data.night=false;
        fishing=true;Assert(CurrentGuide().key=="fish","fishing uses timing controls");fishing=false;brewing=true;Assert(CurrentGuide().key=="brew","cooking uses timing controls");brewing=false;trial=true;Assert(CurrentGuide().key=="trial","trial explains movement");trial=false;
        data.lunchState=2;Assert(CurrentGuide().key=="lunch"&&CurrentGuide().target==GuideSpot(20).pos,"valley packed lunch points to region gate");ChangeRegion(true);Assert(CurrentGuide().target==GuideSpot(22).pos,"mountain packed lunch follows current Momiji position");data.lunchState=0;
        data.notebookStage=1;Assert(CurrentGuide().key=="clue"&&CurrentGuide().target==GuideSpot(28).pos,"notebook guide selects visible first clue");data.notebookStage=5;Assert(CurrentGuide().key=="trail"&&CurrentGuide().target==GuideSpot(21).pos,"epilogue returns player to valley");ChangeRegion(false);
        data.day=3;data.tea=1;player.position=spots[0].pos;Assert(CurrentGuide().key=="rain","rain tea hint remains optional");
        data.day=1;data.tea=0;data.leaves=0;foreach(var s in spots)if(s.kind==0)data.harvested.Add(s.id);Assert(CurrentGuide().key=="rest","depleted daily ingredients suggest rest rather than hidden resources");
        var east=GuideTo("test","","",player.position+Vector3.right*10);Assert(GuideDirection(east).Contains("東"),"compass reflects world east without claiming pathfinding");
        data.guideHidden=true;Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.guideHidden,"hidden guide preference survives save and load");
        data=JsonUtility.FromJson<SaveData>("{\"version\":1,\"day\":5,\"money\":80}");Assert(!data.guideHidden&&data.money==80,"legacy guide defaults visible without changing cash");
        Debug.Log("QA GUIDE PASS: contextual priority, no transactions, menus, cooking, service, tray target, timing controls, travel, moving actor, clues, rain, depletion, compass and preference persistence.");
    }
}
