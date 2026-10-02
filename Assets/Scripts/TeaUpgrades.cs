using UnityEngine;

public partial class MountainTeaGame
{
    static readonly string[] UpgradeNames={"舒心坐墊","備餐手冊"};
    static readonly int[] UpgradeCosts={150,120};
    bool UpgradeOwned(int i){return i==0?data.comfortUpgrade:i==1&&data.prepUpgrade;}
    float PatienceRate(){return data.comfortUpgrade?.3f:.4f;}
    bool CanImprove(){return started&&!data.night&&NearTea()&&!data.holding&&data.lunchState!=2&&!brewing&&!trial&&!fishing&&!modal&&!paused&&!settingsOpen&&!result;}
    bool BuyUpgrade(int i)
    {
        if(i<0||i>=2||!CanImprove()||UpgradeOwned(i)||data.money<UpgradeCosts[i])return false;
        RecordLedger(3,UpgradeCosts[i]);data.money-=UpgradeCosts[i];if(i==0)data.comfortUpgrade=true;else data.prepUpgrade=true;
        SyncUpgradeArt();data.journal.Add("第 "+data.day+" 天 · 茶屋升級："+UpgradeNames[i]);Notify(UpgradeNames[i]+"已啟用 · 永久有效",5);Play(chime);Save(false);return true;
    }
    int BatchCount(int dish)
    {
        if(dish<0||dish>=DishNames.Length||!RecipeUnlocked(dish)||!OnMenu(dish))return 0;
        NormalizePlanning();int n=Mathf.Clamp(data.prepTargets[dish]-Stock(dish),0,5);
        if(dish==0)n=Mathf.Min(n,data.leaves/2);
        else if(dish==1)n=Mathf.Min(n,Mathf.Min(data.mushrooms,data.bamboo));
        else if(dish==2)n=Mathf.Min(n,data.fish);
        else if(dish==3)n=Mathf.Min(n,Mathf.Min(data.mushrooms/2,data.leaves));
        else if(dish==4)n=Mathf.Min(n,Mathf.Min(data.leaves,data.bamboo));
        else if(dish==5)n=Mathf.Min(n,Mathf.Min(data.chestnuts/2,data.mushrooms));
        else n=Mathf.Min(n,Mathf.Min(data.berries/2,data.leaves));
        return Mathf.Max(0,n);
    }
    int MakeBatch(int dish)
    {
        if(!data.prepUpgrade||!CanImprove())return 0;
        int count=BatchCount(dish),made=0;
        for(int i=0;i<count;i++)if(Craft(dish))made++;else break;
        if(made>0){Notify("備好 "+DishNames[dish]+" ×"+made+"（一般品質）",5);Save(false);}return made;
    }
    void DrawTeaUpgrades()
    {
        Text(190,239,1040,60,"茶屋便利 · 一次購買，永久有效。白天回料理台附近升級。",heading);
        for(int i=0;i<2;i++)
        {
            float x=190+i*535;Panel(new Rect(x,315,510,330));TeaSeal(new Rect(x+400,339,64,64));
            Text(x+25,342,365,40,UpgradeNames[i],heading);
            Text(x+25,402,455,125,i==0?"候餐耐心下降速度減少 25%。\n從每秒 0.4 降至 0.3；新舊客人立即生效。\n不增加小費，不影響享用與聊天時間。":"按菜單的備餐目標，一次備好最多五份。\n只製作上架且已解鎖的料理，材料照常扣除。\n皆為一般品質；精品仍需手動掌握火候。",small);
            Text(x+25,538,455,35,UpgradeOwned(i)?"已擁有 · 效果啟用中":"一次性費用："+UpgradeCosts[i]+" 文 · 無維護費",heading);
            if(Button(x+25,585,455,UpgradeOwned(i)?"已升級":"購買 · "+UpgradeCosts[i]+" 文",CanImprove()&&!UpgradeOwned(i)&&data.money>=UpgradeCosts[i]))BuyUpgrade(i);
        }
        Text(190,681,1040,90,"備餐手冊用法：每日菜單設定目標，再點「備 N 份」。材料不足時只做得出的份數。\n已達目標、未上架、營業中或端著托盤／便當時不會製作，也不會扣材料。\n原有庭院裝修仍是外觀選擇；兩項便利升級不影響七日手帖的庭院目標。",small);
        if(Button(190,750,340,"返回每日菜單"))planningTab=0;
    }
    void TestTeaUpgrades()
    {
        NewGame();modal=false;result=false;player.position=new Vector3(-12,0,-3);
        Assert(!data.comfortUpgrade&&!data.prepUpgrade&&PatienceRate()==.4f,"legacy and new saves have no purchased upgrades");
        Assert(!BuyUpgrade(0)&&data.money==40,"insufficient funds leave ownership and money unchanged");data.money=500;
        player.position=new Vector3(0,0,-12);Assert(!BuyUpgrade(0)&&data.money==500,"upgrade purchase requires tea house proximity");player.position=new Vector3(-12,0,-3);
        data.night=true;Assert(!BuyUpgrade(0),"cannot purchase during service");data.night=false;data.holding=true;Assert(!BuyUpgrade(0),"tray blocks purchase");data.holding=false;
        Assert(!BuyUpgrade(-1)&&!BuyUpgrade(2),"invalid upgrade IDs cannot charge");
        Assert(BuyUpgrade(0)&&BuyUpgrade(1)&&data.money==230&&PatienceRate()==.3f,"two upgrades charge once and apply effects");
        Assert(!BuyUpgrade(0)&&!BuyUpgrade(1)&&data.money==230,"owned upgrades cannot be bought twice");
        data.guests.Add(new Guest(Friends[0],0){state=1});TickCafe(10,true);Assert(Mathf.Abs(data.guests[0].patience-97)<.001f,"comfort reduces real waiting decay by 25 percent");data.guests.Clear();
        NormalizePlanning();data.prepTargets[0]=9;data.leaves=20;data.tea=1;data.qualityTea=1;int crafted=data.weekCraft;
        Assert(MakeBatch(0)==5&&data.tea==6&&data.leaves==10&&data.qualityTea==1&&data.weekCraft==crafted+5,"batch cap consumes normal ingredients preserves premium and counts recipes");
        Assert(MakeBatch(0)==3&&data.tea==9&&data.leaves==4,"batch stops exactly at preparation target");
        Assert(MakeBatch(0)==0&&data.leaves==4,"reached target consumes nothing");
        data.prepTargets[1]=5;data.mushrooms=2;data.bamboo=1;Assert(MakeBatch(1)==1&&data.meal==1&&data.mushrooms==1&&data.bamboo==0,"batch limited by shared recipe ingredients");
        SetMenu(0,false);data.prepTargets[0]=12;Assert(MakeBatch(0)==0&&data.leaves==4,"off menu batch does not consume");SetMenu(0,true);
        Assert(MakeBatch(3)==0&&MakeBatch(-1)==0&&MakeBatch(7)==0,"locked and invalid batch recipes rejected");
        data.lunchState=2;Assert(MakeBatch(0)==0,"packed lunch blocks batch");data.lunchState=0;data.night=true;Assert(MakeBatch(0)==0,"night batch disabled");data.night=false;
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.prepUpgrade&&data.comfortUpgrade&&data.money==230&&data.tea==9&&data.qualityTea==1,"upgrade ownership stock quality and cash persist");
        NextDay();modal=false;Assert(data.prepUpgrade&&data.comfortUpgrade&&PatienceRate()==.3f,"upgrades survive next morning without maintenance charge");
        data=JsonUtility.FromJson<SaveData>("{\"version\":1,\"day\":10,\"money\":88,\"restored\":true}");Assert(!data.prepUpgrade&&!data.comfortUpgrade&&data.money==88&&data.restored,"legacy progress retained with upgrades initially unowned");
        Debug.Log("QA UPGRADE PASS: cost, distance, service gates, once-only ownership, waiting decay, batch caps, targets, ingredients, quality, persistence and legacy defaults.");
    }
}
