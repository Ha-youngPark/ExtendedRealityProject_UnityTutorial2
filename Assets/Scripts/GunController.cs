using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
   [SerializeField]
   private Gun currentGun; 
    
   private float currentFireRate;

   private bool isReload = false;

   private bool isFineSightMode = false;
    //본래 포지션 값.
   [SerializeField]
   private Vector3 originPos;

   private AudioSource audioSource;

   void Start(){
       audioSource = GetComponent<AudioSource>();
   }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();  
        TryFire();
        TryReload();
        TryFineSight();
    }

    private void GunFireRateCalc(){
        if(currentFireRate > 0){
            currentFireRate -= Time.deltaTime; //1초 의 역수 = 60분의 1 = 1  
        }
    }

    private void TryFire(){
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload){
             Fire();
        }
    }

    private void Fire(){

        if(!isReload){
            if(currentGun.currentBulletCount > 0)
                Shoot();
            else{
                CancleFineSight();
                StartCoroutine(ReloadCoroutine());
            }
                
        }
        
    }

    private void Shoot(){
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; //연사 속도 재계산 
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();

        StopAllCoroutines();
        //총기 반동 코루틴 실행
        StartCoroutine(RetroActionCoroutin());


        Debug.Log("총알 발사함");
    }


    private void TryReload(){
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount){
            CancleFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    IEnumerator ReloadCoroutine(){
        if(currentGun.carryBulletCount > 0){
            isReload = true;
            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount =   currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.reloadTime);

            if(currentGun.carryBulletCount >= currentGun.reloadBulletCount){
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else{
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false;
        }
        else{
            Debug.Log("소유한 총알이 없습니다.");
        }
    }

    private void TryFineSight(){
        if(Input.GetButtonDown("Fire2") && !isReload){
            FineSight();
        }
    }

    public void CancleFineSight(){
        if(isFineSightMode)
        FineSight();
    }
    private void FineSight(){
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);
        if(isFineSightMode){
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCorouting());
        }else{
            StopAllCoroutines();
            StartCoroutine(FineSightDeactivateCorouting());
        }
    }

    IEnumerator FineSightActivateCorouting(){
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos){
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;

        }
    } 

    IEnumerator FineSightDeactivateCorouting(){
        while(currentGun.transform.localPosition != originPos){
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;

        }
    } 

    IEnumerator RetroActionCoroutin(){
        //정조준 안했을때 최대 반동
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z);
        //정조준 했을 때의 최대 반동
        Vector3 retroActionRecoilBakc = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); 
        
        if(!isFineSightMode){
           currentGun.transform.localPosition = originPos;

           //반동 시작
           while(currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f){
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
           }     

           //원위치
           while(currentGun.transform.localPosition != originPos){
               currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f); 
               yield return null;
           }
        }else{
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

           //반동 시작
           while(currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f){
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBakc, 0.4f);
                yield return null;
           }     

           //원위치
           while(currentGun.transform.localPosition != currentGun.fineSightOriginPos){
               currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f); 
               yield return null;
           }
        }
    }


    private void PlaySE(AudioClip _clip){
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
