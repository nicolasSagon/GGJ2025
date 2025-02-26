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
            playerScript.addBallInRange(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))       
        {
            playerScript.removeBallInRange(other.gameObject);
        }
    }
}
