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

    private float speed = 0.01f;
    private float dblClickSpeed = 0.5f;             //더블 클릭 시간
    private float wheelingSpeed = 0.1f;             //휠 굴리는 시간(딜레이)
    private float comboTime = 0.5f;                 //콤보 입력을 받는 동안 기다리는 시간
    private KeyCode[] keycode = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S };             //수정바람. (사용자가 입력한 키 일수도 있음)
    private float[] dblClick = new float[4];        //더블 클릭 시간 (왼쪽, 오른쪽, 앞, 뒤)
    private bool[] isClick = new bool[4];           //클릭했냐? (왼쪽, 오른쪽, 앞, 뒤)
    private float mouseWheeling = 0.1f;
    private bool isWheel = false;                   //휠 굴리는 중이냐?
    private bool isHold = false;                    //홀딩중이냐?
    private float combodelay = 0.5f;                //콤보 딜레이
    private int atkCount = 0;                       //공격 횟수
    private bool isMouseDownL = false;
    private bool isMouseDownR = false;
    private float mouseTime = 0.5f;
    private float mouseDelay;                       //마우스가 홀딩이냐 동시클릭이냐?


    public Vector3 ch_pos           //플레이어의 위치값
    {
        get
        {
            return mTrans.position;
        }
    }   
    public Vector3 move             //플레이어의 이동값
    {
        get; private set;
    }
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

    void Awake()
    {
        OnValidate();
        mGameObj = this.gameObject;
        mTrans = this.transform;

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

        switch(m_currentMotion)
        {
            case Motion.Idle:
                break;
            case Motion.Avoid:
                //if(animatorCtrl.MotionEnd("avoid"))    //애니메이션 재생이 끝났다면
                //{
                    Debug.Log("회피 모션 끝!");
                    for(int i = 0; i < keycode.Length; i++)
                    {
                        OnValidateDbl(i);
                    }
                    m_currentMotion = Motion.Idle;
                //}
                break;
            case Motion.Move:
                break;
            case Motion.InputAttack:
                MouseJudgment();
                break;
            case Motion.Attack:
                //콥보어택 딜레이 타임
                if(animatorCtrl.MotionEnd("attack"))    //애니메이션 재생이 끝났다면
                {
                    //isAttack = false;
                    combodelay = comboTime;
                    m_currentMotion = Motion.AfterAttack;
                    Debug.Log("공격 모션 끝!");
                }
                break;
            case Motion.HoldAttack:
                if(animatorCtrl.MotionEnd("hold"))    //애니메이션 재생이 끝났다면
                {
                    Debug.Log("홀드 공격 모션 끝!");
                    m_currentMotion = Motion.Idle;
                }                
                break;
            case Motion.AfterAttack:
                ComboJudgment();
                break;
        }
    }

    //마우스 판정 메소드
    private void MouseJudgment()        
    {
        if(isMouseDownL || isMouseDownR)     //홀드 , 동시 클릭인지 판단
        {
            mouseDelay -= Time.deltaTime;
            if(Input.GetMouseButtonUp(1))       //도중에 마우스를 때면 홀드공격이 아니다.
            {
                isHold = false;
            }
            if(Input.GetMouseButtonDown(0))     //좌클릭을 우클릭을 누르는동안 해도 홀드공격이 아니다
            {
                isMouseDownL = true;
                isHold = false;
            }
            if(Input.GetMouseButtonDown(1))
            {
                isMouseDownR = true;
            }
        }
        if(!isHold)             //홀드 공격이 아니다.
        {
            m_currentMotion = Motion.Attack;
            Attack(isMouseDownL, isMouseDownR);
            isMouseDownL = false;
            isMouseDownR = false;
            return;
        }
        if(mouseDelay <= 0)     //홀드 공격이다.
        {
            m_currentMotion = Motion.HoldAttack;
            animatorCtrl.HoldAttack();
            isMouseDownL = false;
            isMouseDownR = false;
            isHold = false;
            mouseDelay = mouseTime;            
        }
    }

    private void ComboJudgment()
    {
        if(combodelay <= 0 || atkCount >= 2)         //콤보 딜레이가 0이거나, atkCount가 최대일 때 ☆ (공격 카운터가 몇인지 미정 수정필요)
        {
            //Debug.Log("콤보 시간 초과");
            atkCount = 0;
            m_currentMotion = Motion.Idle;
        }

        combodelay -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        //화면 회전(마우스를 통한)은 여기서

        if(Motioning())
        {
            return;
        }


        IsDbl();            //더블(연속)클릭 판정 (회피 판정)
        IsWheel();          //휠 클릭 판정

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool mouse_l = Input.GetMouseButtonDown(0);
        bool mouse_r = Input.GetMouseButtonDown(1);

        //마우스가 클릭되었다면
        if(mouse_l || mouse_r)
        {
            //이동 애니메이션을 정지 시킨다
            for(int i = 0; i < 4; i++)
            {
                animatorCtrl.Move(false,i);
            }

            if(mouse_r)
            {
                isHold = true;
                isMouseDownR = true;
            }
            if(mouse_l)
            {
                isMouseDownL = true;
            }
            mouseDelay = mouseTime;                         //0.5초 기다려!
            m_currentMotion = Motion.InputAttack;           //공격 입력 상태
        }
        else if(h != 0 || v != 0)            //공격중이 아닐 때, 움직이는 판정을 적용한다
        {
            m_currentMotion = Motion.Move;
            if(targetObj == null)       //타겟팅이 된 오브젝트가 없다
            {
                if(mCam != null)
                {
                    //Debug.Log("h = " + h + ", v = " + v);
                    //cam_rot가 맞나????
                    cam_move = Vector3.Scale(mCam.forward,new Vector3(1,0,1)).normalized;       //카메라가 위에 있어서 1,0,1을 곱한건가?
                    move = v * cam_move + h * mCam.right;
                }
                else                    //카메라가 없을 때
                {
                    move = v * Vector3.forward + h * Vector3.right;
                }                
            }
            else                        //타겟팅이 된 오브젝트가 있다. (원운동)
            {
                if(mCam != null)
                {
                    transform.RotateAround(targetObj.transform.position,Vector3.down,h);                    //원운동 속도 제어는 여기서
                    transform.LookAt(targetObj.transform.position);
                    //Debug.Log("h = " + h + ", v = " + v);
                    
                    Vector3 cam_way = Vector3.Scale(mCam.forward,new Vector3(1,0,1)).normalized;
                    move = v * cam_way;                                                                   //앞뒤 속도 제어는 여기서
                    //Debug.Log(move);
                }
                else                    //카메라가 없을 때
                {
                    transform.RotateAround(targetObj.transform.position,Vector3.down,h);
                    transform.LookAt(targetObj.transform.position);
                    move = v * Vector3.forward;     //아마 오류
                    //move = new Vector3(targetObj_pos.x + Mathf.Sin(h),move_pos.y,targetObj_pos.z + Mathf.Cos(h));
                    //move = new Vector3(targetObj_pos.x + distance * Mathf.Cos(Mathf.Atan(distance/h)),move_pos.y,targetObj_pos.z + distance * Mathf.Sin(Mathf.Atan(distance/h)));
                    //move = v * Vector3.forward + Mathf.Cos(h) * distance * Vector3.right + Mathf.Sin(h) * distance * Vector3.right;
                }
            }
            mTrans.position += move * speed;

            if(h != 0)
            {
                animatorCtrl.Move(true,h < 0 ? 0 : 1);               //0 = 좌, 1 = 우
            }
            else
            {
                animatorCtrl.Move(false,0);
                animatorCtrl.Move(false,1);
            }
            if(v != 0)
            {
                animatorCtrl.Move(true,v > 0 ? 2 : 3);               //2 = 앞, 3 = 뒤
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
            for(int i = 0; i < 4; i++)
            {
                animatorCtrl.Move(false,i);
            }
        }
        //해당 자료는 Trigger이기 때문에,
    }
    private bool Motioning()
    {
        if(m_currentMotion == Motion.Attack || m_currentMotion == Motion.Avoid || m_currentMotion == Motion.InputAttack)     //공격중, 회피중, 공격입력중에는 다른 모션을 취할 수 없음.
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
        //애니메이션이 끝나기 직전~Combodelay 사이에 시간에 입력을 받음

        combodelay = comboTime;
        //isAttack = true;
        Debug.Log("atkCount : " + atkCount);
        Debug.Log("left = " + mouseL + ", right = " + mouseR);
        animatorCtrl.Attack(mouseL,mouseR,++atkCount);
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
        //animatorCtrl.Avoid(true, direction);
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
        //animatorCtrl.Avoid(false, direction);
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
