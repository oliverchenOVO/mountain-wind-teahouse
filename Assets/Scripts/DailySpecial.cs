using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    const int SpecialBonus=6;
    static readonly string[] ServingLabels={"清茶","菇飯","鹽燒","菇湯","竹茶","栗飯","莓茶"};
    void NormalizeSpecial()
    {
        if(data.specialDay==data.day&&data.specialDish>=0&&data.specialDish<DishNames.Length)return;
        var choices=new List<int>();
        for(int d=0;d<DishNames.Length;d++)if(RecipeUnlocked(d)&&(!IsRainDay||d==0||d==4||d==6))choices.Add(d);
        data.specialDay=data.day;data.specialDish=choices[(data.day-1)%choices.Count];
    }
    int DailyDish(){NormalizeSpecial();return data.specialDish;}
    int DishBonus(int dish){return dish==DailyDish()?SpecialBonus:0;}
    string SpecialSummary(){int d=DailyDish();return "今日推薦："+DishNames[d]+" · 每份 ＋6 文"+(IsRainDay?" · 雨天暖茶":"")+"\n"+(OnMenu(d)?"已上架":"未上架（可自由選擇）")+" · 庫存 "+Stock(d)+" · "+Recipes[d];}
    string GuestTasteHint(Guest g)
    {return "今晚想吃："+DishNames[g.order]+"\n點單 ＋15 · 精品 ＋10"+(DishBonus(g.order)>0?" · 推薦 ＋6":"");}
    void TestDailySpecial()
    {
        NewGame();modal=false;Assert(DailyDish()==0,"first day recommends an unlocked tea");
        Friendship(0).stage=1;Friendship(1).stage=1;data.trailRecipes=true;int today=DailyDish();
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(DailyDish()==today,"same-day unlocks and reload cannot reroll recommendation");
        data.tea=3;data.qualityTea=1;player.position=new Vector3(-12,0,-3);Assert(OpenShop(),"recommended stock can open normally");TickCafe(0,true);
        int cash=data.money;Assert(Serve(0,0)&&data.guests[0].reward==63&&data.guests[0].dailyBonus==6&&data.money==cash,"recommendation stacks with order and premium but waits for payment");
        var served=data.guests[0];Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));served=data.guests[0];
        Assert(served.reward==63&&served.dailyBonus==6&&!served.paid,"delivered meal preserves locked bonus on reload");
        PayGuest(served);PayGuest(served);Assert(data.money==cash+63&&data.nightSpecialIncome==6&&data.nightIncome==63&&data.nightTips==31,"bonus and report count once per payment");
        cash=data.money;ReportSummary();ReportSummary();Assert(data.money==cash,"reading report cannot pay recommendation again");
        NextDay();modal=false;Assert(DailyDish()==1&&data.nightSpecialIncome==6,"next morning rotates recommendation and retains last report");
        data.day=3;Assert(DailyDish()==6,"unlocked rain recommendation uses warm tea pool");
        data.day=4;int fixedDish=DailyDish();SetMenu(fixedDish,false);Assert(DailyDish()==fixedDish,"off-menu recommendation does not reroll");
        data.meal=3;SetMenu(1,true);Assert(OpenShop(),"non-recommended menu can still open");TickCafe(0,true);Assert(Serve(0,1)&&data.guests[0].dailyBonus==0&&data.nightSpecialIncome==0,"other dishes earn no bonus and new night resets report");
        data=JsonUtility.FromJson<SaveData>("{\"version\":1,\"day\":3,\"money\":90,\"night\":true,\"guests\":[{\"name\":\"射命丸文\",\"order\":0,\"dish\":0,\"state\":2,\"reward\":47}]}");
        NormalizeSpecial();Assert(DailyDish()==0&&data.money==90&&data.guests[0].reward==47&&data.guests[0].dailyBonus==0,"legacy rainy save initializes without retroactive meal rewards");
        Debug.Log("QA SPECIAL PASS: deterministic rotation, rain pool, locked same-day choice, stacked price, delayed once-only payment, reports, optional menu, legacy meals.");
    }
}
