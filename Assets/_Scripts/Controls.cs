// GENERATED AUTOMATICALLY FROM 'Assets/New Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""New Controls"",
    ""maps"": [
        {
            ""name"": ""Video Player"",
            ""id"": ""324e699c-19f3-4fc2-b5a0-7645559173a1"",
            ""actions"": [
                {
                    ""name"": ""Play"",
                    ""type"": ""Button"",
                    ""id"": ""a1d3a550-3168-46e3-b992-51f454506c7a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Seek Left"",
                    ""type"": ""Button"",
                    ""id"": ""03ced835-e5e0-4137-9485-792021011bdd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Seek Right"",
                    ""type"": ""Button"",
                    ""id"": ""0a6a3f61-59e6-486c-b552-1921b046133f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Mouse Interact"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d8a16e92-6300-451e-8499-1fce70048f10"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Volume Up"",
                    ""type"": ""PassThrough"",
                    ""id"": ""c5d96f4c-bc3a-4d27-ba41-f8bc712092c6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Volume Down"",
                    ""type"": ""PassThrough"",
                    ""id"": ""b679467e-1cac-4726-826b-6f2fd8c75756"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3cf8e8f4-2cad-4c94-b406-9453d2ca385a"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Play"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4c5e062b-cf16-4480-81c1-5472431f9367"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Play"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""98f74ae9-9314-42b7-8119-c8577be44dda"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Seek Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c022f8c3-836d-4cce-8308-965f85bca70a"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Seek Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""84611fc6-46a9-4060-8c26-a667dfb709f0"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Seek Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ef88d02f-e722-45d8-90e3-12ee2b5828c2"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Seek Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e2ebc08c-ab73-485d-8994-d49ca709d491"",
                    ""path"": ""<Mouse>/position/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ce4f43f5-991b-4d12-8d7c-0ad93215e046"",
                    ""path"": ""<Mouse>/position/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b6f5a068-37d9-43e7-a69b-7d49e443f64a"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Mouse Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bfdebf43-22a4-4527-bc77-26c26ff86e1e"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Volume Up"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c78a833f-b971-4f90-89b7-5f448cc57508"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Volume Down"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Video Player
        m_VideoPlayer = asset.FindActionMap("Video Player", throwIfNotFound: true);
        m_VideoPlayer_Play = m_VideoPlayer.FindAction("Play", throwIfNotFound: true);
        m_VideoPlayer_SeekLeft = m_VideoPlayer.FindAction("Seek Left", throwIfNotFound: true);
        m_VideoPlayer_SeekRight = m_VideoPlayer.FindAction("Seek Right", throwIfNotFound: true);
        m_VideoPlayer_MouseInteract = m_VideoPlayer.FindAction("Mouse Interact", throwIfNotFound: true);
        m_VideoPlayer_VolumeUp = m_VideoPlayer.FindAction("Volume Up", throwIfNotFound: true);
        m_VideoPlayer_VolumeDown = m_VideoPlayer.FindAction("Volume Down", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Video Player
    private readonly InputActionMap m_VideoPlayer;
    private IVideoPlayerActions m_VideoPlayerActionsCallbackInterface;
    private readonly InputAction m_VideoPlayer_Play;
    private readonly InputAction m_VideoPlayer_SeekLeft;
    private readonly InputAction m_VideoPlayer_SeekRight;
    private readonly InputAction m_VideoPlayer_MouseInteract;
    private readonly InputAction m_VideoPlayer_VolumeUp;
    private readonly InputAction m_VideoPlayer_VolumeDown;
    public struct VideoPlayerActions
    {
        private @Controls m_Wrapper;
        public VideoPlayerActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Play => m_Wrapper.m_VideoPlayer_Play;
        public InputAction @SeekLeft => m_Wrapper.m_VideoPlayer_SeekLeft;
        public InputAction @SeekRight => m_Wrapper.m_VideoPlayer_SeekRight;
        public InputAction @MouseInteract => m_Wrapper.m_VideoPlayer_MouseInteract;
        public InputAction @VolumeUp => m_Wrapper.m_VideoPlayer_VolumeUp;
        public InputAction @VolumeDown => m_Wrapper.m_VideoPlayer_VolumeDown;
        public InputActionMap Get() { return m_Wrapper.m_VideoPlayer; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(VideoPlayerActions set) { return set.Get(); }
        public void SetCallbacks(IVideoPlayerActions instance)
        {
            if (m_Wrapper.m_VideoPlayerActionsCallbackInterface != null)
            {
                @Play.started -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnPlay;
                @Play.performed -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnPlay;
                @Play.canceled -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnPlay;
                @SeekLeft.started -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnSeekLeft;
                @SeekLeft.performed -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnSeekLeft;
                @SeekLeft.canceled -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnSeekLeft;
                @SeekRight.started -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnSeekRight;
                @SeekRight.performed -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnSeekRight;
                @SeekRight.canceled -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnSeekRight;
                @MouseInteract.started -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnMouseInteract;
                @MouseInteract.performed -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnMouseInteract;
                @MouseInteract.canceled -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnMouseInteract;
                @VolumeUp.started -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnVolumeUp;
                @VolumeUp.performed -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnVolumeUp;
                @VolumeUp.canceled -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnVolumeUp;
                @VolumeDown.started -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnVolumeDown;
                @VolumeDown.performed -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnVolumeDown;
                @VolumeDown.canceled -= m_Wrapper.m_VideoPlayerActionsCallbackInterface.OnVolumeDown;
            }
            m_Wrapper.m_VideoPlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Play.started += instance.OnPlay;
                @Play.performed += instance.OnPlay;
                @Play.canceled += instance.OnPlay;
                @SeekLeft.started += instance.OnSeekLeft;
                @SeekLeft.performed += instance.OnSeekLeft;
                @SeekLeft.canceled += instance.OnSeekLeft;
                @SeekRight.started += instance.OnSeekRight;
                @SeekRight.performed += instance.OnSeekRight;
                @SeekRight.canceled += instance.OnSeekRight;
                @MouseInteract.started += instance.OnMouseInteract;
                @MouseInteract.performed += instance.OnMouseInteract;
                @MouseInteract.canceled += instance.OnMouseInteract;
                @VolumeUp.started += instance.OnVolumeUp;
                @VolumeUp.performed += instance.OnVolumeUp;
                @VolumeUp.canceled += instance.OnVolumeUp;
                @VolumeDown.started += instance.OnVolumeDown;
                @VolumeDown.performed += instance.OnVolumeDown;
                @VolumeDown.canceled += instance.OnVolumeDown;
            }
        }
    }
    public VideoPlayerActions @VideoPlayer => new VideoPlayerActions(this);
    public interface IVideoPlayerActions
    {
        void OnPlay(InputAction.CallbackContext context);
        void OnSeekLeft(InputAction.CallbackContext context);
        void OnSeekRight(InputAction.CallbackContext context);
        void OnMouseInteract(InputAction.CallbackContext context);
        void OnVolumeUp(InputAction.CallbackContext context);
        void OnVolumeDown(InputAction.CallbackContext context);
    }
}
