using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zapnet;

[RequireComponent(typeof(PlayerState))]
public class BasePlayer : BaseControllable<PlayerInputEvent>
{
    public Camera cameraPrefab;

    private bool _captureCursor;

    public SyncString Name = new SyncString("");

    public float sensitivity = 1.5f;

    private Dictionary<KeyCode, ZN_Demo_InputFlag> _keyInputMap;

    private float _pitch;
    private float _yaw;

    private Camera _camera;
    public CharacterController _characterController;

    public bool IsLocalPlayer
    {
        get
        {
            return (Controller.Value == Zapnet.Player.LocalPlayer);
        }
    }

    public override void Tick()
    {
        if (Zapnet.Network.IsClient)
        {
            if (IsLocalPlayer)
            {
                UpdateCamera();
            }
        }

        base.Tick();
    }

    public override void ReadState(bool isSpawning)
    {
        var state = GetState<PlayerState>();

        base.ReadState(isSpawning);
    }

    public override void WriteSpawn(Player player, NetOutgoingMessage message)
    {
        base.WriteSpawn(player, message);
    }

    public override void ReadSpawn(NetIncomingMessage message)
    {
        base.ReadSpawn(message);
    }

    /// <summary>
    /// When the entity is created we can subscribe to the entity events we need.
    /// </summary>
    public override void OnCreated()
    {
        //Subscribe<CreateProjectileEvent>(OnCreateProjectileEvent);
        //Subscribe<WeaponFireEvent>(OnWeaponFireEvent);

        base.OnCreated();
    }

    /// <summary>
    /// When the entity is removed we can unsubscribe from entity events.
    /// </summary>
    public override void OnRemoved()
    {
        //Unsubscribe<CreateProjectileEvent>(OnCreateProjectileEvent);
        //Unsubscribe<WeaponFireEvent>(OnWeaponFireEvent);

        base.OnRemoved();
    }


    protected override void OnTeleported()
    {
        if (IsLocalPlayer)
        {
            UpdateCamera();
        }

        base.OnTeleported();
    }

    protected override void ResetController()
    {
        var state = GetState<PlayerState>();

        // grab any state and set it on the apropriate places. (Demo sets motor's velocity and stuff)
        _characterController.Move(state.velocity * Zapnet.Network.FixedDeltaTime);
    }

    protected override float GetTeleportDistance()
    {
        var state = GetState<PlayerState>();
        Debug.Log(state.velocity.magnitude + 1f);
        return state.velocity.magnitude + 1f;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        if (HasControl())
        {
            UpdateCamera();
        }

        base.Start();
    }

    protected override void Awake()
    {
        _keyInputMap = new Dictionary<KeyCode, ZN_Demo_InputFlag>()
        {
            [KeyCode.W] = ZN_Demo_InputFlag.Forward,
            [KeyCode.S] = ZN_Demo_InputFlag.Backward,
            [KeyCode.A] = ZN_Demo_InputFlag.Left,
            [KeyCode.D] = ZN_Demo_InputFlag.Right,
            [KeyCode.LeftShift] = ZN_Demo_InputFlag.Sprint,
            [KeyCode.Mouse0] = ZN_Demo_InputFlag.Fire
        };

        Name.onValueChanged += OnNameChanged;
        
        _camera = Camera.main;

        base.Awake();
    }

    protected override void OnPlayerControlLost(Player player)
    {
        base.OnPlayerControlLost(player);

        if (Zapnet.Network.IsServer)
        {

        }

        if (player.IsLocalPlayer)
        {

        }

        player.SetEntity(null);
    }

    protected override void OnPlayerControlGained(Player player)
    {
        base.OnPlayerControlGained(player);

        if (Zapnet.Network.IsServer)
        {

        }
        else
        {
            if (player.IsLocalPlayer)
            {

            }
            else
            {

            }
        }

        player.SetEntity(this);
    }

