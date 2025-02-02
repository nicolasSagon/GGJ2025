using JetBrains.Annotations;
using UnityEditor.Callbacks;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    public float baseVelocity = 1f;
    public float defaultVelocityIncrease = 5f;
    public float maxVelocity = 15f;
    public float minVelocity = 1f;
    public float baseDamage=10f;
    public Vector3 minScale = new(0.25f, 0.25f, 0.25f);
    public Vector3 maxScale = new(2.5f, 2.5f, 2.5f);
    private PlayerController currentOwner = null;
    private bool merging = false;
    private Collider fusedCollider;
    private Rigidbody rb;

    public AudioClip mergeSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fusedCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SwitchOwner(PlayerController newOwner){
        gameObject.layer = 0;
        if (currentOwner != null) {
            Physics.IgnoreCollision(currentOwner.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
        currentOwner = newOwner;
        Physics.IgnoreCollision(currentOwner.GetComponent<Collider>(), GetComponent<Collider>(), true);
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_FresnelColour", newOwner.playerColor);
    }

    public PlayerController GetCurrentOwner(){
        return currentOwner;
    }

    public float GetVelocity(){
        return baseVelocity;
    }
    public void IncreaseVelocity(){
        IncreaseVelocity(defaultVelocityIncrease);
    }

    public void IncreaseVelocity(float delta){
        float newVelocity = baseVelocity + delta;
        baseVelocity = (newVelocity > maxVelocity) ? maxVelocity : newVelocity;
    }

    public void ReduceVelocity(float delta = 5f){
        float newVelocity = baseVelocity - delta;
        baseVelocity = (newVelocity < minVelocity) ? minVelocity : newVelocity;
    }

    public void IncreaseScale(Vector3 delta){
        Vector3 newScale = GetScale() + delta;
        if (newScale.x > maxScale.x || newScale.y > maxScale.y || newScale.z > maxScale.z) {
            newScale = maxScale;
        }
        SetScale(newScale);
        //RepositionIfNecessary();
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
        GetRigidBody().AddForce(direction * baseVelocity, ForceMode.VelocityChange);
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
        if (IsMaxScale() || otherBall.IsMaxScale()) return;
        SetMerging();
        otherBall.SetMerging();
        IncreaseScale(otherBall.GetScale());
        IncreaseVelocity(otherBall.GetVelocity());
        Destroy(otherBall.gameObject);
        UnsetMerging();
    }

/// <summary>
    /// Appeler cette méthode juste après la fusion pour repositionner la boule si besoin.
    /// </summary>
    public void RepositionIfNecessary()
    {
        // Récupère tous les colliders dans le voisinage de la boule
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, fusedCollider.bounds.extents.magnitude);
        
        foreach (Collider col in nearbyColliders)
        {
            // On ne traite que les collisions avec les murs
            if (col.CompareTag("Wall"))
            {
                Vector3 direction;
                float distance;
                
                // Vérifie s'il y a une pénétration entre la boule fusionnée et le mur
                if (Physics.ComputePenetration(
                        fusedCollider, transform.position, transform.rotation,
                        col, col.transform.position, col.transform.rotation,
                        out direction, out distance))
                {
                    // Décale la boule pour sortir du mur
                    transform.position += direction * distance;
                }
            }
        }
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
        return transform.localScale;
    }

    public bool IsMaxScale(){
        return GetScale() == maxScale;
    }
    public void SetScale(Vector3 scale){
        transform.localScale = scale;
    }
    private Rigidbody GetRigidBody() {
        return GetComponent<Rigidbody>();
    }

    public float GetDamage() {
        return transform.localScale.x*baseDamage;
    }
}
