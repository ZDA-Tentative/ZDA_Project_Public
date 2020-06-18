using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 공격을 해야해
// 그럼 일단 공격 상태가 됐어
// 패턴중 하나를 설정 해
// 설정한 것 하나를 실행해 ( 찌빠찌찌 이렇게 있으면 이거 다 ) // 여기에 쿨탐을 좀 줄까 생각해보자 일단 넣긴 넣자 
// 실행이 끝나면 상태를 바꾸자 => 추적 // 근데 이건 알아서 바뀜 // 근데 지금은 안바뀌는데? 응ㅁ??? 

public class Sc_EnemyAttack : MonoBehaviour
{

    
    /// <summary>
    /// 애니메이션 패턴, 이 패턴의 int 값과 애니메이션 진입 값이 같아야한다.
    /// </summary>
    public enum Boss_AttackPattern
    {
        up_Cutting = 0,                         // 위로 베어 올리기
        down_Cutting = 1,                       // 아래로 베어 내리기
        horizontal_Cutting = 2,                 // 횡베기
        both_sides_down_Cutting = 3,            // 양쪽베기
        degree_360_Cutting = 4 ,                // 360도 베기
        Kick = 5,                               // 킥

        down_Skills = 100,                        // 내려찍기 특수공격
        combo1_Skills = 101,                      // 콤보1 특수공격 
        combo2_Skills = 102,                      // 콤보2 특수공격
    }
    const int NOMAL_PATTERN_LENGTH = 6;
    const int PATTERN_LENGTH = 8;


    public Sc_Boss sc_Boss;
    public Sc_BossMoveAI sc_BossMoveAI;

    int aniRandIndex = -1;
    int aniCurrentCounter = 0;
    int aniCounter = 4; // 

    int arrPointer;
    public int ArrPointer
    {
        get { return arrPointer; }
        set
        {
            arrPointer = value;
            if(100 <= arrPointer )
            {
                arrPointer = 0;
            }
            
        }
    }

   

    public List<Boss_AttackPattern> boss_Attck_Pattern_Temp = new List<Boss_AttackPattern>(); // 스킬 패턴 Temp
  
    public List<Boss_AttackPattern>[] bossAttackPatternArr;   // 스킬 패턴 저장 

    List<Boss_AttackPattern> boss_AttacksPattern_0 = new List<Boss_AttackPattern>();
    List<Boss_AttackPattern> boss_AttacksPattern_1 = new List<Boss_AttackPattern>();
    List<Boss_AttackPattern> boss_AttacksPattern_2 = new List<Boss_AttackPattern>();
    List<Boss_AttackPattern> boss_AttacksPattern_3 = new List<Boss_AttackPattern>();
    List<Boss_AttackPattern> boss_AttacksPattern_4 = new List<Boss_AttackPattern>();
    List<Boss_AttackPattern> boss_AttacksPattern_5 = new List<Boss_AttackPattern>();
    List<Boss_AttackPattern> boss_AttacksPattern_6 = new List<Boss_AttackPattern>();

    private bool isAttack = false;
    private bool isTryPattern = false;
    public bool IsAttackOver
    {
        get
        {
            return isTryPattern;
        }
    }

    public bool IsAttack
    {
        get { return isAttack; }
        set
        {
            isAttack = value;
            //CasePatternAttack();
            RndAttack();
        }
    }

    
    /// <summary>
    /// 단순 랜덤으로 공격하는 패턴
    /// </summary>
    public void RndAttack()
    {

        if (isAttack)
        {
            //Debug.Log("ani Time : " + sc_Boss.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            //if (sc_Boss.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            //{
            //    Debug.Log("공격 애니메이션 하나 완료: ");
            //}

            if (sc_Boss.IsAnimationOver)
            {
                arrPointer++;
                sc_Boss.IsAnimationOver = false;
                isTryPattern = true;
                Debug.Log("공격 애니메이션 하나 완료: " + arrPointer);
            }
        }
        
    }

