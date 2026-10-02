using UnityEngine;
using System;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    [Serializable] public class LedgerDay
    {
        public int day,service,materials,events,improvements;public bool partial,closed;
    }
    int ledgerOffset;
    void StartLedger(bool partial)
    {
        data.ledgerStarted=true;data.ledger=new List<LedgerDay>();
        data.ledger.Add(new LedgerDay{day=data.day,partial=partial,service=partial&&data.reportDay==data.day?Mathf.Max(0,data.nightIncome):0,materials=partial&&data.supplyDay==data.day?Mathf.Max(0,data.supplySpent):0});
    }
    void NormalizeLedger()
    {
        if(!data.ledgerStarted){StartLedger(true);return;}
        if(data.ledger==null)data.ledger=new List<LedgerDay>();
        var days=new HashSet<int>();
        for(int i=data.ledger.Count-1;i>=0;i--)
        {
            var row=data.ledger[i];if(row==null||row.day<1||row.day>data.day||!days.Add(row.day)){data.ledger.RemoveAt(i);continue;}
            row.service=Mathf.Max(0,row.service);row.materials=Mathf.Max(0,row.materials);row.events=Mathf.Max(0,row.events);row.improvements=Mathf.Max(0,row.improvements);
            row.closed=row.day<data.day;
        }
        if(!days.Contains(data.day))data.ledger.Add(new LedgerDay{day=data.day});
        data.ledger.Sort((a,b)=>a.day.CompareTo(b.day));while(data.ledger.Count>30)data.ledger.RemoveAt(0);
    }
    LedgerDay TodayLedger(){NormalizeLedger();return data.ledger[data.ledger.Count-1];}
    // Bookkeeping only: callers keep their original cash mutation and once-only guards.
    void RecordLedger(int category,int amount)
    {
        if(amount<=0)return;var row=TodayLedger();
        if(category==0)row.service+=amount;else if(category==1)row.materials+=amount;else if(category==2)row.events+=amount;else if(category==3)row.improvements+=amount;
    }
    static long OperatingBalance(LedgerDay row){return (long)row.service-row.materials;}
    static long RecordedBalance(LedgerDay row){return OperatingBalance(row)+row.events-row.improvements;}
    void DrawLedger()
    {
        NormalizeLedger();ledgerOffset=Mathf.Clamp(ledgerOffset,0,data.ledger.Count-1);var row=data.ledger[data.ledger.Count-1-ledgerOffset];
        Text(190,239,1050,40,"茶屋帳本 · 第 "+row.day+" 天 · "+(row.closed?"已結日":"今日累計")+(row.partial?" · 部分紀錄":""),heading);
        Text(190,284,1050,58,row.partial?"由舊存檔帶入已知的當日營業與雜貨數字；之前的委託、裝修與過往日期無法還原。":"餐費結帳才入帳，送餐與未付款的訂單不算收入。最近 30 個遊戲日可翻閱。",small);
        for(int i=0;i<3;i++)
        {
            float x=190+i*354;Panel(new Rect(x,360,335,140));
            Text(x+20,377,295,32,new[]{"營業收入","材料購買支出","營業收支差額"}[i],heading);
            long amount=i==0?row.service:i==1?row.materials:OperatingBalance(row);
            Text(x+20,426,295,42,(i==2&&amount>0?"＋":"")+amount+" 文",heading);
        }
        Text(190,525,1040,90,"委託與謝禮收入：＋"+row.events+" 文　　裝修與升級支出：−"+row.improvements+" 文\n已記錄現金變動："+(RecordedBalance(row)>0?"＋":"")+RecordedBalance(row)+" 文（營業差額 ＋ 謝禮 − 裝修）",body);
        long income=0,expense=0;int count=0;bool partial=false;
        foreach(var r in data.ledger)if(r.day>=data.day-6){income+=r.service;expense+=r.materials;count++;partial|=r.partial;}
        Text(190,629,1040,65,"最近七日（已記錄 "+count+" 日）營業 "+income+" 文 · 材料支出 "+expense+" 文 · 差額 "+(income-expense)+" 文"+(partial?" · 含部分紀錄":"")+"\n這是現金收支，不是料理毛利：舊庫存、免費採集與未用完的材料沒有折算成本。",small);
        if(Button(190,715,235,"← 較早一天",ledgerOffset<data.ledger.Count-1))ledgerOffset++;
        if(Button(440,715,235,"較新一天 →",ledgerOffset>0))ledgerOffset--;
        if(Button(690,715,235,"回到今日"))ledgerOffset=0;
        if(Button(940,715,300,"查看營業結算"))SelectPlanningTab(2);
        Text(190,763,1040,28,"帳本只供查看，不發錢、不收費，也不改變原有獎勵與交易。",small);
    }
    void TestLedger()
    {
        Load(System.IO.Path.Combine(qaDir,"legacy-player-save.json"));Assert(TodayLedger().partial&&data.ledger.Count==1,"legacy ledger marks partial without inventing history");
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.money=500;var row=TodayLedger();
        Assert(!row.partial&&row.service==0&&row.materials==0&&row.events==0&&row.improvements==0,"new journey ledger starts empty and complete");
        Assert(BuySupply(0,3)&&row.materials==24&&OperatingBalance(row)==-24,"actual supply purchase enters material column");
        int cash=data.money;BuySupply(0,3);Assert(!BuySupply(0,1)&&row.materials==48&&data.money==cash-24,"failed purchase never books an expense");
        var guest=new Guest(Friends[0],0){dish=0,reward=57,state=2};PayGuest(guest);PayGuest(guest);
        Assert(row.service==57&&OperatingBalance(row)==9,"guest payment booked once independently of delivery");
        data.questAccepted=true;data.bamboo=4;Assert(CompleteQuest()&&row.events==80&&row.service==57,"quest reward separated from service");Assert(!CompleteQuest()&&row.events==80,"duplicate quest cannot enter ledger");
        Assert(BuyUpgrade(1)&&row.improvements==120,"upgrade cost entered in improvement column");Assert(!BuyUpgrade(1)&&row.improvements==120,"duplicate upgrade cannot enter ledger");
        Assert(BuyGarden(2,1,true)&&row.improvements==155,"garden purchase booked separately");BuyGarden(2,1,true);Assert(row.improvements==155,"owned garden swap has no expense");
        Assert(RecordedBalance(row)==-66,"recorded cash balance uses all four categories");cash=data.money;
        for(int i=0;i<5;i++){NormalizeLedger();OperatingBalance(row);RecordedBalance(row);}Assert(data.money==cash&&row.service==57&&row.events==80,"ledger queries cannot change cash or duplicate entries");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));row=TodayLedger();Assert(row.service==57&&row.materials==48&&row.events==80&&row.improvements==155,"ledger columns survive same-day load");
        data.night=true;data.guests.Clear();data.guests.Add(new Guest(Friends[1],1){dish=1,reward=42,state=2});data.guests.Add(new Guest(Friends[2],2){dish=2,state=1});
        NextDay();modal=false;Assert(data.ledger[0].closed&&data.ledger[0].service==99&&TodayLedger().day==2&&TodayLedger().service==0,"early rest books served meals before closing prior day");
        Assert(TodayLedger().materials==0&&TodayLedger().events==0&&TodayLedger().improvements==0,"new day starts fresh without wiping history");
        cash=data.money;Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.ledger.Count==2&&data.ledger[0].service==99&&data.money==cash,"closed day history survives loading without repayment");
        for(int i=0;i<31;i++)NextDay();Assert(data.ledger.Count==30&&data.ledger[0].day==4&&TodayLedger().day==33,"ledger retains exactly thirty newest days");
        data.ledgerStarted=false;data.ledger=null;data.reportDay=data.day;data.nightIncome=70;data.supplyDay=data.day;data.supplySpent=16;
        NormalizeLedger();Assert(TodayLedger().partial&&TodayLedger().service==70&&TodayLedger().materials==16&&TodayLedger().events==0,"legacy current known totals imported once");NormalizeLedger();Assert(TodayLedger().service==70&&TodayLedger().materials==16,"migration totals not duplicated by repeated normalization");
        NewGame();modal=false;Assert(data.ledger.Count==1&&!TodayLedger().partial&&TodayLedger().service==0,"new journey clears prior ledger records");
        player.position=new Vector3(-12,0,-3);data.day=3;data.tea=1;Assert(ServeRainTea()&&TodayLedger().events==20&&TodayLedger().service==0,"rain tea is event income not service");modal=false;
        data.weekStartDay=1;data.weekCraft=5;Assert(ClaimWeek(2)&&TodayLedger().events==50,"journal reward booked with event income");Assert(!ClaimWeek(2)&&TodayLedger().events==50,"journal reward cannot book twice");
        data.day=7;data.tea=data.meal=data.grilled=1;for(int i=0;i<3;i++){ShareFestival(i);modal=false;}
        Assert(TodayLedger().events==60&&TodayLedger().service==0&&!ShareFestival(0),"festival once-only reward is separated from service");
        NewGame();modal=false;
        Debug.Log("QA LEDGER PASS: payments, purchases, rewards, improvements, no duplicate booking, read-only queries, migration, history and retention.");
    }
}
