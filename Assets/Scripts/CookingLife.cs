using UnityEngine;

public partial class MountainTeaGame
{
    GameObject cookingRoot, cookingFire;
    Transform[] cookingPuffs=new Transform[12], flames=new Transform[3];
    Renderer[] puffRenderers=new Renderer[12];
    MaterialPropertyBlock cookingTint=new MaterialPropertyBlock();
    Light cookingLight;
    float cookingClock, cookingFinishTime;
    bool cookingPerfect;
    bool TeaBrew(int dish){return dish==0||dish==4||dish==6;}
    bool CookingFrozen {get{return !started||paused||notebook||modal||result||relationships||planning||travelBook||settingsOpen;}}
    void BuildCookingLife()
    {
        cookingRoot=new GameObject("Responsive kitchen steam");
        var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);
        for(int y=0;y<32;y++)for(int x=0;x<32;x++){float r=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2)));}
        texture.Apply();
        var material=new Material(Shader.Find("MountainTea/SoftParticle")){name="Kitchen soft steam",mainTexture=texture};
        for(int i=0;i<12;i++)
        {
            var puff=GameObject.CreatePrimitive(PrimitiveType.Quad);puff.name="Cached kitchen puff "+i;puff.transform.SetParent(cookingRoot.transform,false);Destroy(puff.GetComponent<Collider>());
            cookingPuffs[i]=puff.transform;puffRenderers[i]=puff.GetComponent<Renderer>();puffRenderers[i].sharedMaterial=material;
            puffRenderers[i].shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;puffRenderers[i].receiveShadows=false;
        }
        cookingFire=new GameObject("Responsive stove fire");
        TeaHouseWorld.Shape("Stove opening",PrimitiveType.Cube,new Vector3(-9.2f,.63f,1.385f),new Vector3(.72f,.42f,.025f),new Color(.12f,.10f,.08f));
        for(int i=0;i<3;i++)flames[i]=TeaHouseWorld.Shape("Warm flame",PrimitiveType.Sphere,new Vector3(-9.4f+i*.2f,.59f,1.35f),new Vector3(.18f,.3f,.05f),i==1?new Color(1,.78f,.26f):new Color(.96f,.39f,.10f),cookingFire.transform).transform;
        var lightObject=new GameObject("Kitchen warmth");lightObject.transform.SetParent(cookingFire.transform,false);
        cookingLight=lightObject.AddComponent<Light>();cookingLight.type=LightType.Point;cookingLight.color=new Color(1,.57f,.23f);cookingLight.range=3;cookingLight.shadows=LightShadows.None;cookingLight.transform.position=new Vector3(-9.2f,.8f,1.1f);
        ResetCookingLife();
    }
    void ResetCookingLife()
    {
        cookingClock=0;cookingFinishTime=0;
        if(cookingRoot)cookingRoot.SetActive(false);if(cookingFire)cookingFire.SetActive(false);
    }
    void CancelBrew(){brewing=false;ResetCookingLife();Notify("已取消，材料沒有消耗。");}
    void TickCookingLife(float dt)
    {
        if(!cookingRoot)return;
        bool visible=started&&!data.onTrail&&(brewing||cookingFinishTime>0)&&!closing;
        cookingRoot.SetActive(visible);cookingFire.SetActive(visible&&brewing&&!TeaBrew(brewDish));
        if(!visible)return;
        if(!CookingFrozen){cookingClock+=dt;cookingFinishTime=Mathf.Max(0,cookingFinishTime-dt);}
        Vector3 origin=TeaBrew(brewDish)?new Vector3(-11.1f,1.99f,-.55f):new Vector3(-9.2f,1.45f,1.9f);
        for(int i=0;i<12;i++)
        {
            float phase=Mathf.Repeat(cookingClock*.45f+i/12f,1),size=.22f+phase*.65f;
            cookingPuffs[i].position=origin+new Vector3(Mathf.Sin(i*2.4f+phase*3)*(.07f+phase*.22f),phase*1.7f,Mathf.Cos(i*1.7f)*.12f);
            cookingPuffs[i].rotation=cam.transform.rotation;cookingPuffs[i].localScale=Vector3.one*size;
            Color tint=cookingFinishTime>0&&cookingPerfect?new Color(1,.88f,.52f):new Color(.97f,.98f,.92f);
            tint.a=Mathf.Sin(phase*Mathf.PI)*.48f;cookingTint.SetColor("_Color",tint);puffRenderers[i].SetPropertyBlock(cookingTint);
        }
        for(int i=0;i<3;i++)flames[i].localScale=new Vector3(.18f,.26f+Mathf.Sin(cookingClock*9+i*2)*.08f,.05f);
        cookingLight.intensity=.65f+Mathf.Sin(cookingClock*7)*.12f;
    }
    void DrawCookingLife()
    {
        if(cookingFinishTime<=0||CookingFrozen||photoMode||brewing)return;
        Panel(new Rect(450,305,540,65));TeaSeal(new Rect(468,316,40,40));
        Text(520,320,445,35,(cookingPerfect?"精品出爐 · ":"暖暖完成 · ")+DishNames[brewDish],heading);
    }
    void DrawCookingIllustration()
    {
        // A cached-time illustration also makes the process legible beneath the roof.
        float cx=504,cy=359;
        RoundFill(new Rect(cx-35,cy-12,70,32),sage);RoundFill(new Rect(cx-40,cy-15,80,8),gold);
        if(TeaBrew(brewDish)){RoundFill(new Rect(cx+30,cy-9,25,11),sage);RoundFill(new Rect(cx-7,cy-23,14,8),sage);}
        else for(int i=0;i<3;i++){float h=13+Mathf.Sin(cookingClock*7+i*2)*4;RoundFill(new Rect(cx-21+i*16,cy+27-h,10,h),gold);}
        for(int i=0;i<3;i++){float p=Mathf.Repeat(cookingClock*.45f+i/3f,1);RoundFill(new Rect(cx-22+i*20+Mathf.Sin(p*4)*4,cy-27-p*33,6,12),new Color(.48f,.59f,.49f,(1-p)*.7f));}
        Text(570,328,400,34,TeaBrew(brewDish)?"溫壺 · 茶香慢慢舒展":"添柴 · 小火照亮鍋沿",heading);
        Text(570,366,400,28,"不趕時間，等火候落在金色區域。",small);
    }
    void TestCookingLife()
    {
        NewGame();modal=false;data.leaves=20;data.mushrooms=20;data.bamboo=20;data.fish=20;data.chestnuts=20;data.berries=20;data.trailRecipes=true;Friendship(0).stage=1;Friendship(1).stage=1;
        BeginBrew(0);int leaves=data.leaves,stock=data.tea;TickCookingLife(.5f);
        Assert(cookingRoot.activeSelf&&!cookingFire.activeSelf&&cookingClock>.4f,"tea has responsive steam without stove fire");
        Assert(data.leaves==leaves&&data.tea==stock,"cooking visuals cannot consume or produce ingredients");
        float time=cookingClock,timer=brewTimer;paused=true;Update();TickCookingLife(2);Assert(cookingClock==time&&brewTimer==timer,"pause freezes kitchen animation and fire timing");paused=false;
        settingsOpen=true;TickCookingLife(2);Assert(cookingClock==time,"settings freeze kitchen animation");settingsOpen=false;
        CancelBrew();Assert(!brewing&&!cookingRoot.activeSelf&&!cookingFire.activeSelf&&data.leaves==leaves,"cancel clears effects without consuming ingredients");
        BeginBrew(0);brewTimer=(PerfectStart+PerfectEnd)*.5f/.42f;FinishBrew();int after=data.leaves;
        Assert(data.tea==stock+1&&data.qualityTea==1&&cookingFinishTime>0&&cookingPerfect,"perfect completion creates one premium dish and warm feedback");
        FinishBrew();Assert(data.leaves==after&&data.tea==stock+1,"repeated completion cannot craft twice");
        paused=true;float finish=cookingFinishTime;TickCookingLife(4);Assert(cookingFinishTime==finish,"completion feedback duration freezes on pause");paused=false;
        TickCookingLife(4);TickCookingLife(0);Assert(!cookingRoot.activeSelf&&cookingFinishTime==0,"completion effect expires");
        for(int d=0;d<7;d++){BeginBrew(d);Assert(brewing,"all seven unlocked recipes can prepare");TickCookingLife(.1f);Assert(cookingFire.activeSelf==!TeaBrew(d),"recipe-specific stove state");CancelBrew();}
        data.leaves=0;BeginBrew(0);TickCookingLife(.1f);Assert(!brewing&&!cookingRoot.activeSelf,"insufficient ingredients do not light kitchen");
        data.leaves=20;BeginBrew(0);Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(!brewing&&cookingFinishTime==0&&!cookingRoot.activeSelf,"loading clears transient cooking but keeps materials");
        BeginBrew(1);ReturnMenu();Assert(!brewing&&!cookingRoot.activeSelf&&!cookingFire.activeSelf,"return to menu clears kitchen");
        NewGame();modal=false;data.leaves=20;BeginBrew(0);brewTimer=0;FinishBrew();Assert(data.qualityTea==0&&!cookingPerfect,"ordinary completion retains original quality rules");
        Debug.Log("QA COOKING PASS: recipe visuals, freeze, cancel, completion idempotency, expiry, load and menu lifecycle.");
    }
}
