using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeController : CloseWeaponController
{
    
    //현재 활성화 여부.
    public static bool isActivate = false; 

    // Update is called once per frame
    void Update()
    {
        if(isActivate)
            TryAttack();
    }

    protected override IEnumerator HitCoroutine(){
       while(isSwing){
            if(CheckObject()){
                isSwing = false; 
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }

    public override void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        base.CloseWeaponChange(_closeWeapon);
        isActivate = true;
    }

}
