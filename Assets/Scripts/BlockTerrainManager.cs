using UnityEngine;

sealed class BlockTerrainManager : MonoBehaviour, ISceneComponent
{
	// CONFIGURATION

	[Header("Chunk setup")]
	[SerializeField] BlockTerrainChunk m_TerrainChunk = null;
	[Range(8, 64)]
	[SerializeField] int               m_ChunkWidth   = 16;
	[Range(8, 32)]
	[SerializeField] int               m_ChunkHeight  = 32;

	[Header("Noise setup")]
	[SerializeField] float             m_PerlinScale  = 2f;
	[SerializeField] Vector2           m_PerlinOffset = Vector2.zero;

	// PRIVATE MEMBERS

	private PlayerController           m_PlayerController;

	// ISCENECOMPONENT INTERFACE

	void ISceneComponent.Initialize(MainScene scene)
	{
		m_PlayerController = scene.PlayerController;

		m_TerrainChunk.gameObject.SetActive(false);

		for (int i = -5; i < 5; ++i)
			for (int j = -5; j < 5; ++j)
				AddChunk(i, j);
	}

	// MONOBEHAVIOUR INTERFACE

	private void Update()
	{
		// TODO
	}

	// PRIVATE METHODS

	private void AddChunk(int x, int z)
	{
		var worldX = x * m_ChunkWidth;
		var worldZ = z * m_ChunkWidth;

		var newChunk = Instantiate(m_TerrainChunk, new Vector3(worldX, 0, worldZ), Quaternion.identity, transform);
		newChunk.gameObject.SetActive(true);

		newChunk.Initialize(m_ChunkWidth, m_ChunkHeight, m_PerlinScale, m_PerlinOffset);
		newChunk.GenerateHeightmap(worldX, worldZ);
		newChunk.UpdateMesh();
	}
}
