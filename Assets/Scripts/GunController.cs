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
            else
                StartCoroutine(ReloadCoroutine());
        }
        
    }

    private void Shoot(){
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; //연사 속도 재계산 
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Debug.Log("총알 발사함");
    }


    private void TryReload(){
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount){
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

    

    private void PlaySE(AudioClip _clip){
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
