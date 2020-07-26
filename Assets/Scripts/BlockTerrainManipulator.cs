using UnityEngine;

sealed class BlockTerrainManipulator : MonoBehaviour, ISceneComponent
{
	// CONFIGURATION

	[Header("Block setup")]
	[SerializeField] TerrainBlock m_TerrainBlock       = null;
	[SerializeField] EBlockType[] m_BlockVariants      = null;

	[Header("Raycast setup")]
	[SerializeField] float        m_MaxRaycastDistance = 5f;
	[SerializeField] LayerMask    m_RaycastLayerMask   = default;

	// PRIVATE MEMBERS

	private PlayerController      m_Player;
	private BlockTerrainManager   m_TerrainManager;
	private InputManager          m_InputManager;
	private int                   m_SelectedBlockIdx;

	// ISCENECOMPONENT INTERFACE

    void ISceneComponent.Initialize(MainScene scene)
	{
		m_Player         = scene.PlayerController;
		m_TerrainManager = scene.TerrainManager;
		m_InputManager   = scene.InputManager;

		m_TerrainBlock.Initialize(m_TerrainManager.BlockSettings);
		m_TerrainBlock.SetBlockType(GetSelectedBlock());

		ShowBlock(false);
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
		HandleBlockChange();

		var headTransform = m_Player.HeadTransform;
		var        origin = headTransform.position;
		var     direction = headTransform.forward;

		Debug.DrawLine(origin, origin + direction * m_MaxRaycastDistance);

		if (Physics.Raycast(origin, direction, out var hitInfo, m_MaxRaycastDistance, m_RaycastLayerMask) == true)
		{
			var newBlockPosition = Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal * 0.5f);

			m_TerrainBlock.transform.position = newBlockPosition;
			ShowBlock(true);

			if (m_InputManager.Fire == true)
			{
				m_TerrainManager.AddBlock(newBlockPosition, GetSelectedBlock());
			}

			Debug.DrawLine(origin, hitInfo.point, Color.green);
			Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 0.5f, Color.green);
		}
		else
		{
			ShowBlock(false);

			Debug.DrawLine(origin, origin + direction * m_MaxRaycastDistance, Color.red);
		}
	}

	// PRIVATE METHODS

	private void HandleBlockChange()
	{
		var change = m_InputManager.ChangeWeapon;
		if (change == 0)
			return;

		m_SelectedBlockIdx += change;

		if (m_SelectedBlockIdx < 0)
		{
			m_SelectedBlockIdx = m_BlockVariants.Length - 1;
		}
		else if (m_SelectedBlockIdx >= m_BlockVariants.Length)
		{
			m_SelectedBlockIdx = 0;
		}

		m_TerrainBlock.SetBlockType(GetSelectedBlock());
	}

	private EBlockType GetSelectedBlock()
	{
		return m_BlockVariants[m_SelectedBlockIdx];
	}

	private void ShowBlock(bool show)
	{
		var go = m_TerrainBlock.gameObject;
		if (go.activeSelf == show)
			return;

		go.SetActive(show);
	}
}
