using UnityEngine;
using UnityEngine.InputSystem;

sealed class InputManager : MonoBehaviour, ISceneComponent
{
	// PUBLIC MEMBERS

	public Vector3 MoveDir   { get; private set; }
	public Vector2 LookDelta { get; private set; }
	public bool    Jump      { get; private set; }
	public bool    Sprint    { get; private set; }
	public bool    Fire      { get; private set; }

	// PRIVATE MEMBERS

	private float  m_JumpTime;

	// ISCENECOMPONENT INTERFACE

	void ISceneComponent.Initialize(MainScene scene) { }

	// MONOBEHAVIOUR INTERFACE

	private void LateUpdate()
	{
		if (Jump == true && Time.time - m_JumpTime > 0.2f)
		{
			Jump = false;
		}

		Fire = false;
	}

	// PLAYER INPUT INTERFACE

	public void OnMove(InputAction.CallbackContext context)
    {
		var moveDir = context.ReadValue<Vector2>();

		MoveDir = new Vector3(moveDir.x, 0f, moveDir.y);
	}

	public void OnLook(InputAction.CallbackContext context)
    {
		LookDelta = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
    {
		Jump = context.ReadValue<float>() > 0f;

		if (Jump == true)
		{
			m_JumpTime = Time.time;
		}
	}

	public void OnSprint(InputAction.CallbackContext context)
    {
		Sprint = context.ReadValue<float>() > 0f;
    }

	public void OnFire(InputAction.CallbackContext context)
	{
		Fire = context.ReadValue<float>() > 0f;
	}
}
