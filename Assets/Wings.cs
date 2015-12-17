using UnityEngine;
using System.Collections;

public class Wings : MonoBehaviour {
    Transform linkeHand, rechteHand, head;
    public float gravity = 0f;
    public float dampforce;
    public float forcethresh = 0.01f;
    Queue lQueue, rQueue;
    public int max = 4;
    Vector3 lastleftpos, lastrightpos;
    public Vector3 leftvel, rightvel;
    Rigidbody rigid;
    public float forcefactor = 1f;

    Quaternion bodrotation;

    Quaternion previousrot;

    // Use this for initialization
    void Start() {
        head = GameObject.Find("Main Camera").transform;

        rigid = GetComponent<Rigidbody>();

        lQueue = new Queue();
        rQueue = new Queue();

        leftvel = Vector3.zero;
        rightvel = Vector3.zero;
        linkeHand = GameObject.Find("Hand - Left").transform;
        rechteHand = GameObject.Find("Hand - Right").transform;
    }

    void FixedUpdate() {
        float glide = SixenseInput.GetController(SixenseHands.LEFT).Trigger;
        //rigid.velocity = rigid.velocity * (1f - glide) + glide * Vector3.Lerp(rigid.velocity,Vector3.Project(rigid.velocity,rigid.transform.forward),Mathf.Min(1f,Mathf.Pow(rigid.velocity.magnitude,8f)));

        dampforce = rigid.angularDrag/2f;
               
        rigid.AddForce(Vector3.up * 9.81f * gravity);

        // calculate velocities of hydras
        CalcVelocity();

        if ((Mathf.Abs(leftvel.y) > forcethresh || Mathf.Abs(rightvel.y) > forcethresh)) {
            //// only act a force if y-velocities are under -0.01f
            //if (leftvel.y < -forcethresh) {
            //    rigid.AddForceAtPosition((Vector3.up * -leftvel.y  )* forcefactor, linkeHand.position);
            //}
            //if (rightvel.y < -forcethresh) {
            //    rigid.AddForceAtPosition((Vector3.up * -rightvel.y ) * forcefactor, rechteHand.position);
            //}
        }
        if (
            //if triggers are held, rotate like the triangle formed by hydras and head
            (SixenseInput.GetController(SixenseHands.LEFT).Trigger > 0.5f && SixenseInput.GetController(SixenseHands.RIGHT).Trigger > 0.5f)
            ) {
            //previousrot = rigid.rotation;
            //bodrotation = (Quaternion.Inverse(rigid.rotation) * previousrot * 
            //    Quaternion.LookRotation(
            //    ((rigid.transform.position + head.localPosition) - (linkeHand.position + rechteHand.position) / 2f)
            //    , -Vector3.Cross(linkeHand.position - (rigid.transform.position + head.localPosition),
            //    rechteHand.position - (rigid.transform.position + head.localPosition)
            //    ))
            //    );
            //rigid.MoveRotation(Quaternion.Lerp(Quaternion.identity,bodrotation,rigid.velocity.sqrMagnitude));

            Vector3 triangleforward = 
                -((rigid.transform.TransformPoint(head.localPosition)) - 
                (linkeHand.position + rechteHand.position) / 2f).normalized;
            Vector3 trianglenormal = Vector3.Cross(linkeHand.position - (rigid.transform.TransformPoint(head.localPosition)), 
                rechteHand.position - (rigid.transform.TransformPoint(head.localPosition))).normalized;
            Vector3 triangleright = Vector3.Cross(triangleforward,trianglenormal).normalized;

            Debug.DrawLine(rigid.transform.position, rigid.transform.position + triangleforward, Color.yellow);
            Debug.DrawLine(rigid.transform.position, rigid.transform.position + trianglenormal, Color.yellow);
            Debug.DrawLine(rigid.transform.position, rigid.transform.position + triangleright, Color.yellow);


            //limits/dampening towards upright orientation
            rigid.AddTorque(
                dampforce * Vector3.Cross(triangleforward, 
                    Vector3.ProjectOnPlane(
                        new Vector3(triangleforward.x, 0f, triangleforward.z
                        ).normalized, 
                        Vector3.Cross(triangleforward, Vector3.up)
                    )
                ), ForceMode.VelocityChange
            );
            rigid.AddTorque(
                dampforce * Vector3.Cross(
                    triangleright, 
                    Vector3.ProjectOnPlane(
                        new Vector3(triangleright.x, 0f, triangleright.z
                        ).normalized, 
                        Vector3.Cross(triangleright, Vector3.forward)
                    )
                ), ForceMode.VelocityChange
            );

        }

        Debug.DrawLine(rigid.transform.TransformPoint(head.localPosition), linkeHand.position, Color.green);
        Debug.DrawLine(rigid.transform.TransformPoint(head.localPosition), rechteHand.position, Color.green);
        Debug.DrawLine(rechteHand.position, linkeHand.position, Color.green);
        Debug.DrawLine(rigid.transform.position, rigid.transform.position + ((rigid.transform.TransformPoint(head.localPosition)) - (linkeHand.position + rechteHand.position) / 2f), Color.red);

        // clamped between to values, create drag based on deviation from the velocity vector
        //rigid.drag = Mathf.Max(0.05f, 10f* Mathf.Min(0.1f, Mathf.Log((Vector3.Cross(rigid.transform.forward, rigid.velocity.normalized)).magnitude)));

        //limits/dampening towards upright orientation
        rigid.AddTorque(dampforce * 
            Vector3.Cross(rigid.transform.forward, 
            Vector3.ProjectOnPlane(new Vector3(rigid.transform.forward.x, 0f, rigid.transform.forward.z).normalized, 
            Vector3.Cross(rigid.transform.forward, Vector3.up))), ForceMode.VelocityChange);
        rigid.AddTorque(dampforce * 
            Vector3.Cross(rigid.transform.right, 
            Vector3.ProjectOnPlane(new Vector3(rigid.transform.right.x, 0f, rigid.transform.right.z).normalized, 
            Vector3.Cross(rigid.transform.right, Vector3.forward))), ForceMode.VelocityChange);

        //rigid.AddTorque(rigid.velocity.magnitude * Vector3.Cross(rigid.transform.forward, rigid.velocity.normalized));//*Mathf.Pow(Vector3.Cross(rigid.transform.forward, rigid.velocity).magnitude, 2f));
        //rigid.AddForceAtPosition(0.001f*rigid.transform.up*rigid.velocity.sqrMagnitude, rigid.position);
        //rigid.AddTorque(rigid.velocity.sqrMagnitude * 1f/Vector3.Cross(rigid.transform.forward, rigid.velocity).magnitude * rigid.transform.right);
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
