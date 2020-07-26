using UnityEngine;

interface ISceneComponent
{
	void Initialize(MainScene scene);
}

sealed class MainScene : MonoBehaviour
{
	// CONFIGURATION

	[SerializeField] string    m_SaveFile     = "quicksave.sav";
	[SerializeField] bool      m_LoadFromSave = false;

	// PUBLIC MEMBERS

	public PlayerController    PlayerController { get; private set; }
	public InputManager        InputManager     { get; private set; }
	public BlockTerrainManager TerrainManager { get; private set; }

	// MONOBEHAVIOUR INTERFACE

	private void Awake()
	{
		InitializeSceneComponents();

		if (m_LoadFromSave == true)
		{
			LoadFromFile();
		}
		else
		{
			SetDefaultPosition();
		}

		InputManager.QuickSave += OnQuickSave;
	}

	// HANDLERS

	private void OnQuickSave()
	{
		SaveToFile();
	}

	// PRIVATE METHODS

	private void InitializeSceneComponents()
	{
		var sceneComponents = GetComponentsInChildren<ISceneComponent>(true);

		PlayerController = FindComponent<PlayerController   >(sceneComponents);
		InputManager     = FindComponent<InputManager       >(sceneComponents);
		TerrainManager   = FindComponent<BlockTerrainManager>(sceneComponents);

		System.Array.ForEach(sceneComponents, component => component.Initialize(this));
	}

	private void SetDefaultPosition()
	{
		PlayerController.transform.position = new Vector3(TerrainManager.ChunkWidth / 2, TerrainManager.ChunkHeight + 10, TerrainManager.ChunkWidth / 2);
	}

	private void SaveToFile()
	{
		var playerPosition = PlayerController.transform.position;

		var saveData = new Persistence.SaveData
		{
			PlayerPosX    = playerPosition.x,
			PlayerPosY    = playerPosition.y,
			PlayerPosZ    = playerPosition.z
		};

		TerrainManager.Save(saveData);

		var path = Persistence.Save(m_SaveFile, saveData);

		Debug.Log("Saved to " + path);
	}

	private bool LoadFromFile()
	{
		var saveData = Persistence.Load(m_SaveFile);
		if (saveData == null)
			return false;

		PlayerController.transform.position = new Vector3(saveData.PlayerPosX, saveData.PlayerPosY, saveData.PlayerPosZ);
		TerrainManager.Load(saveData);

		return true;
	}

	// HELPERS

	private static T FindComponent<T>(ISceneComponent[] components) where T : Component
	{
		return System.Array.Find(components, component => component is T) as T;
	}
}
