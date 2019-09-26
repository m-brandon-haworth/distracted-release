// ShowGoldenPath
using UnityEngine;
using UnityEngine.AI;

public class ShowGoldenPath_Distraction : MonoBehaviour
{
	public Transform target;
	private NavMeshPath path;
	Rigidbody rb;
	public float prefered_speed = 1.3f;
	public float deviation_factor = 0.1f;
	public float attentiveness_level = 0.1f; // The level of attentiveness only when the agent is partially distracted
	public float max_distraction_time = 5.0f;
	public float min_attentive_time = 5.0f;
	public float distraction_speed = 0.6f;
	public float percent_chance_become_distracted = 50; //The likelihood an agent becomes distracted on a given frame, expressed as a percentage
	int check2= 1;
    private float elapsed = 0.0f;
	private float elapsed_time_distracted = 0.0f; //The time the agent has been distracted
	private float elapsed_time_attentive = 0.0f; //The time the agent has been fully attentive
	float current_attentiveness = 1.0f; //The current attentiveness of the agent
	private float distractedChance;
	Vector3 direction;
	bool is_distracted = false;

    void Start()
	{
		path = new NavMeshPath();
		rb = GetComponent<Rigidbody> ();
	    NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
		direction = (path.corners [check2] - transform.position);
		int randDevDir = Random.Range(0, 2);
		if (randDevDir == 1) { // Determines whether the agents deviates left or right while distracted (e.g. whether the agent is right-brained or left-brained)
			deviation_factor = -deviation_factor;
		}
		distractedChance = percent_chance_become_distracted / 100;

	}

	public bool checkDistracted(){
		return is_distracted;
	}

	//Make the agent pay attention. Put in its own function so it can be called from outside this script
	public void PayAttention(){
		elapsed_time_distracted = 0.0f;
		is_distracted = false;
		NavMesh.CalculatePath (transform.position, target.position, NavMesh.AllAreas, path);
		current_attentiveness = 1.0f;
	}
		
	public void BecomeDistracted(){
		is_distracted = true;
		direction = (path.corners [check2] - transform.position);
		direction += new Vector3 (-direction.z * deviation_factor, 0, direction.x * deviation_factor); //lateral deviation from a straight line while distracted
		current_attentiveness = attentiveness_level;
		elapsed_time_attentive = 0.0f;
	}

	void FixedUpdate()
	{

		elapsed += Time.deltaTime;
		//Distraction logic
		float randNum = Random.value;
		if (is_distracted == false && randNum <= distractedChance && elapsed_time_attentive > min_attentive_time && elapsed >= 1.0f) {
			BecomeDistracted ();
		} else if (is_distracted == true && elapsed_time_distracted < max_distraction_time) {
			elapsed_time_distracted += Time.deltaTime;
		} else if (elapsed_time_distracted >= max_distraction_time) {
			PayAttention (); 
		} else if (is_distracted == false) {
			elapsed_time_attentive += Time.deltaTime;
		} 

		if (elapsed > 1.0f && (transform.position - target.position).magnitude > 1.5f) {
			elapsed -= 1.0f;
			NavMesh.CalculatePath (transform.position, target.position, NavMesh.AllAreas, path);
			check2 = 1;
		}

		// Update the way to the goal every second.
		for (int i = 0; i < path.corners.Length - 1; i++)
			Debug.DrawLine (path.corners [i], path.corners [i + 1], Color.red);

        // Goal driven force.
		if (current_attentiveness == 1.0f) { //Not distracted
			rb.AddForce (((path.corners [check2] - transform.position).normalized * prefered_speed - rb.velocity) / 0.5f);
		} else { //Distracted
			rb.AddForce ( ( direction.normalized*distraction_speed- rb.velocity)/0.5f );
		}

		if (is_distracted == false) {
			Debug.DrawLine (transform.position, transform.position + (path.corners [check2] - transform.position).normalized * 3.0f, Color.green);
			gameObject.GetComponentInChildren<Renderer> ().material.color = Color.green;
		} else {
			Debug.DrawLine (transform.position, transform.position + (path.corners [check2] - transform.position).normalized * 3.0f, Color.blue);
			gameObject.GetComponentInChildren<Renderer> ().material.color = Color.blue;
		}

		if ( (transform.position - path.corners [check2]).magnitude < 0.5f && path.corners.Length-check2 >1)
			check2 = check2 + 1;
	}
}