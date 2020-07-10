using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorCtrl : MonoBehaviour
{
    [SerializeField] private Animator mAnimator;
    
    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if(mAnimator == null)
        {
            mAnimator = GetComponent<Animator>();
        }
    }

    internal void Move(bool moving, int direction)                       //direction 좌우앞뒤 = 0,1,2,3
    {
        /*if(moving)
        {           
            //Debug.Log("걷기 실행 : " + direction + ", Tostring 확인 : move" + direction.ToString());
        }*/
        mAnimator.SetBool("move" + direction.ToString(), moving);       //SetTrigger으로 작동하나?
        mAnimator.SetBool("moved",moving);
    }

    internal void Rotate(string direction)
    {
        mAnimator.SetTrigger("rotate" + direction);
    }

    internal void Dash(bool dash)
    {
        mAnimator.SetBool("dash",dash);
    }

    internal void Attack(bool atk_l, bool atk_r, int atk_num)
    {        
        if(atk_l && atk_r)
        {
            Debug.Log("동시클릭이잖아??");     //모션 없음
        }
        else if(atk_l)
        {
            mAnimator.SetTrigger("attackL" + atk_num);
            Debug.Log("attackL" + atk_num);
        }
        else if(atk_r)
        {
            mAnimator.SetTrigger("attackR" + atk_num);
            Debug.Log("attackR" + atk_num);
        }
    }

    public bool MotionEnd(string motionName)
    {
        bool isMotion = false;
        switch(motionName)
        {
            case "attack":
                for(int i = 1; i <= 3 && !isMotion; i++)
                {
                    //isMotion = (mAnimator.GetCurrentAnimatorStateInfo(0).IsName(motionName + i.ToString()) && mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f) || isMotion;
                    isMotion = (mAnimator.GetCurrentAnimatorStateInfo(0).IsName(motionName + "L" + i.ToString()) && mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f) || isMotion;
                    if(i == 3)
                    {
                        break;
                    }
                    isMotion = (mAnimator.GetCurrentAnimatorStateInfo(0).IsName(motionName + "R" + i.ToString()) && mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f) || isMotion;
                }
                break;
            case "avoid":
                for(int i = 0; i < 4 && !isMotion; i++)
                {
                    isMotion = (mAnimator.GetCurrentAnimatorStateInfo(0).IsName(motionName + i.ToString()) && mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f) || isMotion;
                }
                break;
            default:                //공격 모션 이외에는 추가되는 변수명이 존재하지 않는다.
                //Debug.Log("모션End 접근중 : " + motionName);
                isMotion = (mAnimator.GetCurrentAnimatorStateInfo(0).IsName(motionName) && mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f) || isMotion;
                break;
        }

        if(motionName == "avoid")
        {
            mAnimator.SetBool("avoided",false);
        }

        return isMotion;
    }

    internal void HoldAttack()
    {
        Debug.Log("홀드 어택");
        mAnimator.SetTrigger("holdAttack");
    }

    //해당 메소드 필요 없음 (혹은 차지모션 중간에 풀릴경우, 이 메소드가 실행하도록 하자!)
    internal void HoldAttackFake()
    {
        Debug.Log("홀드Fake 어택 출력");
        mAnimator.SetTrigger("holdAttackFake");
    }


    internal void Avoid(int direction)
    {
        mAnimator.SetTrigger("avoid" + direction);
        mAnimator.SetBool("avoided",true);
    }
}
