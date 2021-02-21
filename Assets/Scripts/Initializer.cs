using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using zapnet;

public class Initializer : MonoBehaviour
{

    public int serverPort = 1227;
    public string serverHost = "127.0.0.1";
    public int serverVersion;

    public NetSimulation serverSimulation;
    public NetSimulation clientSimulation;

    public Camera _camera;

#if UNITY_EDITOR
    [Header("Editor Only")]
    public bool isServerMode;
#endif

    bool gui_showServerWindow = true;
    int windowId;
    Rect windowRect;

    private const string SCENE_NAME = "VoxelsTest";

    public bool IsHeadless()
    {
        return (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private void Awake()
    {
        Zapnet.Initialize();

        Zapnet.Network.RegisterPacket<LoginCredentials>();
        Zapnet.Network.RegisterPacket<PlayerInputEvent>();

        SceneManager.sceneLoaded += OnSceneLoaded;

        windowRect = new Rect();
        windowRect.width = 300.0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SCENE_NAME)
        {
            gui_showServerWindow = false;

            // Load all Network Prefabs in the Resources/Network directory.
            var ress = Resources.LoadAll<NetworkPrefab>("Network");
            Zapnet.Prefab.LoadAll("Network");

            if (Zapnet.Network.IsServer)
            {
                // The game scene has finished loading and we're the server.
            }
            else
            {
                // The game scene has finished loading and we're the client.
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
#if UNITY_EDITOR
        if (isServerMode)
        {
            Run(true);
        }
#elif UNITY_SERVER
        Run(true);
#else
        if (IsHeadless())
        {
            Run(true);
        }
#endif
    }

    // Start is called before the first frame update
    void Run(bool isServer)
    {
        if (isServer)
        {
            Zapnet.Network.Host(serverPort, new ServerHandler(serverVersion), serverSimulation);
        } else
        {
            Zapnet.Network.Connect(serverHost, serverPort, new ClientHandler(serverVersion), clientSimulation);
        }

        if (_camera)
        {
            Destroy(_camera.gameObject);
            _camera = null;
        }

        SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if (gui_showServerWindow)
        {
            windowRect = GUILayout.Window(windowId, windowRect, OnWindow, "Multiplayer");
            windowRect.x = (Screen.width - windowRect.width) / 2.0f;
            windowRect.y = (Screen.height - windowRect.height) / 2.0f;
        }
    }

    private void OnWindow(int windowId)
    {
        //GUILayout.BeginHorizontal();
        GUILayout.Label("Port: " + serverPort);
        //var portText = GUILayout.TextField(serverPort.ToString());
        //portText = Regex.Replace(portText, @"[^a-zA-Z0-9 ]", "");
        //serverPort = int.Parse(portText);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        GUILayout.Label("Host: " + serverHost);
        //serverHost = GUILayout.TextField(serverHost);
        //GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Server"))
        {
            Run(true);
        }
        if (GUILayout.Button("Client"))
        {
            Run(false);
        }
        GUILayout.EndHorizontal();

        //GUILayout.Label((isServer) ? "[Server] Client" : "Server [Client]");
        //isServer = GUILayout.Toggle(isServer, "Is Server");

        //if (GUILayout.Button("Start")) {
        //    OnStart();
        //}
    }
}
