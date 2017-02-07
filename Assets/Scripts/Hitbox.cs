using UnityEngine;
using System.Collections;

public class Hitbox : MonoBehaviour {

    public enum Height
    {
        High,
        Low
    }

    public Height height;
    public int damage;
    public int hitstun;
    public int blockstun;
    public float hitPushback;
    public float blockPushback;

    public GameObject Opponent;

    private bool collided = false;

    new private Collider collider;
    private Collider[] opponentColliders;

	// Use this for initialization
	void Start () {
        collider = GetComponent<Collider>();
        if (Opponent == null)
        {
            Debug.LogError("Opponent not set! Can't fight what you can't see!");
        }
        else
        {
            opponentColliders = Opponent.GetComponentsInChildren<Collider>();
        }
	}

    void OnEnable()
    {
        collided = false;
    }

    public void TestCollisions()
    {
        if (!collided && opponentColliders != null)
        {
            foreach (Collider opponentCollider in opponentColliders)
            {
                if (collider.bounds.Intersects(opponentCollider.bounds))
                {
                    opponentCollider.GetComponentInParent<StateMachine>().ReceiveHit(height, damage, hitstun, blockstun, hitPushback, blockPushback);
                    collided = true;
                }
            }
        }
    }
}
