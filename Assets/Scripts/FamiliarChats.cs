using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    bool ambientDialogue;Spot ambientSource;string ambientReturnText="";
    // Stable daily variants: opening a conversation never rerolls or writes progress.
    // Friend order matches Friends: Aya, Nitori, Momiji. Context order is
    // sunny day, rainy day, sunny dusk, rainy dusk, sunny dinner, rainy dinner.
    static readonly string[][][] FamiliarLines={
        new[]{
            new[]{"今天的風把茶香送得很遠。\n我想寫一篇沒有大事件的山中日記。","文文，今天先把相機收起來。\n你忙你的，我坐著看看山裡的光就好。"},
            new[]{"雨把記事本的邊角打濕了。\n今天不追新聞，聽一會兒屋簷的雨聲。","今天的頭條？茶屋裡很暖和。\n別急著招待，我只是來躲躲雨。"},
            new[]{"夕陽照在門簾上，很適合拍一張。\n等你忙完，我再問今天的茶香從哪裡來。","山路上的影子已經拉長了。\n晚上的報導，就從茶屋亮燈開始吧。"},
            new[]{"雨裡的燈籠，比晴天更醒目。\n路過的人一看，就知道這裡能歇腳。","鏡頭起霧了，今天不拍也沒關係。\n記住這盞燈，也算一份好報導。"},
            new[]{"今晚不急著追下一則新聞。\n讓我把這份茶屋的日常慢慢記下來。","山風停了一會兒，杯裡也安靜了。\n明早再趕稿，現在先好好吃飯。"},
            new[]{"雨聲把外面的喧鬧都隔開了。\n今晚的採訪，到這一餐就好。","鞋尖還帶著山路上的雨水。\n坐下吃過飯，回程就不那麼漫長了。"}
        },
        new[]{
            new[]{"水車的聲音，聽久了也像節拍。\n我在想，茶壺要不要配個更順手的把手。","今天溪水很清，適合看看水路。\n先在這裡歇一下，工具就不急著拿出來。"},
            new[]{"工具包先放乾燥的地方吧。\n等雨小一點，再去看看溪邊的水位。","雨點落在屋頂，聲音很規律呢。\n這一回不用測量，坐著聽就好了。"},
            new[]{"爐口的光比工坊的燈柔和。\n你先準備晚飯，我不在旁邊拆東西啦。","今天的工具該收起來了。\n工坊的事明天再想，晚飯可不能忘。"},
            new[]{"雨夜的路有點滑，慢慢走就好。\n茶屋亮著燈，我就知道方向沒走錯。","潮氣會讓零件生鏽，先擦乾再收好。\n你這裡的茶香，倒是一點也沒被雨沖淡。"},
            new[]{"今晚不談零件的尺寸了。\n飯桌上的事，還是慢慢吃最重要。","我本來還想畫一張工具草圖。\n不過先把飯吃完，再想也來得及。"},
            new[]{"雨天忙完，坐下來才覺得肩膀放鬆。\n謝謝招待，工具包也能在旁邊歇歇了。","屋簷的滴水聲還在。\n吃完再等一會兒，說不定回程雨就小了。"}
        },
        new[]{
            new[]{"今天的山路很安靜。\n巡查間歇能來這裡歇腳，心裡也踏實。","樹影移到小橋的另一側了。\n你慢慢忙，我再坐片刻就出發。"},
            new[]{"雨天的石階要比平常更小心。\n不必特地準備什麼，能避雨就很好。","我會等雨小一些再上山。\n暫時把巡查的腳步放慢，也不礙事。"},
            new[]{"山路漸漸暗下來了。\n看到茶屋亮燈，就知道回程有地方休息。","晚風吹過竹林，聲音比白天清楚。\n你專心備餐，外面的路我會留意。"},
            new[]{"雨夜看路要靠近些才清楚。\n這盞燈，從橋的另一端就能看見。","披肩先晾一晾，免得一直帶著寒氣。\n等忙完了，你也記得坐下休息。"},
            new[]{"巡查時總是走得很快。\n只有坐在這裡吃飯，才會慢下來。","今晚的山路很平靜。\n吃過這一餐，我再沿著小橋慢慢回去。"},
            new[]{"雨聲還在，心裡倒是安靜下來了。\n回山之前，能有這一餐真好。","披肩上的水還沒全乾。\n不用趕著收桌，我再歇一會兒就出發。"}
        }
    };
    int FamiliarContext(bool dining=false){return (dining?4:data.clock>=300?2:0)+(IsRainDay?1:0);}
    string FamiliarTopic(bool dining=false)
    {return (IsRainDay?"雨":"晴")+(dining?"夜餐後":data.clock>=300?"晚備餐":"日歇腳");}
    string FamiliarRemark(int friend,bool dining=false)
    {
        if(friend<0||friend>=Friends.Length)return "山風今天也很溫柔。";
        return FamiliarLines[friend][FamiliarContext(dining)][(data.day+friend)%2];
    }
    void ResetAmbientTalking(){ambientDialogue=false;ambientSource=null;ambientReturnText="";}
    int AmbientFriend(Spot source)
    {return source==null?-1:source.kind==11||source.kind==27?1:source.kind==12||source.kind==26?0:source.kind==13||source.kind==22?2:-1;}
    void OfferAmbientChat(Spot source)
    {if(AmbientFriend(source)>=0&&!data.night)ambientSource=source;}
    bool CanAmbientChat()
    {
        int friend=AmbientFriend(ambientSource);
        return started&&modal&&!ambientDialogue&&friend>=0&&speaker==Friends[friend]&&!data.night&&!rainDialogue&&!festivalDialogue&&storyGuest<0&&eventFriend<0&&!paused&&!settingsOpen&&!brewing&&!trial&&VisibleLivingSpot(ambientSource)&&Vector3.Distance(player.position,ambientSource.pos)<3;
    }
    bool OpenAmbientChat()
    {
        if(!CanAmbientChat())return false;
        var source=ambientSource;int friend=AmbientFriend(source);string previous=dialogue;Say(Friends[friend],FamiliarRemark(friend));
        ambientDialogue=true;ambientSource=source;ambientReturnText=previous;SetDialogueMood(1);return true;
    }
    void CloseAmbientChat(bool back)
    {
        var source=ambientSource;string previous=ambientReturnText;ResetAmbientTalking();
        if(back&&AmbientFriend(source)>=0){Say(Friends[AmbientFriend(source)],previous);OfferAmbientChat(source);}
        else{modal=false;speaker="";}
    }
    void DrawAmbientDialog()
    {
        Text(365,585,710,30,"只是聊聊，不消耗材料，也不增加好感或報酬。",small);
        if(Button(365,633,350,"返回原對話"))CloseAmbientChat(true);
        if(Button(730,633,345,"慢慢歇腳 [Esc]"))CloseAmbientChat(false);
    }
    void TestFamiliarChats()
    {
        NewGame();modal=false;var unique=new HashSet<string>();
        for(int friend=0;friend<3;friend++)for(int context=0;context<6;context++)for(int variant=0;variant<2;variant++)
        {
            data.day=context%2==0?(variant==0?1:2):(variant==0?3:6);data.clock=context>=2?320:0;
            string before=JsonUtility.ToJson(data),line=FamiliarRemark(friend,context>=4);
            Assert(line.Length>20&&line.Contains("\n")&&unique.Add(line),"unique familiar chat for friend weather time and daily variant");
            Assert(FamiliarRemark(friend,context>=4)==line&&JsonUtility.ToJson(data)==before,"chat lookup is stable and has no save mutations");
        }
        Assert(unique.Count==36,"thirty-six authored daily and dinner remarks");
        NewGame();modal=false;player.position=spots[1].pos;Interact(spots[1]);string quest=dialogue,beforeChat=JsonUtility.ToJson(data);
        Assert(CanAmbientChat()&&OpenAmbientChat()&&ambientDialogue&&speaker==Friends[1],"Nitori quest dialogue offers separate free chat");
        Assert(!OpenAmbientChat()&&JsonUtility.ToJson(data)==beforeChat,"chat cannot duplicate or accept quest or affinity");
        CloseAmbientChat(true);Assert(dialogue==quest&&!ambientDialogue&&CanAmbientChat()&&!data.questAccepted,"return preserves original unaccepted quest");
        data.questAccepted=true;Interact(spots[1]);quest=dialogue;beforeChat=JsonUtility.ToJson(data);OpenAmbientChat();CloseAmbientChat(true);
        Assert(dialogue==quest&&JsonUtility.ToJson(data)==beforeChat,"accepted quest delivery remains unchanged by chat");
        data.day=2;Friendship(1).affection=3;OpenFriendEvent(1);Assert(!CanAmbientChat()&&!OpenAmbientChat()&&eventFriend==1,"role chapter cannot be replaced by small talk");
        eventFriend=-1;modal=false;data.questDone=true;Interact(spots[1]);OpenAmbientChat();CloseAmbientChat(false);Assert(!modal&&ambientSource==null,"close clears all ambient routing");
        data.day=3;Interact(spots[1]);OpenAmbientChat();Assert(FamiliarTopic()=="雨日歇腳"&&dialogue==FamiliarRemark(1),"rain context appears without serving or consuming tea");
        Say(Friends[1],"避雨故事");rainDialogue=true;Assert(!CanAmbientChat()&&!ambientDialogue&&ambientSource==null,"rewarded rain dialogue clears ambient state");
        Say(Friends[0],"山中聚會");festivalDialogue=true;Assert(!CanAmbientChat(),"festival dialogue cannot be replaced");
        modal=false;data.day=1;data.clock=300;player.position=spots[2].pos;Interact(spots[2]);OpenAmbientChat();Assert(FamiliarTopic()=="晴晚備餐","sunset conversation threshold uses evening clock");CloseAmbientChat(false);
        ChangeRegion(true);var momiji=spots.Find(s=>s.kind==22);player.position=momiji.pos;data.notebookStage=4;InteractTrip(momiji);
        Assert(!CanAmbientChat()&&data.notebookStage==5,"notebook handover retains priority and no ambient button");
        data.notebookStage=0;data.lunchState=2;InteractTrip(momiji);Assert(!CanAmbientChat()&&data.lunchState==3,"lunch delivery retains priority and reward dialogue");
        InteractTrip(momiji);Assert(CanAmbientChat()&&OpenAmbientChat(),"ordinary mountain visit offers Momiji chat");CloseAmbientChat(true);
        player.position=momiji.pos+Vector3.right*4;Assert(!CanAmbientChat(),"small talk unavailable away from speaker");
        ChangeRegion(false);data.night=true;var guest=new Guest(Friends[0],0){dish=0,reward=32,state=3};data.guests.Clear();data.guests.Add(guest);storyGuest=0;
        string meal=MealConversation(guest);Assert(meal.Contains(FamiliarRemark(0,true))&&FamiliarTopic(true)=="晴夜餐後","night service includes matching dinner remark");
        Say(guest.name,meal);Assert(!OpenAmbientChat(),"meal dialogue cannot be replaced by daytime free chat");int cash=data.money;FinishStory();int paid=data.money,affection=Friendship(0).affection,journal=data.journal.Count;FinishStory();
        Assert(paid==cash+32&&data.money==paid&&Friendship(0).affection==affection&&data.journal.Count==journal,"dinner still pays and adds friendship only once");
        data.day=3;Assert(MealConversation(guest).Contains(FamiliarRemark(0,true))&&FamiliarTopic(true)=="雨夜餐後","rain dinner has distinct context");
        data.night=false;data.guests.Clear();player.position=spots[1].pos;Interact(spots[1]);OpenAmbientChat();Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));
        Assert(!ambientDialogue&&ambientSource==null&&!modal&&data.day==3,"load clears small talk without new persisted fields");
        Interact(spots[1]);OfferAmbientChat(spots[1]);NextDay();Assert(!ambientDialogue&&ambientSource==null,"next day clears old speaker context");
        NewGame();modal=false;player.position=spots[1].pos;Interact(spots[1]);OpenAmbientChat();ReturnMenu();Assert(!ambientDialogue&&ambientSource==null,"title clears ambient dialogue");
        NewGame();modal=false;
        Debug.Log("QA FAMILIAR PASS: 36 remarks, stable context, no transactions, quest chapter rain festival lunch notebook priority, distance, dinner payment and persistence.");
    }
}
