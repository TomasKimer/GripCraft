using System.Collections.Generic;
using UnityEngine;
using BlockData = BlockTerrainChunk.BlockData;

sealed class BlockTerrainManager : MonoBehaviour, ISceneComponent
{
	// CONFIGURATION

	[Header("Block setup")]
	[SerializeField] BlockSettings     m_BlockSettings = null;

	[Header("Chunk setup")]
	[SerializeField] BlockTerrainChunk m_TerrainChunk  = null;
	[Range(8, 128)]        
	[SerializeField] int               m_ChunkWidth    = 16;
	[Range(8, 128)]
	[SerializeField] int               m_ChunkHeight   = 32;
	[Range(1, 32)]
	[SerializeField] int               m_ChunkDistance = 10;

	[Header("Noise setup")]
	[SerializeField] float             m_PerlinScale   = 0.025f;
	[SerializeField] Vector2           m_PerlinOffset  = Vector2.zero;

	// PUBLIC MEMBERS

	public  int                        ChunkWidth    => m_ChunkWidth;
	public  int                        ChunkHeight   => m_ChunkHeight;
	public  BlockSettings              BlockSettings => m_BlockSettings;

	// PRIVATE MEMBERS

	private Transform                  m_PlayerTransform;
	private Vector2Int                 m_PlayerChunkPosition = new Vector2Int(int.MinValue, int.MinValue);

	private Dictionary<Vector2Int, BlockTerrainChunk> m_ActiveChunks    = new Dictionary<Vector2Int, BlockTerrainChunk>();
	private List<Vector2Int>                          m_ChunksToRemove  = new List<Vector2Int>();
	private Dictionary<Vector2Int, BlockData[,,]>     m_CachedChunkData = new Dictionary<Vector2Int, BlockData[,,]>();

	// PUBLIC METHODS

	public void AddBlock(Vector3Int position, EBlockType blockType)
	{
		var (chunk, positionInChunkX, positionInChunkY, positionInChunkZ) = GetTargetChunk(position);
		if  (chunk == null)
			return;

		chunk.SetBlock(positionInChunkX, positionInChunkY, positionInChunkZ, blockType);
	}

	public void DamageBlock(Vector3Int position, float damage)
	{
		var (chunk, positionInChunkX, positionInChunkY, positionInChunkZ) = GetTargetChunk(position);
		if  (chunk == null)
			return;

		chunk.DamageBlock(positionInChunkX, positionInChunkY, positionInChunkZ, damage);
	}

	// ISCENECOMPONENT INTERFACE

	void ISceneComponent.Initialize(MainScene scene)
	{
		m_PlayerTransform = scene.PlayerController.transform;

		m_TerrainChunk.gameObject.SetActive(false);
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
		UpdateChunks();
	}	

	// PRIVATE METHODS

	private void UpdateChunks()
	{
		var playerChunkPosition = GetChunkPosition(m_PlayerTransform.position);
		if (playerChunkPosition == m_PlayerChunkPosition)
			return;

		m_PlayerChunkPosition = playerChunkPosition;
		
		CreateNewChunks();
		RemoveFarChunks();
	}

	private void CreateNewChunks()
	{
		for (int x = m_PlayerChunkPosition.x - m_ChunkDistance; x <= m_PlayerChunkPosition.x + m_ChunkDistance; ++x)
		{
			for (int z = m_PlayerChunkPosition.y - m_ChunkDistance; z <= m_PlayerChunkPosition.y + m_ChunkDistance; ++z)
			{
				var chunkPosition = new Vector2Int(x, z);

				if (m_ActiveChunks.ContainsKey(chunkPosition) == false)
				{
					m_ActiveChunks[chunkPosition] = CreateChunk(chunkPosition);
				}
			}
		}
	}

	private void RemoveFarChunks()
	{
		foreach (var chunkPosition in m_ActiveChunks.Keys)
		{
			var diff = chunkPosition - m_PlayerChunkPosition;
			if (Mathf.Abs(diff.x) <= m_ChunkDistance && Mathf.Abs(diff.y) <= m_ChunkDistance)
				continue;
			
			m_ChunksToRemove.Add(chunkPosition);
		}

		foreach (var chunkToRemove in m_ChunksToRemove)
		{
			var chunk = m_ActiveChunks[chunkToRemove];
			if (chunk.Changed == true)
			{
				m_CachedChunkData.Add(chunkToRemove, chunk.Blocks);
			}

			Destroy(chunk.gameObject);
			m_ActiveChunks.Remove(chunkToRemove);
		}

		m_ChunksToRemove.Clear();
	}

	private BlockTerrainChunk CreateChunk(Vector2Int chunkPos)
	{
		var worldX = chunkPos.x * m_ChunkWidth;
		var worldZ = chunkPos.y * m_ChunkWidth;

		var newChunk = Instantiate(m_TerrainChunk, new Vector3(worldX, 0, worldZ), Quaternion.identity, transform);
		newChunk.name = $"Chunk {chunkPos}";
		newChunk.gameObject.SetActive(true);
		
		m_CachedChunkData.TryGetValue(chunkPos, out var cachedData);
		newChunk.Initialize(m_ChunkWidth, m_ChunkHeight, m_PerlinScale, m_PerlinOffset, m_BlockSettings, cachedData);

		if (cachedData == null)
		{
			newChunk.GenerateHeightmap(worldX, worldZ);
		}
		else
		{
			m_CachedChunkData.Remove(chunkPos);
		}

		newChunk.UpdateMesh();

		return newChunk;
	}

	private (BlockTerrainChunk chunk, int x, int y, int z) GetTargetChunk(Vector3Int position)
	{
		var chunkPosition = GetChunkPosition(position);
		m_ActiveChunks.TryGetValue(chunkPosition, out var chunk);

		var positionInChunkX = position.x - chunkPosition.x * m_ChunkWidth;
		var positionInChunkY = position.y;
		var positionInChunkZ = position.z - chunkPosition.y * m_ChunkWidth;

		return (chunk, positionInChunkX, positionInChunkY, positionInChunkZ);
	}

	private Vector2Int GetChunkPosition(Vector3 position)
	{
		return new Vector2Int(
			Mathf.FloorToInt(position.x / m_ChunkWidth),
			Mathf.FloorToInt(position.z / m_ChunkWidth)
		);
	}
}
