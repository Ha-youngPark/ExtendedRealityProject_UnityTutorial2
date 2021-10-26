using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sound{
    public string name; //곡의 이름
    public AudioClip clip; //곡 
}

public class SoundManager : MonoBehaviour
{
    static public SoundManager instance; 
    //싱글턴. Singleton. 1개.
    #region singleton
    void Awake(){ //객체 생성시 최초 실행.
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    #endregion singleton

    public AudioSource[] audioSourceEffect;
    public AudioSource[] audioSourceBgm;

    public string[] playSoundName;

    public Sound[] effectSounds;
    public Sound[] bgmSounds; 

    private void Start(){
        playSoundName = new string[audioSourceEffect.Length];
    }

    public void PlaySE(string _name){
        for(int i = 0; i < effectSounds.Length; i ++){
            if(_name == effectSounds[i].name){
                for(int j = 0; j< audioSourceEffect.Length; j++){
                    //재생중이지 않은 것을 찾는다.
                    if(!audioSourceEffect[j].isPlaying){
                        playSoundName[j] = effectSounds[i].name;
                        audioSourceEffect[j].clip = effectSounds[i].clip;
                        audioSourceEffect[j].Play();
                        return;
                    }
                }
                Debug.Log("모든 가용 AudioSource가 사용중입니다.");
                return;

            }
        }
        Debug.Log(_name + "사운드가 SoundManager에 등록 되지 않았습니다.");
    }

    public void StopAllSE(){
        //모든 사운드 실행 취소
        for(int i = 0; i < audioSourceEffect.Length; i++){
            audioSourceEffect[i].Stop();
        }
    }

    public void StopSE(string _name){
        for(int i = 0; i < audioSourceEffect.Length; i++){
            if(playSoundName[i] == _name){
                audioSourceEffect[i].Stop();
                return;
            }       
        }
        Debug.Log("재생 중인" + _name + "사운드가 없습니다."); 
    }

}
