using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sc_Boss : MonoBehaviour
{
    public enum BossState
    {
        idle,   // 대기
        chase,  // 추적 
        attack, // 공격
        die,    // 죽음 
        waiting_for_battle, // 전투 대기 
        anytime

    }

// == 기본 스테이터스 및 변수 ==

    public BossState bossState = BossState.idle;


    public const int HP_MAX = 100;                      // 보스 max hp 
    private int hp_Current = 0;                         // 보스 현재 hp
    public Animator animator;                                  // 애니메이션
    Sc_GMng gameManager;                                // 게임매니저 인스턴스 

    [Range(0f,0.5f)]
    public const float SKILLCHECK_HP = 0.2f;            // 보스 특수 스킬 쓸 hp 퍼센트 외부 입력값
    private float skillCheck_HpChecker = 0;             // 보스 특수 스킬 쓸 hp 퍼센트 비율 적용

    
    private bool isAnimationOver = false;                // 애니메이션 끝났을 때 이벤트 함수로 인해 True가 됨.
    /// <summary>
    /// 애니메이션이 끝나면 True, 애니메이션이 시작되면 false. 아직 자동으로는 처리는 안됨 따라서 추후 False 처리 해줘야 함.
    /// </summary>
    public bool IsAnimationOver
    {
        get
        {
            return isAnimationOver;
        }
        set
        {
            isAnimationOver = value;
            //StartCoroutine(StartTimer(1));

        }
    }

    public Sc_BossMoveAI sc_BossMoveAi;                 // AI로 움직이는 스크립트 
    public Sc_EnemyAttack sc_EnemyAttack;               // 공격 패턴 스크립트 

    public bool isPlayerAlive = true;

    private WaitForSeconds ws; // 코루틴 지연 시간 

//  [Move] 

    [SerializeField]
    private float moveSpeed = 5.0f;
    public NavMeshAgent navMeshAgent;

// [Chase]

    float dist;

// [waiting_for_battle]
    public float Waiting_for_battle_WaitTIme = 2;
    public float Waiting_for_battle_WaitTImeAdd = 1f;
    public float waiting_for_battle = 0.2f; // 전투대기 범위

// [Attack]

    public GameObject target;       // 공격할 대상 
    public float attackDis = 1.5f;     // 공격 감지 범위 

// TEST
    public Material testMat;

// [애니메이터 컨트롤러에 정의한 파라미터의 해시값을 미리 추출]

    private readonly int hash_attackIndex = Animator.StringToHash("Index_attack");
    private readonly int hash_Trigger_attack_trigger = Animator.StringToHash("Trigger_attack");
    private readonly int hash_Trigger_waiting_for_battle_trigger = Animator.StringToHash("Trigger_waiting_for_battle");
    private readonly int hash_Is_Attack = Animator.StringToHash("Is_Attack");
    private readonly int hash_Trigger_Chase = Animator.StringToHash("Trigger_Chase");
    private readonly int hash_Trigger_Idle = Animator.StringToHash("Trigger_Idle");


    
    private void Awake()
    {
        Reset();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        sc_BossMoveAi = GetComponent<Sc_BossMoveAI>();
        sc_EnemyAttack = GetComponent<Sc_EnemyAttack>();

        ws = new WaitForSeconds(1f);
    }

    IEnumerator StartTimer(float maxTime)
    {
        float currentTIme = 0;
        while(currentTIme <= maxTime)
        {
            currentTIme += Time.deltaTime;
            Debug.Log("타이머 : " + currentTIme);
            yield return null;
        }
        if(maxTime <= currentTIme)
        {
            StopCoroutine("StartCoroutine");
        }

    }
    void Start()
    {

    

    }
    //초기화 
    void Reset()
    {
        // hp 체크 초기화 
        skillCheck_HpChecker = HP_MAX * SKILLCHECK_HP;
        // hp 초기화 
        hp_Current = HP_MAX;
        navMeshAgent.SetDestination(target.transform.position);
    }
    void OnEnable()
    {

        gameManager = Sc_GMng.instance;
        if (Sc_GMng.instance == null)
        {
            //Debug.Log("test null");
        }

        //CheckState 코루틴 함수 실행
        StartCoroutine(CheckState());
        //Action 코루틴 함수 실행
        StartCoroutine(Action());
      
    }
    void OnDisable()
    {
        
    }

    //적 캐릭터의 상태를 검사하는 코루틴 함수
    IEnumerator CheckState()
    {
        //오브젝트 풀에 생성 시 다른 스크립트의 초기화를 위해 대기
        yield return new WaitForSeconds(1.0f);

        //적 캐릭터가 사망하기 전까지 도는 무한루프
        while (isPlayerAlive)
        {
            yield return null;

// == anitime Start == 

        // 죽음 상태로 진입 체크
            if (hp_Current <= 0)
            {
                bossState = BossState.die;
                yield break;
            }
        // 특수 스킬 상태로 진입 체크 

            // hp 100 100 < 0.2
            if (hp_Current < skillCheck_HpChecker)
            {
                Debug.Log("특수 스킬 진입 체크");
                // 3번 진행되니까 20퍼 일때 0.3배씩 계속 차감 
                //=> 20 , 14퍼 , 9.8퍼
                skillCheck_HpChecker -= HP_MAX * (SKILLCHECK_HP * 0.7f);
                yield break;
            }
            // 히트 되었을때  // 히트 된 부분은 따로 설정 해야함.

            // == anitime End == 

            //if(sc_EnemyAttack.IsAttackOver)
            //{
            //    Debug.Log("공격 끝났음");
            //    bossState = BossState.chase;
            //    yield break; 
            //}


            dist = Vector3.Distance(target.transform.position, transform.position);
            // if문으로 조건을 처리하다 보니 기본값은 하위로 체크가 먼저 필요한 것은 상위로 배치 된다. 
            Debug.Log("거리 : " + dist);
        // == 공격 == 
            if (dist < attackDis)
            {
                bossState = BossState.attack;
                //yield break;


            }
        // == 추적 ==  
            else if ((attackDis <  dist) && IsAnimationOver )
            {
                Debug.Log("BossState.chase : " + dist);
                bossState = BossState.chase;
                //yield break;

            }

            

        }
    }

    //상태에 따라 적 캐릭터의 행동을 처리하는 코루틴 함수
    IEnumerator Action()
    {
        

        while (isPlayerAlive)
        {
            yield return ws;
            switch (bossState)
            {
                case BossState.idle:
                    bossState = BossState.chase;
                    animator.SetTrigger(hash_Trigger_Idle);
                    idle();
                    //sc_EnemyAttack.IsAttack = false;
                    break;

                case BossState.chase:
                    
                    sc_BossMoveAi.TraceTarget = target.transform.position; // 실행 프로퍼티 
                    animator.SetTrigger(hash_Trigger_Chase);
                    Chase();

                    sc_EnemyAttack.IsAttack = false;
                    break; // 

                case BossState.attack:
                    
                    sc_EnemyAttack.IsAttack = true; // 실행프로퍼티 공격을 시작한다. 
                    sc_BossMoveAi.Stop();           // 공격할때는 움직이지 않는다. 
                    animator.SetTrigger(hash_Trigger_attack_trigger);
                    if(0 < sc_EnemyAttack.boss_Attck_Pattern_Temp.Count) // 예외처리 
                    {
                        animator.SetInteger(hash_attackIndex, (int)sc_EnemyAttack.boss_Attck_Pattern_Temp[sc_EnemyAttack.ArrPointer]);
                    }
                    // 어택이 한번 끝나고 destination을 초기화 해주기 위해
                    //bossState = BossState.waiting_for_battle;
                    break;

                case BossState.waiting_for_battle:
                    
                    dist = Vector3.Distance(navMeshAgent.destination, transform.position);
                    break;

                case BossState.die:
                    Die();
                    break;
            }
        }
       
    }
   

    // Update is called once per frame
    void Update()
    {
        //sc_BossMoveAi.Trace(target.transform.position);
    }
    void idle()
    {
        
    }

    // 추적
    void Chase()
    {
        //추적 시작
        
        Debug.DrawRay(transform.position, (navMeshAgent.destination - transform.position).normalized * 0.1f, Color.green);
      
    }

 



    void Die()
    {

    }



    /// <summary>
    /// 애니메이션이 끝나면 호출되게 설정할 수 있는 이벤트 함수
    /// </summary>
    private void AniEvt_IsAnimationOver()
    {
        Debug.Log(" 애니메이션 끝.");
        isAnimationOver = true;
    }
    private void AniEvt_IsAnimationStart()
    {
        Debug.Log(" 애니메이션 끝.");
        isAnimationOver = true;
    }

}

///
/// 버그리스트
/// 
/// 1. 애니메이션이 끝나기도 전에 움직여서 굉장히 보기 안좋다. 
/// 
/// isAnimationOver가 true가 되었을때 그리고 공격범위를 벗어났을때 Chase를 하자
/// => 안됨.
/// 조건을 검색하고 함수 실행중인데 조건이 매 프레임마다 일어나서 조건이 맞자 마자 바로 상태변이가 일어나고 
/// 바로 실행시켜서 어택도중에 Chase가 되어버리는거같다. 
/// => 그럼 공격중에는 함수 실행안되게 예외처리 하자.
///     => 공격중이라는것을 어떻게 알것인가? 
///        공격중에는 애니메이션을 실행한다. 애니메이션이 실행될때
///     
/// 
/// 
/// 
///