using UnityEngine;
using System;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    bool planning;int planningTab,guestTarget;GameObject gardenDecor;
    static readonly string[] ShortDishes={"清茶","菇飯","鹽燒","香菇湯","竹葉茶","栗子飯","莓果茶"};
    static readonly string[] Recipes={"茶葉 ×2","菇 ×1 ＋ 筍 ×1","魚 ×1","菇 ×2 ＋ 茶葉 ×1","茶葉 ×1 ＋ 筍 ×1","山栗 ×2 ＋ 菇 ×1","野莓 ×2 ＋ 茶葉 ×1"};
    static readonly string[] GardenSlots={"桌椅配色","庭院燈籠","入口盆栽","庭院紙傘"};
    static readonly string[] GardenStyles={"原有布置","竹青風格","秋楓風格"};
    static readonly int[] GardenCosts={60,45,35,80};
    bool RecipeUnlocked(int d){return d<3||d==3&&Friendship(1).stage>=1||d==4&&Friendship(0).stage>=1||d>=5&&data.trailRecipes;}
    bool Ingredients(int d){return d==0?data.leaves>=2:d==1?data.mushrooms>=1&&data.bamboo>=1:d==2?data.fish>=1:d==3?data.mushrooms>=2&&data.leaves>=1:d==4?data.leaves>=1&&data.bamboo>=1:d==5?data.chestnuts>=2&&data.mushrooms>=1:data.berries>=2&&data.leaves>=1;}
    void NormalizePlanning()
    {
        if(data.menu==null)data.menu=new[]{true,true,true,false,false,false,false};else if(data.menu.Length!=DishNames.Length)Array.Resize(ref data.menu,DishNames.Length);
        if(data.prepTargets==null)data.prepTargets=new[]{2,2,2,0,0,0,0};else if(data.prepTargets.Length!=DishNames.Length)Array.Resize(ref data.prepTargets,DishNames.Length);
        if(data.gardenOwned==null||data.gardenOwned.Length!=4)data.gardenOwned=new int[4];
        if(data.gardenStyle==null||data.gardenStyle.Length!=4)data.gardenStyle=new int[4];
        if(data.sales==null)data.sales=new int[DishNames.Length];else if(data.sales.Length!=DishNames.Length)Array.Resize(ref data.sales,DishNames.Length);
    }
    bool OnMenu(int d){NormalizePlanning();return d>=0&&d<DishNames.Length&&data.menu[d]&&RecipeUnlocked(d);}
    bool SetMenu(int d,bool value)
    {
        if(data.night||d<0||d>=DishNames.Length||!RecipeUnlocked(d))return false;
        NormalizePlanning();data.menu[d]=value;Save(false);return true;
    }
    int MenuStock(){int total=0;for(int d=0;d<DishNames.Length;d++)if(OnMenu(d))total+=Stock(d);return total;}
    void ChooseGuestOrders()
    {
        var available=new List<int>();for(int d=0;d<DishNames.Length;d++)if(OnMenu(d))available.Add(d);
        if(available.Count==0)return; // legacy in-progress saves keep their original orders
        for(int i=0;i<data.guests.Count;i++)if(!OnMenu(data.guests[i].order))data.guests[i].order=available[(data.wave+i)%available.Count];
    }
    bool NearTea(){return !data.onTrail&&Vector3.Distance(player.position,new Vector3(-12,0,-2.2f))<6;}
    bool BuyGarden(int slot,int style,bool test=false)
    {
        NormalizePlanning();
        if(data.night||slot<0||slot>=4||style<0||style>2||(!test&&!NearTea()))return false;
        int mask=1<<style;int cost=GardenCosts[slot]+(style==2?40:0);
        if(style>0&&(data.gardenOwned[slot]&mask)==0)
        {if(data.money<cost)return false;data.money-=cost;data.gardenOwned[slot]|=mask;}
        data.gardenStyle[slot]=style;UpdateGarden();Save(false);return true;
    }
    void UpdateGarden()
    {
        NormalizePlanning();if(gardenDecor)Destroy(gardenDecor);gardenDecor=new GameObject("Custom tea garden");
        Color wood=data.gardenStyle[0]==1?new Color(.36f,.54f,.42f):data.gardenStyle[0]==2?new Color(.63f,.3f,.2f):new Color(.52f,.31f,.19f);
        foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if(r.name=="Guest table"||r.name=="Cushion")r.sharedMaterial=TeaHouseWorld.Mat("Garden palette "+data.gardenStyle[0]+r.name,data.gardenStyle[0]==0&&r.name=="Cushion"?new Color(.63f,.28f,.23f):wood);
            if(r.name=="Paper parasol")
            {
                int style=data.gardenStyle[3];Color canopy=style==1?new Color(.36f,.69f,.61f):style==2?new Color(.9f,.49f,.23f):r.transform.position.x<-10?new Color(.65f,.25f,.20f):new Color(.74f,.61f,.35f);
                r.sharedMaterial=TeaHouseWorld.Mat("Custom parasol "+style+" "+(r.transform.position.x<-10?"L":"R"),canopy);
            }
        }
        for(int slot=1;slot<3;slot++)if(data.gardenStyle[slot]>0)
        {
            Color color=data.gardenStyle[slot]==1?new Color(.36f,.69f,.61f):new Color(.9f,.49f,.23f);
            for(int side=0;side<2;side++)
            {
                Vector3 p=new Vector3(side==0?-18:-7,0,slot==1?-5:slot==2?-2:-11);
                if(slot==1)
                {
                    Decor("Lantern post",PrimitiveType.Cylinder,p+Vector3.up*1.4f,new Vector3(.12f,1.4f,.12f),wood);
                    Decor("Garden lantern",PrimitiveType.Sphere,p+Vector3.up*2.6f,new Vector3(.65f,.8f,.65f),color);
                    var lamp=new GameObject("Garden lantern light");lamp.transform.SetParent(gardenDecor.transform);lamp.transform.position=p+Vector3.up*2.5f;
                    var light=lamp.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.7f,.35f);light.range=4;light.intensity=1.5f;light.shadows=LightShadows.None;
                }
                else if(slot==2)
                {
                    Decor("Flower pot",PrimitiveType.Cylinder,p+Vector3.up*.25f,new Vector3(.7f,.25f,.7f),new Color(.65f,.36f,.24f));
                    for(int f=0;f<5;f++){Vector3 q=p+new Vector3(Mathf.Cos(f*1.3f)*.25f,.8f,Mathf.Sin(f*1.3f)*.25f);Decor("Flower stem",PrimitiveType.Cylinder,q-Vector3.up*.2f,new Vector3(.04f,.25f,.04f),sage);Decor("Garden flower",PrimitiveType.Sphere,q,new Vector3(.25f,.17f,.25f),color);}
                }
            }
        }
        SyncUpgradeArt();
    }
    GameObject Decor(string n,PrimitiveType type,Vector3 pos,Vector3 scale,Color color){return TeaHouseWorld.Shape(n,type,pos,scale,color,gardenDecor.transform);}
    void OpenPlanning(int tab=0){NormalizePlanning();planning=true;planningTab=tab;notebook=false;travelBook=false;modal=false;eventFriend=-1;}
    string ReportSummary()
    {
        NormalizePlanning();int best=0;for(int d=1;d<DishNames.Length;d++)if(data.sales[d]>data.sales[best])best=d;
        return "第 "+data.reportDay+" 天 · 接待 "+data.served+" 位／離席 "+data.lost+" 位\n營業收入 "+data.nightIncome+" 文（小費與加價 "+data.nightTips+" 文）\n其中今日推薦加價："+data.nightSpecialIncome+" 文（已包含在收入內）\n"+(data.nightIncome>0?"熱門餐點："+DishNames[best]+" · "+data.sales[best]+" 份":"今天尚未售出餐點")+"\n收益已在結帳時入帳，不會重複發放。";
    }
    void FinishNightReport(){data.reportDay=data.day;Say("今晚的營業紀錄",ReportSummary());Save(false);}
    void DrawPlanning()
    {
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.4f));Panel(new Rect(155,105,1130,695));
        Text(190,126,700,40,"茶屋計畫 · "+data.money+" 文",heading);
        if(Button(1080,125,170,"收起 [Esc]")){Save(false);planning=false;}
        for(int t=0;t<6;t++)if(Button(190+t*177,183,165,new[]{"每日菜單","庭院裝修","營業結算","七日手帖","茶屋升級","小祭典"}[t]))planningTab=t;
        if(planningTab==0)
        {
            Text(190,237,1050,74,SpecialSummary()+"　"+(data.night?"今晚菜單已鎖定":"目標數量只作提醒，不自動扣材料"),small);
            for(int d=0;d<DishNames.Length;d++)
            {
                float y=320+d*52;bool unlocked=RecipeUnlocked(d);
                Text(190,y,235,32,DishNames[d]+(d==DailyDish()?" ★":""),heading);Text(430,y,250,52,unlocked?Recipes[d]+"\n售價 "+Prices[d]+" 文 · 庫存 "+Stock(d):d>=5?"首次送達巡山便當解鎖":"完成"+(d==3?"荷取":"文")+"故事第一段解鎖",small);
                if(Button(705,y,135,OnMenu(d)?"供應 ✓":"不上架",!data.night&&unlocked))SetMenu(d,!data.menu[d]);
                Text(860,y+8,110,30,"目標 "+data.prepTargets[d],small);
                if(Button(968,y,45,"−",!data.night)){data.prepTargets[d]=Mathf.Max(0,data.prepTargets[d]-1);Save(false);}
                if(Button(1018,y,45,"＋",!data.night)){data.prepTargets[d]=Mathf.Min(20,data.prepTargets[d]+1);Save(false);}
                if(Button(1080,y,data.prepUpgrade?76:160,data.prepUpgrade?"火候":"製作",unlocked&&NearTea()&&!data.holding)){planning=false;BeginBrew(d);}
                if(data.prepUpgrade&&Button(1163,y,80,"備"+BatchCount(d)+"份",CanImprove()&&BatchCount(d)>0))MakeBatch(d);
            }
            Text(190,690,1050,28,"上架料理 "+MenuStock()+" 份（至少 3 份開店）。推薦可不上架，客人只點菜單內料理。",small);
            Text(190,718,1050,28,"常見口味：文喜歡清茶 · 荷取喜歡菇飯 · 椛喜歡鹽燒。今晚以客人的實際點單為準。",small);
            if(Button(190,750,340,"儲存備餐計畫"))Save();
            if(Button(550,750,340,"開店",!data.night&&NearTea()&&MenuStock()>=3)){planning=false;OpenShop();}
        }
        else if(planningTab==1)
        {
            Text(190,242,1050,35,"固定位置布置，不改變座位與通路。白天在茶屋附近購買，已擁有的款式免費切換。",small);
            for(int s=0;s<4;s++)
            {
                float y=305+s*105;Text(190,y,300,35,GardenSlots[s],heading);Text(190,y+39,320,30,"目前："+GardenStyles[data.gardenStyle[s]],small);
                for(int v=0;v<3;v++)
                {
                    bool owned=v==0||(data.gardenOwned[s]&(1<<v))!=0;int price=GardenCosts[s]+(v==2?40:0);
                    if(Button(535+v*235,y,220,GardenStyles[v]+(owned?" · 已有":" · "+price+" 文"),!data.night&&NearTea()&&(owned||data.money>=price)))BuyGarden(s,v);
                }
            }
            Text(190,740,1050,35,"裝修為外觀選擇，不收維護費，也不影響角色故事。",small);
        }
        else if(planningTab==3)DrawTeaWeek();
        else if(planningTab==4)DrawTeaUpgrades();
        else if(planningTab==5)DrawFestival();
        else
        {
            Text(190,255,1000,175,data.reportDay==0?"第一次營業後，這裡會留下完整結算。":ReportSummary(),body);
            for(int d=0;d<DishNames.Length;d++)Text(190,440+d*36,800,35,DishNames[d]+"　售出 "+data.sales[d]+" 份",small);
            Text(190,710,1000,60,"下一個裝修目標：入口盆栽 35 文／庭院燈籠 45 文\n提前休息也會結算已送出的餐點，未送出的托盤會退回。",small);
        }
    }
    void DrawCompactGuests()
    {
        if(data.guests.Count==0)return;guestTarget=Mathf.Clamp(guestTarget,0,data.guests.Count-1);
        Panel(new Rect(1065,205,350,485));Text(1080,216,320,35,"客人 · 第 "+(data.wave+1)+" 輪",heading);
        for(int i=0;i<data.guests.Count;i++)
        {
            var g=data.guests[i];float y=261+i*68;Portrait(g.name,new Rect(1080,y,36,44));
            Text(1125,y,270,27,g.name+(i==guestTarget?" ◀":""),small);
            Text(1125,y+26,200,30,g.done?"已離席":g.state==0?"入座中":g.state==1?ShortDishes[g.order]:g.state==2?"享用中":g.state==3?"聊天中":"離開中",small);
            if(g.state==1&&Button(1325,y+12,74,"選擇",!modal&&!data.holding))guestTarget=i;
            Box(new Rect(1125,y+56,265*Mathf.Clamp01(g.patience/100),3),gold);
        }
        bool near=NearTea()&&!modal&&!data.holding&&data.guests[guestTarget].state==1;
        for(int d=0;d<DishNames.Length;d++)if(Button(1080+d%3*108,478+d/3*46,102,ServingLabels[d]+" "+Stock(d),near&&OnMenu(d)&&Stock(d)>0))PickTray(guestTarget,d);
        Text(1080,622,320,60,GuestTasteHint(data.guests[guestTarget]),small);
    }
    void DrawTableOrders()
    {
        if(!data.night)return;
        for(int i=0;i<data.guests.Count;i++)
        {
            var g=data.guests[i];if(g.done||g.state!=1)continue;
            Vector3 p=cam.WorldToViewportPoint(new Vector3(-16+i*3,2.5f,-9.2f));float x=p.x*1440,y=(1-p.y)*900;
            if(x<50||x>1010||y<135||y>680)continue;
            Box(new Rect(x-68,y-28,136,30),sage);Text(x-60,y-26,125,30,(data.holding&&data.heldGuest==i?"[E] 送餐":ShortDishes[g.order]),label);
        }
    }
    void TestTeaPlanning()
    {
        NewGame();modal=false;NormalizePlanning();
        Assert(OnMenu(0)&&OnMenu(1)&&OnMenu(2)&&!OnMenu(3),"legacy default menu");
        data.leaves=12;data.mushrooms=8;data.bamboo=6;
        Assert(!Craft(3)&&!Craft(4),"new recipes require character chapter");
        Friendship(0).stage=1;Friendship(1).stage=1;
        Assert(Craft(3)&&Craft(4)&&data.soup==1&&data.leafTea==1&&data.mushrooms==6&&data.bamboo==5&&data.leaves==10,"two unlocked recipes consume correct ingredients");
        for(int d=0;d<5;d++)SetMenu(d,false);
        Assert(!OpenShop(),"empty menu cannot open");SetMenu(3,true);
        data.tea=6;Assert(!OpenShop(),"off-menu stock cannot satisfy opening minimum");
        Craft(3);Craft(3);data.qualitySoup=1;
        Assert(OpenShop(),"soup-only menu can open");
        Assert(data.guests.TrueForAll(g=>g.order==3),"guests only order on-menu dishes");
        Assert(!SetMenu(0,true)&&!BuyGarden(0,1,true),"night locks menu and furnishing");
        TickCafe(0,true);Assert(!Serve(0,4)&&data.leafTea==1,"off-menu meal cannot be served");
        Assert(Serve(0,3)&&Serve(1,3)&&Serve(2,3),"new recipe can be served to whole wave");TestFinishWave();
        Assert(data.nightIncome==199&&data.nightTips==55&&data.sales[3]==3,"night report includes actual payments and tips");
        int cash=data.money;ReportSummary();ReportSummary();Assert(data.money==cash,"report is read-only and cannot pay twice");
        NextDay();Assert(data.reportDay==1&&data.nightIncome==199,"report survives next morning");
        data.money=0;Assert(!BuyGarden(0,1,true)&&data.gardenOwned[0]==0,"insufficient furnishing budget leaves ownership unchanged");
        data.money=2000;
        for(int s=0;s<4;s++)for(int v=1;v<3;v++)
        {
            int before=data.money;Assert(BuyGarden(s,v,true)&&data.money==before-GardenCosts[s]-(v==2?40:0),"furnishing charges exactly once");
            before=data.money;Assert(BuyGarden(s,v,true)&&data.money==before,"owned style swaps for free");
        }
        Assert(BuyGarden(0,0,true)&&data.gardenOwned[0]==6,"original style retains purchased ownership");
        data.prepTargets[3]=7;Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));
        Assert(data.gardenOwned[3]==6&&data.gardenStyle[3]==2&&data.prepTargets[3]==7&&data.menu[3]&&!data.menu[0]&&data.nightIncome==199,"furnishing menu targets and report persist");
        Debug.Log("QA PLANNING PASS: unlocked recipes, menu validation, payments, reports, furnishings, persistence.");
    }
}
