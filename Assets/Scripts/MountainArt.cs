using UnityEngine;
using System.Collections.Generic;

// All structures share the original interaction coordinates and save IDs.
public static class MountainArt
{
    static Transform root;
    static Color timber=new Color(.26f,.16f,.11f), cedar=new Color(.46f,.29f,.17f), ivory=new Color(.93f,.86f,.67f), tile=new Color(.18f,.33f,.32f);
    public static float Height(float x,float z)
    {
        // The shrine trail climbs continuously; the river, courtyard and tea terrace remain level.
        return Mathf.SmoothStep(0,1,Mathf.InverseLerp(7,19,z))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,1,x))*2.4f;
    }
    static Vector3 Ground(float x,float z,float y=0){return new Vector3(x,Height(x,z)+y,z);}
    static GameObject S(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color,Transform parent=null)
    {return TeaHouseWorld.Shape(name,type,p,scale,color,parent?parent:root);}
    static GameObject MeshObject(string name,Mesh mesh,Material material)
    {
        var g=new GameObject(name);g.transform.SetParent(root);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;return g;
    }
    public static void Build()
    {
        root=new GameObject("山風茶屋 · 第二版美術").transform;
        Terrain();River();Trails();TeaHouse();Workshop();Shrine();Forest();Details();
        var batch=new List<GameObject>();
        foreach(var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            if(MountainTeaGame.IsViewCanopy(r.name)||MountainTeaGame.IsTeaRoof(r.name)||r.name=="Paper lantern"||r.name=="Tea sign"||r.GetComponent<TextMesh>()||r.GetComponent<NorenWind>()||r.GetComponent<WaterRibbon>()||r.GetComponentInParent<WaterwheelSpin>())continue;
            batch.Add(r.gameObject);
        }
        StaticBatchingUtility.Combine(batch.ToArray(),root.gameObject);
        var ambience=new GameObject("溪谷的風與水").AddComponent<ValleyAtmosphere>();
        ambience.Build();
    }
    static void Terrain()
    {
        var verts=new List<Vector3>();var cols=new List<Color>();var tris=new List<int>();
        for(int x=-42;x<42;x+=2)for(int z=-34;z<34;z+=2)
        {
            Vector3[] p={Ground(x,z),Ground(x,z+2),Ground(x+2,z+2),Ground(x+2,z)};
            for(int t=0;t<6;t++)
            {
                int index=new[]{0,1,2,0,2,3}[t];Vector3 v=p[index];
                if(v.x>=6&&v.x<=10)v.y=-.48f;
                if(Mathf.Abs(v.x)>27||Mathf.Abs(v.z)>22)v.y-=.5f;
                float noise=Mathf.PerlinNoise(v.x*.11f+27,v.z*.11f+11);
                verts.Add(v);cols.Add(Color.Lerp(new Color(.26f,.40f,.27f),new Color(.51f,.60f,.35f),noise));tris.Add(tris.Count);
            }
        }
        var mesh=new Mesh{name="Meadow elevation and vertex colors"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.SetColors(cols);mesh.RecalculateNormals();
        MeshObject("Mountain meadow",mesh,new Material(Resources.Load<Shader>("Shaders/Meadow")));
        Random.InitState(24);
        for(int i=0;i<18;i++)
        {
            float x=-37+i*4.3f;float z=23+Random.Range(0,5);
            if(x>3&&x<13)continue;
            Facet("North rock face",new Vector3(x,1.5f,z),new Vector3(4.3f,5+Random.value*4,3.2f),new Color(.34f,.39f,.32f),9);
            Facet("Mossy cliff crown",new Vector3(x,5.7f,z),new Vector3(4.4f,1.3f,3.3f),new Color(.35f,.47f,.30f),9);
            if(i%2==0) Pine(new Vector3(x,6.2f,z),1.3f);
        }
        for(int i=0;i<8;i++)Facet("Distant mountain",new Vector3(-37+i*10,4,34+i%2*3),new Vector3(10,9+i%3*3,7),new Color(.33f,.43f,.39f),7);
        S("Waterfall back wall",PrimitiveType.Cube,new Vector3(8,3.5f,23.5f),new Vector3(6,7.5f,1),new Color(.37f,.44f,.34f));
        for(int i=0;i<4;i++)Facet("Waterfall lip",new Vector3(5.7f+i*1.5f,7.3f,23),new Vector3(.85f,.38f,.7f),new Color(.47f,.53f,.39f),7);
        for(int i=0;i<12;i++)Facet("Western rock terrace",Ground(-26-i%2*2,-20+i*3.6f,.3f),new Vector3(2.2f,2.5f+i%3,2.5f),new Color(.39f,.43f,.32f),8);
    }
    static void River()
    {
        var mesh=new Mesh();var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
        for(int z=-32;z<=26;z++)
        {
            float bend=Mathf.Sin(z*.25f)*.09f;
            v.Add(new Vector3(6.05f+bend,-.12f,z));v.Add(new Vector3(9.95f+bend,-.12f,z));uv.Add(new Vector2(0,z*.2f));uv.Add(new Vector2(1,z*.2f));
            if(z>-32){int a=v.Count-4;t.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
        }
        mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();
        var water=new Material(Resources.Load<Shader>("Shaders/Water"));MeshObject("Flowing jade stream",mesh,water);
        Random.InitState(183);
        for(float z=-26;z<23;z+=.9f)
        {
            for(int side=-1;side<=1;side+=2)
            {
                float x=8+side*(2.12f+Random.value*.3f);
                if(Mathf.Abs(z+4)<1.6f||Mathf.Abs(z-12)<1.6f)continue;
                Facet("Rounded river stones",new Vector3(x,-.06f,z),new Vector3(.36f+Random.value*.34f,.24f+Random.value*.35f,.35f+Random.value*.25f),new Color(.49f,.52f,.43f),7);
                if(Random.value>.65f) Reed(Ground(x+side*.28f,z),.7f);
            }
        }
        Bridge(-4);Bridge(12);RiverBankDetails();
        // Animated ribbons spill between the rock walls, with spray at their foot.
        for(int i=0;i<5;i++)
        {
            var fall=S("Waterfall ribbon",PrimitiveType.Cube,new Vector3(7+i*.48f,3.9f,22.15f),new Vector3(.47f,7.8f,.11f),new Color(.70f,.87f,.81f));
            fall.AddComponent<WaterRibbon>().phase=i;
        }
        S("Waterfall basin",PrimitiveType.Cylinder,new Vector3(8,-.10f,20.8f),new Vector3(5,.015f,3.2f),new Color(.29f,.59f,.57f));
        for(int i=0;i<5;i++)Facet("Waterfall outcrop",new Vector3(5.5f+i*1.15f,.20f,20.9f+Mathf.Sin(i)),new Vector3(.65f,.55f,.62f),new Color(.41f,.49f,.40f),8);
    }
    static void Ribbon(string name,List<Vector3> centers,float width,Color c)
    {
        var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<centers.Count;i++)
        {
            Vector3 tangent=centers[Mathf.Min(i+1,centers.Count-1)]-centers[Mathf.Max(0,i-1)];tangent.y=0;
            Vector3 side=Vector3.Cross(Vector3.up,tangent.normalized)*(width+Mathf.Sin(i*1.7f)*.08f);
            for(int j=-1;j<=1;j+=2){Vector3 p=centers[i]+side*j;p.y=Height(p.x,p.z)+.022f;v.Add(p);}
            if(i>0){int a=v.Count-4;t.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
        }
        var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();MeshObject(name,mesh,TeaHouseWorld.Mat(name,c));
    }
    static void Trails()
    {
        var centers=new List<Vector3>();for(int x=-26;x<=26;x++)centers.Add(Ground(x,-4));Ribbon("Ochre mountain path",centers,1.10f,new Color(.69f,.60f,.40f));
        centers.Clear();for(int z=-20;z<=20;z++)centers.Add(Ground(-5+Mathf.Sin(z*.25f)*.12f,z));Ribbon("Climbing shrine trail",centers,.84f,new Color(.67f,.59f,.39f));
        centers.Clear();for(int i=0;i<=10;i++)centers.Add(Ground(-5-i*.75f,-4+i*.14f));Ribbon("Teahouse approach",centers,.75f,new Color(.69f,.60f,.42f));
        Random.InitState(68);
        for(int i=0;i<36;i++)
        {
            float x=-17.7f+(i%9)*1.28f,z=-10.7f+(i/9)*1.18f;
            Facet("Tea garden paving",Ground(x,z,.06f),new Vector3(.69f,.075f,.63f),new Color(.64f+Random.value*.06f,.61f,.48f),6);
        }
        for(int i=0;i<20;i++)Facet("Pathside pebble",Ground(-22+i*2.3f,-5.25f,.03f),new Vector3(.19f,.13f,.16f),new Color(.66f,.62f,.49f),6);
        for(int i=0;i<9;i++)S("Shrine foothold",PrimitiveType.Cube,Ground(-5,10+i*.8f,.06f),new Vector3(1.35f,.10f,.22f),new Color(.51f,.51f,.42f));
    }
    static void TeaHouse()
    {
        S("Raised tea foundation",PrimitiveType.Cube,new Vector3(-12,.10f,3),new Vector3(9,.20f,7),new Color(.45f,.43f,.34f));
        for(int i=0;i<24;i++)S("Cedar floorboard",PrimitiveType.Cube,new Vector3(-16.35f+i*.38f,.26f,3),new Vector3(.35f,.15f,6.8f),i%3==0?new Color(.48f,.31f,.19f):cedar);
        S("Tea back wall",PrimitiveType.Cube,new Vector3(-12,1.7f,6.1f),new Vector3(8,2.9f,.18f),ivory);
        for(int x=-16;x<=-8;x+=4)for(int z=0;z<=6;z+=6)S("Polished cedar pillar",PrimitiveType.Cube,new Vector3(x,1.75f,z),new Vector3(.23f,3.2f,.23f),timber);
        for(int z=1;z<=5;z+=2)for(int side=-1;side<=1;side+=2)
        {
            S("Shoji paper",PrimitiveType.Cube,new Vector3(-12+side*4,1.7f,z),new Vector3(.08f,2.2f,1.8f),ivory);
            for(int j=0;j<5;j++)S("Shoji lattice",PrimitiveType.Cube,new Vector3(-12+side*4,1.7f,z-.9f+j*.45f),new Vector3(.11f,2.3f,.045f),cedar);
            for(int j=0;j<4;j++)S("Shoji crossbar",PrimitiveType.Cube,new Vector3(-12+side*4,.65f+j*.7f,z),new Vector3(.11f,.04f,1.85f),cedar);
        }
        for(int side=-1;side<=1;side+=2)
        {
            var roof=S("Ceramic roof base",PrimitiveType.Cube,new Vector3(-12,3.95f,3+side*1.9f),new Vector3(10,.16f,4.3f),tile);roof.transform.rotation=Quaternion.Euler(side*20,0,0);
            for(int col=0;col<24;col++)for(int row=0;row<4;row++)
            {
                float offset=.45f+row*.95f;float y=4.64f-offset*.34f;
                var roofTile=S("Individual ceramic tile",PrimitiveType.Cylinder,new Vector3(-16.6f+col*.40f,y,3+side*offset),new Vector3(.34f,.52f,.12f),col%4==0?new Color(.23f,.40f,.37f):tile);
                roofTile.transform.rotation=Quaternion.Euler(90+side*20,0,0);
            }
        }
        S("Roof ridge",PrimitiveType.Cylinder,new Vector3(-12,4.7f,3),new Vector3(.23f,5.1f,.23f),tile).transform.rotation=Quaternion.Euler(0,0,90);
        S("Front cedar lintel",PrimitiveType.Cube,new Vector3(-12,3.35f,-.25f),new Vector3(9,.20f,.24f),timber);
        for(int x=-15;x<=-9;x++)
        {
            var curtain=S("Indigo noren",PrimitiveType.Cube,new Vector3(x,2.98f,-.28f),new Vector3(.94f,.62f,.025f),new Color(.21f,.37f,.40f));curtain.AddComponent<NorenWind>().phase=x;
            S("Curtain cream crest",PrimitiveType.Cylinder,new Vector3(x,2.96f,-.32f),new Vector3(.18f,.009f,.18f),ivory).transform.rotation=Quaternion.Euler(90,0,0);
        }
        S("Tea counter",PrimitiveType.Cube,new Vector3(-12,.78f,-.55f),new Vector3(5,1.25f,.82f),cedar);
        S("Countertop",PrimitiveType.Cube,new Vector3(-12,1.44f,-.55f),new Vector3(5.2f,.14f,.97f),timber);
        for(int i=0;i<12;i++)S("Counter joinery",PrimitiveType.Cube,new Vector3(-14.3f+i*.42f,.8f,-.98f),new Vector3(.055f,1.16f,.055f),timber);
        for(int i=0;i<3;i++)Cup(new Vector3(-13.3f+i*.65f,1.55f,-.55f));
        S("Teapot",PrimitiveType.Sphere,new Vector3(-11.1f,1.69f,-.55f),new Vector3(.43f,.35f,.43f),new Color(.32f,.48f,.33f));
        S("Teapot lid",PrimitiveType.Cylinder,new Vector3(-11.1f,1.88f,-.55f),new Vector3(.26f,.035f,.26f),new Color(.32f,.48f,.33f));
        S("Teapot spout",PrimitiveType.Capsule,new Vector3(-10.83f,1.70f,-.55f),new Vector3(.10f,.18f,.10f),new Color(.32f,.48f,.33f)).transform.rotation=Quaternion.Euler(0,0,-45);
        S("Cooking stove",PrimitiveType.Cube,new Vector3(-9.2f,.67f,1.9f),new Vector3(1.25f,.78f,1),new Color(.40f,.36f,.29f));
        S("Cooking pot",PrimitiveType.Cylinder,new Vector3(-9.2f,1.19f,1.9f),new Vector3(.85f,.18f,.85f),new Color(.19f,.22f,.22f));
        S("Chimney",PrimitiveType.Cube,new Vector3(-9.4f,4.55f,5),new Vector3(.50f,2,.55f),new Color(.41f,.37f,.31f));
        Lantern(new Vector3(-16,2.4f,-.7f));Lantern(new Vector3(-8,2.4f,-.7f));
        S("Tea sign post",PrimitiveType.Cylinder,new Vector3(-7.6f,.6f,-1.5f),new Vector3(.12f,.6f,.12f),timber);
        S("Tea sign",PrimitiveType.Cube,new Vector3(-7.6f,1.35f,-1.5f),new Vector3(1.6f,.65f,.13f),new Color(.42f,.39f,.31f));
        WorldText("山風茶屋",new Vector3(-12,3.6f,-.5f),.30f,ivory);
        WorldText("山風",new Vector3(-7.6f,1.4f,-1.59f),.14f,ivory);
        for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(-16+i*3,0,-8);
            S("Guest table",PrimitiveType.Cylinder,p+Vector3.up*.83f,new Vector3(1.65f,.10f,1.65f),cedar);
            S("Table leg",PrimitiveType.Cylinder,p+Vector3.up*.4f,new Vector3(.15f,.4f,.15f),timber);
            S("Bamboo table mat",PrimitiveType.Cube,p+new Vector3(0,.95f,0),new Vector3(.9f,.015f,.60f),new Color(.76f,.67f,.45f));
            Cup(p+new Vector3(.37f,.98f,-.05f));
            for(int side=-1;side<=1;side+=2)
            {
                S("Stool",PrimitiveType.Cylinder,p+new Vector3(0,.32f,side*1.15f),new Vector3(.67f,.3f,.67f),timber);
                S("Cushion",PrimitiveType.Cylinder,p+new Vector3(0,.65f,side*1.15f),new Vector3(.70f,.06f,.70f),new Color(.63f,.28f,.23f));
            }
        }
        Parasol(new Vector3(-19,0,-12),new Color(.65f,.25f,.20f));
        Parasol(new Vector3(-6.9f,0,-12),new Color(.74f,.61f,.35f));
        for(int i=0;i<7;i++)
        {
            float z=-10+i*1.8f;S("Garden fence post",PrimitiveType.Cylinder,new Vector3(-18.4f,.65f,z),new Vector3(.12f,.65f,.12f),cedar);
        }
        for(int i=0;i<2;i++)S("Garden bamboo fence",PrimitiveType.Cylinder,new Vector3(-18.4f,.4f+i*.45f,-4.6f),new Vector3(.07f,5.6f,.07f),cedar).transform.rotation=Quaternion.Euler(90,0,0);
        for(int i=0;i<4;i++)
        {
            Vector3 p=new Vector3(-16.8f+i*.5f,.43f,5.1f);S("Storage basket",PrimitiveType.Cylinder,p,new Vector3(.39f,.22f,.39f),new Color(.72f,.55f,.31f));
        }
    }
    static void Cup(Vector3 p)
    {
        S("Tea saucer",PrimitiveType.Cylinder,p,new Vector3(.30f,.017f,.30f),ivory);
        S("Ceramic cup",PrimitiveType.Cylinder,p+Vector3.up*.09f,new Vector3(.20f,.09f,.20f),new Color(.79f,.83f,.67f));
        S("Tea surface",PrimitiveType.Cylinder,p+Vector3.up*.184f,new Vector3(.16f,.008f,.16f),new Color(.38f,.44f,.17f));
    }
    static void Parasol(Vector3 p,Color c)
    {
        S("Parasol pole",PrimitiveType.Cylinder,p+Vector3.up*1.5f,new Vector3(.08f,1.5f,.08f),timber);
        Facet("Paper parasol",p+Vector3.up*3.1f,new Vector3(1.8f,.45f,1.8f),c,12);
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI*2/12;Beam("Parasol bamboo rib",p+Vector3.up*3.5f,p+new Vector3(Mathf.Cos(a)*1.75f,3.08f,Mathf.Sin(a)*1.75f),.024f,ivory);
        }
    }
    static void Workshop()
    {
        S("Workshop",PrimitiveType.Cube,new Vector3(15,1.1f,3),new Vector3(5,2.2f,4),new Color(.56f,.58f,.42f));
        S("Workshop roof",PrimitiveType.Cube,new Vector3(15,2.35f,3),new Vector3(5.6f,.3f,4.6f),tile);
        for(int i=0;i<12;i++)S("Workshop roof seam",PrimitiveType.Cube,new Vector3(12.4f+i*.48f,2.54f,3),new Vector3(.06f,.06f,4.6f),new Color(.23f,.40f,.38f));
        S("Workshop door",PrimitiveType.Cube,new Vector3(15,1, .95f),new Vector3(1.1f,1.8f,.10f),timber);
        WorldText("河童工房",new Vector3(15,2.07f,.85f),.18f,ivory);
        for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(12+i,.45f,-.5f);S("Wood crate",PrimitiveType.Cube,p,Vector3.one*.8f,cedar);
            for(int j=-1;j<=1;j+=2)S("Crate strap",PrimitiveType.Cube,p+new Vector3(j*.25f,0,-.42f),new Vector3(.05f,.8f,.035f),timber);
        }
        var wheel=new GameObject("Kappa waterwheel");wheel.transform.SetParent(root);wheel.transform.position=new Vector3(11.3f,1.4f,4.8f);wheel.AddComponent<WaterwheelSpin>();
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI*2/12;
            var spoke=S("Wheel spoke",PrimitiveType.Cube,wheel.transform.position+new Vector3(0,Mathf.Cos(a)*.70f,Mathf.Sin(a)*.70f),new Vector3(.18f,1.45f,.09f),cedar,wheel.transform);spoke.transform.rotation=Quaternion.Euler(i*30,0,0);
            var paddle=S("Wheel paddle",PrimitiveType.Cube,wheel.transform.position+new Vector3(0,Mathf.Cos(a)*1.5f,Mathf.Sin(a)*1.5f),new Vector3(.65f,.13f,.4f),timber,wheel.transform);paddle.transform.rotation=Quaternion.Euler(i*30,0,0);
        }
        for(int side=-1;side<=1;side+=2)S("Wheel axle",PrimitiveType.Cube,new Vector3(11.3f+side*.6f,.9f,4.8f),new Vector3(.15f,1.8f,.2f),timber);
        S("Practice courtyard",PrimitiveType.Cube,new Vector3(16,.032f,12),new Vector3(10,.06f,10),new Color(.63f,.63f,.47f));
        for(int side=-1;side<=1;side+=2)
        {
            S("Practice boundary",PrimitiveType.Cube,new Vector3(16+side*5,.075f,12),new Vector3(.10f,.03f,10),new Color(.81f,.66f,.36f));
            S("Practice boundary",PrimitiveType.Cube,new Vector3(16,.075f,12+side*5),new Vector3(10,.03f,.10f),new Color(.81f,.66f,.36f));
        }
    }
    static void Shrine()
    {
        for(int side=-1;side<=1;side+=2)
        {
            Vector3 p=Ground(-5+side*2,15);S("Torii post",PrimitiveType.Cylinder,p+Vector3.up*1.8f,new Vector3(.30f,1.8f,.30f),new Color(.68f,.23f,.16f));
            S("Torii stone foot",PrimitiveType.Cylinder,p+Vector3.up*.12f,new Vector3(.6f,.12f,.6f),new Color(.49f,.50f,.41f));
        }
        S("Torii beam",PrimitiveType.Cube,Ground(-5,15,3.7f),new Vector3(5.8f,.26f,.38f),new Color(.68f,.23f,.16f));
        S("Torii crown",PrimitiveType.Cube,Ground(-5,15,3.92f),new Vector3(6.2f,.14f,.45f),timber);
        S("Torii lower",PrimitiveType.Cube,Ground(-5,15,2.9f),new Vector3(4.6f,.17f,.25f),new Color(.68f,.23f,.16f));
        for(int i=0;i<5;i++)S("Sacred rope tassel",PrimitiveType.Cube,Ground(-6.5f+i*.7f,14.8f,2.55f),new Vector3(.09f,.35f,.03f),ivory).transform.rotation=Quaternion.Euler(0,0,(i%2==0?20:-20));
        for(int side=-1;side<=1;side+=2)
        {
            Vector3 p=Ground(-5+side*3,17);S("Stone lantern base",PrimitiveType.Cube,p+Vector3.up*.15f,new Vector3(.85f,.3f,.85f),new Color(.49f,.51f,.40f));
            S("Stone lantern column",PrimitiveType.Cylinder,p+Vector3.up*.75f,new Vector3(.28f,.55f,.28f),new Color(.53f,.55f,.43f));
            S("Stone lantern chamber",PrimitiveType.Cube,p+Vector3.up*1.35f,new Vector3(.62f,.45f,.62f),ivory);
            Facet("Stone lantern cap",p+Vector3.up*1.67f,new Vector3(.62f,.18f,.62f),new Color(.44f,.48f,.38f),4);
        }
    }
    static bool Clear(Vector3 p)
    {
        if(Mathf.Abs(p.z+4)<2.3f||Mathf.Abs(p.x+5)<1.8f||Mathf.Abs(p.x-8)<3.8f)return false;
        if(p.x>-20&&p.x<-5&&p.z>-13&&p.z<8)return false;
        if(p.x>10&&p.x<23&&p.z>-.8f&&p.z<19)return false;
        if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(-3,6))<2.5f)return false;
        Vector2[] forage={new Vector2(-2,-9),new Vector2(1,-11),new Vector2(-7,9),new Vector2(-12,12),new Vector2(2,9),new Vector2(-17,12),new Vector2(12,-10),new Vector2(17,-9),new Vector2(20,6),new Vector2(-19,-14),new Vector2(-11,-14),new Vector2(1,15),new Vector2(12,9),new Vector2(19,15),new Vector2(-19,5),new Vector2(-1,1),new Vector2(-9,10),new Vector2(18,-14),new Vector2(-2,-15),new Vector2(3,4),new Vector2(13,-14),new Vector2(-20,-8),new Vector2(-15,10),new Vector2(20,0)};
        foreach(var q in forage)if(Vector2.Distance(new Vector2(p.x,p.z),q)<1.5f)return false;
        return true;
    }
    static void Forest()
    {
        Random.InitState(318);
        for(int i=0;i<240;i++)
        {
            Vector3 p=Ground(Random.Range(-30f,30f),Random.Range(-24f,23f));if(!Clear(p))continue;
            float s=Random.Range(.72f,1.3f);
            if(i%5==0)Maple(p,s);else if(i%5==1)Bamboo(p,s);else Pine(p,s);
        }
        // A warm maple frames the terrace without covering the player or tables.
        Maple(Ground(-20,-4),1.25f);Maple(Ground(-4,4),.85f);Bamboo(Ground(-18,8),1.1f);Bamboo(Ground(3,-14),.85f);
    }
    static void Pine(Vector3 p,float s)
    {
        S("Cedar trunk",PrimitiveType.Cylinder,p+Vector3.up*1.5f*s,new Vector3(.22f,1.5f,.22f)*s,timber);
        for(int i=0;i<4;i++)
        {
            float width=(1.65f-i*.29f)*s;
            Facet("Faceted cedar canopy",p+Vector3.up*(2+i*.65f)*s,new Vector3(width,.95f*s,width),new Color(.17f+i*.025f,.33f+i*.035f,.23f+i*.018f),9);
        }
    }
    static void Maple(Vector3 p,float s)
    {
        S("Maple trunk",PrimitiveType.Cylinder,p+Vector3.up*1.6f*s,new Vector3(.22f,1.6f,.22f)*s,cedar);
        for(int i=0;i<5;i++)
        {
            float a=i*Mathf.PI*2/5;Vector3 tip=p+new Vector3(Mathf.Cos(a)*.85f,2.4f,Mathf.Sin(a)*.85f)*s;
            Beam("Maple branch",p+Vector3.up*s,tip,.09f*s,cedar);
            Facet("Autumn maple crown",tip+Vector3.up*.5f*s,new Vector3(1.0f,.8f,1.0f)*s,i%2==0?new Color(.74f,.38f,.23f):new Color(.84f,.53f,.25f),8);
        }
    }
    static void Bamboo(Vector3 p,float s)
    {
        for(int i=0;i<4;i++)
        {
            Vector3 q=p+new Vector3(Mathf.Sin(i*2)*.5f,0,Mathf.Cos(i*2)*.5f);float h=(2.6f+i*.22f)*s;
            S("Bamboo stalk",PrimitiveType.Cylinder,q+Vector3.up*h*.5f,new Vector3(.10f,h*.5f,.10f),new Color(.38f,.51f,.25f));
            for(int j=0;j<5;j++)S("Bamboo joint",PrimitiveType.Cylinder,q+Vector3.up*(j*.55f+.3f)*s,new Vector3(.13f,.035f,.13f),new Color(.61f,.65f,.35f));
            for(int j=0;j<3;j++)Facet("Bamboo leaf spray",q+new Vector3((j-1)*.48f,h-.3f+j*.1f,0),new Vector3(.57f,.13f,.30f)*s,new Color(.35f,.52f,.25f),6);
        }
    }
    static void Details()
    {
        Random.InitState(100);
        for(int i=0;i<560;i++)
        {
            Vector3 p=Ground(Random.Range(-24f,24f),Random.Range(-20f,20f));
            if(Mathf.Abs(p.x-8)<3||Mathf.Abs(p.z+4)<1.5f||Mathf.Abs(p.x+5)<1.1f||p.x>10&&p.z>6||p.x>-18&&p.x<-7&&p.z>-11&&p.z<7)continue;
            if(i%3==0)Reed(p,Random.Range(.18f,.4f));
            else
            {
                Color c=i%7==0?new Color(.87f,.71f,.36f):new Color(.55f,.67f,.38f);
                Facet("Meadow leaf",p+Vector3.up*.06f,new Vector3(.15f,.07f,.10f),c,5);
            }
        }
        for(int i=0;i<16;i++)
        {
            Vector3 p=Ground(-22+i%4*.8f,7+i/4*.75f,.07f);
            Facet("Moss stone garden",p,new Vector3(.42f,.25f,.39f),new Color(.46f,.53f,.34f),8);
        }
        Vector3 f=Ground(5,-11,.03f);Facet("Fishing ledge",f,new Vector3(1.2f,.13f,1.25f),new Color(.65f,.63f,.49f),8);
        S("Fishing basket",PrimitiveType.Cylinder,Ground(4,-11,.28f),new Vector3(.45f,.27f,.45f),new Color(.66f,.47f,.27f));
        Beam("Fishing rod",Ground(4.5f,-12,.1f),new Vector3(6,1.5f,-11),.025f,timber);
    }
    static void Reed(Vector3 p,float h)
    {
        for(int i=0;i<3;i++)
        {
            Vector3 end=p+new Vector3((i-1)*.12f,h*(.8f+i*.1f),.04f);
            Beam("Grass blade",p,end,.017f,new Color(.48f,.57f,.28f));
        }
    }
    static void RiverBankDetails()
    {
        // Small wet shelves stop below the walking bank; no new colliders or crossings.
        for(int side=-1;side<=1;side+=2)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int z=-20;z<19;z++)
            {
                if(Mathf.Abs(z+4)<2||Mathf.Abs(z-12)<2||side<0&&Mathf.Abs(z+11)<2)continue;
                float edge=8+side*2.02f,outer=edge+side*(.23f+Mathf.Sin(z*.83f)*.065f);int a=vertices.Count;
                vertices.Add(new Vector3(edge,-.095f,z));vertices.Add(new Vector3(outer,-.04f,z));vertices.Add(new Vector3(outer+Mathf.Sin(z)*.03f,-.04f,z+1));vertices.Add(new Vector3(edge,-.095f,z+1));
                if(side>0)triangles.AddRange(new[]{a,a+2,a+1,a,a+3,a+2});else triangles.AddRange(new[]{a,a+1,a+2,a,a+2,a+3});
            }
            var mesh=new Mesh{name="Irregular wet river bank"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();MeshObject("Wet river bank shelf",mesh,TeaHouseWorld.Mat("Wet river silt",new Color(.35f,.40f,.30f)));
            for(int i=0;i<13;i++)
            {
                float z=-18+i*2.8f;if(Mathf.Abs(z+4)<2||Mathf.Abs(z-12)<2||side<0&&Mathf.Abs(z+11)<2)continue;
                Vector3 p=new Vector3(8+side*2.05f,-.03f,z);
                Facet("Low wet river pebble",p,new Vector3(.28f,.11f,.32f),new Color(.37f,.43f,.38f),6);
                Facet("River pebble moss",p+new Vector3(side*.07f,.08f,.025f),new Vector3(.20f,.026f,.22f),new Color(.40f,.52f,.30f),6);
                if(i%3==0)Reed(p+new Vector3(side*.31f,.05f,.24f),.42f);
            }
        }
    }
    static void Bridge(float z)
    {
        for(int i=0;i<16;i++)
        {
            S("Bridge weathered plank",PrimitiveType.Cube,new Vector3(5.5f+i*.33f,.13f,z),new Vector3(.30f,.22f,2.45f),i%3==0?new Color(.54f,.37f,.22f):cedar);
            for(int side=-1;side<=1;side+=2)S("Bridge iron nail",PrimitiveType.Cylinder,new Vector3(5.5f+i*.33f,.247f,z+side*.83f),new Vector3(.043f,.007f,.043f),new Color(.28f,.28f,.23f));
        }
        for(int side=-1;side<=1;side+=2)
        {
            for(int x=6;x<=10;x+=2)S("Bridge post",PrimitiveType.Cube,new Vector3(x,.48f,z+side*1.17f),new Vector3(.13f,.96f,.13f),timber);
            for(int x=6;x<=10;x+=2)for(int wrap=0;wrap<3;wrap++)S("Bridge hemp joint",PrimitiveType.Cylinder,new Vector3(x,.86f+wrap*.035f,z+side*1.17f),new Vector3(.20f,.009f,.20f),new Color(.66f,.56f,.36f));
            S("Bridge top rail",PrimitiveType.Cube,new Vector3(8,.94f,z+side*1.17f),new Vector3(5.3f,.11f,.11f),cedar);
            S("Bridge lower rail",PrimitiveType.Cube,new Vector3(8,.52f,z+side*1.17f),new Vector3(5.3f,.07f,.07f),cedar);
        }
    }
    static void Lantern(Vector3 p)
    {
        var g=S("Paper lantern",PrimitiveType.Capsule,p,new Vector3(.47f,.48f,.47f),new Color(1,.74f,.38f));
        var mat=new Material(Resources.Load<Shader>("Shaders/SoftParticle"));mat.color=new Color(1,.74f,.38f);mat.SetColor("_EmissionColor",new Color(.42f,.19f,.04f));g.GetComponent<Renderer>().sharedMaterial=mat;
        for(int i=0;i<6;i++)S("Lantern bamboo hoop",PrimitiveType.Cylinder,p+Vector3.up*(-.36f+i*.14f),new Vector3(.48f,.013f,.48f),new Color(.70f,.44f,.23f));
        var light=g.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.58f,.20f);light.range=7;light.intensity=2.3f;
    }
    public static void WorldText(string text,Vector3 p,float size,Color color)
    {
        var g=new GameObject(text);g.transform.SetParent(root);g.transform.position=p;
        var tm=g.AddComponent<TextMesh>();tm.text=text;tm.characterSize=size;tm.fontSize=56;tm.anchor=TextAnchor.MiddleCenter;tm.color=color;
        tm.font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft JhengHei","Microsoft YaHei"},56);g.GetComponent<MeshRenderer>().sharedMaterial=tm.font.material;
    }
    static void Beam(string name,Vector3 a,Vector3 b,float radius,Color c)
    {
        var g=S(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(radius,(b-a).magnitude*.5f,radius),c);g.transform.up=(b-a).normalized;
    }
    static GameObject Facet(string name,Vector3 p,Vector3 scale,Color c,int sides)
    {
        var vertices=new List<Vector3>();var triangles=new List<int>();
        for(int ring=0;ring<3;ring++)
        {
            float y0=ring==0?-1: ring==1?-.25f:.6f;float y1=ring==0?-.25f:ring==1?.6f:1;
            float r0=ring==0?.30f:ring==1?1:.76f,r1=ring==0?1:ring==1?.76f:.04f;
            for(int j=0;j<sides;j++)
            {
                float a=j*Mathf.PI*2/sides,b=(j+1)*Mathf.PI*2/sides;
                Vector3[] v={new Vector3(Mathf.Cos(a)*r0,y0,Mathf.Sin(a)*r0),new Vector3(Mathf.Cos(a)*r1,y1,Mathf.Sin(a)*r1),new Vector3(Mathf.Cos(b)*r1,y1,Mathf.Sin(b)*r1),new Vector3(Mathf.Cos(b)*r0,y0,Mathf.Sin(b)*r0)};
                foreach(int k in new[]{0,1,2,0,2,3}){vertices.Add(Vector3.Scale(v[k],scale));triangles.Add(triangles.Count);}
            }
        }
        var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();var g=MeshObject(name,mesh,TeaHouseWorld.Mat(c.ToString(),c));g.transform.position=p;return g;
    }
}
