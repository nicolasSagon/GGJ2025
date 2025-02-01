using JetBrains.Annotations;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private float velocity = 1f;
    public float maxVelocity = 15f;
    public float minVelocity = 1f;
    private PlayerController currentOwner = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SwitchOwner(PlayerController newOwner){
        if (currentOwner != null) {
            Physics.IgnoreCollision(currentOwner.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
        currentOwner = newOwner;
        Physics.IgnoreCollision(currentOwner.GetComponent<Collider>(), GetComponent<Collider>(), true);
        gameObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_FresnelColour", newOwner.playerColor);
    }

    public PlayerController GetCurrentOwner(){
        return currentOwner;
    }

    public float GetVelocity(){
        return velocity;
    }

    public void IncreaseVelocity(float delta = 5f){
        float newVelocity = velocity + delta;
        velocity = (newVelocity > maxVelocity) ? maxVelocity : newVelocity;
    }

    public void ReduceVelocity(float delta = 5f){
        float newVelocity = velocity - delta;
        velocity = (newVelocity < minVelocity) ? minVelocity : newVelocity;
    }

    public void HitBall(PlayerController player, Vector2 direction){
        SwitchOwner(player);
        Rigidbody ballRb = GetComponent<Rigidbody>();
        ballRb.linearVelocity = Vector2.zero;
        IncreaseVelocity();
        ballRb.AddForce(direction * velocity, ForceMode.VelocityChange);
    }
}
