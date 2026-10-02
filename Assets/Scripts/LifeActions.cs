using UnityEngine;

public partial class MountainTeaGame
{
    void TestLifeActions()
    {
        NewGame();modal=false;player.position=new Vector3(-12,0,-3);data.tea=4;data.meal=3;data.grilled=2;OpenShop();TickCafe(0,true);
        int cash=data.money;Assert(Serve(0,0),"tea service starts visual action");var motion=visitors[0].GetComponent<AvatarMotion>();var face=visitors[0].GetComponent<AvatarExpression>();
        Assert(motion.MealDish==0&&motion.CupVisible&&!motion.FoodVisible&&motion.LifeAction=="收到餐點","tea receives cup and greeting without food utensils");
        motion.TickMeal(.6f,false);face.TickFace(0,false);Assert(face.Contented&&face.Smiling&&face.MealNod>0,"meal reception uses warm smile and nod");
        float clock=motion.MealClock;var pose=motion.MealArmPose;motion.TickMeal(3,true);Assert(motion.MealClock==clock&&motion.MealArmPose==pose,"paused meal clock and arm pose remain fixed");
        motion.TickMeal(1,false);Assert(motion.MealClock>clock&&motion.LifeAction=="喝茶"&&motion.MealArmPose!=pose,"unpaused drinking cycle resumes");
        for(int dish=0;dish<7;dish++){motion.SetMeal(dish,true);bool tea=dish==0||dish==4||dish==6;Assert(motion.CupVisible==tea&&motion.FoodVisible!=tea,"all dishes select tea or food props correctly");}
        motion.SetMeal(0,false);Assert(motion.LifeAction=="喝茶"&&data.money==cash&&!data.guests[0].paid,"visual actions cannot pay meal or repeat reception rewards");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));motion=visitors[0].GetComponent<AvatarMotion>();Assert(motion.MealDish==0&&motion.CupVisible&&motion.LifeAction=="喝茶"&&data.money==cash,"loading a served meal restores action without greeting replay or payout");
        motion.Dining=false;motion.TickMeal(0,true);Assert(!motion.CupVisible&&!motion.FoodVisible&&!motion.GetComponent<AvatarExpression>().Contented,"ending dining hides props and clears contented expression");
        Assert(Serve(1,1)&&visitors[1].GetComponent<AvatarMotion>().FoodVisible,"real rice delivery uses bowl and utensils");
        Debug.Log("QA LIFE ACTION PASS: dish-specific props, reception smile/nod, pause/resume, seven dishes, no payments, load restoration and dining cleanup.");
    }
}
