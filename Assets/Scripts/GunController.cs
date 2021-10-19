using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    //현재 장착된 총
   [SerializeField]
   private Gun currentGun; 
    
    //연사 속도 계산
   private float currentFireRate;


    //상태 변수들 
   private bool isReload = false;
   [HideInInspector]
   private bool isFineSightMode = false;
    
    //본래 포지션 값 
   private Vector3 originPos;

    //효과음 재생
   private AudioSource audioSource;

    //레이저 충돌 정보 받아옴 
   private RaycastHit hitInfo; 
   
   //필요한 컴포넌트 
   [SerializeField]
   private Camera theCam; 
   private Crosshair theCrosshair;  

    //피격 이펙트 
   [SerializeField]
   private GameObject hit_effect_prefab;

   void Start(){
       originPos = Vector3.zero;
       audioSource = GetComponent<AudioSource>();
       theCrosshair = FindObjectOfType<Crosshair>();
   }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();  
        TryFire();
        TryReload();
        TryFineSight();
    }

    //연사속도 재계산 
    private void GunFireRateCalc(){
        if(currentFireRate > 0){
            currentFireRate -= Time.deltaTime; //1초 의 역수 = 60분의 1 = 1  
        }
    }


    //발사 시도
    private void TryFire(){
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload){
             Fire();
        }
    }

    //발사 전 계산 
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

    //발사 후 계산
    private void Shoot(){
        theCrosshair.FireAnimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; //연사 속도 재계산 
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();
        StopAllCoroutines();
        //총기 반동 코루틴 실행
        StartCoroutine(RetroActionCoroutin());

    }


    private void Hit(){
        if(Physics.Raycast(theCam.transform.position, theCam.transform.forward + 
        new Vector3(Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() - currentGun.accuracy),
                    Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() - currentGun.accuracy),
                    0)
        , out hitInfo, currentGun.range)){
           GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
           Destroy(clone, 2f);
        }
    }

    //재장전 시도
    private void TryReload(){
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount){
            CancleFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    //재장전
    IEnumerator ReloadCoroutine(){
        if(currentGun.carryBulletCount > 0){
            isReload = true;
            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount;
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

    //정조준 시도
    private void TryFineSight(){
        if(Input.GetButtonDown("Fire2") && !isReload){
            FineSight();
        }
    }

    //정조준 취소
    public void CancleFineSight(){
        if(isFineSightMode)
        FineSight();
    }
    
    //정조준 로직 가동
    private void FineSight(){
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);
        theCrosshair.FineSightAnimation(isFineSightMode);
       
        if(isFineSightMode){
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCorouting());
        }else{
            StopAllCoroutines();
            StartCoroutine(FineSightDeactivateCorouting());
        }
    }

    //정조준 활성화
    IEnumerator FineSightActivateCorouting(){
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos){
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;

        }
    } 

    //정조준 비활성화
    IEnumerator FineSightDeactivateCorouting(){
        while(currentGun.transform.localPosition != originPos){
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;

        }
    } 

    //반동 코루틴
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

    //사운드 재생
    private void PlaySE(AudioClip _clip){
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public Gun GetGun(){
        return currentGun;
    }

    public bool GetFineSightMode(){
        return isFineSightMode;
    }
}
