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

    // 이 패턴의 int 값과 애니메이션 진입 값이 같아야한다.
    public enum Boss_AttackPattern
    {
        prick =0 , //찌르기
        vertical_Cutting = 1, // 종베기
        horizontal_Cutting =2, // 횡베기
        rush = 3, // 돌진 
        special_Skills, // 특수공격
        backstep // 백스탭
    }


    public Sc_Boss sc_Boss;
    public Sc_BossMoveAI sc_BossMoveAI;

    int aniRandIndex = -1;
    int aniCurrentCounter = 0;
    int aniCounter = 4; // 


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
    public bool IsAttack
    {
        get { return isAttack; }
        set
        {
            isAttack = value;

            if(isAttack)
            {


                // Chase 로 상태 전이 함 
                if (boss_Attck_Pattern_Temp.Count <= 0 &&
                    aniCounter <= aniCurrentCounter)
                {

                }

                // 맨 처음 패턴 불러오기 및 초기화 
                if (boss_Attck_Pattern_Temp.Count <= 0)
                {
                    // 공격하니까 멈추기
                    sc_BossMoveAI.agent.ResetPath();
                    // 패턴 랜덤으로 지정
                    int skillMaxNum = bossAttackPatternArr.Length; // @이거 왜 안되냐
                    aniRandIndex = Random.Range(0, skillMaxNum);

                    for (int i = 0; i < bossAttackPatternArr[aniRandIndex].Count; i++) // AnimationRan
                    {
                        boss_Attck_Pattern_Temp.Add(bossAttackPatternArr[aniRandIndex][i]);
                        Debug.Log(aniRandIndex+"번째 보스 패턴: " + (int)boss_Attck_Pattern_Temp[i]+" : " + boss_Attck_Pattern_Temp[i].ToString()  );
                    }
                    aniCounter = boss_Attck_Pattern_Temp.Count;
                    Debug.Log("공격 애니메이션 초기화 완료");
                }

                // 애니메이션 하나 끝났다면 
                if (sc_Boss.isAnimationOver && 0 < boss_Attck_Pattern_Temp.Count)
                {
                    boss_Attck_Pattern_Temp.RemoveAt(0);
                    sc_Boss.isAnimationOver = false;
                    aniCurrentCounter++;
                }
            }
            
        }
    }
    private void Awake()
    {
        sc_Boss = GetComponent<Sc_Boss>();
        sc_BossMoveAI = GetComponent<Sc_BossMoveAI>();
        AttackPattern();
    }
    void Start()
    {
        for (int i = 0; i < bossAttackPatternArr[0].Count; i++)
        {
            Debug.Log("초기화 : " + bossAttackPatternArr[0][i].ToString());
        }
        Debug.Log("배열 크기 : " + bossAttackPatternArr.Length);
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

        

        StateAdd(boss_AttacksPattern_0,
            Boss_AttackPattern.rush, Boss_AttackPattern.prick,
            Boss_AttackPattern.prick, Boss_AttackPattern.horizontal_Cutting);

        StateAdd(boss_AttacksPattern_1,
            Boss_AttackPattern.rush, Boss_AttackPattern.vertical_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.vertical_Cutting);
        // 이 아래부터는 패턴 다 똑같음 추후 기획팀이 수정 
        StateAdd(boss_AttacksPattern_2,
            Boss_AttackPattern.rush, Boss_AttackPattern.vertical_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.vertical_Cutting);

        StateAdd(boss_AttacksPattern_3,
            Boss_AttackPattern.rush, Boss_AttackPattern.vertical_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.vertical_Cutting);

        StateAdd(boss_AttacksPattern_4,
            Boss_AttackPattern.rush, Boss_AttackPattern.vertical_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.vertical_Cutting);

        StateAdd(boss_AttacksPattern_5,
            Boss_AttackPattern.rush, Boss_AttackPattern.vertical_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.vertical_Cutting);

        StateAdd(boss_AttacksPattern_6,
            Boss_AttackPattern.rush, Boss_AttackPattern.vertical_Cutting,
            Boss_AttackPattern.horizontal_Cutting, Boss_AttackPattern.vertical_Cutting);



        // == 
        bossAttackPatternArr = new List<Boss_AttackPattern>[7];
        bossAttackPatternArr[0] = boss_AttacksPattern_0;
        bossAttackPatternArr[1] = boss_AttacksPattern_1;
        bossAttackPatternArr[2] = boss_AttacksPattern_2;
        bossAttackPatternArr[3] = boss_AttacksPattern_3;
        bossAttackPatternArr[4] = boss_AttacksPattern_4;
        bossAttackPatternArr[5] = boss_AttacksPattern_5;
        bossAttackPatternArr[6] = boss_AttacksPattern_6;
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
