using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    static readonly Color TrailWood=new Color(.37f,.24f,.16f),TrailIvory=new Color(.91f,.85f,.69f);
    void TrailBeam(string name,Vector3 a,Vector3 b,float radius,Color color)
    {var g=TrailShape(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(radius,(b-a).magnitude*.5f,radius),color);g.transform.up=(b-a).normalized;}
    GameObject TrailRock(string name,Vector3 p,Vector3 scale,Color color,int seed)
    {
        var v=new List<Vector3>();var t=new List<int>();
        for(int ring=0;ring<2;ring++)for(int face=0;face<7;face++)
        {
            float a=face*Mathf.PI*2/7,b=(face+1)*Mathf.PI*2/7;
            float r0=ring==0?.85f:1,r1=ring==0?1:.48f,y0=ring==0?-.5f:.05f,y1=ring==0?.05f:.65f;
            Vector3[] q={new Vector3(Mathf.Cos(a)*r0,y0,Mathf.Sin(a)*r0),new Vector3(Mathf.Cos(a)*r1,y1,Mathf.Sin(a)*r1),new Vector3(Mathf.Cos(b)*r1,y1,Mathf.Sin(b)*r1),new Vector3(Mathf.Cos(b)*r0,y0,Mathf.Sin(b)*r0)};
            foreach(int k in new[]{0,1,2,0,2,3}){v.Add(q[k]);t.Add(t.Count);}
        }
        for(int f=0;f<7;f++){float a=f*Mathf.PI*2/7,b=(f+1)*Mathf.PI*2/7;v.Add(new Vector3(Mathf.Cos(a)*.48f,.65f,Mathf.Sin(a)*.48f));v.Add(new Vector3(0,.78f,0));v.Add(new Vector3(Mathf.Cos(b)*.48f,.65f,Mathf.Sin(b)*.48f));t.Add(v.Count-3);t.Add(v.Count-2);t.Add(v.Count-1);}
        var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();
        var g=new GameObject(name);g.transform.SetParent(trailRoot);g.transform.position=p;g.transform.localScale=scale;g.transform.rotation=Quaternion.Euler(4,seed*37,seed%3*4);
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=TeaHouseWorld.Mat(color.ToString(),color);return g;
    }
    void DetailedTrailPath(Vector3[] route)
    {
        var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();
        for(int segment=0;segment<route.Length-1;segment++)
        {
            Vector3 direction=(route[segment+1]-route[segment]).normalized;Vector3 side=new Vector3(direction.z,0,-direction.x).normalized;
            int steps=Mathf.CeilToInt(Vector3.Distance(route[segment],route[segment+1]));
            for(int j=0;j<steps;j++)
            {
                Vector3 a=Vector3.Lerp(route[segment],route[segment+1],j/(float)steps),b=Vector3.Lerp(route[segment],route[segment+1],(j+1)/(float)steps);
                float wa=1.15f+Mathf.Sin((a.x+a.z)*1.4f)*.16f,wb=1.15f+Mathf.Sin((b.x+b.z)*1.4f)*.16f;
                Vector3[] q={a-side*wa,b-side*wb,b+side*wb,a+side*wa};
                foreach(int k in new[]{0,1,2,0,2,3}){Vector3 p=q[k];p.y=TrailHeight(p.x,p.z)+.035f;v.Add(p);t.Add(t.Count);c.Add(Color.Lerp(new Color(.54f,.48f,.34f),new Color(.73f,.64f,.46f),Mathf.PerlinNoise(p.x*.8f,p.z*.8f)));}
            }
        }
        var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetColors(c);mesh.RecalculateNormals();
        var g=new GameObject("Worn natural trail");g.transform.SetParent(trailRoot);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=new Material(Resources.Load<Shader>("Shaders/Meadow"));
    }
    void DetailedWaterfall()
    {
        for(int i=0;i<4;i++)
        {
            var ribbon=TrailShape("Cascading waterfall ribbon",PrimitiveType.Cube,TP(97.05f+i*.61f,25,3.6f),new Vector3(.67f,7.0f+(i%2)*.6f,.09f),new Color(.68f,.87f,.83f));
            ribbon.GetComponent<Renderer>().sharedMaterial=new Material(Resources.Load<Shader>("Shaders/Waterfall"));ribbon.AddComponent<WaterRibbon>().phase=i*.7f;
        }
        for(int i=0;i<8;i++)TrailShape("Waterfall foam",PrimitiveType.Sphere,TP(97+i*.27f,23.7f,.11f),new Vector3(.65f,.05f,.45f),new Color(.83f,.94f,.89f));
    }
    float DistanceToTrail(Vector3 p,Vector3[] route)
    {
        float best=100;
        for(int i=0;i<route.Length-1;i++){Vector2 a=new Vector2(route[i].x,route[i].z),b=new Vector2(route[i+1].x,route[i+1].z),q=new Vector2(p.x,p.z);float f=Mathf.Clamp01(Vector2.Dot(q-a,b-a)/(b-a).sqrMagnitude);best=Mathf.Min(best,Vector2.Distance(q,a+(b-a)*f));}
        return best;
    }
    void TrailDressing(Vector3[] route)
    {
        for(int bank=0;bank<2;bank++)for(int i=0;i<43;i++)
        {
            float z=-17+i*1.05f;if(Mathf.Abs(z-2)<2.6f)continue;float x=(bank==0?95.65f:100.35f)+Mathf.Sin(i*2.1f)*.20f;
            TrailRock("Creek bank stone",TP(x,z,.13f),new Vector3(.45f+(i%3)*.13f,.40f,.55f),i%4==0?new Color(.37f,.46f,.36f):new Color(.48f,.5f,.43f),i);
            if(i%3==0)for(int r=0;r<3;r++)TrailBeam("Creek reed",TP(x+(bank==0?-.3f:.3f)+r*.09f,z),TP(x+(bank==0?-.3f:.3f)+r*.15f,z,.6f+r*.12f),.018f,new Color(.47f,.58f,.33f));
        }
        for(int i=0;i<420;i++)
        {
            float x=78+(i*7.731f)%40,z=-15+(i*3.973f)%39;Vector3 p=TP(x,z);
            if(x>94.8f&&x<101.2f||DistanceToTrail(p,route)<1.45f||Vector2.Distance(new Vector2(x,z),new Vector2(106,20))<3||Vector2.Distance(new Vector2(x,z),new Vector2(86,21))<3)continue;
            if(i%6==0)TrailRock("Mossy trail stone",p+Vector3.up*.10f,new Vector3(.3f,.28f,.38f),new Color(.39f,.49f,.34f),i);
            else if(i%3==0)TrailShape("Autumn fallen leaf",PrimitiveType.Sphere,p+Vector3.up*.045f,new Vector3(.18f,.015f,.09f),new Color(.76f,.48f,.24f)).transform.rotation=Quaternion.Euler(0,i*53,0);
            else for(int blade=0;blade<3;blade++)TrailBeam("Meadow grass blade",p+Vector3.right*blade*.08f,p+new Vector3(blade*.1f,.22f+(i%4)*.06f,.09f),.013f,new Color(.4f,.55f,.32f));
        }
        for(int side=-1;side<=1;side+=2)
        {
            TrailBeam("Bridge handrail",TP(95.5f,2+side*1.3f,.98f),TP(100.4f,2+side*1.3f,.98f),.045f,TrailWood);
            TrailBeam("Bridge lower rail",TP(95.5f,2+side*1.3f,.5f),TP(100.4f,2+side*1.3f,.5f),.025f,TrailWood);
        }
        for(int i=0;i<5;i++)TrailShape("Trail marker",PrimitiveType.Cube,TP(102.8f+i*.4f,3,.09f),new Vector3(.34f,.15f,.48f),new Color(.63f,.57f,.46f));
        TrailShape("Water inspection toolbox",PrimitiveType.Cube,TP(103.7f,5,.25f),new Vector3(.6f,.45f,.4f),new Color(.24f,.47f,.5f));
        TrailShape("Camera tripod head",PrimitiveType.Cube,TP(90,-4.5f,1.0f),new Vector3(.32f,.23f,.18f),new Color(.19f,.2f,.18f));
        for(int i=0;i<3;i++){float angle=i*Mathf.PI*2/3;TrailBeam("Camera tripod",TP(90+Mathf.Cos(angle)*.4f,-4.5f+Mathf.Sin(angle)*.4f),TP(90,-4.5f,.9f),.025f,TrailWood);}
    }
    void DetailedShelter(Vector3 p,string text)
    {
        for(int side=-1;side<=1;side+=2)
        {
            var roof=TrailShape("Pavilion solid roof underlay",PrimitiveType.Cube,p+new Vector3(0,2.78f,side*.85f),new Vector3(4.85f,.10f,1.95f),new Color(.2f,.36f,.34f));roof.transform.rotation=Quaternion.Euler(side*23,0,0);
        }
        for(int plank=0;plank<11;plank++)TrailShape("Pavilion floor plank",PrimitiveType.Cube,p+new Vector3(-1.85f+plank*.37f,.14f,0),new Vector3(.35f,.18f,3.1f),TrailWood);
        for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2)
        {
            TrailShape("Pavilion cedar pillar",PrimitiveType.Cube,p+new Vector3(a*1.7f,1.4f,b*1.2f),new Vector3(.15f,2.7f,.15f),TrailWood);
            TrailBeam("Pavilion roof brace",p+new Vector3(a*1.7f,1.95f,b*1.2f),p+new Vector3(a*1.15f,2.6f,b*1.2f),.07f,TrailWood);
        }
        for(int side=-1;side<=1;side+=2)for(int row=0;row<7;row++)
        {
            float z=side*(row*.24f+.1f),y=3.15f-row*.10f;
            for(int tile=0;tile<16;tile++){var g=TrailShape("Pavilion ceramic roof tile",PrimitiveType.Cube,p+new Vector3(-2.32f+tile*.31f,y,z),new Vector3(.29f,.07f,.30f),row%2==0?new Color(.2f,.36f,.34f):new Color(.24f,.4f,.37f));g.transform.rotation=Quaternion.Euler(side*23,0,0);}
        }
        TrailBeam("Pavilion ridge",p+new Vector3(-2.6f,3.23f,0),p+new Vector3(2.6f,3.23f,0),.09f,new Color(.17f,.3f,.29f));
        for(int side=-1;side<=1;side+=2)
        {
            TrailShape("Pavilion bench",PrimitiveType.Cube,p+new Vector3(side*1.3f,.54f,.25f),new Vector3(.5f,.13f,1.7f),TrailWood);
            TrailShape("Bench cushion",PrimitiveType.Cube,p+new Vector3(side*1.3f,.65f,.25f),new Vector3(.42f,.07f,1.45f),new Color(.64f,.34f,.23f));
            for(int j=-1;j<=1;j+=2)TrailShape("Bench foot",PrimitiveType.Cube,p+new Vector3(side*1.3f,.3f,.25f+j*.6f),new Vector3(.15f,.5f,.15f),TrailWood);
            TrailBeam("Pavilion side railing",p+new Vector3(side*1.7f,1, -1.2f),p+new Vector3(side*1.7f,1,1.2f),.04f,TrailWood);
        }
        TrailShape("Pavilion back shelf",PrimitiveType.Cube,p+new Vector3(0,.9f,1),new Vector3(2.1f,.09f,.4f),TrailWood);
        for(int i=0;i<3;i++)TrailShape("Rest stop ceramic cup",PrimitiveType.Cylinder,p+new Vector3(-.55f+i*.55f,1.03f,1),new Vector3(.15f,.09f,.15f),TrailIvory);
        TrailShape("Tengu notice board",PrimitiveType.Cube,p+new Vector3(0,1.85f,1.15f),new Vector3(1.3f,.7f,.08f),TrailWood);
        for(int i=0;i<3;i++)TrailShape("Posted trail notice",PrimitiveType.Cube,p+new Vector3(-.38f+i*.38f,1.88f,1.09f),new Vector3(.29f,.44f,.015f),TrailIvory);
        for(int i=0;i<2;i++)
        {
            Vector3 q=p+new Vector3(i==0?-2.1f:2.1f,2.3f,-1.1f);
            TrailShape("Pavilion paper lantern",PrimitiveType.Sphere,q,new Vector3(.38f,.54f,.38f),new Color(.95f,.71f,.34f));
            for(int hoop=0;hoop<4;hoop++)TrailShape("Lantern bamboo hoop",PrimitiveType.Cylinder,q+Vector3.up*(-.18f+hoop*.12f),new Vector3(.4f,.015f,.4f),TrailWood);
        }
        MountainArt.WorldText(text,p+new Vector3(0,4.25f,.15f),.105f,TrailIvory);
    }
    void DetailIngredients(Spot s,int kind)
    {
        Vector3 p=s.pos;Transform parent=s.visual.transform;
        for(int i=0;i<4;i++)
        {
            Vector3 q=p+new Vector3(Mathf.Sin(i*2)*.35f,.34f,Mathf.Cos(i*2)*.35f);
            TeaHouseWorld.Shape(kind==23?"Chestnut spiky husk":"Berry bush foliage",PrimitiveType.Sphere,q,kind==23?new Vector3(.35f,.18f,.35f):new Vector3(.5f,.5f,.45f),kind==23?new Color(.56f,.6f,.28f):new Color(.27f,.45f,.3f),parent);
            if(kind==23)for(int spike=0;spike<5;spike++){float a=spike*Mathf.PI*2/5;TeaHouseWorld.Shape("Chestnut husk spike",PrimitiveType.Cube,q+new Vector3(Mathf.Cos(a)*.17f,.04f,Mathf.Sin(a)*.17f),new Vector3(.04f,.12f,.04f),new Color(.72f,.7f,.36f),parent);}
            else for(int berry=0;berry<3;berry++)TeaHouseWorld.Shape("Wild berry cluster",PrimitiveType.Sphere,q+new Vector3((berry-1)*.08f,.19f,-.18f),Vector3.one*.11f,new Color(.74f,.24f,.38f),parent);
        }
    }
}
