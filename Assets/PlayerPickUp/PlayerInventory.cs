using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Item handheld;

    void Awake() {
        cam = transform.GetChild(0).GetComponent<Camera>();
    }

    void Update() {
        if (Input.GetKeyDown(Config.cfg.pickUpKey)) {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit info, Config.cfg.interactionRange)) {
                switch (info.transform.tag) {
                    case "Item":
                        TryPickupItem(info.transform.gameObject.GetComponent<Item>());
                        break;
                    case "Placeable Area":
                        TryPlaceItem();
                        break;
                }
            }
        }
    }

    void TryPickupItem(Item item) {
        if (handheld != null) {
            DropItem();
        }

        handheld = item;
        handheld.transform.SetParent(transform.GetChild(0));
        handheld.GetComponent<Rigidbody>().useGravity = false;
        handheld.GetComponent<BoxCollider>().enabled = false;

        handheld.transform.localPosition = new Vector3(0, 0, 0);
    }

    void DropItem() {

    }

    void TryPlaceItem() {

    }
}
