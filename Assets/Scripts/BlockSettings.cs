using System.Collections.Generic;
using UnityEngine;

enum EBlockType
{
	None,
	Grass,
	Dirt,
	Ice,
	Stone
}

[CreateAssetMenu(fileName = "BlockSettings", menuName = "Scriptable Objects/BlockSettings", order = 1)]
sealed class BlockSettings : ScriptableObject
{
	// PUBLIC CLASSES

	public class BlockInfo
	{
		public readonly Vector2[] TopUVs;
		public readonly Vector2[] SideUVs;
		public readonly Vector2[] BottomUVs;
		public readonly int       FromHeight;

		public BlockInfo(Vector2[] topUVs, Vector2[] sideUVs, Vector2[] bottomUVs, int fromHeight)
		{
			TopUVs     = topUVs;
			SideUVs    = sideUVs;
			BottomUVs  = bottomUVs;
			FromHeight = fromHeight;
		}
	}

	// PRIVATE ENUMS / CLASSES

    private enum ETile
	{
		Grass,
		GrassSide,
		Stone,
		Dirt,
		Ice,
		IceSide
	}

	[System.Serializable]
	private class BlockSetup
	{
		public EBlockType BlockType  = EBlockType.Grass;
		public ETile      TileTop    = ETile.Grass;
		public ETile      TileSide   = ETile.Grass;
		public ETile      TileBottom = ETile.Grass;
		public int        FromHeight = 0;
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
			settings.FromHeight
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
			new Vector2((position.x    ) / (float)m_TileSize, (position.y    ) / (float)m_TileSize),
			new Vector2((position.x    ) / (float)m_TileSize, (position.y + 1) / (float)m_TileSize),
			new Vector2((position.x + 1) / (float)m_TileSize, (position.y + 1) / (float)m_TileSize),
			new Vector2((position.x + 1) / (float)m_TileSize, (position.y    ) / (float)m_TileSize)
		};
	}
}
