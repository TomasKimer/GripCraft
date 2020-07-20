using UnityEngine;

public class PlayerController : MonoBehaviour, ISceneComponent
{
	// CONFIGURATION

	[Header("Move")]
	[SerializeField] float      m_MoveSpeed   = 6f;
	[SerializeField] float      m_SprintSpeed = 10f;
	[SerializeField] float      m_JumpHeight  = 6f;

	[Header("Look")]
	[SerializeField] float      m_LookSensitivity = 0.2f;
	[SerializeField] Transform  m_HeadTransform   = null;

	// PRIVATE MEMBERS

	private InputManager        m_InputManager;
	private CharacterController m_CharacterController;
	private float               m_VelocityY;
	private float               m_HeadRotationX;

	// ISCENECOMPONENT INTERFACE

	void ISceneComponent.Initialize(MainScene scene)
	{
		m_InputManager        = scene.InputManager;
		m_CharacterController = GetComponent<CharacterController>();

		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
		UpdateMove();
		UpdateLook();
	}

	// PRIVATE METHODS

	private void UpdateMove()
	{
		var deltaTime = Time.deltaTime;

		if (m_CharacterController.isGrounded == true)
		{
			m_VelocityY = m_InputManager.Jump ? m_JumpHeight : -1f;
		}
		else
		{
			m_VelocityY += Physics.gravity.y * deltaTime;
		}

		var     speed = m_InputManager.Sprint ? m_SprintSpeed : m_MoveSpeed;
		var deltaMove = transform.TransformDirection(m_InputManager.MoveDir) * speed;
		deltaMove.y  += m_VelocityY;

		m_CharacterController.Move(deltaMove * deltaTime);
	}

	private void UpdateLook()
	{
		var lookDelta = m_InputManager.LookDelta * m_LookSensitivity;

		transform.Rotate(lookDelta.x * Vector3.up);

		m_HeadRotationX = Mathf.Clamp(m_HeadRotationX - lookDelta.y, -90f, 90f);
		m_HeadTransform.localRotation = Quaternion.Euler(m_HeadRotationX, 0f, 0f);
	}
}
