using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using BlockData = BlockTerrainChunk.BlockData;

static class Persistence
{
	// CLASSES

	[System.Serializable]
	public sealed class SaveData
	{
		public float   PlayerPosX, PlayerPosY, PlayerPosZ; // Vector3 not marked as serializable
		public int     ChunkWidth;
		public int     ChunkHeight;
		public float   PerlinScale;
		public float   PerlinOffsetX, PerlinOffsetY;       // Vector2 not marked as serializable

		public List<SerializableBlockData> ChangedBlocks;  // Vector2Int not market as serializable
	}

	[System.Serializable]
	public struct SerializableBlockData
	{
		public int           X, Y;
		public BlockData[,,] Data;
	}

	// PUBLIC METHODS

	public static string Save(string fileName, SaveData data)
	{
		var path = Path.Combine(Application.persistentDataPath, fileName);
		var   fs = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.OpenOrCreate);
		var   bf = new BinaryFormatter();

		bf.Serialize(fs, data);
		fs.Close();

		return path;
	}

	public static SaveData Load(string fileName)
	{
		var path = Path.Combine(Application.persistentDataPath, fileName);
		if (File.Exists(path) == false)
			return null;

		var   bf = new BinaryFormatter();
		var   fs = File.Open(path, FileMode.Open);
		var data = (SaveData)bf.Deserialize(fs);

		fs.Close();

		return data;
	}

	// HELPERS

	public static List<SerializableBlockData> CreateBlockSaveData(Dictionary<Vector2Int, BlockTerrainChunk> activeChunks, Dictionary<Vector2Int, BlockData[,,]> changedBlocks)
	{
		var list = new List<SerializableBlockData>();

		foreach (var kvp in activeChunks)
		{
			if (kvp.Value.Changed == false)
				continue;

			list.Add(new SerializableBlockData
			{
				X = kvp.Key.x,
				Y = kvp.Key.y,
				Data = kvp.Value.Blocks
			});
		}

		foreach (var kvp in changedBlocks)
		{
			list.Add(new SerializableBlockData
			{
				X = kvp.Key.x,
				Y = kvp.Key.y,
				Data = kvp.Value
			});
		}

		return list;
	}

	public static Dictionary<Vector2Int, BlockData[,,]> ConvertFromSaveData(List<SerializableBlockData> blockData)
	{
		var dict = new Dictionary<Vector2Int, BlockData[,,]>();

		foreach (var serializableData in blockData)
		{
			dict.Add(new Vector2Int(serializableData.X, serializableData.Y), serializableData.Data);
		}

		return dict;
	}
}
