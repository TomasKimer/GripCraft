using System.Collections.Generic;
using UnityEngine;

sealed class TerrainGenerator : MonoBehaviour
{
	// CONSTANTS

	private const int TERRAIN_WIDTH     = 64;
	private const int TERRAIN_HEIGHT    = 32;
	private const int VERTICES_PER_FACE = 4;

	// CONFIGURATION

	[SerializeField] float    m_PerlinScale  = 1f;
	[SerializeField] Vector2  m_PerlinOffset = Vector2.zero;

	// PRIVATE MEMBERS

	private readonly bool[,,] m_TerrainData  = new bool[TERRAIN_WIDTH, TERRAIN_HEIGHT, TERRAIN_WIDTH];
	private MeshFilter        m_MeshFilter;
	private MeshCollider      m_MeshCollider;
	private Mesh              m_Mesh;
	private List<Vector3>     m_Vertices     = new List<Vector3>();
	private List<int>         m_Triangles    = new List<int>();

	// MONOBEHAVIOUR INTERFACE

	private void Awake()
	{
		m_MeshFilter   = GetComponent<MeshFilter>();
		m_MeshCollider = GetComponent<MeshCollider>();
		m_Mesh         = new Mesh();
	}

	private void OnEnable()
	{
		GenerateHeightmap();
		UpdateMesh();
	}

	private void GenerateHeightmap()
	{
		for (int x = 0; x < TERRAIN_WIDTH; ++x)
		{
			for (int z = 0; z < TERRAIN_WIDTH; ++z)
			{
				var perlinCoordX = (float)x / TERRAIN_WIDTH * m_PerlinScale + m_PerlinOffset.x;
				var perlinCoordY = (float)z / TERRAIN_WIDTH * m_PerlinScale + m_PerlinOffset.y;
				var perlinSample = Mathf.PerlinNoise(perlinCoordX, perlinCoordY);

				var height = Mathf.FloorToInt(perlinSample * TERRAIN_HEIGHT);

				for (int y = 0; y < TERRAIN_HEIGHT; ++y)
				{
					m_TerrainData[x, y, z] = y <= height;
				}
			}
		}
	}

	private void UpdateMesh()
	{
		m_Vertices.Clear();
		m_Triangles.Clear();

		var pos = new Vector3();

		for (int x = 0; x < TERRAIN_WIDTH; ++x)
		{
			for (int z = 0; z < TERRAIN_WIDTH; ++z)
			{
				for (int y = 0; y < TERRAIN_HEIGHT; ++y)
				{
					if (m_TerrainData[x, y, z] == false)
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

		m_Mesh.Clear();
		m_Mesh.vertices = m_Vertices.ToArray();
		m_Mesh.triangles = m_Triangles.ToArray();
		m_Mesh.RecalculateNormals();

		m_MeshFilter.mesh         = m_Mesh;
		m_MeshCollider.sharedMesh = m_Mesh;
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

		if (x >= TERRAIN_WIDTH || y >= TERRAIN_HEIGHT || z >= TERRAIN_WIDTH)
			return false;

		return m_TerrainData[x, y, z];
	}

	// HELPERS

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