    /// <summary>
    /// 케이스로 공격하는 패턴
    /// </summary>
    public void CasePatternAttack() // 아직 버그가 있음
    {
        if (isAttack)
        {
            // Chase 로 상태 전이 함 //
            //if (boss_Attck_Pattern_Temp.Count <= 0 && isTryPattern)
            //{
            //    Debug.Log("CHase 로 상태전이 함.");


            //}



            // 애니메이션 한 케이스 끝나기 전 까지
            if (sc_Boss.IsAnimationOver && 0 < boss_Attck_Pattern_Temp.Count)
            {
                boss_Attck_Pattern_Temp.RemoveAt(0);
                sc_Boss.IsAnimationOver = false;
                isTryPattern = true;
                Debug.Log("공격 애니메이션 하나 완료: " + boss_Attck_Pattern_Temp.Count);
            }
            // 맨 처음 패턴 불러오기 및 초기화 
            if (boss_Attck_Pattern_Temp.Count <= 0)
            {

                isTryPattern = false; // 패턴 케이스 한번 돌았음
                                      // 공격하니까 멈추기
                sc_BossMoveAI.agent.ResetPath();
                // 패턴 랜덤으로 지정
                int skillMaxNum = bossAttackPatternArr.Length;
                aniRandIndex = Random.Range(0, skillMaxNum);

                for (int i = 0; i < bossAttackPatternArr[aniRandIndex].Count; i++)
                {
                    // 애니메이션 템프에 call by value 복사 
                    boss_Attck_Pattern_Temp.Add(bossAttackPatternArr[aniRandIndex][i]);
                    Debug.Log(aniRandIndex + "번째 보스 패턴: " + (int)boss_Attck_Pattern_Temp[i] + " : " + boss_Attck_Pattern_Temp[i].ToString());
                }
                aniCounter = boss_Attck_Pattern_Temp.Count;
                Debug.Log("공격 애니메이션 초기화 완료 : " + isTryPattern);
            }
        }
    }
    private void Awake()
    {
        sc_Boss = GetComponent<Sc_Boss>();
        sc_BossMoveAI = GetComponent<Sc_BossMoveAI>();
        // 케이스 랜덤 패턴일때 사용하는 생성함수
        //AttackPattern();
    }
    void Start()
    {
        // Rand일때 boss_Attck_Pattern_Temp 초기화한다.
        for (int i = 0; i < 100; i++)
        {
            int rndTemp = Random.Range(0, NOMAL_PATTERN_LENGTH);
            // 
            boss_Attck_Pattern_Temp.Add((Boss_AttackPattern)rndTemp);
            
        }
        for (int i = 0; i < 100; i++)
        {
            Debug.Log("생성된 패턴 : [" + i + "]"+boss_Attck_Pattern_Temp[i].ToString());
        }
    }

    // Update is called once per frame

    void Update()
    {

    }

    void AnimationInit()
    {

    }
    void Attack()
    {

        Debug.Log("attack:");

        // 시각화 
        //Debug.DrawRay(transform.position, (navMeshAgent.destination - transform.position).normalized, Color.red);

        if (boss_Attck_Pattern_Temp.Count <= 0)
        {
            return;
        }

        if (false)
        {
        
        }
    }





    // 공격 모션 초기화
    private void AttackPattern()
    {
        // 원래 이렇게 했었다고~~

        //boss_AttacksPattern_0.Add(Boss_AttackPattern.rush);
        //boss_AttacksPattern_0.Add(Boss_AttackPattern.prick);
        //boss_AttacksPattern_0.Add(Boss_AttackPattern.prick);
        //boss_AttacksPattern_0.Add(Boss_AttackPattern.horizontal_Cutting);

        //boss_AttacksPattern_1.Add(Boss_AttackPattern.rush);
        //boss_AttacksPattern_1.Add(Boss_AttackPattern.vertical_Cutting);
        //boss_AttacksPattern_1.Add(Boss_AttackPattern.horizontal_Cutting);
        //boss_AttacksPattern_1.Add(Boss_AttackPattern.vertical_Cutting);

        
        // 이런식으로 초기화 시켜주면 된다.
        StateAdd(boss_AttacksPattern_0,
            Boss_AttackPattern.down_Cutting, Boss_AttackPattern.horizontal_Cutting,
            Boss_AttackPattern.Kick, Boss_AttackPattern.degree_360_Cutting);

        StateAdd(boss_AttacksPattern_1,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.down_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.Kick);
        // 이 아래부터는 패턴 다 똑같음 추후 기획팀이 수정하길 바람
        



        // == 
        bossAttackPatternArr = new List<Boss_AttackPattern>[7];
        bossAttackPatternArr[0] = boss_AttacksPattern_0;
        bossAttackPatternArr[1] = boss_AttacksPattern_1;
        bossAttackPatternArr[2] = boss_AttacksPattern_2;
        bossAttackPatternArr[3] = boss_AttacksPattern_3;
        bossAttackPatternArr[4] = boss_AttacksPattern_4;
        bossAttackPatternArr[5] = boss_AttacksPattern_5;
        bossAttackPatternArr[6] = boss_AttacksPattern_6;


        // 애니메이션 패턴 초기화 

        for (int i = 0; i < bossAttackPatternArr[0].Count; i++)
        {
            Debug.Log("애니메이션 패턴 초기화 : " + bossAttackPatternArr[0][i].ToString());
        }
        Debug.Log("배열 크기 : " + bossAttackPatternArr.Length);


    }


    void StateAdd(List<Boss_AttackPattern> addList, Boss_AttackPattern a, Boss_AttackPattern b, Boss_AttackPattern c, Boss_AttackPattern d)
    {
        addList.Add(a);
        addList.Add(b);
        addList.Add(c);
        addList.Add(d);
    }
    void StateAdd(List<Boss_AttackPattern> addList, Boss_AttackPattern a, Boss_AttackPattern b, Boss_AttackPattern c)
    {
        addList.Add(a);
        addList.Add(b);
        addList.Add(c);
    }


}
