using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수 
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;

    [SerializeField]
    private float crouchSpeed;
    private float applySpeed;


    [SerializeField]
    private float jumpForce;


    //상태 변수
    private bool isWalk = false;
    private bool isRun =  false; 
    private bool isCrouch = false;
    private bool isGround = true;


    //움직임 체크 변수
    private Vector3 lastPos;


    //앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    //땅 착지 여부
    private CapsuleCollider capsuleCollider;

    //민감도 
    [SerializeField]
    private float lookSensitivity;


    //카메라 한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX = 0f; 
    
    //필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid;
    private GunController theGunController;
    private Crosshair theCrosshair;
    private StatusController theStatusController;

    

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();
        theStatusController = FindObjectOfType<StatusController>(); 

        //초기화
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        
        CameraRotation();
        CharacterRotation();
    }

    void FixedUpdate() {
        MoveCheck();
    }

    //앉기 시도. 
    private void TryCrouch(){
        if(Input.GetKeyDown(KeyCode.LeftControl)){
            Crouch();
        }
    }

    //앉기 동작.
    private void Crouch(){
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);

        if(isCrouch){
          applySpeed = crouchSpeed; 
          applyCrouchPosY = crouchPosY;     
        }else{
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        // theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine());

    }

    //부드러운 동작 실행.
    IEnumerator CrouchCoroutine(){
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY){
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if(count > 15)
                break;  
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
      
    }

    // 움직임 실행.
    private void Move(){
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirX;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

        
    }


    //
    private void MoveCheck(){
        if(!isRun && !isCrouch && isGround){
            if(Vector3.Distance(lastPos, transform.position) >= 0.01f)
                isWalk = true;
            else
                isWalk = false;

            theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position; 
        }

        
    }


    //상하 카메라 회전
    private void CameraRotation(){
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation *lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

   //좌우 캐릭터 회전
    private void CharacterRotation(){
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

        // Debug.Log(myRigid.rotation);
        // Debug.Log(myRigid.rotation.eulerAngles);
    }


    // 지면 체크
    private void IsGround(){
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrosshair.JumpAnimation(!isGround);
    }
 
    // 점프 시도
    private void TryJump(){
        if(Input.GetKeyDown(KeyCode.Space) && isGround && theStatusController.GetCurrentSP() > 0){
            Jump();
        }
    }

    //점프
    private void Jump(){
        //앉은 상태에서 점프시 앉은 상태 해제.
        if(isCrouch){
            Crouch();
        }
        theStatusController.DecreaseStamina(100);
        myRigid.velocity = transform.up * jumpForce;
    }

    // 달리기 시도
    private void TryRun(){
        if(Input.GetKey(KeyCode.LeftShift) && theStatusController.GetCurrentSP() > 0){
             Running(); 
        }

        if(Input.GetKeyUp(KeyCode.LeftShift) || theStatusController.GetCurrentSP() <= 0){
            RunningCancle();
        }
    }

    // 달리기 실행.
    private void Running(){
        //앉은 상태에서 달리기시 앉은 상태 해제.
        if(isCrouch){
            Crouch();
        }
        theGunController.CancleFineSight();

        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        theStatusController.DecreaseStamina(10);
        applySpeed = runSpeed;

    }

    // 달리기 취소.
    private void RunningCancle(){
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }
}


