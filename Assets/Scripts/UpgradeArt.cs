using UnityEngine;
using System.Collections.Generic;

public partial class MountainTeaGame
{
    Transform upgradeArtRoot;GameObject comfortArt,prepArt;
    List<MeshRenderer> oldCushions=new List<MeshRenderer>(),softCushions=new List<MeshRenderer>(),bookCovers=new List<MeshRenderer>();
    List<LineRenderer> cushionSeams=new List<LineRenderer>();
    Color UpgradeCloth(int style){return style==1?new Color(.32f,.58f,.47f):style==2?new Color(.78f,.39f,.23f):new Color(.67f,.29f,.28f);}
    GameObject UpgradeShape(string name,PrimitiveType type,Vector3 p,Vector3 size,Color color,Transform parent)
    {return TeaHouseWorld.Shape(name,type,p,size,color,parent);}
    GameObject UpgradeLocal(string name,PrimitiveType type,Vector3 p,Vector3 size,Color color,Transform parent)
    {var g=UpgradeShape(name,type,Vector3.zero,size,color,parent);g.transform.localPosition=p;g.transform.localRotation=Quaternion.identity;return g;}
    void BuildUpgradeArt()
    {
        upgradeArtRoot=new GameObject("Purchased tea-house improvements").transform;
        comfortArt=new GameObject("Comfort cushion set");comfortArt.transform.SetParent(upgradeArtRoot);
        prepArt=new GameObject("Preparation work station");prepArt.transform.SetParent(upgradeArtRoot);
        foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(r.name=="Cushion")oldCushions.Add(r);
        foreach(var original in oldCushions)
        {
            Vector3 p=original.bounds.center+Vector3.up*.025f;
            var pillow=UpgradeShape("Padded leaf cushion",PrimitiveType.Sphere,p,new Vector3(.83f,.19f,.83f),UpgradeCloth(0),comfortArt.transform);
            softCushions.Add(pillow.GetComponent<MeshRenderer>());
            var seam=new GameObject("Cushion stitched piping");seam.transform.SetParent(comfortArt.transform);var line=seam.AddComponent<LineRenderer>();cushionSeams.Add(line);
            line.useWorldSpace=false;line.loop=true;line.positionCount=32;line.startWidth=line.endWidth=.017f;
            line.sharedMaterial=TeaHouseWorld.Mat("Cushion piping",new Color(.86f,.74f,.48f));
            for(int point=0;point<32;point++){float a=point*Mathf.PI/16;line.SetPosition(point,p+new Vector3(Mathf.Sin(a)*.405f,0,Mathf.Cos(a)*.405f));}
            for(int leaf=-1;leaf<=1;leaf+=2)
            {
                var stitch=UpgradeShape("Cushion embroidered leaf",PrimitiveType.Sphere,p+new Vector3(leaf*.055f,.097f,0),new Vector3(.13f,.009f,.055f),cream,comfortArt.transform);
                stitch.transform.rotation=Quaternion.Euler(0,leaf*35,0);
            }
            UpgradeShape("Cushion center tuft",PrimitiveType.Sphere,p+Vector3.up*.098f,Vector3.one*.035f,gold,comfortArt.transform);
        }
        Color wood=new Color(.43f,.28f,.18f);
        UpgradeShape("Recipe stand base",PrimitiveType.Cube,new Vector3(-15.7f,1.55f,-.24f),new Vector3(2,.09f,1.7f),wood,prepArt.transform);
        for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2)
            UpgradeShape("Preparation stand leg",PrimitiveType.Cube,new Vector3(-15.7f+side*.85f,.75f,-.24f+end*.6f),new Vector3(.14f,1.5f,.14f),wood,prepArt.transform);
        var book=new GameObject("Open preparation handbook");book.transform.SetParent(prepArt.transform);book.transform.position=new Vector3(-16,1.67f,-.28f);book.transform.rotation=Quaternion.Euler(-22,0,0);
        bookCovers.Add(UpgradeLocal("Handbook bound cover",PrimitiveType.Cube,Vector3.zero,new Vector3(1.43f,.055f,.87f),sage,book.transform).GetComponent<MeshRenderer>());
        for(int side=-1;side<=1;side+=2)
        {
            UpgradeLocal("Handbook paper pages",PrimitiveType.Cube,new Vector3(side*.35f,.045f,0),new Vector3(.66f,.033f,.77f),cream,book.transform);
            for(int row=0;row<5;row++)UpgradeLocal("Handwritten recipe line",PrimitiveType.Cube,new Vector3(side*.35f,.065f,.24f-row*.105f),new Vector3(row%2==0?.48f:.34f,.005f,.014f),sage,book.transform);
        }
        UpgradeLocal("Handbook spine",PrimitiveType.Cylinder,new Vector3(0,.045f,0),new Vector3(.035f,.40f,.035f),gold,book.transform).transform.localRotation=Quaternion.Euler(90,0,0);
        UpgradeLocal("Handbook ribbon bookmark",PrimitiveType.Cube,new Vector3(.41f,.069f,-.37f),new Vector3(.07f,.012f,.30f),new Color(.7f,.24f,.22f),book.transform);
        for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(-16.5f+i*.47f,1.57f,.47f);
            UpgradeShape("Preparation ingredient box",PrimitiveType.Cube,p+Vector3.up*.10f,new Vector3(.37f,.20f,.34f),wood,prepArt.transform);
            UpgradeShape("Ingredient box lid",PrimitiveType.Cube,p+Vector3.up*.21f,new Vector3(.4f,.025f,.37f),gold,prepArt.transform);
            UpgradeShape("Ingredient box paper label",PrimitiveType.Cube,p+new Vector3(0,.10f,-.175f),new Vector3(.16f,.10f,.008f),cream,prepArt.transform);
        }
        for(int i=0;i<3;i++)UpgradeShape("Stacked preparation tray",PrimitiveType.Cube,new Vector3(-15,1.56f+i*.055f,-.44f),new Vector3(.60f,.04f,.40f),wood,prepArt.transform);
        prepArt.transform.position=new Vector3(.3f,0,-1.4f);
        SyncUpgradeArt();
    }
    void SyncUpgradeArt()
    {
        if(!upgradeArtRoot)return;NormalizePlanning();int style=data.gardenStyle[0];
        comfortArt.SetActive(data.comfortUpgrade);prepArt.SetActive(data.prepUpgrade);
        foreach(var r in oldCushions)if(r)r.enabled=!data.comfortUpgrade;
        var cloth=TeaHouseWorld.Mat("Upgrade cushion cloth "+style,UpgradeCloth(style));foreach(var r in softCushions)r.sharedMaterial=cloth;
        foreach(var r in bookCovers)r.sharedMaterial=TeaHouseWorld.Mat("Upgrade handbook cover "+style,UpgradeCloth(style));
    }
    void TestUpgradeArt()
    {
        NewGame();modal=false;result=false;player.position=new Vector3(-12,0,-3);data.money=500;
        Assert(oldCushions.Count==6&&softCushions.Count==6&&cushionSeams.Count==6,"six original and six detailed seat cushions registered");
        Assert(!comfortArt.activeSelf&&!prepArt.activeSelf&&oldCushions.TrueForAll(r=>r.enabled),"unowned upgrades preserve original scene");
        int parts=upgradeArtRoot.GetComponentsInChildren<Transform>(true).Length,cash=data.money;
        Assert(BuyUpgrade(0)&&comfortArt.activeSelf&&!prepArt.activeSelf&&oldCushions.TrueForAll(r=>!r.enabled)&&data.money==cash-150,"buying comfort immediately replaces visible cushions once");
        Assert(BuyUpgrade(1)&&prepArt.activeSelf&&data.money==cash-270,"buying preparation immediately adds handbook and workstation");
        SyncUpgradeArt();SyncUpgradeArt();Assert(upgradeArtRoot.GetComponentsInChildren<Transform>(true).Length==parts&&data.money==cash-270,"visual sync is idempotent and does not charge");
        Assert(BuyGarden(0,2,true)&&softCushions[0].sharedMaterial.color==UpgradeCloth(2)&&bookCovers[0].sharedMaterial.color==UpgradeCloth(2),"garden palette changes upgraded cushion and handbook together");
        Assert(upgradeArtRoot.GetComponentsInChildren<Collider>(true).Length==0,"upgrade dressing adds no physical blockers");
        foreach(var r in softCushions)Assert(Walkable(new Vector3(r.transform.position.x,0,-7.1f)),"existing service approach remains walkable");
        Save(false);Load(System.IO.Path.Combine(qaDir,"qa-save.json"));Assert(comfortArt.activeSelf&&prepArt.activeSelf&&softCushions[0].sharedMaterial.color==UpgradeCloth(2),"loading restores purchased scene and palette");
        NewGame();modal=false;Assert(!comfortArt.activeSelf&&!prepArt.activeSelf&&oldCushions.TrueForAll(r=>r.enabled),"new journey restores original cushions without orphaned upgrade props");
        Debug.Log("QA UPGRADE ART PASS: six seats, ownership visibility, immediate purchase, no duplicate props or charges, palette, collision-free paths, persistence and new-game reset.");
    }
}
