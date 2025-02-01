using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private PlayerController currentOwner = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchOwner(PlayerController newOwner){
        if (currentOwner != null) {
            Physics.IgnoreCollision(currentOwner.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
        currentOwner = newOwner;
        Physics.IgnoreCollision(currentOwner.GetComponent<Collider>(), GetComponent<Collider>(), true);
        gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_FresnelColour", newOwner.playerColor);
    }
}
