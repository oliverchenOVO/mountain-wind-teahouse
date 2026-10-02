using UnityEngine;
using System;

public partial class MountainTeaGame
{
    bool festivalDialogue;GameObject festivalArt,festivalKeepsake;
    static readonly string[] FestivalFriends={"射命丸文","河城荷取","犬走椛"};
    bool IsFestivalDay {get{return data.day%7==0;}}
    int NextFestivalDay {get{return data.day+(7-data.day%7)%7;}}
    void NormalizeFestival()
    {
        if(data.festivalShared==null)data.festivalShared=new bool[3];
        else if(data.festivalShared.Length!=3)Array.Resize(ref data.festivalShared,3);
    }
    int FestivalCount(){NormalizeFestival();int count=0;foreach(bool shared in data.festivalShared)if(shared)count++;return count;}
    void BuildFestival()
    {
        festivalArt=new GameObject("Seven day mountain festival");
        Color red=new Color(.82f,.32f,.25f),amber=new Color(.96f,.71f,.33f);
        for(int side=0;side<2;side++)
        {
            float z=side==0?-5.8f:-11.8f;
            for(int end=0;end<2;end++)TeaHouseWorld.Shape("Festival bamboo post",PrimitiveType.Cylinder,new Vector3(end==0?-19:-6,2.2f,z),new Vector3(.11f,2.2f,.11f),sage,festivalArt.transform);
            TeaHouseWorld.Shape("Festival hanging cord",PrimitiveType.Cube,new Vector3(-12.5f,4.35f,z),new Vector3(13,.035f,.035f),ink,festivalArt.transform);
            for(int i=0;i<9;i++)
            {
                float x=-18+i*1.38f;
                TeaHouseWorld.Shape("Festival lantern",PrimitiveType.Sphere,new Vector3(x,3.92f,z),new Vector3(.55f,.67f,.55f),i%2==0?red:amber,festivalArt.transform);
                for(int cap=0;cap<2;cap++)TeaHouseWorld.Shape("Lantern cap",PrimitiveType.Cylinder,new Vector3(x,3.6f+cap*.64f,z),new Vector3(.27f,.04f,.27f),ink,festivalArt.transform);
                TeaHouseWorld.Shape("Lantern tassel",PrimitiveType.Cube,new Vector3(x,3.37f,z),new Vector3(.035f,.28f,.035f),amber,festivalArt.transform);
            }
        }
        festivalKeepsake=new GameObject("Mountain festival keepsake");
        TeaHouseWorld.Shape("Festival memory plaque",PrimitiveType.Cube,new Vector3(-7.2f,1.35f,-2.8f),new Vector3(.8f,1,.14f),new Color(.53f,.32f,.2f),festivalKeepsake.transform);
        for(int i=0;i<3;i++)TeaHouseWorld.Shape("Three friends memory",PrimitiveType.Sphere,new Vector3(-7.43f+i*.23f,1.42f,-2.91f),new Vector3(.17f,.28f,.08f),i==1?sage:amber,festivalKeepsake.transform);
        SyncFestivalArt();
    }
    void SyncFestivalArt()
    {
        if(festivalArt)festivalArt.SetActive(started&&IsFestivalDay&&!data.onTrail);
        if(festivalKeepsake)festivalKeepsake.SetActive(started&&data.festivalRewardClaimed&&!data.onTrail);
    }
    bool CanShareFestival(int friend)
    {
        NormalizeFestival();return friend>=0&&friend<3&&started&&IsFestivalDay&&!data.festivalShared[friend]&&Stock(friend)>0&&!data.night&&NearTea()&&!modal&&!paused&&!result&&!notebook&&!travelBook&&!planning&&!relationships&&!trial&&!fishing&&!brewing&&!data.holding&&data.lunchState!=2;
    }
    bool ShareFestival(int friend)
    {
        if(!CanShareFestival(friend))return false;
        bool premium=Quality(friend)>=Stock(friend);ChangeStock(friend,-1);if(premium)ChangeQuality(friend,-1);
        data.festivalShared[friend]=true;bool finished=FestivalCount()==3&&!data.festivalRewardClaimed;
        if(finished){RecordLedger(2,60);data.festivalRewardClaimed=true;data.money+=60;}
        string[] lines={"今天不追新聞，先替這杯茶留個位置。\n山風把燈籠吹得輕輕搖晃，這就是今天的頭條。\n下一次小祭典，也讓我坐在這裡吧。", "這碗菇飯，讓我想起剛修好水車的那一天。\n機器偶爾也該停一下，人也是。\n能和大家一起吃飯，比再多做一個零件還開心。", "巡山的路我已經確認過了，今天可以慢慢吃。\n山裡的小聚會不需要很熱鬧。\n有一盞燈、有熟悉的人，就足夠了。"};
        data.journal.Add("第 "+data.day+" 天 · 山中小祭典 · 與"+FestivalFriends[friend]+"分享"+DishNames[friend]);
        if(finished)data.journal.Add("山中小祭典完成 · 三友紀念牌與謝禮 60 文");
        Say(FestivalFriends[friend],lines[friend]+"\n\n"+DishNames[friend]+" −1 · 聚會 "+FestivalCount()+" / 3"+(finished?"\n三友紀念牌已掛起 · 謝禮 ＋60 文":"\n未完成的邀請可留到下次祭典。"));
        festivalDialogue=true;SetDialogueMood(1);SyncFestivalArt();Play(chime);Save(false);return true;
    }
    void DrawFestival()
    {
        NormalizeFestival();Text(190,240,1050,40,"山中小祭典 · "+FestivalCount()+" / 3",heading);
        Text(190,287,1050,82,(IsFestivalDay?"今天是小祭典日！白天回茶屋料理台附近參加。":"下次小祭典：第 "+NextFestivalDay+" 天（還有 "+(NextFestivalDay-data.day)+" 天）")+"\n每七天一次，雨天照常。每位友人分享一份料理；完成三次邀請後獲得紀念牌與 60 文。",small);
        for(int i=0;i<3;i++)
        {
            float x=190+i*354;Panel(new Rect(x,385,335,270));Portrait(FestivalFriends[i],new Rect(x+20,405,64,78));
            Text(x+99,415,220,35,FestivalFriends[i],heading);Text(x+99,455,220,30,data.festivalShared[i]?"已留下聚會回憶":"等待邀請",small);
            Text(x+20,505,295,65,DishNames[i]+" ×1 · 庫存 "+Stock(i)+"\n"+Recipes[i],small);
            bool ready=started&&IsFestivalDay&&!data.night&&NearTea()&&!data.holding&&data.lunchState!=2&&!data.festivalShared[i]&&Stock(i)>0;
            if(Button(x+20,590,295,data.festivalShared[i]?"已分享 ✓":"分享料理",ready)){planning=false;if(!ShareFestival(i))planning=true;}
        }
        Text(190,686,1050,70,"錯過不失敗，邀請進度跨次保留；不影響夜間營業或友人故事。\n三份料理與謝禮只計一次；完成後仍會每七天掛起燈籠，不重複發獎勵。",small);
        if(Button(190,750,340,"去每日菜單備餐"))SelectPlanningTab(0);
    }
    void DrawFestivalHUD()
    {
        if(IsFestivalDay&&!data.night&&Button(980,455,435,"山中小祭典 · "+FestivalCount()+" / 3 · 查看邀請"))OpenPlanning(5);
    }
    void TestFestival()
    {
        Load(System.IO.Path.Combine(qaDir,"legacy-player-save.json"));Assert(FestivalCount()==0&&!data.festivalRewardClaimed,"real legacy save receives empty festival progress");
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.tea=data.meal=data.grilled=2;
        Assert(FestivalCount()==0&&NextFestivalDay==7&&!ShareFestival(0),"festival legacy default and predictable first date");
        data.day=7;SyncFestivalArt();Assert(festivalArt.activeSelf&&CanShareFestival(0),"seventh day festival decorations and invitation available");
        SyncWeatherPeople();Assert(spots[1].pos.x<0&&spots[2].pos.x<0&&spots[3].pos.x<0,"festival friends gather at tea house");
        data.night=true;Assert(!ShareFestival(0),"night service blocks festival");data.night=false;
        paused=true;Assert(!ShareFestival(0),"pause blocks festival");paused=false;result=true;Assert(!ShareFestival(0),"result overlay blocks festival");result=false;
        notebook=true;Assert(!ShareFestival(0),"notebook blocks festival");notebook=false;travelBook=true;Assert(!ShareFestival(0),"travel book blocks festival");travelBook=false;
        relationships=true;Assert(!ShareFestival(0),"relationship overlay blocks festival");relationships=false;trial=true;Assert(!ShareFestival(0),"trial blocks festival");trial=false;
        fishing=true;Assert(!ShareFestival(0),"fishing blocks festival");fishing=false;brewing=true;Assert(!ShareFestival(0),"brewing blocks festival");brewing=false;
        Assert(!ShareFestival(-1)&&!ShareFestival(3),"invalid festival invite cannot change stock");
        player.position=Vector3.zero;Assert(!ShareFestival(0),"remote festival sharing blocked");player.position=new Vector3(-12,0,-3);
        data.holding=true;Assert(!ShareFestival(0),"tray blocks festival");data.holding=false;data.lunchState=2;Assert(!ShareFestival(0),"packed lunch blocks festival");data.lunchState=0;
        planning=true;Assert(!ShareFestival(0),"planning cannot bypass festival transaction gates");planning=false;
        int cash=data.money;data.qualityTea=2;Assert(ShareFestival(0)&&data.tea==1&&data.qualityTea==1&&festivalDialogue&&data.money==cash,"festival consumes one premium tea without premature reward");
        Assert(!ShareFestival(1),"festival dialogue blocks another invitation");modal=false;Assert(!ShareFestival(0),"festival invite cannot repeat");
        NextDay();modal=false;SyncFestivalArt();Assert(FestivalCount()==1&&!festivalArt.activeSelf&&NextFestivalDay==14&&!ShareFestival(1),"festival progress survives missed date with no penalty");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(FestivalCount()==1&&!festivalDialogue,"festival progress loads and transient dialogue clears");
        data.day=14;player.position=new Vector3(-12,0,-3);data.meal=0;Assert(!ShareFestival(1),"missing festival dish does not consume progress");data.meal=2;
        Assert(ShareFestival(1)&&data.meal==1,"second festival can continue invitation");modal=false;
        data.day=21;Assert(IsRainDay&&ShareFestival(2)&&data.grilled==1&&data.money==cash+60&&data.festivalRewardClaimed&&festivalKeepsake.activeSelf,"rain festival finishes with exactly one reward and keepsake");modal=false;
        for(int i=0;i<3;i++)Assert(!ShareFestival(i),"completed festival cannot duplicate dish or reward");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(FestivalCount()==3&&data.festivalRewardClaimed&&data.money==cash+60,"festival completion and reward persist");
        data.day=22;SyncFestivalArt();Assert(!festivalArt.activeSelf&&festivalKeepsake.activeSelf,"keepsake remains after festival");
        Assert(festivalArt.GetComponentsInChildren<Collider>(true).Length==0&&festivalKeepsake.GetComponentsInChildren<Collider>(true).Length==0,"festival art adds no collisions");
        NewGame();modal=false;SyncFestivalArt();Assert(FestivalCount()==0&&!data.festivalRewardClaimed&&!festivalKeepsake.activeSelf,"new journey clears festival without changing player save");
        Debug.Log("QA FESTIVAL PASS: dates, invitations, stock, gates, rain, persistence, bounded reward, decorations.");
    }
}
