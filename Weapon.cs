using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    private PlayerController playerScript;

    void Start() {
        playerScript = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Trigger Bal");
            playerScript.addBallInRange(other.gameObject);
        }
    }
}
