using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Sc_Monster : MonoBehaviour
{
    public enum MonsterState
    {
        idle,   // 대기
        patrol, // 순찰
        chase,  // 추적 
        attack, // 공격
        die,    // 죽음 
        waiting_for_battle, // 전투 대기 
        anytime 

    }
    public MonsterState monsterState = MonsterState.idle;


//  [Move] 
    [SerializeField]
    private float moveSpeed = 5.0f;
    private NavMeshAgent navMeshAgent;

//  [Patrol]
    // 순찰할 지역 배열
    public Transform []patrolAreas ;

//  [Chase]
    // 추적 거리
    public float chaseDis = 0.2f;
//  [waiting_for_battle]
    // 전투대기 범위
    public float waiting_for_battle = 0.2f;
//  [Attack]
    // 공격할 대상 
    public GameObject target; //
    // 공격 감지 범위 
    public float attackDis;

    // 


    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        patrolAreas = GameObject.Find("PatrolAreas").GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

        switch(monsterState)
        {


            case MonsterState.idle:
                idle();
                break;
            case MonsterState.patrol:
                Patrol();
                break;
            case MonsterState.chase:
                Chase();
                break;
            case MonsterState.attack:
                Attack();
                break;
            case MonsterState.die:
                Die();
                break;
            case MonsterState.waiting_for_battle:
                Waiting_for_battle();
                break;
         
        }
    }
    void idle()
    {
        monsterState = MonsterState.patrol;
    }

    bool patrolOnce = true;
    int randomTemp = -1;
    private void Patrol()
    {

        if (patrolOnce)
        {
            randomTemp = UnityEngine.Random.Range(0, patrolAreas.Length);
            patrolOnce = false;
            target = patrolAreas[randomTemp].gameObject;
        }
        //Debug.Log("보스 패턴 : " + randomTemp);
        MoveTo(target.transform);
        //발견
        //if()
        //{

        //}

        // 목표 위치(navMeshAgent.destination)와 내 위치(transform.position)의 거리가 0.1미만일 때
        // 즉, 목표 위치에 거의 도착했을 때
        if (Vector3.Distance(navMeshAgent.destination, transform.position) < 0.1f)
        {
         
            // 내 위치를 목표 위치로 설정
            transform.position = navMeshAgent.destination;
            // SetDestination()에 의해 설정된 경로를 초기화. 이동을 멈춘다
            navMeshAgent.ResetPath();
            // 이동 타겟 재설정
            patrolOnce = true; 
        }
    }
    // 추적
    void Chase()
    {
        MoveTo(target.transform);
        // 시각화 
        Debug.DrawRay(transform.position, (navMeshAgent.destination - transform.position).normalized * 0.1f);
        if (Vector3.Distance(navMeshAgent.destination, transform.position) < chaseDis)
        {
            Debug.Log("도착");
            // 내 위치를 목표 위치로 설정
            transform.position = navMeshAgent.destination;
            // SetDestination()에 의해 설정된 경로를 초기화. 이동을 멈춘다
            navMeshAgent.ResetPath();
            // 일정범위 안에 들어왔으니 공격상태로 전이
            monsterState = MonsterState.waiting_for_battle;
        }
    }
    //
    void Attack()
    {
        // 시각화 
        Debug.DrawRay(transform.position, (navMeshAgent.destination - transform.position).normalized);

        if(Vector3.Distance(navMeshAgent.destination, transform.position) < 1f)
        {
            // 공격 모션
            Debug.Log("공격함");
            return;

        }

    }

    void Die()
    {

    }

    void Waiting_for_battle()
    {

    }
    private void MoveTo(Transform goalPosition)
    {
        // 이동 속도 설정
        navMeshAgent.speed = moveSpeed;
        // 목표지점 설정 (목표까지의 경로 계산 후 알아서 움직인다)
        navMeshAgent.SetDestination(goalPosition.transform.position);
        // 이동 애니메이션
        
    }

}
