using JetBrains.Annotations;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private float velocity = 1f;
    public float defaultVelocityIncrease = 5f;
    public float maxVelocity = 15f;
    public float minVelocity = 1f;
    public Vector3 minScale = new(0.25f, 0.25f, 0.25f);
    public Vector3 maxScale = new(2.5f, 2.5f, 2.5f);
    private PlayerController currentOwner = null;
    private bool merging = false;
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
    public void IncreaseVelocity(){
        IncreaseVelocity(defaultVelocityIncrease);
    }

    public void IncreaseVelocity(float delta){
        float newVelocity = velocity + delta;
        velocity = (newVelocity > maxVelocity) ? maxVelocity : newVelocity;
    }

    public void ReduceVelocity(float delta = 5f){
        float newVelocity = velocity - delta;
        velocity = (newVelocity < minVelocity) ? minVelocity : newVelocity;
    }

    public void IncreaseScale(Vector3 delta){
        Vector3 newScale = GetScale() + delta;
        if (newScale.x > maxScale.x || newScale.y > maxScale.y || newScale.z > maxScale.z) {
            newScale = maxScale;
        }
        SetScale(newScale);
    }

    public void ReduceScale(Vector3 delta){
        Vector3 newScale = GetScale() - delta;
        if (newScale.x < maxScale.x || newScale.y < maxScale.y || newScale.z < maxScale.z) {
            newScale = minScale;
        }
        SetScale(newScale);
    }

    public void HitBall(PlayerController player, Vector2 direction){
        SwitchOwner(player);
        GetRigidBody().linearVelocity = Vector2.zero;
        IncreaseVelocity();
        GetRigidBody().AddForce(direction * velocity, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GetCurrentOwner() == null) return;
        if (collision.gameObject.CompareTag("BallHitbox"))
        {
            BubbleController otherBall = collision.gameObject.GetComponent<BubbleController>();
            if (otherBall.GetCurrentOwner() == GetCurrentOwner() || otherBall.GetCurrentOwner() == null) {
                MergeWith(otherBall);
            }
        }
    }
    void MergeWith(BubbleController otherBall){
        // Only merge if not already merging
        if (otherBall.isMerging() || isMerging()) return;
        SetMerging();
        otherBall.SetMerging();
        IncreaseScale(otherBall.GetScale());
        IncreaseVelocity(otherBall.GetVelocity());
        Destroy(otherBall.transform.parent.gameObject);
        UnsetMerging();
    }
    public void SetMerging(){
        merging = true;
    }
    public void UnsetMerging(){
        merging = false;
    }

    public bool isMerging(){
        return merging;
    }

    public Vector3 GetScale(){
        return GetRigidBody().transform.parent.gameObject.transform.localScale;
    }
    public void SetScale(Vector3 scale){
        GetRigidBody().transform.parent.gameObject.transform.localScale = scale;
    }
    private Rigidbody GetRigidBody() {
        return GetComponent<Rigidbody>();
    }
}
