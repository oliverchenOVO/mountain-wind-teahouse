using UnityEngine;

public partial class MountainTeaGame
{
    string TableName(int index){return (index+1)+" 號桌";}
    bool WaitingGuest(Guest g){return g!=null&&!g.done&&g.state==1;}
    bool TrayTarget(int index){return data.holding&&data.heldGuest==index&&index>=0&&index<data.guests.Count&&WaitingGuest(data.guests[index]);}
    int ServiceFocus(){return data.holding?data.heldGuest:guestTarget;}
    string GuestStatus(int index)
    {
        if(index<0||index>=data.guests.Count)return "沒有客人";
        var g=data.guests[index];
        if(g.done||g.state==5)return "已離席";
        if(g.state==0)return "正在入座";
        if(g.state==2)return "享用中";
        if(g.state==3)return "餐後聊聊";
        if(g.state==4)return "結帳離開";
        if(TrayTarget(index))return "正在送餐 · 等待暫停";
        return g.patience<=25?"久候了 · 優先照顧":g.patience<=50?"等候中 · 留意時間":"等候中 · 從容準備";
    }
    Color GuestStatusColor(int index)
    {
        if(index>=0&&index<data.guests.Count&&WaitingGuest(data.guests[index])&&!TrayTarget(index))
            return data.guests[index].patience<=25?new Color(.67f,.32f,.22f):data.guests[index].patience<=50?new Color(.58f,.43f,.20f):sage;
        return TrayTarget(index)?new Color(.66f,.37f,.17f):sage;
    }
    bool ChooseServiceGuest(int index)
    {
        if(!started||!data.night||data.holding||modal||paused||settingsOpen||notebook||travelBook||planning||relationships||result||trial||fishing||brewing||index<0||index>=data.guests.Count||!WaitingGuest(data.guests[index]))return false;
        guestTarget=index;return true;
    }
    string ServiceActionHint()
    {
        int index=ServiceFocus();if(index<0||index>=data.guests.Count)return "等下一位客人入座。";
        if(TrayTarget(index))return "送往 "+TableName(index)+" · "+data.guests[index].name+"\n端著 "+ShortDishes[data.heldDish]+"，桌邊按 E；不會改送其他桌。";
        if(!WaitingGuest(data.guests[index]))return TableName(index)+" · "+GuestStatus(index)+"\n選一位等候中的客人，再到料理台端餐。";
        return "端給 "+TableName(index)+" · "+data.guests[index].name+"\n"+(NearTea()?"在料理台：點下方料理端盤。":"先走近料理台，再點下方料理端盤。");
    }
    void TestServiceReadability()
    {
        NewGame();modal=false;data.tea=8;OpenShop();modal=false;TickCafe(0,true);
        Assert(TableName(0)=="1 號桌"&&TableName(2)=="3 號桌","table numbers match original seat indices");
        var g=data.guests[0];g.patience=51;Assert(GuestStatus(0).Contains("從容"),"high patience has calm text");
        g.patience=50;Assert(GuestStatus(0).Contains("留意"),"half patience boundary has caution text");
        g.patience=25;Assert(GuestStatus(0).Contains("久候"),"quarter patience boundary has priority text");
        Assert(ChooseServiceGuest(2)&&guestTarget==2,"waiting table can be selected without charging");
        string before=JsonUtility.ToJson(data);for(int i=0;i<10;i++){GuestStatus(i%3);GuestStatusColor(i%3);ServiceActionHint();ServiceFocus();}
        Assert(before==JsonUtility.ToJson(data),"service queries do not mutate any persisted gameplay state");
        Assert(PickTray(0,0)&&ServiceFocus()==0&&TrayTarget(0)&&!TrayTarget(2),"held tray overrides previous selection with original recipient");
        Assert(!ChooseServiceGuest(1)&&guestTarget==2,"held tray cannot retarget another table");
        Assert(GuestStatus(0).Contains("等待暫停")&&ServiceActionHint().Contains("1 號桌"),"held target explains existing patience freeze");
        float patience=g.patience;TickCafe(1,true);Assert(g.patience==patience,"readability preserves original held target patience freeze");
        player.position=new Vector3(-16,0,-9.2f)+Vector3.right*2.61f;Assert(!DeliverTray()&&data.holding,"numbered target does not extend delivery radius");
        player.position=new Vector3(-16,0,-9.2f);Assert(DeliverTray()&&GuestStatus(0)=="享用中"&&!WaitingGuest(g),"delivery changes text to dining and stops advertising wait");
        Assert(!ChooseServiceGuest(0),"dining guest cannot be selected for another tray");
        TickCafe(7,true);Assert(GuestStatus(0)=="餐後聊聊","after meal state has distinct text");TickCafe(0,true);FinishStory();
        Assert(GuestStatus(0)=="結帳離開","paid departing state has distinct text");
        g.done=true;Assert(GuestStatus(0)=="已離席"&&!WaitingGuest(g),"done guests do not show patience");
        Assert(GuestStatus(-1)=="沒有客人"&&!ChooseServiceGuest(-1)&&!ChooseServiceGuest(3),"invalid guest index safely rejected");
        modal=false;paused=true;Assert(!ChooseServiceGuest(1),"pause blocks table selection");paused=false;
        planning=true;Assert(!ChooseServiceGuest(1),"planner blocks table selection");planning=false;
        modal=true;Assert(!ChooseServiceGuest(1),"dialogue blocks table selection");modal=false;
        brewing=true;Assert(!ChooseServiceGuest(1),"brew blocks table selection");brewing=false;
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(GuestStatus(0)=="已離席"&&data.guests[0].paid,"saved service state uses same numbered table interpretation");
        NextDay();modal=false;Assert(data.guests.Count==0&&!data.holding,"next day clears guests through original lifecycle");
        Debug.Log("QA SERVICE READABILITY PASS: numbers, statuses, boundaries, tray focus, queries, gates, original delivery and save lifecycle.");
    }
}
