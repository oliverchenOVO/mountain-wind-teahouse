using UnityEngine;
using System.Collections.Generic;

public static class TeaHouseWorld
{
    static Dictionary<string, Material> mats = new Dictionary<string, Material>();
    public static Material Mat(string name, Color color)
    {
        if (mats.ContainsKey(name)) return mats[name];
        var m = new Material(Shader.Find("Standard")); m.color = color;
        m.SetFloat("_Glossiness", .05f); mats[name] = m; return m;
    }
    public static GameObject Shape(string name, PrimitiveType type, Vector3 p, Vector3 s, Color c, Transform parent = null)
    {
        var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.position = p; g.transform.localScale = s;
        if(parent) g.transform.SetParent(parent, true);
        g.GetComponent<Renderer>().sharedMaterial = Mat(c.ToString(), c);
        Object.Destroy(g.GetComponent<Collider>()); return g;
    }
    static Color grass = new Color(.30f,.43f,.31f), wood=new Color(.30f,.20f,.14f);
    public static GameObject Character(string name, Vector3 p)
    {
        var model = Resources.Load<GameObject>("Characters/"+name);
        GameObject g;
        if(model) { g=Object.Instantiate(model,p,Quaternion.identity); g.name=name; }
        else { g=new GameObject(name); g.transform.position=p; Shape("body",PrimitiveType.Capsule,p+Vector3.up, new Vector3(.7f,1,.7f),Color.white,g.transform); }
        // Imported FBX materials retain Blender's diffuse colors. Flat surfaces suit the miniature style.
        foreach(var r in g.GetComponentsInChildren<Renderer>())
        {
            var converted=new Material[r.sharedMaterials.Length];
            for(int i=0;i<converted.Length;i++) { var old=r.sharedMaterials[i]; converted[i]=Mat(name+"_"+old.name,old.color); }
            r.sharedMaterials=converted;
        }
        g.transform.localScale=Vector3.one*1.12f;
        g.AddComponent<AvatarMotion>();
        g.AddComponent<AvatarExpression>();
        return g;
    }
    public static void Build()
    {
        var root=new GameObject("妖怪之山 · 河童溪谷").transform;
        Shape("Surrounding foothills",PrimitiveType.Cube,new Vector3(0,-2.3f,0),new Vector3(110,1,100),new Color(.28f,.39f,.29f),root);
        Shape("Valley",PrimitiveType.Cube,new Vector3(0,-.4f,0),new Vector3(54,.8f,44),grass,root);
        Shape("Cliff base",PrimitiveType.Cube,new Vector3(0,-2,0),new Vector3(55,3,45),new Color(.31f,.34f,.29f),root);
        // River runs north/south; two footbridges provide safe crossings.
        Shape("River",PrimitiveType.Cube,new Vector3(8,.015f,0),new Vector3(4,.06f,43),new Color(.26f,.63f,.68f),root);
        for(int z=-21;z<22;z+=2)
        {
            Shape("River foam",PrimitiveType.Cube,new Vector3(8+Mathf.Sin(z)*.8f,.058f,z),new Vector3(1.1f,.014f,.08f),new Color(.65f,.84f,.80f),root);
            Rock(new Vector3(5.8f,0,z),root,.6f); Rock(new Vector3(10.2f,0,z),root,.65f);
        }
        for(int x=-23;x<23;x++) Shape("Mountain trail",PrimitiveType.Cube,new Vector3(x,.02f,-4),new Vector3(1.03f,.04f,3),new Color(.67f,.62f,.44f),root);
        for(int z=-16;z<16;z++) Shape("Garden trail",PrimitiveType.Cube,new Vector3(-5,.025f,z),new Vector3(2,.04f,1.04f),new Color(.67f,.62f,.44f),root);
        Bridge(-4,root); Bridge(12,root);
        // Teahouse: open front, tiled roof, noren curtain, porch and warm lanterns.
        Shape("Tea porch",PrimitiveType.Cube,new Vector3(-12,.18f,3),new Vector3(9,.35f,7),wood,root);
        Shape("Tea back wall",PrimitiveType.Cube,new Vector3(-12,1.8f,6.1f),new Vector3(8,3.2f,.3f),new Color(.85f,.78f,.60f),root);
        for(int x=-16;x<=-8;x+=4) for(int z=0;z<=6;z+=6)
            Shape("Cedar pillar",PrimitiveType.Cube,new Vector3(x,1.7f,z),new Vector3(.24f,3.2f,.24f),wood,root);
        for(int side=-1;side<=1;side+=2)
        {
            var roof=Shape("Jade tile roof",PrimitiveType.Cube,new Vector3(-12,3.8f,3+side*1.9f),new Vector3(10,.22f,4.4f),new Color(.16f,.30f,.29f),root);
            roof.transform.rotation=Quaternion.Euler(side*22,0,0);
        }
        for(int x=-15;x<=-9;x++) Shape("Noren",PrimitiveType.Cube,new Vector3(x,2.7f,-.1f),new Vector3(.92f,.66f,.05f),new Color(.64f,.28f,.24f),root);
        Shape("Tea counter",PrimitiveType.Cube,new Vector3(-12,.8f,-.5f),new Vector3(5,1.2f,.9f),new Color(.52f,.34f,.21f),root);
        for(int i=0;i<3;i++) Shape("Tea cups",PrimitiveType.Cylinder,new Vector3(-13+i,1.5f,-.5f),new Vector3(.2f,.12f,.2f),new Color(.85f,.89f,.72f),root);
        Lantern(new Vector3(-16,2,-.5f),root); Lantern(new Vector3(-8,2,-.5f),root);
        Shape("Tea sign post",PrimitiveType.Cylinder,new Vector3(-7.6f,.5f,-1.5f),new Vector3(.10f,.5f,.10f),wood,root);
        Shape("Tea sign",PrimitiveType.Cube,new Vector3(-7.6f,1.1f,-1.5f),new Vector3(1.2f,.5f,.10f),new Color(.42f,.39f,.31f),root);
        for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(-16+i*3,0,-8);
            Shape("Guest table",PrimitiveType.Cylinder,p+Vector3.up*.75f,new Vector3(1.45f,.10f,1.45f),wood,root);
            Shape("Table leg",PrimitiveType.Cylinder,p+Vector3.up*.35f,new Vector3(.15f,.35f,.15f),wood,root);
            for(int side=-1;side<=1;side+=2) Shape("Stool",PrimitiveType.Cylinder,p+new Vector3(0,.3f,side*1.15f),new Vector3(.65f,.3f,.65f),wood,root);
        }
        // Kappa workshop and mountain shrine.
        Shape("Workshop",PrimitiveType.Cube,new Vector3(15,1.1f,3),new Vector3(5,2.2f,4),new Color(.45f,.51f,.43f),root);
        Shape("Workshop roof",PrimitiveType.Cube,new Vector3(15,2.35f,3),new Vector3(5.6f,.3f,4.6f),new Color(.25f,.40f,.41f),root);
        Shape("Practice courtyard",PrimitiveType.Cube,new Vector3(16,.032f,12),new Vector3(10,.06f,10),new Color(.58f,.60f,.43f),root);
        for(int side=-1;side<=1;side+=2)
        {
            Shape("Practice boundary",PrimitiveType.Cube,new Vector3(16+side*5,.075f,12),new Vector3(.10f,.03f,10),new Color(.81f,.66f,.36f),root);
            Shape("Practice boundary",PrimitiveType.Cube,new Vector3(16,.075f,12+side*5),new Vector3(10,.03f,.10f),new Color(.81f,.66f,.36f),root);
        }
        for(int i=0;i<3;i++) Shape("Workshop crate",PrimitiveType.Cube,new Vector3(12+i,.45f,-.5f),Vector3.one*.8f,wood,root);
        for(int side=-1;side<=1;side+=2) Shape("Torii post",PrimitiveType.Cylinder,new Vector3(-5+side*2,1.8f,15),new Vector3(.35f,1.8f,.35f),new Color(.66f,.24f,.19f),root);
        Shape("Torii beam",PrimitiveType.Cube,new Vector3(-5,3.7f,15),new Vector3(5.8f,.3f,.4f),new Color(.66f,.24f,.19f),root);
        Shape("Torii lower",PrimitiveType.Cube,new Vector3(-5,2.9f,15),new Vector3(4.6f,.2f,.3f),wood,root);
        // Distant mountains and waterfall are visible behind the playable garden.
        for(int i=0;i<10;i++)
        {
            var p=new Vector3(-28+i*6,2,24+Mathf.Sin(i)*3);
            Shape("Mountain",PrimitiveType.Sphere,p,new Vector3(9,8+i%3*2,7),new Color(.32f+i%3*.025f,.43f,.39f),root);
        }
        Shape("Waterfall",PrimitiveType.Cube,new Vector3(8,4.5f,21),new Vector3(2.6f,9,.14f),new Color(.55f,.78f,.78f),root);
        Random.InitState(712);
        for(int i=0;i<95;i++)
        {
            Vector3 p=new Vector3(Random.Range(-25f,25f),0,Random.Range(-20f,20f));
            bool edge=Mathf.Abs(p.x)>21||Mathf.Abs(p.z)>17;
            bool free=Mathf.Abs(p.z+4)>3 && Mathf.Abs(p.x+5)>2 && Mathf.Abs(p.x-8)>4;
            bool buildings=(p.x>-18&&p.x<-6&&p.z>-11&&p.z<8)||(p.x>11&&p.x<19&&p.z>-3&&p.z<7);
            bool arena=p.x>9&&p.x<23&&p.z>6&&p.z<19;
            if((edge||free)&&!buildings&&!arena) Tree(p,root,Random.Range(.75f,1.3f));
        }
        for(int i=0;i<100;i++)
        {
            Vector3 p=new Vector3(Random.Range(-24f,24f),0,Random.Range(-19f,19f));
            if(Mathf.Abs(p.x-8)<3 || Mathf.Abs(p.z+4)<2) continue;
            Shape("Wildflower",PrimitiveType.Sphere,p+Vector3.up*.18f,new Vector3(.13f,.17f,.13f),i%3==0?new Color(.96f,.77f,.48f):new Color(.77f,.70f,.82f),root);
        }
    }
    static void Bridge(float z,Transform root)
    {
        for(int i=0;i<12;i++) Shape("Bridge plank",PrimitiveType.Cube,new Vector3(5.6f+i*.44f,.14f,z),new Vector3(.40f,.24f,2.6f),new Color(.52f,.36f,.23f),root);
        for(int side=-1;side<=1;side+=2)
        {
            Shape("Bridge rail",PrimitiveType.Cube,new Vector3(8,.8f,z+side*1.2f),new Vector3(5.3f,.12f,.12f),wood,root);
            for(int x=6;x<=10;x+=2) Shape("Bridge post",PrimitiveType.Cube,new Vector3(x,.45f,z+side*1.2f),new Vector3(.14f,.9f,.14f),wood,root);
        }
    }
    static void Rock(Vector3 p,Transform root,float s) { Shape("River stone",PrimitiveType.Sphere,p+Vector3.up*.15f,new Vector3(s,.5f,s*.8f),new Color(.49f,.53f,.47f),root); }
    static void Tree(Vector3 p,Transform root,float s)
    {
        Shape("Cedar trunk",PrimitiveType.Cylinder,p+Vector3.up*s,new Vector3(.24f,1.1f,.24f)*s,wood,root);
        for(int i=0;i<3;i++) Shape("Cedar canopy",PrimitiveType.Sphere,p+Vector3.up*(1.8f+i*.65f)*s,new Vector3(2-i*.35f,1.35f,2-i*.35f)*s,new Color(.17f+i*.035f,.34f+i*.035f,.25f),root);
    }
    static void Lantern(Vector3 p,Transform root)
    {
        var g=Shape("Lantern",PrimitiveType.Sphere,p,new Vector3(.5f,.7f,.5f),new Color(1,.72f,.36f),root);
        var light=g.AddComponent<Light>(); light.type=LightType.Point; light.color=new Color(1,.61f,.25f); light.intensity=1.1f; light.range=6;
    }
}
