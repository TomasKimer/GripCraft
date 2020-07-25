using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
sealed class TerrainBlock : MonoBehaviour
{
	// CONSTANTS

	private const int FACE_COUNT        = 6;
	private const int INDICES_PER_FACE  = 6;
	private const int VERTICES_PER_FACE = 4;
	private const int VERTEX_COUNT      = FACE_COUNT * VERTICES_PER_FACE;
	private const int INDEX_COUNT       = FACE_COUNT * INDICES_PER_FACE;

	// PRIVATE MEMBERS

	private BlockSettings m_Settings;
	private EBlockType    m_BlockType;
	private MeshFilter    m_MeshFilter;

	// PUBLIC METHODS

	public void Initialize(BlockSettings blockSettings)
	{
		m_Settings   = blockSettings;
		m_MeshFilter = GetComponent<MeshFilter>();

		CreateMesh();
	}

	public void SetBlockType(EBlockType blockType)
	{
		if (m_BlockType == blockType)
			return;
		m_BlockType = blockType;

		UpdateUVs();
	}

	// PRIVATE METHODS

	private void CreateMesh()
	{
		var vertices = new List<Vector3>(VERTEX_COUNT);
		var  indices = new List<int>(INDEX_COUNT);

		vertices.AddRange(BlockSettings.LEFT_VERTICES);
		vertices.AddRange(BlockSettings.RIGHT_VERTICES);
		vertices.AddRange(BlockSettings.FRONT_VERTICES);
		vertices.AddRange(BlockSettings.BACK_VERTICES);
		vertices.AddRange(BlockSettings.TOP_VERTICES);
		vertices.AddRange(BlockSettings.BOTTOM_VERTICES);

		for (int i = 0; i < FACE_COUNT; ++i)
		{
			var idx = i * VERTICES_PER_FACE;

			indices.Add(idx);
			indices.Add(idx + 1);
			indices.Add(idx + 2);

			indices.Add(idx);
			indices.Add(idx + 2);
			indices.Add(idx + 3);
		}

		var mesh = new Mesh
		{
			vertices = vertices.ToArray(),
			triangles = indices.ToArray()
		};

		mesh.RecalculateNormals();
		mesh.Optimize();

		m_MeshFilter.mesh = mesh;
	}

	private void UpdateUVs()
	{
		var blockInfo = m_Settings.GetBlockInfo(m_BlockType);
		var       uvs = new List<Vector2>(VERTEX_COUNT);

		uvs.AddRange(blockInfo.SideUVs);
		uvs.AddRange(blockInfo.SideUVs);
		uvs.AddRange(blockInfo.SideUVs);
		uvs.AddRange(blockInfo.SideUVs);
		uvs.AddRange(blockInfo.TopUVs);
		uvs.AddRange(blockInfo.BottomUVs);

		m_MeshFilter.mesh.uv = uvs.ToArray();
	}
}
