using UnityEngine;
using UnityEngine.SceneManagement;
using zapnet;

public class ZN_Demo_Initializer : MonoBehaviour
{
    [Header("Connection Settings")]
    public int serverPort = 1227;
    public string serverHost = "127.0.0.1";
    public int serverVersion;

    [Header("Debug Settings")]
    public bool isServerMode = false;
    public NetSimulation serverSimulation;
    public NetSimulation clientSimulation;

    public bool IsHeadless()
    {
        return (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private void Awake()
    {
        Zapnet.Initialize();

        Zapnet.Network.RegisterPacket<LoginCredentials>();
        Zapnet.Network.RegisterPacket<PlayerJumpEvent>();
        Zapnet.Network.RegisterPacket<ZN_Demo_PlayerInputEvent>();
        Zapnet.Network.RegisterPacket<WeaponFireEvent>();
        Zapnet.Network.RegisterPacket<CreateProjectileEvent>();

        SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            Zapnet.Prefab.LoadAll("Network");

            if (Zapnet.Network.IsServer)
            {
                OnServerGameLoaded();
            }
            else
            {
                OnClientGameLoaded();
            }
        }
    }

    private void OnClientGameLoaded()
    {
        
    }

    private void OnServerGameLoaded()
    {
       
    }

    private void Start()
    {
        if (isServerMode || IsHeadless())
        {
            Zapnet.Network.Host(serverPort, new ZN_Demo_ServerHandler(serverVersion), serverSimulation);
        }
        else
        {
            Zapnet.Network.Connect(serverHost, serverPort, new ZN_Demo_ClientHandler(serverVersion), clientSimulation);
        }

        SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
    }

    private void OnDestroy()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
       
    }
}
