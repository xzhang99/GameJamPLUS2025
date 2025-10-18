using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBrick : MonoBehaviour
{
    [Header("Prefab with separated brick pieces")]
    public GameObject brickPiecesPrefab;

    [Header("Force settings")]
    public float explosionForce = 5f;
    public float explosionRadius = 1f;
    public float upwardModifier = 1f;

    [Header("Timing")]
    public float destroyDelay = 1.2f;  // how long before debris disappears

    private bool isBroken = false;

    // -----------------------------
    // Main logic: triggered when hit from below or otherwise
    // -----------------------------
    public void Break()
    {
        if (isBroken) return;
        isBroken = true;

        // Spawn the debris prefab
        GameObject pieces = Instantiate(brickPiecesPrefab, transform.position, Quaternion.identity);

        // Apply random forces to all fragments
        foreach (Rigidbody rb in pieces.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 randomDir = Random.insideUnitSphere + Vector3.up * upwardModifier;
            rb.AddForce(randomDir.normalized * explosionForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * explosionForce, ForceMode.Impulse);
        }

        // Destroy debris after delay
        Destroy(pieces, destroyDelay);

        // Destroy the original brick
        Destroy(gameObject);
    }

    // -----------------------------
    // Collision-based trigger example
    // -----------------------------
    private void OnCollisionEnter(Collision collision)
    {
        // Detect player hitting from below
        if (collision.relativeVelocity.y > 1f && collision.contacts[0].normal.y < -0.5f)
        {
            Break();
        }
    }

    // -----------------------------
    // TEST / DEBUG CODE (can comment out or remove in final game)
    // -----------------------------
#if UNITY_EDITOR
    // Trigger break from Inspector button
    [ContextMenu("Test Break")]
    void TestBreak()
    {
        Break();
    }
#endif

    // Trigger break by key press (Play Mode only)
    void Update()
    {
        // ======= TEST ONLY =======
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Break();
        }
        // ======= END TEST =======
    }
}
