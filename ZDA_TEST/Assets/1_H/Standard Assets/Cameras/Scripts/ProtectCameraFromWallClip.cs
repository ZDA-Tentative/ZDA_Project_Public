using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Cameras
{
    public class ProtectCameraFromWallClip : MonoBehaviour
    {
        public float clipMoveTime = 0.05f;              // Ŭ������ ���� �� �̵��ϴ� �� �ɸ��� �ð� (���� �� = ����) // clip �ڸ��ٴ� �ǹ̷� ���̴� �� 
        public float returnTime = 0.4f;                 // Ŭ�������� ���� �� ���ϴ� ��ġ�� �ǵ��ư��� �� �ɸ��� �ð� (�Ϲ������� clipMoveTime���� ���� ���̾�� ��)
        public float sphereCastRadius = 0.1f;           // ī�޶�� ��� ������ ��ü�� �׽�Ʈ�ϴ� �� ���Ǵ� ���� �ݰ�
        public bool visualiseInEditor;                  // �����Ϳ��� ���� ĳ��Ʈ ������ ���� �˰����� �ð�ȭ�ϱ����� ���
        public float closestDistance = 0.5f;            // ī�޶� ��󿡼� ���� ����� �Ÿ�
        public bool protecting { get; private set; }    // ���� ī�޶� ���̿� ��ü�� �ִ��� Ȯ���ϴ� �� ���
        public string dontClipTag = "Player";           // �� �±׸� ����Ͽ� ��ü�� Ŭ�������� �ʽ��ϴ� (Ÿ���� �� ��ü�� Ŭ�������� �ʴ� �� ������)

        private Transform m_Cam;                  // ī�޶��� transform
        private Transform m_Pivot;                // �����ϱ� ���� ī�޶� ȸ���ϴ� ����
        private float m_OriginalDist;             // ī�޶���� ���� �Ÿ�
        private float m_MoveVelocity;             // ī�޶� �̵� �� �ӵ�
        private float m_CurrentDist;              // ī�޶󿡼� �������� ���� �Ÿ�
        private Ray m_Ray = new Ray();                        // ���� ĳ��Ʈ ���� �Ÿ��� ���ϱ� ���� ī�޶�� ��� ������ ĳ���� 
        private RaycastHit[] m_Hits;              // ī�޶�� ���
        private RayHitComparer m_RayHitComparer;  // ���� ĳ��Ʈ ���� �Ÿ��� ���ϴ� ����


        private void Start()
        {
            // hierarchy  ī�޶� ã���ϴ�
            m_Cam = GetComponentInChildren<Camera>().transform;
            m_Pivot = m_Cam.parent;
            m_OriginalDist = m_Cam.localPosition.magnitude;
            m_CurrentDist = m_OriginalDist;

            // create a new RayHitComparer
            m_RayHitComparer = new RayHitComparer();
        }


        private void LateUpdate()
        {
            // initially set the target distance // ó���� ��ǥ �Ÿ��� ����
            float targetDist = m_OriginalDist;
            Debug.DrawRay(m_Ray.origin, m_Ray.direction,Color.blue);
            m_Ray.origin = m_Pivot.position + m_Pivot.forward*sphereCastRadius;
            m_Ray.direction = -m_Pivot.forward;

            // spherecast�� ������ ������ �����ϴ��� Ȯ��
            var cols = Physics.OverlapSphere(m_Ray.origin, sphereCastRadius);

            bool initialIntersect = false;// ù�Ӹ� ������? 
            bool hitSomething = false;

            Debug.Log(initialIntersect);

            //��� �浹�� �ݺ��Ͽ� �浹?�� �ִ��� Ȯ���մϴ�.
            // �ƴ� ������ �ص� cols.Length �ȹٲ�;; �� ���� ���ؼ�???
            //Ʈ���Ű� ��������
            for (int i = 0; i < cols.Length; i++)
            {

                // �ݶ��̴��� Tirgger�� ���� !(������ٵ� ������ �ְ� �±װ�(dontClipTag)�� ���ٸ�)
                if ((!cols[i].isTrigger) &&
                    !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag))) // cols[i].attachedRigidbody �ݶ��̴��� �������ִ� ��ü�� ������ٵ� �ִٸ� �� ������ٵ� �����´�. ������ٵ� ���ٸ� Null�� �����Ѵ�. 
                {
                    Debug.Log("!cols[i].isTrigger + " + !cols[i].isTrigger + "�ƹ�ư �� : " + !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag)));
                    initialIntersect = true;
                    break;
                }
            }

            // if there is a collision // ���� �͵��� �ȵ����� initialIntersect�� false �ۿ� �ȵ� 
            if (initialIntersect)
            {
                m_Ray.origin += m_Pivot.forward*sphereCastRadius;

                // do a raycast and gather all the intersections
                m_Hits = Physics.RaycastAll(m_Ray, m_OriginalDist - sphereCastRadius); // RaycastAll�� Dis��ŭ �浹�� ��� �͵��� ���� 
            }
            else // �׷��� else�� ����
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
                //���� ) Ʈ���Ű� �ƴ� ���� �浹�� ������ dontClipTag �±װ� ������ ��ü�� �������� ���� ��쿡�� �浹�� ó���մϴ�.
                if (m_Hits[i].distance < nearest && (!m_Hits[i].collider.isTrigger) &&
                    !(m_Hits[i].collider.attachedRigidbody != null && // ����ڵ� #attachedRigidbody
                      m_Hits[i].collider.attachedRigidbody.CompareTag(dontClipTag))) // 
                {
                    // change the nearest collision to latest
                    nearest = m_Hits[i].distance; // ray�� �������κ��� �浹 ���������� �Ÿ��� ��Ÿ���ϴ�.
                    targetDist = -m_Pivot.InverseTransformPoint(m_Hits[i].point).z; // ���� 
                    transform.InverseTransformPoint(m_Hits[i].point);
                    hitSomething = true;
                }
            }

            // visualise the cam clip effect in the editor // ���� �¾����� ���������� ���̽��༭ ���־������� ���̰� ���� 
            if (hitSomething)
            {
                Debug.DrawRay(m_Ray.origin, -m_Pivot.forward*(targetDist + sphereCastRadius), Color.red);
            }

            // ��ſ� �¾����� ī�޶� �� ���� ��ġ�� �ű�
            protecting = hitSomething; // protecting�� ���߿� �ٸ���ũ��Ʈ���� �����ؼ� ó��������
            // �̰� float ���̶� Vector3�� smoothDamp�� �ƴ�
            // ������ ���ָ鼭 
            
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
