using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {

    public GameObject target;
    public int max = 11;
    public float maxdist = 4f;
    Transform t_transform;
    Queue avgQueue, closerQueue;
    Vector3 avgVector, closerVector;
    Rigidbody rigid;

    Vector3 prevPos;

	// Use this for initialization
	void Start () {
        rigid = GetComponent<Rigidbody>();

        t_transform = target.transform;
        avgVector = t_transform.position;
        closerQueue = new Queue();
        avgQueue = new Queue();


        prevPos = transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        closerQueue.Enqueue(t_transform.position);
        if (closerQueue.Count == Mathf.Max(1,max/2)) {
            closerQueue.Dequeue();
            Vector3 sum = Vector3.zero;
            foreach (Vector3 v in closerQueue) {
                sum += v;
            }
            sum /= closerQueue.Count;
            closerVector = sum;
        }
        if (Vector3.Distance(avgVector, t_transform.position) > maxdist) {

            avgQueue.Enqueue(t_transform.position);
            if (avgQueue.Count == max + 1) {
                avgQueue.Dequeue();
                Vector3 sum = Vector3.zero;
                foreach (Vector3 v in avgQueue) {
                    sum += v;
                }
                sum /= avgQueue.Count;
                avgVector = sum;
            }
        }
        if (avgQueue.Count == max) {
            prevPos = transform.position;
            rigid.MovePosition(Vector3.Slerp(avgVector,prevPos,0.5f));
            Vector3 look = closerVector - avgVector;
            look.y = 0f;
            rigid.MoveRotation(Quaternion.LookRotation(look.normalized, Vector3.up));

        }
    }
}
