using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

//animatorCtrl은 현재 없기 때문에 제대로 작동하지 않는다 -> 따라서 주석

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] private PlayerAnimatorCtrl animatorCtrl;
    [SerializeField] private Game game;

    public string CharacterName;    //플레이어 캐릭터의 이름 (이 정보를 바탕으로 세이브/로드한다.)
    //public                        //텍스쳐(스킨/복장)

    //이동 제어 변수
    private float speed = 1f;
    private float h, v;
    private Vector3 movement;


    bool mouse_l = false, mouse_r = false;
    private float dblClickSpeed = 0.2f;             //더블 클릭 시간
    private float wheelingSpeed = 0.1f;             //휠 굴리는 시간(딜레이)
    private float comboTime = 0.5f;                 //콤보 입력을 받는 동안 기다리는 시간
    private KeyCode[] keycode = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S };             //수정바람. (사용자가 입력한 키 일수도 있음)
    private float[] dblClick = new float[4];        //더블 클릭 시간 (왼쪽, 오른쪽, 앞, 뒤)
    private bool[] isClick = new bool[4];           //클릭했냐? (왼쪽, 오른쪽, 앞, 뒤)
    private float mouseWheeling = 0.1f;
    private bool isWheel = false;                   //휠 굴리는 중이냐?
    private bool isHold = false;                    //홀딩중이냐?
    private float combodelay = 0.5f;                //콤보 딜레이    
    private float mouseTime = 0.2f;
    private float mouseDelay;                       //마우스가 홀딩이냐 동시클릭이냐?
    private bool isAttackAble = true;

    private int atkTotalCount = 0;                  //전체 공격 횟수
    private int atkLeftCount = 0;                   //좌클릭 공격 횟수
    private int atkRightCount = 0;                  //우클릭 공격 횟수


    public Vector3 ch_pos           //플레이어의 위치값
    {
        get
        {
            return mTrans.position;
        }
    }   
    /*public Vector3 move             //플레이어의 이동값
     * {
     *    get; private set;
     * }
     */
    public Vector3 ch_rot           //플레이어의 회전값
    {
        get
        {
            return mTrans.eulerAngles;
        }        
    }
    public Vector3 cam_pos          //카메라의 위치값      //cam_pos는 자동으로 움직이기 때문에 저장하는데 필요 없음
    {
        get
        {
            return mCam.position;
        }        
    }
    public Vector3 cam_rot          //카메라의 회전값      //플레이어의 회전값이 잇으므로, 이것도 필요 없음
    {
        get
        {
            return mCam.eulerAngles;
        }
        private set
        {
            mCam.eulerAngles = value;
        }
    }
    public Vector3 cam_move         //카메라의 이동값
    {
        get; private set;
    }
    /*public bool canJump
    {
        get
        {
            return 
        }
    }*/

    public enum Motion
    {
        Idle,
        Avoid,
        Move,
        InputAttack,        //공격 입력중
        Attack,
        HoldAttackFake,     //임시 변수명, (홀드 어택이 3가지 모션으로 바뀌면 삭제)
        HoldAttack,
        AfterAttack,        //공격이 끝난후
    }
    Motion m_currentMotion;

    private Transform mCam;         //플레이어를 따라다니는 카메라

    public GameObject[] TargetObjs;
    //private List<GameObject> TargetObjs = new List<GameObject>();         //나중에 Object pool 사용할 것
    private GameObject targetObj;    //타켓팅이 된 오브젝트
    private GameObject mGameObj;    //캐릭터
    private Transform mTrans;
    private Rigidbody mRigid;

    void Awake()
    {
        OnValidate();
        mGameObj = this.gameObject;
        mTrans = this.transform;
        mRigid = this.GetComponent<Rigidbody>();

        m_currentMotion = Motion.Idle;
        CharacterName = mGameObj.name;
    }

    void OnValidate()           //초기화
    {
        if(Camera.main != null)
        {
            mCam = Camera.main.transform;
            Debug.Log("main Camera on");
        }
        else
        {
            Debug.Log("No main Camera");
        }
        if(animatorCtrl == null)
        {
            animatorCtrl = GetComponentInChildren<PlayerAnimatorCtrl>();            //InChildren? Can Kicker에서 확인하자
        }
        for(int i = 0; i < keycode.Length; i++)
        {
            OnValidateDbl(i);
        }
        //TargetObjs = GameObject.FindGameObjectsWithTag("Enemy");        //타겟이 가능한 오브젝트 집어넣기
        
    }
    void OnValidateDbl(int i)       //더블 클릭 초기화
    {
        isClick[i] = false;
        dblClick[i] = dblClickSpeed;
    }

    private void Update()
    {
        CoolTime();

        //Debug.Log("모션 : " + m_currentMotion);

        switch(m_currentMotion)
        {
            case Motion.Idle:
                IsDbl();            //더블(연속)클릭 판정 (회피 판정)
                break;
            case Motion.Avoid:
                if(animatorCtrl.MotionEnd("avoid"))    //애니메이션 재생이 끝났다면
                {
                    Debug.Log("회피 모션 끝!");
                    for(int i = 0; i < keycode.Length; i++)
                    {
                        OnValidateDbl(i);
                    }
                    m_currentMotion = Motion.Idle;
                }
                break;
            case Motion.Move:
                IsDbl();            //더블(연속)클릭 판정 (회피 판정)
                break;
            case Motion.InputAttack:
                MouseJudgment();
                break;
            case Motion.Attack:
                //콥보어택 딜레이 타임
                if(animatorCtrl.MotionEnd("attack"))    //애니메이션 재생이 끝났다면
                {
                    isAttackAble = true;
                    combodelay = comboTime;
                    m_currentMotion = atkTotalCount == 0 ? Motion.Idle : Motion.AfterAttack;        //콤보 공격이 끝났다면 대기상태, 콤보 중이라면 콤보조작 대기 상태
                    Debug.Log("공격 모션 끝!");
                }
                break;
            case Motion.HoldAttack:
                if(animatorCtrl.MotionEnd("hold"))    //애니메이션 재생이 끝났다면
                {
                    isAttackAble = true;
                    Debug.Log("홀드 공격 모션 끝!");
                    m_currentMotion = Motion.Idle;
                }                
                break;
            case Motion.HoldAttackFake:
                if(animatorCtrl.MotionEnd("holdFake"))    //애니메이션 재생이 끝났다면
                {
                    isAttackAble = true;
                    Debug.Log("홀드Fake 공격 모션 끝!");
                    m_currentMotion = Motion.Idle;
                }
                break;
            case Motion.AfterAttack:
                IsDbl();            //더블(연속)클릭 판정 (회피 판정)
                ComboJudgment();
                break;
        }
        //Debug.Log("L : " + mouse_l + ", R : " + mouse_r + ", hold : " + isHold);

        PlayerCameraCtrl();
        //화면 회전(마우스를 통한)은 여기서
                
        IsWheel();          //휠 클릭 판정

        if(isAttackAble)
        {
            if(Input.GetMouseButtonDown(0))
            {
                mouse_l = true;
            }
            if(Input.GetMouseButtonDown(1))
            {
                mouse_r = true;
            }
            if(mouse_l || mouse_r)
            {
                //이동 애니메이션을 정지 시킨다
                for(int i = 0; i < 4; i++)
                {
                    animatorCtrl.Move(false,i);
                }
                mouseDelay = mouseTime;                         //0.2초 기다려!
                isAttackAble = false;
                m_currentMotion = Motion.InputAttack;           //공격 입력 상태
            }
        }

        if(Motioning())
        {
            return;
        }

        //플레이어의 이동을 위한 값은 미리 Update에서 받는다 +특정 모션 중이라면 입력받을 필요 없다.
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
    }

    //마우스 판정 메소드
    private void MouseJudgment()        
    {
        /*
        if(mouse_l || mouse_r)     //홀드 , 동시 클릭인지 판단
        {            
            if(Input.GetMouseButtonUp(0))       //도중에 좌클릭을 때면 홀드공격이 아니다.
            {
                isHold = false;
            }
            if(Input.GetMouseButtonUp(1))       //도중에 우클릭을 때면 홀드공격이 아니다.
            {
                isHold = false;
            }
            if(Input.GetMouseButtonDown(0))
            {
                mouse_l = true;
            }
            if(Input.GetMouseButtonDown(1))
            {
                mouse_r = true;
            }
        }
        */
        mouseDelay -= Time.deltaTime;

        if(mouseDelay <= 0)
        {
            mouse_l = (mouse_l == true) ? mouse_l : Input.GetMouseButton(0);        //이미 입력이 true라면 true
            mouse_r = (mouse_r == true) ? mouse_r : Input.GetMouseButton(1);        
            if(mouse_l && mouse_r)            //동시 클릭이라면
            {
                isHold = true;
            }

            /*if(isHold == false && (mouse_l && mouse_r))         //홀드 공격은 아닌데, 동시클릭 입력을 받았다
            {
                m_currentMotion = Motion.HoldAttackFake;
                animatorCtrl.HoldAttackFake();
                mouse_l = false;
                mouse_r = false;
                isHold = false;
                return;
            }*/
            if(isHold == false)                                 //홀드 공격이 아니다.
            {
                m_currentMotion = Motion.Attack;
                Attack(mouse_l,mouse_r);
                mouse_l = false;
                mouse_r = false;
                isHold = false;
                return;
            }
            if(isHold == true)                                  //홀드 공격이다.
            {
                m_currentMotion = Motion.HoldAttack;
                animatorCtrl.HoldAttack();
                mouse_l = false;
                mouse_r = false;
                isHold = false;
                atkTotalCount = 0;
                atkLeftCount = 0;
                atkRightCount = 0;
            }
        }
        
    }

    private void ComboJudgment()
    {
        //Debug.Log("콤보 수 = " + atkTotalCount + ", combodelay = " + combodelay);
        if(combodelay <= 0)         //콤보 딜레이가 0이면 초기화
        {
            //Debug.Log("콤보 시간 초과");
            atkTotalCount = 0;
            atkLeftCount = 0;
            atkRightCount = 0; 
            m_currentMotion = Motion.Idle;
        }

        combodelay -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if(Motioning())     //모션 중엔 작업할 필요 없다.
        {
            return;
        }
        if(h != 0 || v != 0)            //공격중이 아닐 때, 움직이는 판정을 적용한다
        {
            m_currentMotion = Motion.Move;
            if(targetObj == null)       //타겟팅이 된 오브젝트가 없다
            {
                if(mCam != null)
                {
                    //Debug.Log("h = " + h + ", v = " + v);
                    //>>>해당 부분은 Transform을 이용한 이동 방법
                    /*
                     * cam_move = Vector3.Scale(mCam.forward,new Vector3(1,0,1)).normalized;       //카메라가 위에 있어서 1,0,1을 곱한건가?
                     * move = v * cam_move + h * mCam.right;
                     */
                }
                else                    //카메라가 없을 때
                {
                    //>>>move = v * Vector3.forward + h * Vector3.right;
                }                
            }
            else                        //타겟팅이 된 오브젝트가 있다. (원운동)
            {
                if(mCam != null)
                {
                    transform.RotateAround(targetObj.transform.position,Vector3.down,h);                    //원운동 속도 제어는 여기서
                    transform.LookAt(targetObj.transform.position);
                    //Debug.Log("h = " + h + ", v = " + v);

                    //>>>Transform을 이용한 이동 방법
                    /*
                     * Vector3 cam_way = Vector3.Scale(mCam.forward,new Vector3(1,0,1)).normalized;
                     * move = v * cam_way;                                                                   //앞뒤 속도 제어는 여기서
                     * //Debug.Log(move);
                     */
                    

                }
                else                    //카메라가 없을 때
                {
                    transform.RotateAround(targetObj.transform.position,Vector3.down,h);
                    transform.LookAt(targetObj.transform.position);
                    //>>>move = v * Vector3.forward;     //아마 오류
                    //move = new Vector3(targetObj_pos.x + Mathf.Sin(h),move_pos.y,targetObj_pos.z + Mathf.Cos(h));
                    //move = new Vector3(targetObj_pos.x + distance * Mathf.Cos(Mathf.Atan(distance/h)),move_pos.y,targetObj_pos.z + distance * Mathf.Sin(Mathf.Atan(distance/h)));
                    //move = v * Vector3.forward + Mathf.Cos(h) * distance * Vector3.right + Mathf.Sin(h) * distance * Vector3.right;
                }
            }
            //>>>mTrans.position += move * speed;
            //Rigidbody를 이용한 이동방법.
            movement.Set(h,0f,v);
            movement = movement.normalized * speed * Time.deltaTime;
            mRigid.MovePosition(mTrans.position + movement);


            if(h != 0)
            {
                animatorCtrl.Move(true,h < 0 ? 0 : 1);               //0 = 좌, 1 = 우
                animatorCtrl.Move(false,h < 0 ? 1 : 0);               //0 = 좌, 1 = 우
            }
            else
            {
                animatorCtrl.Move(false,0);
                animatorCtrl.Move(false,1);
            }
            if(v != 0)
            {
                animatorCtrl.Move(true,v > 0 ? 2 : 3);               //2 = 앞, 3 = 뒤
                animatorCtrl.Move(false,v > 0 ? 3 : 2);               //0 = 좌, 1 = 우
            }
            else
            {
                animatorCtrl.Move(false,2);
                animatorCtrl.Move(false,3);
            }

            if(Input.GetKey(KeyCode.LeftShift))    //대쉬 판정
            {
                animatorCtrl.Dash(true);
            }
            else
            {
                animatorCtrl.Dash(false);
            }
        }
        else
        {
            m_currentMotion = Motion.Idle;
            for(int i = 0; i < 4; i++)
            {
                animatorCtrl.Move(false,i);
            }
        }
        //해당 자료는 Trigger이기 때문에,
    }
    private bool Motioning()
    {
        if(m_currentMotion == Motion.Attack || m_currentMotion == Motion.Avoid || m_currentMotion == Motion.InputAttack || m_currentMotion == Motion.AfterAttack || m_currentMotion == Motion.HoldAttack || m_currentMotion == Motion.HoldAttackFake)     //공격중, 회피중, 공격입력중에는 다른 모션을 취할 수 없음.
        {
            return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        
    }

    private void Attack(bool mouseL, bool mouseR)
    {
        if(mouseL && mouseR)
        {
            return;
        }
        //애니메이션이 끝나기 직전~Combodelay 사이에 시간에 입력을 받음
        int atkCount = 0;
        atkTotalCount++;
        atkLeftCount++;     //좌클릭은 전체클릭과 동일하게 증가시켜도 상관 없음.
         
        if(atkTotalCount > 1 || mouseR)     //공격모션을 1회 끝마친 상태이다.
        {
            atkRightCount++;
        }

        if(atkRightCount == atkLeftCount)   //좌클릭, 우클릭 입력횟수가 같다면(우클릭을 먼저 했다면)
        {
            if(mouseL)                      //좌클릭이 안되는 상황에서 좌클릭을 했다면
            {
                atkTotalCount = 0;
                atkLeftCount = 0;
                atkRightCount = 0;
                return;
            }
        }

        //combodelay = comboTime;
        //isAttack = true;
        //Debug.Log("atkTotalCount : " + atkTotalCount + ", LeftCount = " + atkLeftCount + ", RightCount = " + atkRightCount);
        if(mouseL)
        {
            atkCount = atkLeftCount;
            Debug.Log("LeftCount = " + atkLeftCount);
        }
        if(mouseR)
        {
            atkCount = atkRightCount;
            Debug.Log("RightCount = " + atkRightCount);
        }
        
        Debug.Log("atkCount = " + atkCount);
        animatorCtrl.Attack(mouseL,mouseR,atkCount);

        if(atkTotalCount >= 3 || atkRightCount >= 2)        //전체공격 횟수가 3번(좌클릭 3번, 우클릭 2번)이 넘었다면, 콤보 초기화 -> 애초에 좌클릭만 콤보가 되기 때문에 우클릭의 카운트가 3이상 될 수가 없다
        {
            atkTotalCount = 0;
            atkLeftCount = 0;
            atkRightCount = 0;
        }
    }

    private void CoolTime()
    {        
        //더블클릭 쿨타임
        for(int i = 0; i < 4; i++)
        {
            if(isClick[i])
            {
                dblClick[i] -= Time.deltaTime;
            }
            if(dblClick[i] <= 0)
            {
                OnValidateDbl(i);
            }
        }
        //마우스 휠 쿨타임
        if(isWheel)
        {
            mouseWheeling -= Time.deltaTime;
        }
        if(mouseWheeling <= 0)
        {
            mouseWheeling = wheelingSpeed;
            isWheel = false;
        }

    }
    
    private void IsDbl()                //더블클릭 판정 (회피 판정)
    {
        for(int i = 0; i < keycode.Length; i++)
        {
            if(isClick[i] && Input.GetKeyDown(keycode[i]))
            {
                AvoidAnimation(i);
                m_currentMotion = Motion.Avoid;
                for(int j = 0; j < keycode.Length; j++)
                {
                    OnValidateDbl(j);
                }
                return;      //회피모션 중 대기
            }
            if(Input.GetKeyUp(keycode[i]))
            {
                for(int j = 0; j < keycode.Length; j++)     //방향키 중 어떤 키가 Up되었을 때, 그 방향키를 제외한 모든 키를 초기화 (ADA를 빠르게 눌렀을 때, 회피가 되지 않음. / D를 누른 상태로 AA를 빠르게 눌렀을 때, 회피가 됨
                {
                    if(i != j)
                    {
                        OnValidateDbl(j);
                    }
                }
                isClick[i] = true;
            }
        }
    }

    private void IsWheel()              //마우스 휠 이벤트
    {
        if(Input.GetMouseButtonDown(2))     //마우스 휠 클릭
        {
            if(targetObj == null)
            {
                targetObj = Targeting();
            }
            else                        //타겟팅된 Obj가 있다면
            {
                targetObj = null;
            }
        }
        if(targetObj != null)
        {
            if(Input.GetAxis("Mouse ScrollWheel") != 0 && !isWheel)
            {
                isWheel = true;
                targetObj = Targeting(targetObj,Input.GetAxis("Mouse ScrollWheel") < 0 ? -1 : 1);       //마우스 휠을 위로 굴리면 1(더 먼 타겟팅), 아래로 굴리면 -1(더 가까운 타겟팅)
            }
        }
        
    }

    private void AvoidAnimation(int direction)          //회피 애니메이션
    {

        animatorCtrl.Avoid(direction);
        string direc = "에러";
        if(direction == 0)
        {
            direc = "좌";
        }
        else if(direction == 1)
        {
            direc = "우";
        }
        else if(direction == 2)
        {
            direc = "앞";
        }
        else if(direction == 3)
        {
            direc = "뒤";
        }
        Debug.Log("회피 : " + direc);
        
    }

    private void PlayerCameraCtrl()                 
    {
        //마우스를 통해 캐릭터를 회전시키면, mCam은 돌아가지만, mTrans는 돌아가지 않아야 함.
        //mTrans는 mCam과의 사이각이 ()이상 차이나면 mCam과 일치하게 한다.
        float sheta = mTrans.eulerAngles.y - mCam.eulerAngles.y;
        //sheta값이 180보다 크다면, -360도를 해줄 필요가 있다 (-180~+180 회전값 반환을 위해)
        sheta = sheta < 180 ? sheta : sheta - 360;
        Vector3 camVector = new Vector3(mCam.forward.x, 0, mCam.forward.z);         //캠이 보는 방향(파란색)
        Vector3 playerVector = new Vector3(mTrans.forward.x,0,mTrans.forward.z);    //캐릭터가 보는 방향(빨간색)

        //float Dot = Vector3.Dot(playerVector,camVector);
        //float sheta = Mathf.Acos(Dot);
        //float sheta = Quaternion.FromToRotation(Vector3.up,camVector - playerVector).eulerAngles.z;
        //Vector3 v = camVector - playerVector;
        //float sheta = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            
        //Debug.Log("카메라-플레이어 사이각 : " + sheta);
        Debug.DrawRay(mCam.position,camVector * 10,Color.blue);
        Debug.DrawRay(mTrans.position,playerVector * 10,Color.red);
    }

    private GameObject Targeting()
    {
        //타겟팅될 오브젝트를 모두 TargetObj에 집어넣는다.
        TargetObjs = GameObject.FindGameObjectsWithTag("Enemy");        //타겟이 가능한 오브젝트 집어넣기        
        //거리순으로 정렬한다.        
        Debug.Log("현재 캐릭터의 좌표는 " + ch_pos);
        Array.Sort<GameObject>(TargetObjs, (x,y) => (ch_pos - x.transform.position).sqrMagnitude.CompareTo((ch_pos - y.transform.position).sqrMagnitude));
        //TargetObjs.Sort((x,y) => (ch_pos - x.transform.position).sqrMagnitude.CompareTo((ch_pos -y.transform.position).sqrMagnitude));  //소트가 제대로 되지 않음 (거리기준으로 되야 됨)
        for(int i = 0; i < TargetObjs.Length; i++)
        {
            Debug.Log(i + "번째 오브젝트(" + TargetObjs[i].name + ")의 거리 = " + (ch_pos - TargetObjs[i].transform.position).sqrMagnitude);
        }
        //가장 가까운 오브젝트를 반환한다.
        return TargetObjs[0];
    }

    private GameObject Targeting(GameObject target/*이미 타겟팅된 Object*/, int wheelScroll)    //휠을 굴렸을 때 발동
    {
        //타겟팅될 오브젝트를 모두 TargetObj에 집어넣는다.
        TargetObjs = GameObject.FindGameObjectsWithTag("Enemy");        //타겟이 가능한 오브젝트 집어넣기
        //거리순으로 정렬한다.
        Array.Sort<GameObject>(TargetObjs,(x,y) => (ch_pos - x.transform.position).sqrMagnitude.CompareTo((ch_pos - y.transform.position).sqrMagnitude));
        //TargetObjs.Sort((x,y) => (ch_pos - x.transform.position).sqrMagnitude.CompareTo((ch_pos - y.transform.position).sqrMagnitude));
        //타겟팅 반환
        for(int i = 0; i < TargetObjs.Length; i++)
        {
            if(target == TargetObjs[i % TargetObjs.Length])      //타겟팅된 상대를 찾았다면
            {
                //Debug.Log((i + wheelScroll) % TargetObjs.Count);
                return TargetObjs[(i + wheelScroll + TargetObjs.Length) % TargetObjs.Length];
            }
        }
        /*for(int i = 0; i < TargetObjs.Count; i++)
        {
            if(target == TargetObjs[i % TargetObjs.Count])      //타겟팅된 상대를 찾았다면
            {
                //Debug.Log((i + wheelScroll) % TargetObjs.Count);
                return TargetObjs[(i + wheelScroll + TargetObjs.Count) % TargetObjs.Count];
            }
        }*/

        Debug.Log("타겟팅에러");
        return null;
    }

}
