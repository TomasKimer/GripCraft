using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	// CONFIGURATION

	[Header("Move")]
	[SerializeField] float      m_MoveSpeed   = 6f;
	[SerializeField] float      m_SprintSpeed = 10f;
	[SerializeField] float      m_JumpHeight  = 6f;

	[Header("Look")]
	[SerializeField] float      m_LookSensitivity = 1f;
	[SerializeField] Transform  m_HeadTransform   = null;

	// PRIVATE MEMBERS

	private CharacterController m_CharacterController;
	private float               m_VelocityY;
	private float               m_HeadRotationX;

	private Vector3             m_InputMoveDir;
	private Vector2             m_InputLookDelta;
	private bool                m_InputJump;
	private bool                m_InputSprint;

	// MONOBEHAVIOUR INTERFACE

	private void Awake()
	{
		m_CharacterController = GetComponent<CharacterController>();

		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
	}

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
			if (m_InputJump == true)
			{
				m_InputJump = false;
				m_VelocityY = m_JumpHeight;
			}
			else
			{
				m_VelocityY = -1f;
			}
		}
		else
		{
			m_InputJump = false;
			m_VelocityY += Physics.gravity.y * deltaTime;
		}

		var     speed = m_InputSprint ? m_SprintSpeed : m_MoveSpeed;
		var deltaMove = transform.TransformDirection(m_InputMoveDir) * speed;
		deltaMove.y  += m_VelocityY;

		m_CharacterController.Move(deltaMove * deltaTime);
	}

	private void UpdateLook()
	{
		var lookDelta = m_InputLookDelta * m_LookSensitivity * Time.deltaTime;

		transform.Rotate(lookDelta.x * Vector3.up);

		m_HeadRotationX = Mathf.Clamp(m_HeadRotationX - lookDelta.y, -90f, 90f);
		m_HeadTransform.localRotation = Quaternion.Euler(m_HeadRotationX, 0f, 0f);
	}

	// INPUT SYSTEM CALLBACKS

	public void OnInputMove(InputAction.CallbackContext context)
    {
		var moveDir = context.ReadValue<Vector2>();

		m_InputMoveDir.x = moveDir.x;
		m_InputMoveDir.z = moveDir.y;
	}

	public void OnInputLook(InputAction.CallbackContext context)
	{
		m_InputLookDelta = context.ReadValue<Vector2>();
	}

	public void OnInputJump(InputAction.CallbackContext context)
    {
		m_InputJump = context.ReadValue<float>() > 0f;
    }

	public void OnInputSprint(InputAction.CallbackContext context)
	{
		m_InputSprint = context.ReadValue<float>() > 0f;
	}
}