    protected override void ApplyInput(PlayerInputEvent input, bool isFirstTime = false)
    {
        var inputFlags = input.InputFlags;
        var isWalking = inputFlags.Has(ZN_Demo_InputFlag.Forward | ZN_Demo_InputFlag.Backward | ZN_Demo_InputFlag.Left | ZN_Demo_InputFlag.Right);
        var isSprinting = (isWalking && inputFlags.Has(ZN_Demo_InputFlag.Sprint));
        var moveDirection = Vector3.zero;

        transform.localRotation = Quaternion.Euler(input.Pitch, input.Yaw, 0.0f);

        if (inputFlags.Has(ZN_Demo_InputFlag.Forward) ^ inputFlags.Has(ZN_Demo_InputFlag.Backward))
        {
            moveDirection.z = inputFlags.Has(ZN_Demo_InputFlag.Forward) ? 1 : -1;
        }
        
        if (inputFlags.Has(ZN_Demo_InputFlag.Left) ^ inputFlags.Has(ZN_Demo_InputFlag.Right))
        {
            moveDirection.x = inputFlags.Has(ZN_Demo_InputFlag.Right) ? 1 : -1;
        }

        if (moveDirection.x != 0 || moveDirection.z != 0)
        {
            moveDirection = Vector3.Normalize(Quaternion.Euler(input.Pitch, input.Yaw, 0) * moveDirection);
        }

        var moveSpeed = 3.0f;

        if (isSprinting)
        {
            moveSpeed = 6f;
        }

        var vel = moveDirection * moveSpeed;

        // Demo uses motor to apply movement
        _characterController.Move(vel * Zapnet.Network.FixedDeltaTime);
        // TODO WT: rigidbody movement for proper collisions
        //transform.position += moveDirection * moveSpeed * Time.deltaTime;


        var state = GetState<PlayerState>();

        state.velocity = vel;
        state.inputFlags = inputFlags;

        base.ApplyInput(input, isFirstTime);
    }

    protected override void SendInput(PlayerInputEvent input)
    {
        input.InputFlags.Clear();

        foreach (var kv in _keyInputMap)
        {
            if (Input.GetKey(kv.Key))
            {
                input.InputFlags.Add(kv.Value);
            }
        }

        input.Pitch = _pitch;
        input.Yaw = _yaw;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (Zapnet.Network.IsClient)
        {
            if (IsLocalPlayer)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _captureCursor = true;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }

                if (Input.GetKeyDown(KeyCode.F1))
                {
                    _captureCursor = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }

                if (_captureCursor)
                {
                    UpdateAim();
                }


                // handle keypresses and stuff. Demo calls FireWeapon which sends WeaponFireEvent.
                // TODO WT: Handle chunk interaction.
                // TODO WT: ChunkInteractionEvent
            }
        }

        base.Update();
    }

    private void UpdateAim()
    {
        var mouseY = -Input.GetAxis("Mouse Y");

        if (mouseY > 0.0f || mouseY < 0.0f)
        {
            var pitchMovement = mouseY;
            pitchMovement *= sensitivity;

            _pitch += pitchMovement;
            _pitch = Mathf.Clamp(_pitch, -90, 90);
        }

        var mouseX = Input.GetAxis("Mouse X");

        if (mouseX > 0 || mouseX < 0)
        {
            var yawMovement = mouseX;
            yawMovement *= sensitivity;

            _yaw += yawMovement;
            _yaw %= 360f;
        }
    }

    private void OnNameChanged()
    {
        if (!string.IsNullOrEmpty(Name.LastValue))
        {
            Debug.Log(Name.LastValue + " changed their name to " + Name.Value);
        }
    }

    private void UpdateCamera()
    {
        _camera.transform.position = transform.position;
        _camera.transform.rotation = transform.rotation;
    }

    private void OnDrawGizmos()
    {
        //if (Application.isPlaying && Zapnet.Network.IsClient)
        //{
        //    if (!IsLocalPlayer)
        //    {
        //        Gizmos.DrawFrustum(transform.position, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);
        //    }
        //}
    }
}
