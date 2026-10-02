using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    int lastPlanningTab,prepFocus;
    static readonly string[] PlanningTabs={"每日菜單","庭院裝修","營業結算","七日手帖","茶屋升級","小祭典","材料雜貨","帳本"};
    // Ingredient order matches SupplyNames. Quantities describe one normal recipe,
    // not its target count; navigation never buys or crafts anything.
    static readonly int[,] PrepIngredients={
        {2,0,0,0,0,0},{0,1,1,0,0,0},{0,0,0,1,0,0},
        {1,2,0,0,0,0},{1,0,1,0,0,0},{0,1,0,0,2,0},{1,0,0,0,0,2}
    };
    void ResetConvenience(){lastPlanningTab=0;planningTab=0;prepFocus=0;travelTab=0;}
    bool SelectPlanningTab(int tab,bool resetLedger=true)
    {
        if(tab<0||tab>=PlanningTabs.Length)return false;
        planningTab=lastPlanningTab=tab;if(tab==7&&resetLedger)ledgerOffset=0;return true;
    }
    bool SelectTravelTab(int tab){if(tab<0||tab>1)return false;travelTab=tab;return true;}
    bool FocusRecipe(int dish)
    {if(dish<0||dish>=DishNames.Length||!RecipeUnlocked(dish))return false;prepFocus=dish;return true;}
    int FocusedNeed(int ingredient)
    {
        if(ingredient<0||ingredient>=6||prepFocus<0||prepFocus>=DishNames.Length||!RecipeUnlocked(prepFocus))return 0;
        return Mathf.Max(0,PrepIngredients[prepFocus,ingredient]-SupplyHeld(ingredient));
    }
    string PrepFocusSummary()
    {
        prepFocus=Mathf.Clamp(prepFocus,0,DishNames.Length-1);
        if(!RecipeUnlocked(prepFocus))return "備餐焦點："+DishNames[prepFocus]+" · 食譜尚未解鎖";
        var missing=new List<string>();for(int i=0;i<6;i++)if(FocusedNeed(i)>0)missing.Add(SupplyNames[i]+" ×"+FocusedNeed(i));
        return "備餐焦點："+DishNames[prepFocus]+" · "+(missing.Count>0?"缺 "+string.Join("、",missing):"材料足夠 · F3 進入火候");
    }
    bool CanNavigatePlanning()
    {return started&&planning&&!paused&&!settingsOpen&&!modal&&!notebook&&!travelBook&&!relationships&&!result&&!trial&&!fishing&&!brewing;}
    bool CanStartPlanningBrew(int dish)
    {return CanNavigatePlanning()&&(planningTab==0||planningTab==6)&&dish>=0&&dish<DishNames.Length&&RecipeUnlocked(dish)&&Ingredients(dish)&&NearTea()&&!data.holding&&data.lunchState!=2;}
    bool StartPlanningBrew(int dish)
    {
        if(!CanStartPlanningBrew(dish))return false;
        FocusRecipe(dish);BeginBrew(dish);if(!brewing)return false;planning=false;return true;
    }
    bool PlanningShortcut(KeyCode key)
    {
        if(!CanNavigatePlanning())return false;
        if(key==KeyCode.F1)return SelectPlanningTab(0);
        if(key==KeyCode.F2)return SelectPlanningTab(6);
        return key==KeyCode.F3&&StartPlanningBrew(prepFocus);
    }
    void TickPlanningShortcuts()
    {
        if(!CanNavigatePlanning())return;
        if(Input.GetKeyDown(KeyCode.F1))PlanningShortcut(KeyCode.F1);
        else if(Input.GetKeyDown(KeyCode.F2))PlanningShortcut(KeyCode.F2);
        else if(Input.GetKeyDown(KeyCode.F3)&&!PlanningShortcut(KeyCode.F3))Notify("暫時不能備餐：確認材料、料理台距離及托盤／便當。",4);
    }
    void TestPlanningConvenience()
    {
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);
        Assert(lastPlanningTab==0&&prepFocus==0&&travelTab==0,"new journey starts with default convenience views");
        OpenPlanning(6);planning=false;OpenPlanning();Assert(planningTab==6,"general reopen resumes supplies instead of resetting menu");
        SelectPlanningTab(4);planning=false;OpenPlanning();Assert(planningTab==4,"tab clicks update remembered planning page");
        Assert(!SelectPlanningTab(-1)&&!SelectPlanningTab(8)&&planningTab==4,"invalid planning page cannot change selection");
        OpenPlanning(7);ledgerOffset=1;planning=false;OpenPlanning();Assert(planningTab==7&&ledgerOffset==1,"general ledger reopen retains viewing offset");OpenPlanning(7);Assert(ledgerOffset==0,"explicit ledger shortcut still opens today");
        OpenPlanning(0);Assert(FocusRecipe(1)&&!FocusRecipe(3)&&!FocusRecipe(-1)&&!FocusRecipe(7)&&prepFocus==1,"focus accepts unlocked recipe only without changing on-menu status");
        string before=JsonUtility.ToJson(data);Assert(FocusedNeed(1)==1&&FocusedNeed(2)==1&&FocusedNeed(0)==0&&PrepFocusSummary().Contains("野生香菇 ×1"),"one meal shows exact mushroom and bamboo gaps");
        Assert(PlanningShortcut(KeyCode.F2)&&planningTab==6&&prepFocus==1&&JsonUtility.ToJson(data)==before,"F2 retains focused meal and cannot buy or modify progress");
        Assert(!PlanningShortcut(KeyCode.F3)&&planning&&!brewing&&JsonUtility.ToJson(data)==before,"missing ingredients keep planning open and cannot start or consume");
        Assert(PlanningShortcut(KeyCode.F1)&&planningTab==0&&JsonUtility.ToJson(data)==before,"F1 returns to menu without gameplay writes");
        data.money=100;PlanningShortcut(KeyCode.F2);Assert(BuySupply(1,1)&&BuySupply(2,1)&&data.money==78&&FocusedNeed(1)==0&&FocusedNeed(2)==0,"manual purchases reduce focus gaps using original prices");
        int mushrooms=data.mushrooms,bamboo=data.bamboo,meal=data.meal;Assert(PlanningShortcut(KeyCode.F3)&&brewing&&!planning&&brewDish==1&&data.mushrooms==mushrooms&&data.bamboo==bamboo&&data.meal==meal,"F3 starts original timing game but never crafts early");
        CancelBrew();OpenPlanning();Assert(planningTab==6&&prepFocus==1&&data.mushrooms==mushrooms,"cancel and reopen retain supplies focus without consumption");
        PlanningShortcut(KeyCode.F1);StartPlanningBrew(1);brewTimer=0;FinishBrew();Assert(data.meal==meal+1&&data.mushrooms==mushrooms-1&&data.bamboo==bamboo-1,"focused preparation consumes once only on completion");
        OpenPlanning(6);data.mushrooms=data.bamboo=2;player.position=Vector3.zero;Assert(!PlanningShortcut(KeyCode.F3)&&planning,"remote shortcut cannot bypass cooking proximity");player.position=new Vector3(-12,0,-3);
        data.holding=true;Assert(!PlanningShortcut(KeyCode.F3),"carrying tray blocks focused preparation");data.holding=false;data.lunchState=2;Assert(!PlanningShortcut(KeyCode.F3),"packed lunch blocks focused preparation");data.lunchState=0;
        paused=true;Assert(!PlanningShortcut(KeyCode.F1),"pause blocks convenience keys");paused=false;settingsOpen=true;Assert(!PlanningShortcut(KeyCode.F1),"settings block convenience keys");settingsOpen=false;
        modal=true;Assert(!PlanningShortcut(KeyCode.F1),"dialogue blocks convenience keys");modal=false;notebook=true;Assert(!PlanningShortcut(KeyCode.F1),"notebook blocks convenience keys");notebook=false;
        travelBook=true;Assert(!PlanningShortcut(KeyCode.F1),"travel book blocks convenience keys");travelBook=false;relationships=true;Assert(!PlanningShortcut(KeyCode.F1),"friend screen blocks convenience keys");relationships=false;
        result=true;Assert(!PlanningShortcut(KeyCode.F1),"result blocks convenience keys");result=false;trial=true;Assert(!PlanningShortcut(KeyCode.F1),"trial blocks convenience keys");trial=false;
        fishing=true;Assert(!PlanningShortcut(KeyCode.F1),"fishing blocks convenience keys");fishing=false;brewing=true;Assert(!PlanningShortcut(KeyCode.F1),"brewing blocks convenience keys");brewing=false;
        SelectPlanningTab(1);Assert(!PlanningShortcut(KeyCode.F3),"F3 does not start food from garden page");PlanningShortcut(KeyCode.F2);
        data.night=true;before=JsonUtility.ToJson(data);Assert(!BuySupply(1,1)&&JsonUtility.ToJson(data)==before,"night supplies stay locked through quick navigation");
        Assert(PlanningShortcut(KeyCode.F3)&&brewing,"night manual cooking keeps existing replenishment behavior");CancelBrew();data.night=false;OpenPlanning(6);
        Friendship(0).stage=Friendship(1).stage=1;data.trailRecipes=true;data.leaves=data.mushrooms=data.bamboo=data.fish=data.chestnuts=data.berries=0;
        int[] totals={2,2,1,3,2,3,3};for(int d=0;d<7;d++){FocusRecipe(d);int total=0;for(int i=0;i<6;i++)total+=FocusedNeed(i);Assert(total==totals[d]&&PrepFocusSummary().Contains(DishNames[d]),"all recipe focus summaries use correct one-portion quantities");}
        Assert(FocusedNeed(0)==1&&FocusedNeed(5)==2&&FocusedNeed(-1)==0&&FocusedNeed(6)==0,"berry tea gaps map to tea leaves and berries safely");
        SelectTravelTab(1);Assert(!SelectTravelTab(-1)&&!SelectTravelTab(2)&&travelTab==1,"travel page uses valid remembered selection");SelectPlanningTab(6);Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));OpenPlanning();
        Assert(planningTab==6&&prepFocus==6&&travelTab==1,"in-session load retains non-persisted view preferences");planning=false;NextDay();OpenPlanning();Assert(planningTab==6&&prepFocus==6&&travelTab==1,"next day retains view choices without extra purchases");
        NewGame();modal=false;Assert(lastPlanningTab==0&&prepFocus==0&&travelTab==0,"new journey resets convenience without carrying old views");
        Debug.Log("QA CONVENIENCE PASS: remembered tabs, explicit shortcuts, focus gaps, zero navigation transactions, manual supply prices, original brewing, overlays, night, save and next-day behavior.");
    }
}
