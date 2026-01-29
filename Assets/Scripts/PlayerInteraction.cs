using MonumentGames.Config;
using MonumentGames.PlayerInventory;
using MonumentGames.PlayerMovement3D;
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private GameObject tabletUI;
    [SerializeField] private TMP_Text tabletInfoText;
    
    private void Update()
    {
        if (Input.GetKeyDown(((GameConfig)Config.cfg).interactionKey))
        {
            Debug.Log("Ray Fired");   
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, Config.cfg.interactionRange))
            {
                Debug.Log("Hit");
                if (hit.collider.CompareTag("Interactable"))
                {
                    Debug.Log("Interactable Hit");
                    hit.transform.GetComponent<Interact>().OnInteract();
                }
            }
        }

        if (Input.GetKeyDown(((GameConfig)Config.cfg).openTabletKey))
        {
            if (inventory.HasItem())
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                movement.enabled = false;
                tabletInfoText.text = inventory.GetItem().GetComponent<Tablet>().GetTabletInfo();
                tabletUI.SetActive(true);
            }
        }
    }
}
