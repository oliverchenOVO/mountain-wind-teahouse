using UnityEngine;
using System;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    [Serializable] public class Bond
    {
        public int affection,stage,lastEventDay,visits,gainDay,dailyGain;
    }
    static readonly string[] Friends={"射命丸文","河城荷取","犬走椛"};
    static readonly string[] ChapterNames={"風中的採訪","河童的茶爐","巡山便當"};
    static readonly string[] Gifts={"文的推薦：清茶售價 ＋5 文","河童茶爐：精品火候區加寬","巡山回禮：每天收到竹筍 ×2"};
    bool relationships;int eventFriend=-1;Vector2 journalScroll;
    int FriendIndex(string name){return Array.IndexOf(Friends,name);}
    Bond Friendship(int i)
    {
        if(data.bonds==null)data.bonds=new List<Bond>();
        while(data.bonds.Count<3)data.bonds.Add(new Bond());
        if(data.bonds[i]==null)data.bonds[i]=new Bond();
        return data.bonds[i];
    }
    void AddFriendship(Guest g)
    {
        int i=FriendIndex(g.name);if(i<0)return;var b=Friendship(i);
        if(b.gainDay!=data.day){b.gainDay=data.day;b.dailyGain=0;}
        int points=Mathf.Min(4-b.dailyGain,1+(g.dish==g.order?1:0)+(g.perfect?1:0));
        b.affection=Mathf.Min(12,b.affection+Mathf.Max(0,points));b.dailyGain+=Mathf.Max(0,points);b.visits++;
    }
    string MountainNews(Guest g)
    {
        int i=FriendIndex(g.name);if(i<0)return "山風今天也很溫柔。";
        var b=Friendship(i);
        string[][] lines={
            new[]{"明早山頂的風轉向，正適合採茶。","瀑布邊有人拍到了彩虹，我打算去看看。","比起大新聞，我也想寫寫山裡平凡的日常。"},
            new[]{"溪谷水車重新運轉，工坊又熱鬧起來了。","新的茶爐零件做好了，還缺一次實際測試。","工坊的朋友們開始問起你的茶屋了！"},
            new[]{"北邊山路很平靜，今晚可以安心休息。","今天巡山遇到一隻迷路的小妖怪，已送回家了。","山路上能聞到晚餐香味，回程也變得有盼頭。"}
        };
        return lines[i][b.visits%3];
    }
    string MealConversation(Guest g)
    {
        int i=FriendIndex(g.name);if(i<0)return "謝謝招待，山風今天也很溫柔。";var b=Friendship(i);
        string greeting=b.affection>=6?"又見面了，今天也想在這裡多坐一會兒。":b.affection>=3?"漸漸習慣來這裡歇腳了。":"謝謝招待，這份餐點很暖心。";
        return greeting+"\n"+MountainNews(g)+"\n"+(b.stage>=3?"送你的禮物，記得好好使用喔。":"白天有空的話，到山路上找我聊聊吧。");
    }
    bool EventReady(int i)
    {
        var b=Friendship(i);
        return !data.night&&data.day>=2&&b.stage<3&&b.affection>=(b.stage+1)*3&&b.lastEventDay<data.day;
    }
    bool EventMaterials(int i)
    {
        if(Friendship(i).stage!=1)return true;
        return i==0?data.tea>=2:i==1?data.bamboo>=4:data.grilled>=1;
    }
    string EventRequirement(int i)
    {
        var b=Friendship(i);
        if(b.stage>=3)return Gifts[i]+" · 已解鎖";
        if(data.night)return "白天再來拜訪";
        if(b.lastEventDay==data.day)return "下一段故事明天再聊";
        if(data.day<2)return "第二天起開放角色事件";
        if(b.affection<(b.stage+1)*3)return "下一段需要好感 "+((b.stage+1)*3)+" / 12";
        return b.stage==1?(i==0?"準備清茶 ×2（一般或精品均可）":i==1?"準備竹筍 ×4":"準備溪魚鹽燒 ×1"):"有新的故事可以聊";
    }
    void OpenFriendEvent(int i)
    {
        if(!EventReady(i))return;
        relationships=false;notebook=false;eventFriend=i;
        string[][] chapters={
            new[]{"我想做一期關於山中歇腳處的報導。\n不必擺出招牌動作，讓我記下你平常泡茶的樣子就好。\n先收下這份茶葉，下次帶兩杯清茶給我吧。",
                "採訪時我發現，客人喜歡的不只是茶，而是有人記得她的口味。\n帶來兩杯清茶了嗎？一杯給我，一杯留給忙完的你。",
                "報導印好了：『山風之中，一杯安心的茶。』\n我沒有誇張喔！讓山裡的朋友都知道這個地方吧。\n解鎖文的推薦：往後每份清茶多收 5 文。"},
            new[]{"你的爐子升溫有點快，我想替你做個調節器。\n別擔心，這次不會噴水！先拿兩份竹筍試做晚飯吧。\n明天有空，再帶四份竹筍給工坊。",
                "工坊的朋友說，你的菇飯比試驗報告更受歡迎。\n帶了四份竹筍嗎？大家吃飽，我就能把茶爐最後的零件裝好。",
                "新茶爐交給你啦，溫度會保持得更穩。\n以後做出好火候更容易，我也能放心來吃晚飯。\n解鎖河童茶爐：精品火候區由 55–78% 加寬為 48–85%。"},
            new[]{"巡山時往往只能啃乾糧。\n如果回程能帶一份熱便當，應該會很有精神。\n這份溪魚先交給你，改天做一份鹽燒給我吧。",
                "今天巡山比較遠，可以帶走一份溪魚鹽燒嗎？\n不是正式委託，只是想把茶屋的味道帶到山路上。\n謝謝你記得我喜歡吃什麼。",
                "便當盒還你，洗乾淨了。\n巡山時會經過一片竹林，以後每天替你帶些新鮮竹筍吧。\n解鎖巡山回禮：從下一天起，每天竹筍 ＋2。"}
        };
        Say(Friends[i],"《"+ChapterNames[i]+"》 · 第 "+(Friendship(i).stage+1)+" 段\n"+chapters[i][Friendship(i).stage]);
    }
    bool CompleteFriendEvent(int i)
    {
        if(!EventReady(i)||!EventMaterials(i))return false;
        var b=Friendship(i);
        if(b.stage==0){if(i==0)data.leaves+=2;else if(i==1)data.bamboo+=2;else data.fish++;}
        if(b.stage==1)
        {
            if(i==0){data.tea-=2;data.qualityTea=Mathf.Min(data.qualityTea,data.tea);}
            else if(i==1)data.bamboo-=4;
            else{data.grilled--;data.qualityFish=Mathf.Min(data.qualityFish,data.grilled);}
        }
        b.stage++;b.lastEventDay=data.day;
        data.journal.Add("第 "+data.day+" 天 · 《"+ChapterNames[i]+"》第 "+b.stage+" 段完成");
        eventFriend=-1;modal=false;speaker="";UpdateFriendDecor();
        Notify(b.stage==3?"故事完成 · "+Gifts[i]:"故事推進 · 下一段在之後的白天開放",6);Play(chime);Save(false);return true;
    }
    GameObject friendDecor;
    void UpdateFriendDecor()
    {
        if(friendDecor)Destroy(friendDecor);
        friendDecor=new GameObject("Friendship keepsakes");
        for(int i=0;i<3;i++)if(Friendship(i).stage==3)
        {
            Vector3 p=new Vector3(-14+i*2,1.6f,-1.6f);
            TeaHouseWorld.Shape(i==0?"Aya newspaper":i==1?"Kappa tea heater":"Momiji lunchbox",PrimitiveType.Cube,p,
                i==0?new Vector3(.7f,.03f,.5f):new Vector3(.45f,.3f,.45f),i==0?cream:i==1?new Color(.25f,.65f,.67f):new Color(.7f,.3f,.2f),friendDecor.transform);
        }
    }
    void DrawRelationships()
    {
        Box(new Rect(0,0,1440,900),new Color(0,0,0,.4f));Panel(new Rect(120,90,1200,735));
        Text(155,115,850,40,"山中友人 · 茶屋的故事",heading);
        if(Button(1100,110,180,"返回手帳")){relationships=false;notebook=true;}
        for(int i=0;i<3;i++)
        {
            var b=Friendship(i);float x=155+i*390;
            Portrait(Friends[i],new Rect(x,180,84,100));Text(x+100,180,270,35,Friends[i],heading);
            Text(x+100,224,260,60,"好感 "+b.affection+" / 12\n招待 "+b.visits+" 次",small);
            Box(new Rect(x,294,345,7),sage);Box(new Rect(x,294,345*b.affection/12f,7),gold);
            Text(x,318,355,35,ChapterNames[i]+" · "+b.stage+" / 3",heading);
            Text(x,366,345,92,EventRequirement(i)+"\n\n"+Gifts[i],small);
            if(Button(x,476,345,"拜訪：閱讀角色事件",EventReady(i)&&Vector3.Distance(player.position,spots[i==0?2:i==1?1:3].pos)<3))OpenFriendEvent(i);
        }
        Text(155,535,1100,38,"白天到角色身邊拜訪。每人每天最多增加 4 好感，每天最多推進一段故事。",small);
        journalScroll=GUI.BeginScrollView(new Rect(155,585,1125,190),journalScroll,new Rect(0,0,1080,Mathf.Max(180,data.journal.Count*34)));
        for(int j=0;j<data.journal.Count;j++)Text(8,j*34,1060,34,data.journal[data.journal.Count-1-j],small);
        GUI.EndScrollView();
    }
    void TestFriendStories()
    {
        NewGame();modal=false;
        for(int day=1;day<=4;day++)
        {
            foreach(string name in Friends)
            {
                var g=new Guest(name,0){dish=0,perfect=true,reward=1,state=2};
                PayGuest(g);int cash=data.money,aff=Friendship(FriendIndex(name)).affection;
                PayGuest(g);Assert(data.money==cash&&Friendship(FriendIndex(name)).affection==aff,"no duplicate payment or friendship");
                PayGuest(new Guest(name,0){dish=0,perfect=true,reward=1,state=2});
                PayGuest(new Guest(name,0){dish=0,perfect=true,reward=1,state=2});
                Assert(Friendship(FriendIndex(name)).dailyGain==4,"daily friendship cap");
            }
            for(int i=0;i<3;i++)
            {
                if(day==1){Assert(!EventReady(i),"stories require another day");continue;}
                Assert(EventReady(i),"next chapter becomes available");
                int stage=Friendship(i).stage;OpenFriendEvent(i);
                Assert(Friendship(i).stage==stage,"reading event does not accept it");
                if(stage==1)
                {
                    if(i==0){data.tea=0;data.qualityTea=0;}else if(i==1)data.bamboo=0;else {data.grilled=0;data.qualityFish=0;}
                    Assert(!CompleteFriendEvent(i)&&Friendship(i).stage==stage,"missing event material blocks transaction");
                    if(i==0){data.tea=2;data.qualityTea=2;}else if(i==1)data.bamboo=4;else {data.grilled=1;data.qualityFish=1;}
                }
                Assert(CompleteFriendEvent(i)&&Friendship(i).stage==stage+1,"chapter completes once");
                Assert(!CompleteFriendEvent(i),"no duplicate chapter rewards or same-day next chapter");
                Assert(data.qualityTea<=data.tea&&data.qualityFish<=data.grilled,"event delivery keeps quality stock consistent");
            }
            if(day<4)NextDay();
        }
        Assert(Friendship(0).stage==3&&Friendship(1).stage==3&&Friendship(2).stage==3,"three complete multi-day character stories");
        data.tea=1;data.qualityTea=0;data.guests.Clear();data.guests.Add(new Guest(Friends[0],0){state=1});
        Assert(Serve(0,0)&&data.guests[0].reward==52+DishBonus(0),"Aya recommendation stacks with daily recommendation");
        Assert(PerfectStart==.48f&&PerfectEnd==.85f,"Nitori heater widens perfect zone");
        int bambooBefore=data.bamboo;NextDay();Assert(data.bamboo==bambooBefore+2,"Momiji delivers daily bamboo");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));
        Assert(data.bamboo==bambooBefore+2&&Friendship(0).stage==3&&Friendship(1).stage==3&&Friendship(2).stage==3,"story rewards persist and reload does not duplicate daily gift");
        Debug.Log("QA STORY PASS: daily affinity, nine chapters across four days, materials, rewards, persistence.");
    }
}
