using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Cameras
{
    public class FreeLookCam : PivotBasedCameraRig
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [SerializeField] private float m_MoveSpeed = 1f;                      // 대상의 위치를 ​​유지하기 위해 리그가 얼마나 빨리 움직입니다.
        [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // 사용자 입력에서 리그가 얼마나 빨리 회전하는지.
        [SerializeField] private float m_TurnSmoothing = 0.0f;                // 마우스 입력의 왜곡을 줄이기 위해 회전 입력에 적용 할 스무딩 양
        [SerializeField] private float m_TiltMax = 75f;                       // 피벗의 x 축 회전 최대 값입니다.
        [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [SerializeField] private bool m_LockCursor = false;                   // 커서를 숨기고 잠글 지 여부
        [SerializeField] private bool m_VerticalAutoReturn = false;           // 수직 축이 자동 복귀해야하는지 여부를 설정

        private float m_LookAngle;                    // 리그의 y 축 회전.
        private float m_TiltAngle;                    // 피벗의 x 축 회전입니다.Tilt는 경사,  기울이다
        private const float k_LookDistance = 100f;    //100 캐릭터의 피처가 피벗에서 얼마나 멀리 떨어져 있는지.
        private Vector3 m_PivotEulers;
		private Quaternion m_PivotTargetRot;
		private Quaternion m_TransformTargetRot;

        protected override void Awake()
        {
            base.Awake();
            // Lock or unlock the cursor.// 커서를 잠 그거나 잠금 해제합니다.
            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_LockCursor;
			m_PivotEulers = m_Pivot.rotation.eulerAngles;

	        m_PivotTargetRot = m_Pivot.transform.localRotation;
			m_TransformTargetRot = transform.localRotation;
        }


        protected void Update()
        {
            HandleRotationMovement();
            if (m_LockCursor && Input.GetMouseButtonUp(0))
            {
                Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !m_LockCursor;
            }
        }


        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        protected override void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime*m_MoveSpeed);
        }


        private void HandleRotationMovement()
        {
            //Debug.Log(Time.timeScale + " ? " +  float.Epsilon);
			if(Time.timeScale < float.Epsilon) // ?? 의미 불명... Time.timeScale가 1.4보다 작으면 리턴된다는게 무엇...?
                return;
            
            // Read the user input
            var x = CrossPlatformInputManager.GetAxis("Mouse X");
            var y = CrossPlatformInputManager.GetAxis("Mouse Y");

            // 회전 속도와 수평 입력에 비례하는 양으로 룩 각도를 조정하십시오.
            m_LookAngle += x*m_TurnSpeed;

            // Rotate the rig (the root object) around Y axis only:
            
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);
            // m_VerticalAutoReturn 자동으로 화면의 중앙을 볼 수 있게 해주는 기능이다.
            // 따라서 회전을 
            if (m_VerticalAutoReturn)
            {
                // 틸트 입력의 경우 마우스를 사용하는지 터치 입력을 사용하는지에 따라 다르게 동작해야합니다.
                // 모바일에서 수직 입력은 기울기 값에 직접 매핑되므로 룩 입력이 해제되면 자동으로 튀어 나옵니다.
                // 최소값과 최대 값이 대칭이 아닌 경우에도 자동으로 0으로 돌아 가려고하므로 0보다 높거나 낮은 지 테스트해야합니다.
                m_TiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
            }
            else
            {
                // 마우스가있는 플랫폼에서는 Y 마우스 입력 및 회전 속도에 따라 현재 각도를 조정합니다
                m_TiltAngle -= y*m_TurnSpeed;
                // 새 값이 기울기 범위 내에 있는지 확인하십시오
                m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
            }

            // Tilt input around X is applied to the pivot (the child of this object)
			m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y , m_PivotEulers.z);

			if (m_TurnSmoothing > 0)
			{
				m_Pivot.localRotation = Quaternion.Slerp(m_Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime);
				transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
			}
			else
			{
				m_Pivot.localRotation = m_PivotTargetRot;
				transform.localRotation = m_TransformTargetRot;
			}
        }
    }
}
