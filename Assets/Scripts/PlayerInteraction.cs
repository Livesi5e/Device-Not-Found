using MonumentGames.Config;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera cam;
    
    private void Update()
    {
        if (Input.GetKeyDown(((GameConfig)Config.cfg).interactionKey))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, Config.cfg.interactionRange))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    hit.transform.GetComponent<Interact>().OnInteract();
                }
            }
        }
    }
}
