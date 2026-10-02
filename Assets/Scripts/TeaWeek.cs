using UnityEngine;
using System;

public partial class MountainTeaGame
{
    static readonly string[] WeekTitles={"第一頁 · 茶香初起","第二頁 · 溪谷的鄰居","第三頁 · 聽雨泡茶","第四頁 · 巡山的午飯","第五頁 · 留下山中故事","第六頁 · 喜歡的庭院","第七頁 · 山風常在"};
    void NormalizeWeek()
    {
        if(data.weekStartDay<1)data.weekStartDay=data.day;
        if(data.weekClaimed==null)data.weekClaimed=new bool[7];
        else if(data.weekClaimed.Length!=7)Array.Resize(ref data.weekClaimed,7);
        data.weekHarvest=Mathf.Max(0,data.weekHarvest);data.weekCraft=Mathf.Max(0,data.weekCraft);data.weekServed=Mathf.Max(0,data.weekServed);
    }
    int WeekPage(){NormalizeWeek();return Mathf.Clamp(data.day-data.weekStartDay,0,6);}
    bool WeekGoal(int page,int goal)
    {
        NormalizeWeek();
        if(page==0)return goal==0?data.weekHarvest>=3:data.weekCraft>=2;
        if(page==1)return goal==0?data.questDone:data.weekServed>=3;
        if(page==2)return goal==0?data.rainStoryStage>=1:data.weekCraft>=5;
        if(page==3)return goal==0?data.deliveries>=1:data.lookoutVisited;
        if(page==4)return goal==0?data.notebookStage>=6:Friendship(0).stage+Friendship(1).stage+Friendship(2).stage>=1;
        if(page==5){if(goal==1)return data.weekServed>=9;NormalizePlanning();foreach(int owned in data.gardenOwned)if(owned!=0)return true;return false;}
        return goal==0?data.restored:data.rainStoryStage>=3;
    }
    string WeekGoalText(int page,int goal)
    {
        string[][] text={new[]{"採集三處食材（"+Mathf.Min(3,data.weekHarvest)+"/3）","製作兩份料理（"+Mathf.Min(2,data.weekCraft)+"/2）"},
            new[]{"完成荷取的水車委託","招待三位客人（"+Mathf.Min(3,data.weekServed)+"/3）"},
            new[]{"送出第一杯避雨茶","累計製作五份料理（"+Mathf.Min(5,data.weekCraft)+"/5）"},
            new[]{"送達一份巡山便當","拜訪瀑布觀景台"},new[]{"完成巡山筆記後日談","完成任一友人的第一段故事"},
            new[]{"購買任一庭院裝修款式","累計招待九位客人（"+Mathf.Min(9,data.weekServed)+"/9）"},new[]{"修繕茶屋招牌","完成三段避雨茶故事"}};
        return (WeekGoal(page,goal)?"完成 · ":"待辦 · ")+text[page][goal];
    }
    bool ClaimWeek(int page)
    {
        NormalizeWeek();if(page<0||page>=7||page>WeekPage()||data.weekClaimed[page]||!WeekGoal(page,0)||!WeekGoal(page,1))return false;
        RecordLedger(2,30);data.weekClaimed[page]=true;data.money+=30;
        data.journal.Add("第 "+data.day+" 天 · 七日手帖："+WeekTitles[page]+" · 謝禮 30 文");
        Notify("手帖留下一枚茶印 · ＋30 文",5);Play(chime);Save(false);return true;
    }
    int WeekStamps(){NormalizeWeek();int n=0;foreach(bool claimed in data.weekClaimed)if(claimed)n++;return n;}
    void DrawTeaWeek()
    {
        NormalizeWeek();Text(190,235,1030,50,"七日手帖 · "+WeekStamps()+" / 7 枚茶印　從第 "+data.weekStartDay+" 天開始",heading);
        Text(190,280,1030,40,"每天解鎖一頁，不限期限、不必依序完成。每頁謝禮 30 文，可隨時補做。",small);
        for(int p=0;p<7;p++)
        {
            float y=330+p*56;bool unlocked=p<=WeekPage();
            TeaTag(new Rect(190,y,270,40),WeekTitles[p],data.weekClaimed[p]?sage:ink);
            Text(480,y,550,50,unlocked?WeekGoalText(p,0)+"　／　"+WeekGoalText(p,1):"第 "+(data.weekStartDay+p)+" 天開啟 · 可以先自由探索",small);
            if(Button(1080,y,160,data.weekClaimed[p]?"已蓋茶印":unlocked?"領取茶印":"尚未開啟",unlocked&&!data.weekClaimed[p]&&WeekGoal(p,0)&&WeekGoal(p,1)))ClaimWeek(p);
        }
        Text(190,738,1050,55,"採集、製作、招待從這份手帖開始累計；修繕與故事沿用原有成果。\n需要指引：手帳看地圖與友人故事，旅行手帳看便當與筆記，雨天回茶屋泡茶。",small);
    }
    void TestTeaWeek()
    {
        NewGame();modal=false;NormalizeWeek();Assert(data.weekStartDay==1&&WeekStamps()==0,"week begins without stamps");
        int cash=data.money;Assert(!ClaimWeek(0)&&!ClaimWeek(-1)&&!ClaimWeek(7)&&data.money==cash,"invalid and incomplete claims leave money unchanged");
        for(int i=0;i<3;i++)Interact(spots.Find(s=>s.kind==i));data.leaves=10;Craft(0);Craft(0);
        Assert(data.weekHarvest==3&&data.weekCraft==2&&ClaimWeek(0)&&data.money==cash+30,"real forage and crafting award first stamp");
        Assert(!ClaimWeek(0)&&data.money==cash+30,"stamp cannot pay twice");
        data.questDone=true;var guest=new Guest(Friends[0],0){reward=1};PayGuest(guest);PayGuest(guest);PayGuest(new Guest(Friends[1],0){reward=1});PayGuest(new Guest(Friends[2],0){reward=1});
        Assert(data.weekServed==3,"paid meals count once per guest");data.fish=0;Assert(!Craft(2)&&data.weekCraft==2,"failed recipes do not count");
        Assert(!ClaimWeek(1),"future page remains locked despite completed goals");
        NextDay();modal=false;Assert(ClaimWeek(1),"next morning unlocks page and retains counters");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(WeekStamps()==2&&data.weekStartDay==1&&!ClaimWeek(1),"week counters and claimed rewards persist");
        data.day=20;data.weekCraft=5;data.weekServed=9;data.rainStoryStage=3;data.deliveries=1;data.lookoutVisited=true;data.notebookStage=6;Friendship(0).stage=1;data.gardenOwned[0]=2;data.restored=true;
        cash=data.money;for(int p=2;p<7;p++)Assert(ClaimWeek(p),"overdue optional pages remain claimable");Assert(WeekStamps()==7&&data.money==cash+150,"all seven stamps have bounded reward");
        data=JsonUtility.FromJson<SaveData>("{\"version\":1,\"day\":12,\"money\":99,\"restored\":true}");NormalizeWeek();
        Assert(data.weekStartDay==12&&WeekPage()==0&&WeekStamps()==0&&data.money==99&&data.restored,"legacy save starts a fresh week without changing existing progress");
        Debug.Log("QA WEEK PASS: actual actions, unlocks, rewards, duplicate protection, no deadline, persistence, legacy initialization.");
    }
}
