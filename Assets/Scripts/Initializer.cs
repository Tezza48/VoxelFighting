using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using zapnet;

public class Initializer : MonoBehaviour
{
    int windowId;
    Rect windowRect;

    public int serverPort = 1337;
    public string serverHost = "127.0.0.1";
    public int serverVersion;

    public bool isServer = false;

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

        SceneManager.sceneLoaded += OnSceneLoaded;

        windowRect = new Rect();
        windowRect.width = 300.0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "VoxelsTest")
        {
            // Load all Network Prefabs in the Resources/Network directory.
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

    // Start is called before the first frame update
    void OnStart()
    {
        if (IsHeadless() || isServer)
        {
            Zapnet.Network.Host(serverPort, new ServerHandler(serverVersion), serverSimulation);
        } else
        {
            Zapnet.Network.Connect(serverHost, serverPort, new ClientHandler(serverVersion), clientSimulation);
        }

        SceneManager.LoadScene("VoxelsTest", LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        windowRect = GUILayout.Window(windowId, windowRect, OnWindow, "Multiplayer");
        windowRect.x = (Screen.width - windowRect.width) / 2.0f;
        windowRect.y = (Screen.height - windowRect.height) / 2.0f;
    }

    private void OnWindow(int windowId)
    {
        GUILayout.Label((isServer) ? "[Server] Client" : "Server [Client]");
        isServer = GUILayout.Toggle(isServer, "Is Server");

        if (GUILayout.Button("Start")) {
            OnStart();
        }
    }
}
