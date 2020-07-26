using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using IndexFormat = UnityEngine.Rendering.IndexFormat;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
sealed class BlockTerrainChunk : MonoBehaviour
{
	// CONSTANTS

	private const int VERTICES_PER_FACE = 4;

	// PRIVATE STRUCTS

	private struct BlockData
	{
		public EBlockType BlockType;
		public float      Health;
	}

	// PRIVATE MEMBERS

	private BlockData[,,]     m_BlockData;
	private int               m_Width;
	private int               m_Height;
	private float             m_PerlinScale; 
	private Vector2           m_PerlinOffset;
	private BlockSettings     m_BlockSettings;
	private MeshFilter        m_MeshFilter;
	private MeshCollider      m_MeshCollider;
	private Mesh              m_Mesh;
	private List<Vector3>     m_Vertices     = new List<Vector3>();
	private List<int>         m_Indices      = new List<int>();
	private List<Vector2>     m_UVs          = new List<Vector2>();

	// PUBLIC METHODS

	public void Initialize(int width, int height, float perlinScale, Vector2 perlinOffset, BlockSettings blockSettings)
	{
		m_Width         = width;
		m_Height        = height;
		m_PerlinScale   = perlinScale;
		m_PerlinOffset  = perlinOffset;
		m_BlockSettings = blockSettings;

		m_BlockData     = new BlockData[m_Width, m_Height, m_Width];
	}

	public void GenerateHeightmap(int posX, int posZ)
	{
		for (int x = 0; x < m_Width; ++x)
		{
			for (int z = 0; z < m_Width; ++z)
			{
				var height = GetTerrainHeight(posX + x, posZ + z);

				for (int y = 0; y < m_Height; ++y)
				{
					var blockType = GetBlockTypeForHeight(y, height);

					m_BlockData[x, y, z].BlockType = GetBlockTypeForHeight(y, height);

					if (blockType != EBlockType.None)
					{
						m_BlockData[x, y, z].Health = m_BlockSettings.GetBlockInfo(blockType).Health;
					}
				}
			}
		}
	}

	public void SetBlock(int x, int y, int z, EBlockType blockType)
	{
		if (CheckBounds(x, y, z) == false)
			return;

		m_BlockData[x, y, z].BlockType = blockType;
		m_BlockData[x, y, z].Health    = m_BlockSettings.GetBlockInfo(blockType).Health;

		UpdateMesh();
	}

	public void DamageBlock(int x, int y, int z, float damage)
	{
		if (CheckBounds(x, y, z) == false)
			return;

		var health = m_BlockData[x, y, z].Health;
		health -= damage;

		if (health <= 0f)
		{
			m_BlockData[x, y, z].BlockType = EBlockType.None;
			UpdateMesh();
		}

		m_BlockData[x, y, z].Health = Mathf.Max(0f, health);
	}

