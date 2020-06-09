using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Cameras
{
    public class ProtectCameraFromWallClip : MonoBehaviour
    {
        public float clipMoveTime = 0.05f;              // 클리핑을 피할 때 이동하는 데 걸리는 시간 (낮은 값 = 빠름) // clip 자르다는 의미로 쓰이는 듯 
        public float returnTime = 0.4f;                 // 클리핑하지 않을 때 원하는 위치로 되돌아가는 데 걸리는 시간 (일반적으로 clipMoveTime보다 높은 값이어야 함)
        public float sphereCastRadius = 0.1f;           // 카메라와 대상 사이의 물체를 테스트하는 데 사용되는 구의 반경
        public bool visualiseInEditor;                  // 에디터에서 레이 캐스트 라인을 통해 알고리즘을 시각화하기위한 토글
        public float closestDistance = 0.5f;            // 카메라가 대상에서 가장 가까운 거리
        public bool protecting { get; private set; }    // 대상과 카메라 사이에 물체가 있는지 확인하는 데 사용
        public string dontClipTag = "Player";           // 이 태그를 사용하여 객체를 클리핑하지 않습니다 (타겟팅 된 객체를 클리핑하지 않는 데 유용함)

        private Transform m_Cam;                  // 카메라의 transform
        private Transform m_Pivot;                // 수정하기 전에 카메라가 회전하는 지점
        private float m_OriginalDist;             // 카메라와의 원래 거리
        private float m_MoveVelocity;             // 카메라가 이동 한 속도
        private float m_CurrentDist;              // 카메라에서 대상까지의 현재 거리
        private Ray m_Ray = new Ray();                        // 레이 캐스트 적중 거리를 비교하기 위해 카메라와 대상 사이의 캐스팅 
        private RaycastHit[] m_Hits;              // 카메라와 대상
        private RayHitComparer m_RayHitComparer;  // 레이 캐스트 적중 거리를 비교하는 변수


        private void Start()
        {
            // hierarchy  카메라를 찾습니다
            m_Cam = GetComponentInChildren<Camera>().transform;
            m_Pivot = m_Cam.parent;
            m_OriginalDist = m_Cam.localPosition.magnitude;
            m_CurrentDist = m_OriginalDist;

            // create a new RayHitComparer
            m_RayHitComparer = new RayHitComparer();
        }


        private void LateUpdate()
        {
            // initially set the target distance // 처음에 목표 거리를 설정
            float targetDist = m_OriginalDist;
            Debug.DrawRay(m_Ray.origin, m_Ray.direction,Color.blue);
            m_Ray.origin = m_Pivot.position + m_Pivot.forward*sphereCastRadius;
            m_Ray.direction = -m_Pivot.forward;

            // spherecast의 시작이 무엇과 교차하는지 확인
            var cols = Physics.OverlapSphere(m_Ray.origin, sphereCastRadius);

            bool initialIntersect = false;// 첫머리 교차점? 
            bool hitSomething = false;

            Debug.Log(initialIntersect);

            //모든 충돌을 반복하여 충돌?이 있는지 확인합니다.
            // 아니 뭔짓을 해도 cols.Length 안바뀜;; 뭘 막기 위해서???
            //트리거가 켜져있으
            for (int i = 0; i < cols.Length; i++)
            {

                // 콜라이더의 Tirgger가 없고 !(리지드바디를 가지고 있고 태그값(dontClipTag)과 같다면)
                if ((!cols[i].isTrigger) &&
                    !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag))) // cols[i].attachedRigidbody 콜라이더를 가지고있는 개체가 리지드바디가 있다면 그 리지드바디를 가져온다. 리지드바디가 없다면 Null을 리턴한다. 
                {
                    Debug.Log("!cols[i].isTrigger + " + !cols[i].isTrigger + "아무튼 뒤 : " + !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag)));
                    initialIntersect = true;
                    break;
                }
            }

            // if there is a collision // 위의 것들이 안들어오니 initialIntersect는 false 밖에 안됨 
            if (initialIntersect)
            {
                m_Ray.origin += m_Pivot.forward*sphereCastRadius;

                // do a raycast and gather all the intersections
                m_Hits = Physics.RaycastAll(m_Ray, m_OriginalDist - sphereCastRadius); // RaycastAll은 Dis만큼 충돌한 모든 것들을 리턴 
            }
            else // 그래서 else만 들어옴
            {
                // if there was no collision do a sphere cast to see if there were any other collisions
                m_Hits = Physics.SphereCastAll(m_Ray, sphereCastRadius, m_OriginalDist + sphereCastRadius);
            }
            Debug.Log("m_Hits : "+ m_Hits.Length);

            // sort the collisions by distance // 
            Array.Sort(m_Hits, m_RayHitComparer);

            // set the variable used for storing the closest to be as far as possible
            float nearest = Mathf.Infinity;

            // loop through all the collisions
            for (int i = 0; i < m_Hits.Length; i++)     
            {
                // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
                //번역 ) 트리거가 아닌 이전 충돌에 가깝고 dontClipTag 태그가 지정된 강체에 부착되지 않은 경우에만 충돌을 처리합니다.
                if (m_Hits[i].distance < nearest && (!m_Hits[i].collider.isTrigger) &&
                    !(m_Hits[i].collider.attachedRigidbody != null && // 방어코드 #attachedRigidbody
                      m_Hits[i].collider.attachedRigidbody.CompareTag(dontClipTag))) // 
                {
                    // change the nearest collision to latest
                    nearest = m_Hits[i].distance; // ray의 원점으로부터 충돌 지점까지의 거리를 나타냅니다.
                    targetDist = -m_Pivot.InverseTransformPoint(m_Hits[i].point).z; // 원래 
                    transform.InverseTransformPoint(m_Hits[i].point);
                    hitSomething = true;
                }
            }

            // visualise the cam clip effect in the editor // 뭔가 맞았을때 빨간색으로 레이쏴줘서 비주얼적으로 보이게 해줌 
            if (hitSomething)
            {
                Debug.DrawRay(m_Ray.origin, -m_Pivot.forward*(targetDist + sphereCastRadius), Color.red);
            }

            // 어떤거에 맞았을때 카메라를 더 좋은 위치로 옮김
            protecting = hitSomething; // protecting는 나중에 다른스크립트에서 접근해서 처리가능함
            // 이건 float 값이라서 Vector3의 smoothDamp가 아님
            // 보간을 해주면서 
            
            m_CurrentDist = Mathf.SmoothDamp(m_CurrentDist, targetDist, ref m_MoveVelocity ,
                                           m_CurrentDist > targetDist ? clipMoveTime : returnTime); // 
            m_CurrentDist = Mathf.Clamp(m_CurrentDist, closestDistance, m_OriginalDist);
            m_Cam.localPosition = -Vector3.forward*m_CurrentDist;
          
        }
        

        private void OnDrawGizmos()
        {
            //Gizmos.color = Color.red;
            //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
            //Gizmos.DrawWireSphere(m_Ray, sphereCastRadius, m_OriginalDist + sphereCastRadius);
            
        }

        // comparer for check distances in ray cast hits
        public class RayHitComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((RaycastHit) x).distance.CompareTo(((RaycastHit) y).distance);
                
            }
        }
    }
}
