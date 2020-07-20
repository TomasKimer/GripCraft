using System.Collections.Generic;
using UnityEngine;

sealed class BlockTerrainManager : MonoBehaviour, ISceneComponent
{
	// CONFIGURATION

	[Header("Chunk setup")]
	[SerializeField] BlockTerrainChunk m_TerrainChunk        = null;
	[Range(8, 128)]        
	[SerializeField] int               m_ChunkWidth          = 16;
	[Range(8, 128)]
	[SerializeField] int               m_ChunkHeight         = 32;
	[Range(1, 32)]
	[SerializeField] int               m_ChunkDistance       = 10;

	[Header("Noise setup")]
	[SerializeField] float             m_PerlinScale         = 0.025f;
	[SerializeField] Vector2           m_PerlinOffset        = Vector2.zero;

	// PRIVATE MEMBERS

	private Transform                  m_PlayerTransform;
	private Vector2Int                 m_PlayerChunkPosition = new Vector2Int(int.MinValue, int.MinValue);

	private Dictionary<Vector2Int, BlockTerrainChunk> m_ActiveChunks   = new Dictionary<Vector2Int, BlockTerrainChunk>();
	private List<Vector2Int>                          m_ChunksToRemove = new List<Vector2Int>();

	// ISCENECOMPONENT INTERFACE

	void ISceneComponent.Initialize(MainScene scene)
	{
		m_PlayerTransform = scene.PlayerController.transform;
		m_PlayerTransform.position = new Vector3(m_ChunkWidth / 2, m_ChunkHeight + 10, m_ChunkWidth / 2);

		m_TerrainChunk.gameObject.SetActive(false);
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
		var playerChunkPosition = GetPlayerChunkPosition();
		if (playerChunkPosition == m_PlayerChunkPosition)
			return;

		m_PlayerChunkPosition = playerChunkPosition;
		
		CreateNewChunks();
		RemoveFarChunks();
	}	

	// PRIVATE METHODS

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
		newChunk.gameObject.SetActive(true);
		newChunk.name = $"Chunk {chunkPos}";

		newChunk.Initialize(m_ChunkWidth, m_ChunkHeight, m_PerlinScale, m_PerlinOffset);
		newChunk.GenerateHeightmap(worldX, worldZ);
		newChunk.UpdateMesh();

		return newChunk;
	}

	private Vector2Int GetPlayerChunkPosition()
	{
		return new Vector2Int(
			Mathf.FloorToInt(m_PlayerTransform.position.x / m_ChunkWidth),
			Mathf.FloorToInt(m_PlayerTransform.position.z / m_ChunkWidth)
		);
	}
}
