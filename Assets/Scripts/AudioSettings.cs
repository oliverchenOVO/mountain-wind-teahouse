using UnityEngine;
using System;
using System.IO;

public partial class MountainTeaGame
{
    [Serializable] public class AudioPreferences
    {public int version=1;public float master=1,music=1,ambience=1,effects=1;public bool muted;}
    AudioPreferences audioPrefs=new AudioPreferences();AudioSource musicSource;
    bool settingsOpen;float saveBadgeTimer;string audioSettingsStatus="";
    string AudioPreferencesPath {get{return qa?Path.Combine(qaDir,"qa-audio-settings.json"):Path.Combine(Application.persistentDataPath,"audio-settings.json");}}
    float MusicVolumeGain {get{return audioPrefs.muted?0:audioPrefs.master*audioPrefs.music;}}
    float AmbienceVolumeGain {get{return audioPrefs.muted?0:audioPrefs.master*audioPrefs.ambience;}}
    float EffectsVolumeGain {get{return audioPrefs.muted?0:audioPrefs.master*audioPrefs.effects;}}
    static float SafeVolume(float value){return float.IsNaN(value)||float.IsInfinity(value)?1:Mathf.Clamp01(value);}
    void NormalizeAudioPreferences()
    {audioPrefs.master=SafeVolume(audioPrefs.master);audioPrefs.music=SafeVolume(audioPrefs.music);audioPrefs.ambience=SafeVolume(audioPrefs.ambience);audioPrefs.effects=SafeVolume(audioPrefs.effects);}
    void LoadAudioPreferences()
    {
        audioPrefs=new AudioPreferences();
        try{if(File.Exists(AudioPreferencesPath)){var loaded=JsonUtility.FromJson<AudioPreferences>(File.ReadAllText(AudioPreferencesPath));if(loaded==null||loaded.version!=1)throw new Exception("Unsupported audio settings");audioPrefs=loaded;}}
        catch(Exception){audioSettingsStatus="設定無法讀取，已使用預設音量。";}
        NormalizeAudioPreferences();ApplyAudioPreferences();
    }
    void ApplyAudioPreferences()
    {
        NormalizeAudioPreferences();if(musicSource)musicSource.volume=.12f*MusicVolumeGain;if(audioSource)audioSource.volume=.28f*EffectsVolumeGain;
        if(AmbienceVolumeGain==0)foreach(var source in new[]{windAudio,streamAudio,fallsAudio,rainAudio})if(source)source.volume=0;
    }
    bool SaveAudioPreferences()
    {
        try
        {
            NormalizeAudioPreferences();string path=AudioPreferencesPath;Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path+".tmp",JsonUtility.ToJson(audioPrefs,true));if(File.Exists(path))File.Copy(path,path+".bak",true);File.Copy(path+".tmp",path,true);File.Delete(path+".tmp");audioSettingsStatus="音量設定已保存。";return true;
        }
        catch(Exception){audioSettingsStatus="設定保存失敗；本次音量仍已套用。";Notify(audioSettingsStatus,5);return false;}
    }
    void CloseAudioSettings(){SaveAudioPreferences();settingsOpen=false;}
    float VolumeSlider(float y,string name,float value)
    {
        Text(465,y,200,33,name,heading);Text(980,y,110,32,Mathf.RoundToInt(value*100)+"%",small);
        Color old=GUI.backgroundColor;GUI.backgroundColor=gold;
        float next=GUI.HorizontalSlider(new Rect(670,y+10,280,25),value,0,1,sliderTrack,sliderThumb);GUI.backgroundColor=old;return next;
    }
    void DrawAudioSettings()
    {
        Box(new Rect(0,0,1440,900),new Color(.07f,.14f,.11f,.58f));Panel(new Rect(380,135,680,635));
        TeaSeal(new Rect(415,165,60,60));Text(495,178,495,43,"聽見山中的日常",heading);
        Text(425,249,565,48,"調整立即生效，關閉時保存；不影響旅程存檔。",small);
        audioPrefs.master=VolumeSlider(318,"總音量",audioPrefs.master);
        audioPrefs.music=VolumeSlider(380,"背景音樂",audioPrefs.music);
        audioPrefs.ambience=VolumeSlider(442,"風雨與溪流",audioPrefs.ambience);
        audioPrefs.effects=VolumeSlider(504,"互動與腳步",audioPrefs.effects);
        if(Button(425,566,270,audioPrefs.muted?"取消靜音":"全部靜音"))audioPrefs.muted=!audioPrefs.muted;
        if(Button(715,566,285,"試聽提示音",!audioPrefs.muted)){ApplyAudioPreferences();Play(chime);}
        if(Button(425,623,270,"恢復預設音量"))audioPrefs=new AudioPreferences();
        if(Button(715,623,285,"保存並返回 [Esc]"))CloseAudioSettings();
        ApplyAudioPreferences();
        Text(425,694,580,40,string.IsNullOrEmpty(audioSettingsStatus)?"音樂、環境聲與效果音可以分別關閉。":audioSettingsStatus,small);
    }
    void TestAudioSettings()
    {
        string legacy=Path.Combine(qaDir,"v8-save.json");
        if(File.Exists(legacy)){var previous=JsonUtility.FromJson<SaveData>(File.ReadAllText(legacy));Load(legacy);Assert(data.rainStoryStage==previous.rainStoryStage&&data.notebookStage==previous.notebookStage&&data.money==previous.money&&data.day==previous.day&&data.lunchState==previous.lunchState,"v8 weather notebook and journey progress preserved");}
        audioPrefs=new AudioPreferences{master=.5f,music=.25f,ambience=.8f,effects=.4f};ApplyAudioPreferences();
        Assert(Mathf.Abs(musicSource.volume-.015f)<.0001f&&Mathf.Abs(audioSource.volume-.056f)<.0001f&&Mathf.Abs(AmbienceVolumeGain-.4f)<.0001f,"independent music ambience effects gains");
        UpdateMountainAudio(1);Assert(Mathf.Abs(windAudio.volume-.08f)<.0001f,"environment source respects ambience setting");
        audioPrefs.muted=true;ApplyAudioPreferences();Assert(musicSource.volume==0&&audioSource.volume==0&&rainAudio.volume==0&&AmbienceVolumeGain==0,"mute all groups immediately");
        Assert(SaveAudioPreferences(),"audio settings save succeeds");audioPrefs=new AudioPreferences();LoadAudioPreferences();Assert(audioPrefs.muted&&audioPrefs.master==.5f&&audioPrefs.ambience==.8f,"audio settings roundtrip independent of game save");
        audioPrefs.master=2;audioPrefs.music=-1;audioPrefs.effects=float.NaN;NormalizeAudioPreferences();Assert(audioPrefs.master==1&&audioPrefs.music==0&&audioPrefs.effects==1,"volume range and invalid number normalization");
        int cash=data.money,day=data.day;audioPrefs=new AudioPreferences();ApplyAudioPreferences();SaveAudioPreferences();Assert(data.money==cash&&data.day==day&&musicSource.volume==.12f,"audio defaults do not mutate game progress");
        data.day=3;AvatarMotion.Frozen=true;TickRainWeather(.1f);Assert(rainDrops.main.simulationSpeed==0,"settings freeze rain simulation without parking jobs");AvatarMotion.Frozen=false;TickRainWeather(.1f);Assert(rainDrops.main.simulationSpeed==1,"rain simulation resumes after settings");data.day=day;
        Debug.Log("QA SETTINGS PASS: independent gains, mute, persistence, clamp, defaults, game-save isolation.");
    }
}
