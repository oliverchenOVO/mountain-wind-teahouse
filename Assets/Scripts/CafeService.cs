using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    bool brewing; int brewDish,storyGuest=-1; float brewTimer;
    float BrewPosition {get{return Mathf.PingPong(brewTimer*.42f,1);}}
    float PerfectStart {get{return Friendship(1).stage==3?.48f:.55f;}}
    float PerfectEnd {get{return Friendship(1).stage==3?.85f:.78f;}}
    int Quality(int d){return d==0?data.qualityTea:d==1?data.qualityMeal:d==2?data.qualityFish:d==3?data.qualitySoup:d==4?data.qualityLeafTea:d==5?data.qualityChestnutRice:data.qualityBerryTea;}
    void ChangeStock(int d,int amount){if(d==0)data.tea+=amount;else if(d==1)data.meal+=amount;else if(d==2)data.grilled+=amount;else if(d==3)data.soup+=amount;else if(d==4)data.leafTea+=amount;else if(d==5)data.chestnutRice+=amount;else data.berryTea+=amount;}
    void ChangeQuality(int d,int amount){if(d==0)data.qualityTea+=amount;else if(d==1)data.qualityMeal+=amount;else if(d==2)data.qualityFish+=amount;else if(d==3)data.qualitySoup+=amount;else if(d==4)data.qualityLeafTea+=amount;else if(d==5)data.qualityChestnutRice+=amount;else data.qualityBerryTea+=amount;}
    void MigrateCafe()
    {
        if(data.journal==null)data.journal=new List<string>();
        if(data.serviceRevision<3){foreach(var g in data.guests)g.state=g.done?5:1;data.holding=false;data.serviceRevision=3;}
        if(data.holding&&(data.heldGuest<0||data.heldGuest>=data.guests.Count))ReturnTray();
        playerMotion.Carrying=data.holding;
    }
    void BeginBrew(int d)
    {
        if(d<0||d>=DishNames.Length||!RecipeUnlocked(d)){Notify("這道食譜尚未解鎖。");return;}
        if(data.holding||data.lunchState==2){Notify("先送餐，或把托盤／便當放回料理台。");return;}
        if(!Ingredients(d)){Notify("材料不足，白天去山中找找。");return;}
        brewDish=d;brewTimer=0;brewing=true;modal=false;
    }
    void FinishBrew()
    {
        bool perfect=BrewPosition>=PerfectStart&&BrewPosition<=PerfectEnd;
        brewing=false;
        if(Craft(brewDish)){if(perfect)ChangeQuality(brewDish,1);Notify((perfect?"恰到好處！精品 ":"製作完成：")+DishNames[brewDish]+(perfect?" · 客人額外付 10 文":""),5);Save(false);}
    }
    bool PickTray(int guest,int dish)
    {
        if(data.holding||data.lunchState==2||!OnMenu(dish)||guest<0||guest>=data.guests.Count||data.guests[guest].state!=1||data.guests[guest].done||Stock(dish)<1)return false;
        data.holding=true;data.heldGuest=guest;data.heldDish=dish;data.heldPerfect=Quality(dish)>0;
        ChangeStock(dish,-1);if(data.heldPerfect)ChangeQuality(dish,-1);
        playerMotion.Carrying=true;Notify("端著"+DishNames[dish]+"，走到"+data.guests[guest].name+"桌邊按 E。",6);Save(false);return true;
    }
    bool DeliverTray(bool test=false)
    {
        if(!data.holding)return false;
        int index=data.heldGuest;
        if(index<0||index>=data.guests.Count||data.guests[index].state!=1){ReturnTray();return false;}
        Vector3 seat=new Vector3(-16+index*3,0,-9.2f);
        if(!test&&Vector3.Distance(player.position,seat)>2.6f){Notify("走到客人的桌邊，再按 E 送餐。");return false;}
        var g=data.guests[index];g.state=2;g.stageTime=0;g.dish=data.heldDish;
        g.perfect=data.heldPerfect;
        g.dailyBonus=DishBonus(g.dish);
        g.reward=Prices[g.dish]+(g.order==g.dish?15:0)+(data.heldPerfect?10:0)+(g.dish==0&&Friendship(0).stage==3?5:0)+g.dailyBonus;
        data.holding=false;playerMotion.Carrying=false;playerMotion.Gesture=1;
        if(index<visitors.Count&&visitors[index])visitors[index].GetComponent<AvatarMotion>().SetMeal(g.dish,true);
        ValleyAtmosphere.Meal(index,g.dish);Notify(g.name+"："+(g.order==g.dish?"正是我想吃的！":"謝謝，這個也很好吃。"));Save(false);return true;
    }
    void ReturnTray()
    {
        if(!data.holding)return;
        ChangeStock(data.heldDish,1);if(data.heldPerfect)ChangeQuality(data.heldDish,1);
        data.holding=false;if(playerMotion)playerMotion.Carrying=false;Save(false);
    }
    void PayGuest(Guest g)
    {
        if(g.paid)return;RecordLedger(0,g.reward);NormalizeWeek();data.weekServed++;g.paid=true;data.money+=g.reward;data.served++;
        if(data.night){NormalizePlanning();data.nightIncome+=g.reward;data.nightTips+=Mathf.Max(0,g.reward-Prices[g.dish]);data.nightSpecialIncome+=g.dailyBonus;data.sales[g.dish]++;}
        string news=g.name+"："+MountainNews(g);AddFriendship(g);
        string record="第 "+data.day+" 天 · "+news;
        if(!data.journal.Contains(record))data.journal.Add(record);
        while(data.journal.Count>30)data.journal.RemoveAt(0);
    }
    void FinishStory()
    {
        if(storyGuest<0||storyGuest>=data.guests.Count)return;
        var g=data.guests[storyGuest];PayGuest(g);g.state=4;g.stageTime=0;
        if(storyGuest<visitors.Count&&visitors[storyGuest]){var m=visitors[storyGuest].GetComponent<AvatarMotion>();m.Dining=false;m.DepartAfterMeal(0,new Vector3(-5,0,-4));}
        storyGuest=-1;modal=false;speaker="";int friend=FriendIndex(g.name);
        Notify("餐費與小費 ＋"+g.reward+" 文"+(friend>=0?" · "+g.name+" 好感 "+Friendship(friend).affection+" / 12":""),5);Play(chime);Save(false);
    }
    void TickCafe(float dt,bool test=false)
    {
        if(data.guests.Count==0)return;
        bool all=true;
        for(int i=0;i<data.guests.Count;i++)
        {
            var g=data.guests[i];if(g.done)continue;all=false;
            g.stageTime+=dt;
            if(g.state==0&&(test||(i<visitors.Count&&visitors[i]&&visitors[i].GetComponent<AvatarMotion>().Seated))){g.state=1;g.stageTime=0;Notify(g.name+"：請給我一份"+DishNames[g.order]+"。");}
            else if(g.state==1)
            {
                if(!data.holding||data.heldGuest!=i)g.patience-=dt*PatienceRate();
                if(g.patience<=0){g.state=5;g.done=true;data.lost++;ShowVisitors();Save(false);}
            }
            else if(g.state==2&&g.stageTime>=6){g.state=3;g.stageTime=0;}
            else if(g.state==3&&storyGuest<0)
            {
                storyGuest=i;
                Say(g.name,MealConversation(g));
            }
            else if(g.state==4&&(test||g.stageTime>6)){g.done=true;g.state=5;ShowVisitors();}
        }
        if(all)
        {
            if(data.wave==0){data.wave=1;NewWave();Notify("第二批客人到了！");}
            else{data.guests.Clear();FinishNightReport();}
            Save(false);
        }
    }
    void DrawCafeOverlay()
    {
        if(data.holding&&!modal&&!notebook&&!paused&&!relationships&&!planning)
        {
            // The context card and ground ring identify the actual delivery target.
            if(Button(960,760,455,"放回料理台",Vector3.Distance(player.position,new Vector3(-12,0,-2.2f))<4))ReturnTray();
        }
        if(!brewing)return;
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.3f));Panel(new Rect(420,350,600,230));
        Text(450,375,540,40,"料理火候 · "+DishNames[brewDish],heading);
        Box(new Rect(450,438,540,24),sage);Box(new Rect(450+540*PerfectStart,438,540*(PerfectEnd-PerfectStart),24),gold);
        Box(new Rect(450+BrewPosition*540-4,430,8,40),ink);
        Text(450,480,540,32,"金色區域按 Space · 精品料理額外 ＋10 文",small);
        if(Button(450,525,260,"完成料理 [Space]"))FinishBrew();
        if(Button(730,525,260,"取消 [Esc]")){brewing=false;Notify("材料沒有消耗。");}
    }
    void TestFinishWave()
    {
        TickCafe(7,true);TickCafe(0,true);
        for(int i=0;i<3;i++){if(storyGuest<0)TickCafe(0,true);FinishStory();TickCafe(0,true);}
        TickCafe(7,true);TickCafe(0,true);
    }
}
