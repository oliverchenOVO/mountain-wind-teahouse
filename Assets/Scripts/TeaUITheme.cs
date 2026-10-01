using UnityEngine;

public partial class MountainTeaGame
{
    GUIStyle roundStyle,sliderTrack,sliderThumb;GUIStyle[] roundSizes;Texture2D roundedPaper,circleTexture,leafTexture;
    void InitTeaTheme()
    {
        roundedPaper=MakeTeaTexture(96,20,false);circleTexture=MakeTeaTexture(96,48,true);
        roundStyle=new GUIStyle{border=new RectOffset(23,23,23,23),normal={background=roundedPaper}};
        roundSizes=new GUIStyle[24];for(int i=0;i<24;i++)roundSizes[i]=new GUIStyle(roundStyle){border=new RectOffset(i,i,i,i)};
        button.border=new RectOffset(20,20,20,20);button.padding=new RectOffset(13,13,0,0);
        button.normal.background=roundedPaper;button.hover.background=roundedPaper;button.active.background=roundedPaper;button.focused.background=roundedPaper;
        button.focused.textColor=gold;
        sliderTrack=new GUIStyle{fixedHeight=12,border=new RectOffset(5,5,5,5),normal={background=roundedPaper}};
        sliderThumb=new GUIStyle{fixedWidth=24,fixedHeight=24,normal={background=circleTexture},hover={background=circleTexture},active={background=circleTexture}};
        leafTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)
        {float u=(x+y-63)/45f,v=(x-y)/17f;float alpha=Mathf.Clamp01((1-u*u-v*v)*9);pixels[y*64+x]=new Color(1,1,1,alpha);}
        leafTexture.SetPixels(pixels);leafTexture.Apply();leafTexture.wrapMode=TextureWrapMode.Clamp;
    }
    Texture2D MakeTeaTexture(int size,int radius,bool circle)
    {
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float px=Mathf.Abs(x-(size-1)*.5f),py=Mathf.Abs(y-(size-1)*.5f);
            float distance=circle?Mathf.Sqrt(px*px+py*py)-(size*.5f-1):new Vector2(Mathf.Max(px-(size*.5f-radius),0),Mathf.Max(py-(size*.5f-radius),0)).magnitude-radius+1;
            float grain=circle?1:1-Mathf.PerlinNoise(x*.42f,y*.42f)*.025f;
            pixels[y*size+x]=new Color(grain,grain,grain,Mathf.Clamp01(.5f-distance));
        }
        texture.SetPixels(pixels);texture.Apply();texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;return texture;
    }
    void RoundFill(Rect rect,Color tint)
    {Color before=GUI.color,background=GUI.backgroundColor;GUI.color=tint;GUI.backgroundColor=Color.white;int inset=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(rect.width,rect.height)*.5f)-1,0,23);GUI.Box(rect,GUIContent.none,roundSizes[inset]);GUI.color=before;GUI.backgroundColor=background;}
    void CircleFill(Rect rect,Color tint)
    {Color before=GUI.color;GUI.color=tint;GUI.DrawTexture(rect,circleTexture);GUI.color=before;}
    void TeaPanel(Rect r)
    {
        RoundFill(new Rect(r.x+2,r.y+8,r.width,r.height),new Color(.09f,.17f,.13f,.28f));
        RoundFill(r,new Color(.42f,.31f,.21f));
        RoundFill(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),new Color(.82f,.71f,.49f));
        RoundFill(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),cream);
        RoundFill(new Rect(r.x+13,r.y+22,4,Mathf.Min(38,r.height-35)),gold);
        Color old=GUI.color;GUI.color=new Color(.43f,.54f,.36f,.13f);GUI.DrawTexture(new Rect(r.xMax-65,r.yMax-62,46,46),leafTexture);GUI.color=old;
    }
    bool TeaButton(Rect r,string text,bool enabled,Color tint)
    {
        bool before=GUI.enabled;GUI.enabled=before&&enabled;Color old=GUI.backgroundColor;
        bool hover=r.Contains(Event.current.mousePosition)&&GUI.enabled;GUI.backgroundColor=hover?Color.Lerp(tint,gold,.24f):tint;
        bool clicked=GUI.Button(r,text,button);GUI.backgroundColor=old;GUI.enabled=before;return clicked;
    }
    void TeaSeal(Rect r)
    {
        CircleFill(r,gold);CircleFill(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),sage);
        RoundFill(new Rect(r.x+r.width*.22f,r.y+r.height*.45f,r.width*.46f,r.height*.26f),cream);
        CircleFill(new Rect(r.x+r.width*.6f,r.y+r.height*.45f,r.width*.22f,r.height*.23f),cream);
        CircleFill(new Rect(r.x+r.width*.65f,r.y+r.height*.49f,r.width*.11f,r.height*.12f),sage);
        RoundFill(new Rect(r.x+r.width*.2f,r.y+r.height*.75f,r.width*.56f,3),gold);
        Color old=GUI.color;GUI.color=gold;GUI.DrawTexture(new Rect(r.x+r.width*.35f,r.y+r.height*.13f,r.width*.32f,r.height*.3f),leafTexture);GUI.color=old;
    }
    void TeaTag(Rect rect,string text,Color tint)
    {RoundFill(rect,tint);GUI.Label(rect,text,label);}
}
