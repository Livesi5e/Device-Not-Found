using PlayerMovement3D;
using UnityEngine;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEditor;

[EditorToolbarElement(id, typeof(SceneView))]
class CreatePlayerButton : EditorToolbarButton
{
    public const string id = "PlayerMovement3D/CreatePlayer";
    public static GameObject cam;
    public CreatePlayerButton ()
    {
        text = "Create Player";
        icon = (Texture2D)EditorGUIUtility.IconContent("AvatarSelector").image;
        tooltip = "Instantiates the player with all components and default settings in the scene";
        clicked += OnClick;
    }

    void OnClick()
    {
        Transform newObj = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
        Object.DestroyImmediate(newObj.GetComponent<MeshRenderer>());
        newObj.name = "Player";
        newObj.gameObject.AddComponent<PlayerMovement>();
        newObj.gameObject.AddComponent<Rigidbody>().freezeRotation = true;
        Undo.RegisterCreatedObjectUndo(newObj.gameObject, "Created Player");
        Transform newCam = new GameObject("Camera").transform;
        newCam.gameObject.AddComponent<Camera>();
        newCam.SetParent(newObj);
        newCam.position = new (0f, 0.5f, 0f);
        newObj.gameObject.GetComponent<PlayerMovement>().cam = newCam.gameObject.GetComponent<Camera>();
        Undo.RegisterCreatedObjectUndo(newCam.gameObject, "Created Camera");
    }
}

[Overlay(typeof(SceneView), "Player Movement")]
[Icon("Assets/unity.png")]
public class PlayerMovementToolbar : ToolbarOverlay
{
    PlayerMovementToolbar() : base(
        CreatePlayerButton.id
    ) {}
}