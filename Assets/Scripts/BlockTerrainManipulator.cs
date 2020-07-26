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
	private EBlockType            m_SelectedBlock;

	// ISCENECOMPONENT INTERFACE

    void ISceneComponent.Initialize(MainScene scene)
	{
		m_Player         = scene.PlayerController;
		m_TerrainManager = scene.TerrainManager;
		m_InputManager   = scene.InputManager;
		m_SelectedBlock  = m_BlockVariants[0];

		m_TerrainBlock.Initialize(m_TerrainManager.BlockSettings);
		m_TerrainBlock.SetBlockType(m_SelectedBlock);

		ShowBlock(false);
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
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
				m_TerrainManager.AddBlock(newBlockPosition, m_SelectedBlock);
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

	private void ShowBlock(bool show)
	{
		var go = m_TerrainBlock.gameObject;
		if (go.activeSelf == show)
			return;

		go.SetActive(show);
	}
}
