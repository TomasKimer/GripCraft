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

	[Header("Damage setup")]
	[SerializeField] float        m_DamagePerSecond    = 25f;

	// PRIVATE MEMBERS

	private Transform             m_HeadTransform;
	private BlockTerrainManager   m_TerrainManager;
	private InputManager          m_InputManager;
	private int                   m_SelectedBlockIdx;

	// ISCENECOMPONENT INTERFACE

    void ISceneComponent.Initialize(MainScene scene)
	{
		m_HeadTransform  = scene.PlayerController.HeadTransform;
		m_TerrainManager = scene.TerrainManager;
		m_InputManager   = scene.InputManager;

		m_TerrainBlock.Initialize(m_TerrainManager.BlockSettings);
		UpdateSelectedBlock(true);
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
		HandleBlockChange();

		var selectedBlock = GetSelectedBlock();
		var        origin = m_HeadTransform.position;
		var     direction = m_HeadTransform.forward;

		Debug.DrawLine(origin, origin + direction * m_MaxRaycastDistance);

		if (Physics.Raycast(origin, direction, out var hitInfo, m_MaxRaycastDistance, m_RaycastLayerMask) == true)
		{
			if (selectedBlock == EBlockType.None)
			{
				if (m_InputManager.DamageBlock == true)
				{
					var damageBlockPosition = Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal * 0.5f);

					m_TerrainManager.DamageBlock(damageBlockPosition, m_DamagePerSecond * Time.deltaTime);
				}
			}
			else
			{
				var newBlockPosition = Vector3Int.FloorToInt(hitInfo.point + hitInfo.normal * 0.5f);

				m_TerrainBlock.transform.position = newBlockPosition;

				if (m_InputManager.PlaceBlock == true)
				{
					m_TerrainManager.AddBlock(newBlockPosition, selectedBlock);
				}

				Debug.DrawLine(origin, hitInfo.point, Color.green);
				Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 0.5f, Color.green);
			}

			UpdateSelectedBlock(true);
		}
		else
		{
			UpdateSelectedBlock(false);

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
	}

	private void UpdateSelectedBlock(bool visible)
	{
		var selectedBlock = GetSelectedBlock();
		if (selectedBlock == EBlockType.None)
		{
			ShowBlock(false);
		}
		else
		{
			m_TerrainBlock.SetBlockType(selectedBlock);
			ShowBlock(visible);
		}
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
