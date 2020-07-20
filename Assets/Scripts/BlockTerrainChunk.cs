using System.Collections.Generic;
using UnityEngine;
using IndexFormat = UnityEngine.Rendering.IndexFormat;

sealed class BlockTerrainChunk : MonoBehaviour
{
	// PRIVATE MEMBERS

	private bool[,,]          m_Heightmap;
	private int               m_Width;
	private int               m_Height;
	private float             m_PerlinScale; 
	private Vector2           m_PerlinOffset;
	private MeshFilter        m_MeshFilter;
	private MeshCollider      m_MeshCollider;
	private Mesh              m_Mesh;
	private List<Vector3>     m_Vertices     = new List<Vector3>();
	private List<int>         m_Triangles    = new List<int>();

	// PUBLIC METHODS

	public void Initialize(int width, int height, float perlinScale, Vector2 perlinOffset)
	{
		m_Width        = width;
		m_Height       = height;
		m_PerlinScale  = perlinScale;
		m_PerlinOffset = perlinOffset;

		m_Heightmap    = new bool[m_Width, m_Height, m_Width];
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
					m_Heightmap[x, y, z] = y <= height;
				}
			}
		}
	}

	public void UpdateMesh()
	{
		var pos = new Vector3();

		for (int x = 0; x < m_Width; ++x)
		{
			for (int z = 0; z < m_Width; ++z)
			{
				for (int y = 0; y < m_Height; ++y)
				{
					if (m_Heightmap[x, y, z] == false)
						continue;

					pos.x = x;
					pos.y = y;
					pos.z = z;

					var faceCount = 0;

					if (IsTerrain(x - 1, y, z) == false)
					{
						AddFaceVertices(pos, LEFT_VERTICES);
						faceCount += 1;
					}

					if (IsTerrain(x + 1, y, z) == false)
					{
						AddFaceVertices(pos, RIGHT_VERTICES);
						faceCount += 1;
					}

					if (IsTerrain(x, y, z - 1) == false)
					{
						AddFaceVertices(pos, FRONT_VERTICES);
						faceCount += 1;
					}

					if (IsTerrain(x, y, z + 1) == false)
					{
						AddFaceVertices(pos, BACK_VERTICES);
						faceCount += 1;
					}

					if (IsTerrain(x, y + 1, z) == false)
					{
						AddFaceVertices(pos, TOP_VERTICES);
						faceCount += 1;
					}

					if (IsTerrain(x, y - 1, z) == false)
					{
						AddFaceVertices(pos, BOTTOM_VERTICES);
						faceCount += 1;
					}

					var startIdx = m_Vertices.Count - faceCount * VERTICES_PER_FACE;
					for (int i = 0; i < faceCount; ++i)
					{
						var idx = startIdx + i * VERTICES_PER_FACE;

						m_Triangles.Add(idx);
						m_Triangles.Add(idx + 1);
						m_Triangles.Add(idx + 2);

						m_Triangles.Add(idx);
						m_Triangles.Add(idx + 2);
						m_Triangles.Add(idx + 3);
					}
				}
			}
		}

		SetMeshIndexFormat(m_Vertices.Count);

		m_Mesh.Clear();
		m_Mesh.vertices = m_Vertices.ToArray();
		m_Mesh.triangles = m_Triangles.ToArray();
		m_Mesh.Optimize();
		m_Mesh.RecalculateNormals();

		m_MeshFilter.mesh         = m_Mesh;
		m_MeshCollider.sharedMesh = m_Mesh;
		
		m_Vertices.Clear();
		m_Triangles.Clear();
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

	private void AddFaceVertices(Vector3 origin, Vector3[] vertices)
	{
		for (int idx = 0; idx < VERTICES_PER_FACE; ++idx)
		{
			m_Vertices.Add(origin + vertices[idx]);
		}
	}

	private bool IsTerrain(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
			return false;

		if (x >= m_Width || y >= m_Height || z >= m_Width)
			return false;

		return m_Heightmap[x, y, z];
	}

	private void SetMeshIndexFormat(int vertexCount)
	{
		var targetFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
		if (targetFormat == m_Mesh.indexFormat)
			return;
		
		m_Mesh.indexFormat = targetFormat;
	}

	// HELPERS

	private const int VERTICES_PER_FACE = 4;

	private static readonly Vector3[] LEFT_VERTICES = {
		new Vector3(0, 0, 1),
		new Vector3(0, 1, 1),
		new Vector3(0, 1, 0),
		new Vector3(0, 0, 0)
	};

	private static readonly Vector3[] RIGHT_VERTICES = {
		new Vector3(1, 0, 0),
		new Vector3(1, 1, 0),
		new Vector3(1, 1, 1),
		new Vector3(1, 0, 1)
	};

	private static readonly Vector3[] FRONT_VERTICES = {
		new Vector3(0, 0, 0),
		new Vector3(0, 1, 0),
		new Vector3(1, 1, 0),
		new Vector3(1, 0, 0)
	};

	private static readonly Vector3[] BACK_VERTICES = {
		new Vector3(1, 0, 1),
		new Vector3(1, 1, 1),
		new Vector3(0, 1, 1),
		new Vector3(0, 0, 1)
	};

	private static readonly Vector3[] TOP_VERTICES = {
		new Vector3(0, 1, 0),
		new Vector3(0, 1, 1),
		new Vector3(1, 1, 1),
		new Vector3(1, 1, 0)
	};

	private static readonly Vector3[] BOTTOM_VERTICES = {
		new Vector3(0, 0, 0),
		new Vector3(1, 0, 0),
		new Vector3(1, 0, 1),
		new Vector3(0, 0, 1)
	};
}