using System.Collections.Generic;
using UnityEngine;

enum EBlockType
{
	None,
	Grass,
	Dirt,
	Snow,
	Stone
}

[CreateAssetMenu(fileName = "BlockSettings", menuName = "Scriptable Objects/BlockSettings", order = 1)]
sealed class BlockSettings : ScriptableObject
{
	// PUBLIC CONSTANTS

	public static readonly Vector3[] LEFT_VERTICES = {
		new Vector3(0, 0, 1),
		new Vector3(0, 1, 1),
		new Vector3(0, 1, 0),
		new Vector3(0, 0, 0)
	};

	public static readonly Vector3[] RIGHT_VERTICES = {
		new Vector3(1, 0, 0),
		new Vector3(1, 1, 0),
		new Vector3(1, 1, 1),
		new Vector3(1, 0, 1)
	};

	public static readonly Vector3[] FRONT_VERTICES = {
		new Vector3(0, 0, 0),
		new Vector3(0, 1, 0),
		new Vector3(1, 1, 0),
		new Vector3(1, 0, 0)
	};

	public static readonly Vector3[] BACK_VERTICES = {
		new Vector3(1, 0, 1),
		new Vector3(1, 1, 1),
		new Vector3(0, 1, 1),
		new Vector3(0, 0, 1)
	};

	public static readonly Vector3[] TOP_VERTICES = {
		new Vector3(0, 1, 0),
		new Vector3(0, 1, 1),
		new Vector3(1, 1, 1),
		new Vector3(1, 1, 0)
	};

	public static readonly Vector3[] BOTTOM_VERTICES = {
		new Vector3(0, 0, 0),
		new Vector3(1, 0, 0),
		new Vector3(1, 0, 1),
		new Vector3(0, 0, 1)
	};

	// PUBLIC CLASSES

	public class BlockInfo
	{
		public readonly Vector2[] TopUVs;
		public readonly Vector2[] SideUVs;
		public readonly Vector2[] BottomUVs;
		public readonly int       FromHeight;
		public readonly float     Health;

		public BlockInfo(Vector2[] topUVs, Vector2[] sideUVs, Vector2[] bottomUVs, int fromHeight, float health)
		{
			TopUVs     = topUVs;
			SideUVs    = sideUVs;
			BottomUVs  = bottomUVs;
			FromHeight = fromHeight;
			Health     = health;
		}
	}

	// PRIVATE ENUMS / CLASSES

    private enum ETile
	{
		Grass,
		GrassSide,
		Stone,
		Dirt,
		Snow,
		SnowSide
	}

	[System.Serializable]
	private class BlockSetup
	{
		public EBlockType BlockType  = EBlockType.Grass;
		public ETile      TileTop    = ETile.Grass;
		public ETile      TileSide   = ETile.Grass;
		public ETile      TileBottom = ETile.Grass;
		public int        FromHeight = 0;
		public float      Health     = 100f;
	}

	[System.Serializable]
	private class TileSetup
	{
		public ETile      Tile     = ETile.Grass;
		public Vector2Int Position = Vector2Int.zero;
	}

	// CONFIGURATION

	[Header("Blocks")]
	[SerializeField] BlockSetup[] m_BlockSettings = null; 

	[Header("Tiles")]
	[SerializeField] int          m_TileSize      = 16;
	[SerializeField] float        m_TileUVOffset  = 0.001f;
	[SerializeField] TileSetup[]  m_TileSettings  = null;

	// PRIVATE MEMBERS

	private Dictionary<EBlockType, BlockInfo> m_CachedBlockInfos = new Dictionary<EBlockType, BlockInfo>(4);

	// PUBLIC METHODS

	public BlockInfo GetBlockInfo(EBlockType blockType)
	{
		if (m_CachedBlockInfos.TryGetValue(blockType, out var blockInfo) == true)
			return blockInfo;

		var settings = System.Array.Find(m_BlockSettings, obj => obj.BlockType == blockType);
		if (settings == null)
			throw new System.ArgumentOutOfRangeException("No settings for block type " + blockType);

		var newBlockInfo = new BlockInfo
		(
			GetUVs(settings.TileTop),
			GetUVs(settings.TileSide),
			GetUVs(settings.TileBottom),
			settings.FromHeight,
			settings.Health
		);

		m_CachedBlockInfos[blockType] = newBlockInfo;

		return newBlockInfo;
	}

	// PRIVATE METHODS

	private Vector2[] GetUVs(ETile tile)
	{
		var settings = System.Array.Find(m_TileSettings, obj => obj.Tile == tile);
		if (settings == null)
			throw new System.ArgumentOutOfRangeException("No settings for tile " + tile);

		var position = settings.Position;
		position.y = m_TileSize - position.y - 1;

		return new Vector2[]
		{
			new Vector2((position.x    ) / (float)m_TileSize + m_TileUVOffset, (position.y    ) / (float)m_TileSize + m_TileUVOffset),
			new Vector2((position.x    ) / (float)m_TileSize + m_TileUVOffset, (position.y + 1) / (float)m_TileSize - m_TileUVOffset),
			new Vector2((position.x + 1) / (float)m_TileSize - m_TileUVOffset, (position.y + 1) / (float)m_TileSize - m_TileUVOffset),
			new Vector2((position.x + 1) / (float)m_TileSize - m_TileUVOffset, (position.y    ) / (float)m_TileSize + m_TileUVOffset)
		};
	}
}