	public void UpdateMesh()
	{
		var pos = new Vector3();

		for (int x = 0; x < m_Width; ++x)
		{
			for (int y = 0; y < m_Height; ++y)
			{
				for (int z = 0; z < m_Width; ++z)
				{
					var blockType = m_BlockData[x, y, z].BlockType;
					if (blockType == EBlockType.None)
						continue;

					pos.x = x;
					pos.y = y;
					pos.z = z;

					var faceCount = 0;
					var blockInfo = m_BlockSettings.GetBlockInfo(blockType);

					if (IsNone(x - 1, y, z))
					{
						AddFaceVertices(pos, BlockSettings.LEFT_VERTICES);
						m_UVs.AddRange(blockInfo.SideUVs);
						faceCount += 1;
					}

					if (IsNone(x + 1, y, z))
					{
						AddFaceVertices(pos, BlockSettings.RIGHT_VERTICES);
						m_UVs.AddRange(blockInfo.SideUVs);
						faceCount += 1;
					}

					if (IsNone(x, y, z - 1))
					{
						AddFaceVertices(pos, BlockSettings.FRONT_VERTICES);
						m_UVs.AddRange(blockInfo.SideUVs);
						faceCount += 1;
					}

					if (IsNone(x, y, z + 1))
					{
						AddFaceVertices(pos, BlockSettings.BACK_VERTICES);
						m_UVs.AddRange(blockInfo.SideUVs);
						faceCount += 1;
					}

					if (IsNone(x, y + 1, z))
					{
						AddFaceVertices(pos, BlockSettings.TOP_VERTICES);
						m_UVs.AddRange(blockInfo.TopUVs);
						faceCount += 1;
					}

					if (IsNone(x, y - 1, z))
					{
						AddFaceVertices(pos, BlockSettings.BOTTOM_VERTICES);
						m_UVs.AddRange(blockInfo.BottomUVs);
						faceCount += 1;
					}

					var startIdx = m_Vertices.Count - faceCount * VERTICES_PER_FACE;
					for (int i = 0; i < faceCount; ++i)
					{
						var idx = startIdx + i * VERTICES_PER_FACE;

						m_Indices.Add(idx);
						m_Indices.Add(idx + 1);
						m_Indices.Add(idx + 2);

						m_Indices.Add(idx);
						m_Indices.Add(idx + 2);
						m_Indices.Add(idx + 3);
					}
				}
			}
		}

		SetMeshIndexFormat(m_Vertices.Count);

		m_Mesh.Clear();
		m_Mesh.vertices  = m_Vertices.ToArray();
		m_Mesh.triangles = m_Indices.ToArray();
		m_Mesh.uv        = m_UVs.ToArray();

		m_Mesh.Optimize();
		m_Mesh.RecalculateNormals();

		m_MeshFilter.mesh         = m_Mesh;
		m_MeshCollider.sharedMesh = m_Mesh;
		
		m_Vertices.Clear();
		m_Indices.Clear();
		m_UVs.Clear();
	}

	// MONOBEHAVIOUR INTERFACE

	private void Awake()
	{
		m_MeshFilter   = GetComponent<MeshFilter>();
		m_MeshCollider = GetComponent<MeshCollider>();
		m_Mesh         = new Mesh();
	}

	// PRIVATE METHODS

	private int GetTerrainHeight(int x, int z)
	{
		var perlinCoordX = x * m_PerlinScale + m_PerlinOffset.x;
		var perlinCoordY = z * m_PerlinScale + m_PerlinOffset.y;
		var perlinSample = Mathf.PerlinNoise(perlinCoordX, perlinCoordY);

		return Mathf.Clamp(Mathf.FloorToInt(perlinSample * m_Height), 0, m_Height - 1);
	}

	private EBlockType GetBlockTypeForHeight(int height, int currMaxHeight)
	{
		if (height > currMaxHeight)
			return EBlockType.None;

		var fraction = height / (float)m_Height;

		if (fraction < 0.2f) // TODO values from settings
			return EBlockType.Stone;
		if (fraction < 0.4f)
			return EBlockType.Dirt;
		if (fraction < 0.6f)
			return EBlockType.Grass;

		return EBlockType.Snow;
	}

	private void AddFaceVertices(Vector3 origin, Vector3[] vertices)
	{
		for (int idx = 0; idx < vertices.Length; ++idx)
		{
			m_Vertices.Add(origin + vertices[idx]);
		}
	}

	private bool IsNone(int x, int y, int z)
	{
		if (CheckBounds(x, y, z) == false)
			return true;

		return m_BlockData[x, y, z].BlockType == EBlockType.None;
	}

	private bool CheckBounds(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
			return false;
		if (x >= m_Width || y >= m_Height || z >= m_Width)
			return false;

		return true;
	}

	private void SetMeshIndexFormat(int vertexCount)
	{
		var targetFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
		if (targetFormat == m_Mesh.indexFormat)
			return;
		
		m_Mesh.indexFormat = targetFormat;
	}
}