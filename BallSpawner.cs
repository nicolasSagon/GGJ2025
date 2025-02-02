using System.Collections;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public float minFrequency = 5.0f;
    public float maxFrequency = 15.0f;
    public float ballVelocity = 100.0f;
    public GameObject ballPrefab;

    private bool isStarted = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isStarted = true;
        StartCoroutine(SpawnBall());
    }

    public void StopSpawning()
    {
        isStarted = false;
    }

    private IEnumerator SpawnBall()
    {
        while (true)
        {
            if (isStarted) {
                // Instantiate a new ball
                GameObject ball = Instantiate(ballPrefab);
                // Set the ball position
                ball.transform.position = transform.localPosition;
                var randomDirectionVector = new Vector3(ballVelocity * Random.Range(-1.0f, 1.0f), ballVelocity * Random.Range(-1.0f,1.0f), 0.0f);
                // Add a force to the ball with a random direction
                ball.GetComponentInChildren<Rigidbody>().AddForce(randomDirectionVector, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(Random.Range(minFrequency, maxFrequency));
        }
    }

}
