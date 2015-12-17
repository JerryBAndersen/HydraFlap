using UnityEngine;
using System.Collections;

public class Aerodynamics : MonoBehaviour {

    public float wings;
    
	Rigidbody rigid;
    [Range(0.0f, 100.0f)]
    public float factor = 8.0f;
    [Range(0.0f, 100.0f)]
    public float scale = 0.4f;
    [Range(0.0f, 20.0f)]
    public float turnspeed = 1.0f;

    // drag
    [Range(0.0f, 200.0f)]
    public float drag = 10.0f;
    [Range(0.0f, 1f)]
    public float draglowerb = 0.05f;
    [Range(0.0f, 50f)]
    public float dragupperb = 0.1f;

    // torques
    [Range(0.0f, 1f)]
    public float torqueTowardVelocity = 1f;

    // Use this for initialization
    void Start () {
		rigid = GetComponent<Rigidbody> ();
    }

    // Update is called once per frame
    void FixedUpdate() {
        // change velocity to a Lerp between the normal velocity direction and a projection of the
        // velocity vector on the forward vector
        rigid.velocity = rigid.velocity * (1f - wings) + wings * Vector3.Slerp(rigid.velocity,
            // this is the projection
            Vector3.Project(rigid.velocity, rigid.transform.forward),
            // this value alternates between 0 and 1, depending on the speed of the object
            Mathf.Min(
                1f, Mathf.Max(0f, Mathf.Pow(scale * rigid.velocity.magnitude, factor * 1 + wings))
                )
            );

        if (rigid.transform.right.y < 0f) {
            //take z axis rotation and create a turn around the y axis to mimick a banked turn
            rigid.AddTorque(turnspeed * Vector3.up * -rigid.transform.right.y * Mathf.Min(rigid.velocity.sqrMagnitude, 1f));

        }
        else if ((-rigid.transform.right).y < 0f) {
            //take z axis rotation and create a turn around the y axis to mimick a banked turn
            rigid.AddTorque(turnspeed * Vector3.up * rigid.transform.right.y * Mathf.Min(rigid.velocity.sqrMagnitude, 1f));

        }

        rigid.AddTorque(Vector3.Cross(transform.forward,rigid.velocity)*Mathf.Log(Vector3.Cross(transform.forward, rigid.velocity).magnitude, 5f));

        // clamped between to values, create drag based on deviation from the velocity vector
        rigid.drag = Mathf.Max(draglowerb, Mathf.Min(dragupperb, drag * Mathf.Log((Vector3.Cross(rigid.transform.forward, rigid.velocity)).magnitude)));
        
    }
}
