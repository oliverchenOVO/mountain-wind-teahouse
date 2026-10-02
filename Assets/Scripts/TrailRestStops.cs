using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    GameObject trailRestRoot;readonly List<Transform> trailRestSigns=new List<Transform>();
    readonly Vector2[] RestSignPositions={new Vector2(83.6f,-8),new Vector2(103.4f,.1f),new Vector2(88.6f,17)};
    GameObject RestShape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)
    {
        var shape=TeaHouseWorld.Shape(name,type,p,scale,color,trailRestRoot.transform);
        var collider=shape.GetComponent<Collider>();if(collider)collider.enabled=false;return shape;
    }
    void RestBeam(string name,Vector3 a,Vector3 b,float radius,Color color)
    {var g=RestShape(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(radius,(b-a).magnitude*.5f,radius),color);g.transform.up=(b-a).normalized;}
    void RestText(string text,Vector3 p,float size)
    {
        var g=new GameObject("Rest route label · "+text);g.transform.SetParent(trailRestRoot.transform);g.transform.position=p;
        var tm=g.AddComponent<TextMesh>();tm.font=font;tm.fontSize=48;tm.characterSize=size;tm.anchor=TextAnchor.MiddleCenter;tm.color=TrailIvory;tm.text=text;
        g.GetComponent<MeshRenderer>().sharedMaterial=font.material;
    }
    void RestRouteSign(int index,string upper,string lower)
    {
        var coordinate=RestSignPositions[index];Vector3 p=TP(coordinate.x,coordinate.y);
        var post=RestShape("Rest route cedar post",PrimitiveType.Cube,p+Vector3.up*.78f,new Vector3(.14f,1.55f,.14f),TrailWood);trailRestSigns.Add(post.transform);
        for(int row=0;row<2;row++)
        {
            Vector3 q=p+new Vector3(0,1.37f-row*.42f,-.10f);
            RestShape("Rest route weathered board",PrimitiveType.Cube,q,new Vector3(1.8f,.34f,.12f),row==0?new Color(.34f,.40f,.28f):TrailWood);
            RestText(row==0?upper:lower,q+Vector3.back*.068f,.075f);
            for(int side=-1;side<=1;side+=2)RestShape("Rest sign peg",PrimitiveType.Sphere,q+new Vector3(side*.73f,0,-.069f),Vector3.one*.045f,new Color(.73f,.64f,.41f));
        }
        RestShape("Rest sign stone foot",PrimitiveType.Cylinder,p+Vector3.up*.05f,new Vector3(.49f,.08f,.45f),new Color(.47f,.50f,.40f));
    }
    void RestCup(Vector3 p)
    {
        RestShape("Rest cup saucer",PrimitiveType.Cylinder,p,new Vector3(.27f,.018f,.27f),TrailIvory);
        RestShape("Rest cup ceramic",PrimitiveType.Cylinder,p+Vector3.up*.08f,new Vector3(.18f,.075f,.18f),new Color(.75f,.81f,.64f));
        RestShape("Rest cup tea surface",PrimitiveType.Cylinder,p+Vector3.up*.157f,new Vector3(.145f,.004f,.145f),new Color(.36f,.43f,.18f));
    }
    void RestBasket(Vector3 p)
    {
        RestShape("Rest basket body",PrimitiveType.Cylinder,p+Vector3.up*.23f,new Vector3(.52f,.22f,.46f),new Color(.65f,.48f,.29f));
        RestShape("Rest basket dark opening",PrimitiveType.Cylinder,p+Vector3.up*.455f,new Vector3(.43f,.006f,.38f),new Color(.34f,.26f,.16f));
        for(int row=0;row<4;row++)RestShape("Rest basket weave ring",PrimitiveType.Cylinder,p+Vector3.up*(.10f+row*.10f),new Vector3(.535f,.012f,.475f),new Color(.76f,.60f,.37f));
        for(int side=-1;side<=1;side+=2)RestBeam("Rest basket handle",p+new Vector3(side*.20f,.42f,0),p+new Vector3(side*.20f,.70f,0),.024f,TrailWood);
        RestBeam("Rest basket handle grip",p+new Vector3(-.20f,.70f,0),p+new Vector3(.20f,.70f,0),.03f,TrailWood);
    }
    void RestBench(Vector3 p)
    {
        for(int plank=0;plank<3;plank++)RestShape("Rest bench seat slat",PrimitiveType.Cube,p+new Vector3(0,.51f,(plank-1)*.16f),new Vector3(1.55f,.10f,.14f),TrailWood);
        for(int side=-1;side<=1;side+=2)RestShape("Rest bench sturdy foot",PrimitiveType.Cube,p+new Vector3(side*.57f,.25f,0),new Vector3(.13f,.5f,.43f),TrailWood);
        RestShape("Rest bench folded cushion",PrimitiveType.Cube,p+new Vector3(.37f,.61f,0),new Vector3(.5f,.10f,.39f),new Color(.55f,.33f,.24f));
        RestShape("Rest cushion seam",PrimitiveType.Cube,p+new Vector3(.37f,.665f,0),new Vector3(.46f,.008f,.023f),TrailIvory);
    }
    void BuildTrailRestStops()
    {
        trailRestRoot=new GameObject("Mountain trail lived-in rest stops");trailRestRoot.transform.SetParent(trailRoot);
        DetailedTrailPath(new[]{TP(94,2),TP(90,2),TP(86,12),TP(86,18.5f)});
        RestRouteSign(0,"小橋 ↗","溪谷 ↙");RestRouteSign(1,"天狗哨所 ↗","溪谷 ↙");RestRouteSign(2,"觀景亭 ↖","小橋 ↘");
        // Keep the centre approach and existing actor/refuge positions clear.
        Vector3 table=TP(83.9f,19.1f);
        RestShape("Rest tea table top",PrimitiveType.Cube,table+Vector3.up*.67f,new Vector3(.91f,.11f,.74f),TrailWood);
        for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2)RestShape("Rest tea table leg",PrimitiveType.Cube,table+new Vector3(a*.33f,.34f,b*.25f),new Vector3(.10f,.66f,.10f),TrailWood);
        RestShape("Rest bamboo serving tray",PrimitiveType.Cube,table+Vector3.up*.738f,new Vector3(.70f,.022f,.50f),new Color(.71f,.61f,.38f));
        RestCup(table+new Vector3(.21f,.76f,-.13f));RestCup(table+new Vector3(.21f,.76f,.16f));
        RestShape("Rest small teapot",PrimitiveType.Sphere,table+new Vector3(-.20f,.91f,0),new Vector3(.28f,.25f,.28f),new Color(.34f,.47f,.33f));
        RestShape("Rest teapot lid",PrimitiveType.Cylinder,table+new Vector3(-.20f,1.045f,0),new Vector3(.18f,.025f,.18f),new Color(.34f,.47f,.33f));
        RestBeam("Rest teapot spout",table+new Vector3(-.10f,.91f,0),table+new Vector3(.02f,1.0f,0),.045f,new Color(.34f,.47f,.33f));
        RestBench(TP(88.15f,18.8f));RestBasket(TP(83.7f,18.15f));
        for(int side=-1;side<=1;side+=2)
        {
            Vector3 pot=TP(86+side*2.5f,21);
            RestShape("Rest fern pot",PrimitiveType.Cylinder,pot+Vector3.up*.20f,new Vector3(.45f,.20f,.45f),new Color(.58f,.37f,.25f));
            for(int leaf=0;leaf<5;leaf++){float angle=leaf*Mathf.PI*2/5;var g=RestShape("Rest fern leaf",PrimitiveType.Capsule,pot+new Vector3(Mathf.Cos(angle)*.18f,.50f,Mathf.Sin(angle)*.18f),new Vector3(.12f,.28f,.08f),new Color(.40f,.56f,.33f));g.transform.rotation=Quaternion.Euler(20,leaf*72,35);}
        }
        RestBench(TP(108.3f,17.9f));RestBasket(TP(103.6f,17.7f));
        Vector3 rack=TP(103.8f,19.0f);
        for(int side=-1;side<=1;side+=2)RestBeam("Rest drying rack upright",rack+new Vector3(side*.45f,0,0),rack+new Vector3(side*.45f,1.2f,0),.04f,TrailWood);
        RestBeam("Rest drying rack rail",rack+new Vector3(-.55f,1.15f,0),rack+new Vector3(.55f,1.15f,0),.035f,TrailWood);
        for(int i=0;i<3;i++)RestShape("Rest folded trail towel",PrimitiveType.Cube,rack+new Vector3(-.30f+i*.30f,.96f,-.015f),new Vector3(.23f,.35f,.05f),i==1?new Color(.53f,.66f,.51f):TrailIvory);
        Vector3 notes=TP(108.15f,19.4f);
        RestShape("Rest patrol supply box",PrimitiveType.Cube,notes+Vector3.up*.24f,new Vector3(.68f,.46f,.48f),TrailWood);
        for(int i=0;i<2;i++){RestShape("Rest patrol notebook",PrimitiveType.Cube,notes+new Vector3(0,.49f+i*.045f,0),new Vector3(.42f,.035f,.28f),i==0?new Color(.50f,.30f,.22f):new Color(.70f,.65f,.47f));}
        for(int i=0;i<4;i++)RestShape("Rest patrol cord spool",PrimitiveType.Cylinder,notes+new Vector3(.20f,.55f+i*.035f,0),new Vector3(.13f,.016f,.13f),new Color(.60f,.29f,.23f));
        for(int stop=0;stop<2;stop++)for(int i=0;i<3;i++)
        {
            float x=(stop==0?86:106)+(i-1)*.65f;
            RestShape("Rest approach low stone",PrimitiveType.Cube,TP(x,stop==0?19.2f:18.1f,.07f),new Vector3(.55f,.12f,.46f),new Color(.56f,.56f,.45f));
        }
    }
    void TestTrailRestStops()
    {
        NewGame();modal=false;ChangeRegion(true);
        var renderers=trailRestRoot.GetComponentsInChildren<MeshRenderer>(true);Assert(renderers.Length>90&&renderers.Length<150,"rest stop dressing is a bounded one-time scene build");
        Assert(trailRestSigns.Count==3,"three route signposts mark entry bridge and lookout approach");
        for(int i=0;i<3;i++)Assert(Vector3.Distance(trailRestSigns[i].position,TP(RestSignPositions[i].x,RestSignPositions[i].y,.78f))<.001f,"route sign follows original hillside height");
        Assert(trailRestRoot.GetComponentsInChildren<TextMesh>().Length==6,"route boards have six visible direction labels");
        Assert(trailRestRoot.GetComponentsInChildren<ParticleSystem>(true).Length==0&&trailRestRoot.GetComponentsInChildren<Light>(true).Length==0,"rest props add no emitters or lighting workload");
        Assert(System.Array.TrueForAll(trailRestRoot.GetComponentsInChildren<Collider>(true),collider=>!collider.enabled),"rest decoration cannot add active physical collision");
        Assert(Walkable(TP(86,21))&&Walkable(TP(86,18.5f))&&Walkable(TP(106,18))&&Walkable(TP(105,20))&&Walkable(TP(107,20)),"lookout centre approach and original patrol rain refuge remain walkable");
        Assert(spots.Find(s=>s.kind==25).pos==TP(86,21)&&spots.Find(s=>s.kind==29).pos==TP(102,2),"lookout and red cord clue retain exact original interaction coordinates");
        string snapshot=JsonUtility.ToJson(data);int count=spots.Count;
        Assert(renderers.Length==trailRestRoot.GetComponentsInChildren<MeshRenderer>(true).Length&&count==spots.Count&&snapshot==JsonUtility.ToJson(data),"static rest scenery cannot create interactions spend materials or change progress");
        player.position=TP(86,21);nearest=SelectNearestSpot();Assert(nearest!=null&&nearest.kind==25,"lookout still selects original reading action");Interact(nearest);Assert(data.lookoutVisited&&modal,"original lookout discovery remains available");modal=false;
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(data.onTrail&&data.lookoutVisited&&trailRestSigns.Count==3,"rest dressing survives load without new progress fields or duplicate props");
        NextDay();modal=false;Assert(trailRestRoot.GetComponentsInChildren<MeshRenderer>(true).Length==renderers.Length,"crossing days does not rebuild or duplicate rest props");
        NewGame();modal=false;Assert(!data.lookoutVisited&&trailRestSigns.Count==3,"new journey resets only original discovery while keeping scenery");
        Debug.Log("QA REST STOPS PASS: bounded dressing, signs, heights, no added effects or active collisions, original lookouts clues refuges, readonly state, load and days.");
    }
}
