using UnityEngine;
using System.Collections;

public class LiftingBody : MonoBehaviour {

    public enum SteeringMode {
        PaperPlane, Wings
    }

    public SteeringMode steeringMode = SteeringMode.PaperPlane;

    // hand controller variables
    Transform linkeHand, rechteHand, head;
    public Vector3 leftvel, rightvel;
    Queue lQueue, rQueue;
    public int max = 4;
    Vector3 lastleftpos, lastrightpos;
    public float forcethresh = 0.01f;
    public float maxforce = 50f;
    public float forcefactor = 500f;
    public Vector3 bodydirection;

    // wing mode variables
    public float wingTurnStrength = 1000f;
    public float wingDeadzone = 0.01f;

    // glide
    public float towardsVel = 1f;
    Rigidbody rigid;
    Vector3 pointdir = Vector3.zero;
    public float steer = 10f;

    void Start () {
        head = GameObject.Find("Main Camera").transform;
        rigid = GetComponent<Rigidbody>();
        lQueue = new Queue();
        rQueue = new Queue();
        leftvel = Vector3.zero;
        rightvel = Vector3.zero;
        linkeHand = GameObject.Find("Hand - Left").transform;
        rechteHand = GameObject.Find("Hand - Right").transform;
    }
	
	void FixedUpdate () {
        float glide = 1f;
        // factor to put in to make effect vanish when slow
        float lowspeedlimit = glide * Mathf.Min(Mathf.Max(rigid.velocity.magnitude,0f),1f);

        // the desired direction, points to a point somewhere on the XY-plane, z is always zero
        // default is keyboard/joystick steering
        pointdir = Vector3.forward + new Vector3(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"), 0f);
        

        // if hydras are active
        if (linkeHand.localPosition.y != rechteHand.localPosition.y) {
            // calculate velocities of hydras
            CalcVelocity();
            // trigger deactivates glide/wings, 1.0 = full glide, 0.0 = no glide/steering
            glide = 1f - (SixenseInput.GetController(SixenseHands.LEFT).Trigger+ SixenseInput.GetController(SixenseHands.RIGHT).Trigger)/2f;

            // steer with the hands

            switch (steeringMode) {
                case SteeringMode.PaperPlane:
                    {
                        // paperplane steering
                        Vector3 bodydirection = (head.localPosition - Vector3.Lerp(linkeHand.localPosition, rechteHand.localPosition, 0.5f));
                        bodydirection = new Vector3(bodydirection.x * 200f, bodydirection.y * 100f, bodydirection.z * 100f);
                        pointdir += glide * bodydirection;
                        break;
                    }
                case SteeringMode.Wings:
                    {
                        // wing steering
                        bodydirection = (head.localPosition - Vector3.Lerp(linkeHand.localPosition, rechteHand.localPosition, 0.5f));
                        // steer by angle between left-right and straight right
                        float horizSteer = 1f- Vector3.Dot(Vector3.right, Vector3.ProjectOnPlane(rechteHand.localPosition - linkeHand.localPosition, Vector3.forward).normalized);
                        //deadzone
                        horizSteer = horizSteer < wingDeadzone ? 0f : horizSteer;
                        // sign the x axis depending on which hand is higher
                        horizSteer = rechteHand.localPosition.y > linkeHand.localPosition.y ? -horizSteer : horizSteer;
                        print(horizSteer);
                        // construct the steer direction vector                        
                        bodydirection = new Vector3(horizSteer * Mathf.Pow(horizSteer,2f)* wingTurnStrength, bodydirection.y * 100f, bodydirection.z * 100f);
                        pointdir += glide * bodydirection;
                        break;
                    }
            }
        }

        if ((Mathf.Abs(leftvel.y) > forcethresh || Mathf.Abs(rightvel.y) > forcethresh)) {
            // only act a force if y-velocities are under -0.01f
            if (leftvel.y < -forcethresh) {
                // get the excerted force for the hand
                Vector3 leftforce = (Vector3.up * -leftvel.y) * forcefactor;
                // lerp between the force and the forces direction capped at maxforce length
                leftforce = Vector3.Lerp(leftforce, leftforce.normalized * maxforce, Mathf.Min(leftforce.magnitude/maxforce,1f));
                rigid.AddForceAtPosition(leftforce, linkeHand.position);
            }
            if (rightvel.y < -forcethresh) {
                // get the excerted force for the hand
                Vector3 rightforce = (Vector3.up * -rightvel.y) * forcefactor;
                // lerp between the force and the forces direction capped at maxforce length
                rightforce = Vector3.Lerp(rightforce, rightforce.normalized * maxforce, Mathf.Min(rightforce.magnitude / maxforce, 1f));
                rigid.AddForceAtPosition(rightforce, rechteHand.position);
            }
        }

        // transform to player space and increase steer by value
        pointdir = rigid.transform.TransformVector(pointdir);
        pointdir = pointdir.normalized * steer;


        // point the rigidbody towards halfway between the current velocity and the desired direction
        rigid.AddTorque(glide * towardsVel * Vector3.Cross(rigid.transform.forward, lowspeedlimit*100f*rigid.velocity.normalized*.5f+.5f*pointdir.normalized));
        rigid.AddTorque(towardsVel * Vector3.Cross(rigid.transform.up, 50f * Vector3.up));

        // change velocity vector to account for fake aerodynamic banking/yawing etc
        rigid.velocity = rigid.velocity * (1f-glide) + glide * Vector3.Lerp(rigid.velocity.normalized, pointdir.normalized, 0.1f) * rigid.velocity.magnitude;


        //position visual arrow
        GameObject.Find("Arrow").transform.position = Vector3.Lerp(linkeHand.position, rechteHand.position, 0.5f);
        GameObject.Find("Arrow").transform.localRotation = Quaternion.LookRotation(head.position - Vector3.Lerp(linkeHand.position, rechteHand.position, 0.5f), Vector3.up);
    }

    void CalcVelocity() {
        Vector3 currleftpos = linkeHand.localPosition;
        Vector3 currrightpos = rechteHand.localPosition;

        leftvel = currleftpos - lastleftpos;
        rightvel = currrightpos - lastrightpos;

        lQueue.Enqueue(leftvel);
        rQueue.Enqueue(rightvel);
        if (lQueue.Count == max + 1) {
            lQueue.Dequeue();
            Vector3 tmp = Vector3.zero;
            foreach (Vector3 v in lQueue) {
                tmp += v;
            }
            tmp /= lQueue.Count;
            leftvel = tmp;
        }
        if (rQueue.Count == max + 1) {
            rQueue.Dequeue();
            Vector3 tmp = Vector3.zero;
            foreach (Vector3 v in rQueue) {
                tmp += v;
            }
            tmp /= rQueue.Count;
            rightvel = tmp;
        }

        lastleftpos = currleftpos;
        lastrightpos = currrightpos;
    }
}
