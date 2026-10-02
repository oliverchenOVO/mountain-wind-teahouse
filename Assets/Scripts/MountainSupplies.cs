using UnityEngine;
using System;

public partial class MountainTeaGame
{
    static readonly string[] SupplyNames={"山茶葉","野生香菇","嫩竹筍","溪魚","山栗","野莓"};
    static readonly int[] SupplyPrices={8,12,10,18,14,12};
    static readonly int[] SupplyLimits={6,4,4,3,4,4};
    GameObject supplyCart;
    void NormalizeSupplies()
    {
        if(data.supplyDay!=data.day){data.supplyDay=data.day;data.supplyBought=new int[6];data.supplySpent=0;}
        if(data.supplyBought==null)data.supplyBought=new int[6];
        else if(data.supplyBought.Length!=6)Array.Resize(ref data.supplyBought,6);
        for(int i=0;i<6;i++)data.supplyBought[i]=Mathf.Clamp(data.supplyBought[i],0,SupplyLimits[i]);
        data.supplySpent=Mathf.Max(0,data.supplySpent);
    }
    int SupplyHeld(int i){return i==0?data.leaves:i==1?data.mushrooms:i==2?data.bamboo:i==3?data.fish:i==4?data.chestnuts:data.berries;}
    int SupplyLeft(int i){NormalizeSupplies();return i>=0&&i<6?SupplyLimits[i]-data.supplyBought[i]:0;}
    bool SupplyUnlocked(int i){return i>=0&&i<6&&(i<4||data.trailRecipes);}
    bool CanTradeSupplies()
    {
        return started&&!data.night&&NearTea()&&!data.holding&&data.lunchState!=2&&!modal&&!paused&&!settingsOpen&&!result&&!notebook&&!travelBook&&!relationships&&!trial&&!fishing&&!brewing&&(!planning||planningTab==6);
    }
    bool CanBuySupply(int i,int count)
    {
        return SupplyUnlocked(i)&&(count==1||count==3)&&CanTradeSupplies()&&SupplyLeft(i)>=count&&data.money>=SupplyPrices[i]*count&&SupplyHeld(i)<=int.MaxValue-count;
    }
    bool BuySupply(int i,int count)
    {
        if(!CanBuySupply(i,count))return false;
        int cost=SupplyPrices[i]*count;data.money-=cost;data.supplyBought[i]+=count;data.supplySpent+=cost;
        if(i==0)data.leaves+=count;else if(i==1)data.mushrooms+=count;else if(i==2)data.bamboo+=count;else if(i==3)data.fish+=count;else if(i==4)data.chestnuts+=count;else data.berries+=count;
        data.journal.Add("第 "+data.day+" 天 · 雜貨補給："+SupplyNames[i]+" ×"+count+" · 支出 "+cost+" 文");
        Notify("購入 "+SupplyNames[i]+" ×"+count+" · −"+cost+" 文",4);Play(pickupSound);Save(false);return true;
    }
    string SupplyStatus()
    {
        if(data.night)return "夜間營業中 · 雜貨白天供應";
        if(!NearTea())return "白天回茶屋料理台附近購買";
        if(data.holding||data.lunchState==2)return "先放回托盤或拆開便當，再購買材料";
        return "白天補給中 · 每份單價固定，沒有自動扣款";
    }
    void DrawSupplies()
    {
        NormalizeSupplies();Text(190,239,1050,38,"山中雜貨 · 第 "+data.day+" 天 · 今日支出 "+data.supplySpent+" 文",heading);
        Text(190,282,1050,54,SupplyStatus()+"\n每日限量隔天補滿；同日讀檔、切換地圖或開關此頁不會補貨。",small);
        for(int i=0;i<6;i++)
        {
            float y=350+i*57;bool unlocked=SupplyUnlocked(i);int left=SupplyLeft(i);
            Text(195,y,215,35,SupplyNames[i],heading);
            Text(420,y+4,285,32,unlocked?"單價 "+SupplyPrices[i]+" 文 · 持有 "+SupplyHeld(i):"首次巡山便當送達後供應",small);
            Text(715,y+4,180,32,unlocked?"剩 "+left+" / "+SupplyLimits[i]+" 份":"山路補給未解鎖",small);
            if(Button(905,y,145,"買 1 · "+SupplyPrices[i]+" 文",CanBuySupply(i,1)))BuySupply(i,1);
            if(Button(1065,y,175,"買 3 · "+SupplyPrices[i]*3+" 文",CanBuySupply(i,3)))BuySupply(i,3);
        }
        Text(190,704,1050,40,"只買材料，不出售成品或回收物品；不算採集茶印，也不改變料理品質與夜間報表。",small);
        if(Button(190,750,340,"返回每日菜單備餐"))planningTab=0;
    }
    void BuildSupplyCart()
    {
        supplyCart=new GameObject("Mountain supply cart");Transform root=supplyCart.transform;Vector3 p=new Vector3(-18.8f,0,-3.5f);Color wood=new Color(.53f,.34f,.21f);
        TeaHouseWorld.Shape("Supply cart shelf",PrimitiveType.Cube,p+Vector3.up*.95f,new Vector3(1.65f,.16f,1.1f),wood,root);
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)TeaHouseWorld.Shape("Supply cart leg",PrimitiveType.Cube,p+new Vector3(x*.7f,.48f,z*.4f),new Vector3(.1f,.9f,.1f),wood,root);
        for(int i=0;i<3;i++)
        {
            Vector3 q=p+new Vector3((i-1)*.5f,1.13f,0);
            TeaHouseWorld.Shape("Supply crate",PrimitiveType.Cube,q,new Vector3(.43f,.2f,.7f),new Color(.7f,.51f,.31f),root);
            for(int j=0;j<3;j++)TeaHouseWorld.Shape("Supply produce",PrimitiveType.Sphere,q+new Vector3(0,.18f,(j-1)*.18f),new Vector3(.22f,.2f,.18f),i==0?sage:i==1?new Color(.72f,.37f,.25f):gold,root);
        }
        TeaHouseWorld.Shape("Supply price plaque",PrimitiveType.Cube,p+new Vector3(0,1.65f,.4f),new Vector3(1.25f,.55f,.1f),cream,root);
        for(int i=0;i<3;i++)TeaHouseWorld.Shape("Supply plaque leaf",PrimitiveType.Sphere,p+new Vector3((i-1)*.3f,1.65f,.33f),new Vector3(.18f,.28f,.05f),sage,root);
        SyncSupplyCart();
    }
    void SyncSupplyCart(){if(supplyCart)supplyCart.SetActive(started&&!data.onTrail);}
    void TestSupplies()
    {
        Load(System.IO.Path.Combine(qaDir,"legacy-player-save.json"));Assert(SupplyLeft(0)==6&&data.supplySpent==0,"legacy supplies default to full stock without expenses");
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.money=1000;int stamps=data.weekHarvest,journal=data.journal.Count;
        Assert(!BuySupply(-1,1)&&!BuySupply(6,1)&&!BuySupply(0,0)&&!BuySupply(0,-1)&&!BuySupply(0,2)&&!BuySupply(0,int.MaxValue),"invalid supply item and quantity rejected");
        Assert(!BuySupply(4,1)&&!BuySupply(5,1),"mountain supplies require first lunch delivery");
        player.position=Vector3.zero;Assert(!BuySupply(0,1),"remote supply purchase blocked");player.position=new Vector3(-12,0,-3);
        data.night=true;Assert(!BuySupply(0,1),"night blocks supplies");data.night=false;data.holding=true;Assert(!BuySupply(0,1),"tray blocks supplies");data.holding=false;
        data.lunchState=2;Assert(!BuySupply(0,1),"packed lunch blocks supplies");data.lunchState=0;modal=true;Assert(!BuySupply(0,1),"dialogue blocks supplies");modal=false;
        paused=true;Assert(!BuySupply(0,1),"pause blocks supplies");paused=false;settingsOpen=true;Assert(!BuySupply(0,1),"settings blocks supplies");settingsOpen=false;
        result=true;Assert(!BuySupply(0,1),"result blocks supplies");result=false;notebook=true;Assert(!BuySupply(0,1),"notebook blocks supplies");notebook=false;
        travelBook=true;Assert(!BuySupply(0,1),"travel book blocks supplies");travelBook=false;relationships=true;Assert(!BuySupply(0,1),"relationship screen blocks supplies");relationships=false;
        trial=true;Assert(!BuySupply(0,1),"trial blocks supplies");trial=false;fishing=true;Assert(!BuySupply(0,1),"fishing blocks supplies");fishing=false;brewing=true;Assert(!BuySupply(0,1),"brewing blocks supplies");brewing=false;
        planning=true;planningTab=0;Assert(!BuySupply(0,1),"other planning pages block supply transaction");planningTab=6;
        data.money=7;Assert(!BuySupply(0,1)&&data.leaves==0&&SupplyLeft(0)==6&&data.supplySpent==0&&data.journal.Count==journal,"insufficient funds cannot alter stock expenses or journal");data.money=1000;
        Assert(BuySupply(0,3)&&data.money==976&&data.leaves==3&&SupplyLeft(0)==3&&data.supplySpent==24,"three supply units charged and delivered exactly once");
        Assert(BuySupply(0,1)&&SupplyLeft(0)==2,"single supply unit purchase");int cash=data.money;
        Assert(!BuySupply(0,3)&&data.money==cash&&data.leaves==4,"batch requires all three units and never partially charges");
        Assert(BuySupply(0,1)&&BuySupply(0,1)&&!BuySupply(0,1)&&data.leaves==6,"daily supply cap enforced");
        data.trailRecipes=true;
        for(int i=1;i<6;i++){int before=SupplyHeld(i),cost=data.money;Assert(BuySupply(i,3)&&SupplyHeld(i)==before+3&&data.money==cost-SupplyPrices[i]*3,"each supply maps to correct material and price");}
        Assert(data.weekHarvest==stamps&&data.qualityTea==0&&data.qualityMeal==0&&data.nightIncome==0,"purchases do not award harvest stamps quality or service income");
        int spent=data.supplySpent;cash=data.money;Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));
        Assert(SupplyLeft(0)==0&&SupplyLeft(3)==0&&data.supplySpent==spent&&data.money==cash&&data.chestnuts==3&&data.berries==3,"same-day supply stock materials expense and cash persist");
        NormalizeSupplies();NormalizeSupplies();ChangeRegion(true);ChangeRegion(false);Assert(SupplyLeft(0)==0&&data.money==cash,"reopening and travel cannot refresh supply stock");
        NextDay();modal=false;Assert(SupplyLeft(0)==6&&SupplyLeft(3)==3&&data.supplySpent==0&&data.leaves==6&&data.money==cash,"next day restocks without charging or removing carried materials");
        player.position=new Vector3(-12,0,-3);Assert(BuySupply(0,1),"restocked supply can be purchased next morning");
        SyncSupplyCart();Assert(supplyCart.activeSelf&&supplyCart.GetComponentsInChildren<Collider>(true).Length==0,"supply cart visible without collisions");data.onTrail=true;SyncSupplyCart();Assert(!supplyCart.activeSelf,"supply cart hides on mountain trail");
        NewGame();modal=false;Assert(SupplyLeft(0)==6&&data.supplySpent==0&&data.money==40,"new journey resets supplies and leaves player save isolated");
        Debug.Log("QA SUPPLIES PASS: legacy, gates, prices, quantities, daily caps, recipes, accounting, persistence, travel, next day, art.");
    }
}
