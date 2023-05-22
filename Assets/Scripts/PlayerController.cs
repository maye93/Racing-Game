using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public int playerLap = 0;
    public int waypointCounter = 0;

    public float speed = 10f;
    public float turnSpeed = 100f;
    public float maxAngle = 30f;
    public float flipDuration = 2f;

    public AudioSource engineSound;
    public AudioSource accelerateSound;
    public AudioSource decelerateSound;
    public AudioSource thudSound;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool controlsEnabled = false;
    private float timeSinceLastFlip = 0f;
    private Quaternion initialRotation;
    private List<Transform> waypoints = new List<Transform>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialRotation = rb.rotation;

        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");

        for (int i = 1; i <= waypointObjects.Length; i++)
        {
            string waypointName = "Waypoint " + i;
            GameObject waypointObject = GameObject.Find(waypointName);
            if (waypointObject != null)
            {
                waypoints.Add(waypointObject.transform);
            }
            else
            {
                Debug.LogError("Could not find object with name " + waypointName);
            }
        }
    }


    void FixedUpdate()
    {
        if (controlsEnabled)
        {
            // Get the input from the player
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            // Move the player's car forward and backward
            Vector3 movement = transform.forward * (-verticalInput) * speed * Time.deltaTime;
            rb.MovePosition(rb.position + movement);

            // Rotate the player's car left and right
            float rotation = horizontalInput * turnSpeed * Time.deltaTime;
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, rotation, 0f));
            rb.MoveRotation(rb.rotation * deltaRotation);

            // Check if the car is flipped over
            if (Vector3.Dot(transform.up, Vector3.down) > 0f)
            {
                // Increment the time since last flip
                timeSinceLastFlip += Time.deltaTime;

                // If the car has been flipped over for long enough, flip it back
                if (timeSinceLastFlip > flipDuration)
                {
                    rb.rotation = initialRotation;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    timeSinceLastFlip = 0f;
                }
                else
                {
                    // Rotate the car back to an upright position
                    float angle = Vector3.Angle(transform.up, Vector3.up);
                    float ratio = Mathf.Clamp01(angle / maxAngle);
                    Quaternion targetRotation = Quaternion.Slerp(rb.rotation, initialRotation, ratio);
                    rb.MoveRotation(targetRotation);
                }
            }
            else
            {
                // Reset the time since last flip if the car is not flipped over
                timeSinceLastFlip = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            // Get the index of the current waypoint
            int waypointIndex = int.Parse(other.gameObject.name.Split(' ')[1]) - 1;
            
            if (waypointIndex == waypointCounter)
            {
                waypointCounter++;
            }

            Debug.Log("Player Waypoint: " + waypointCounter);
        }
        else if (other.CompareTag("FinishLine"))
        {
            waypointCounter = 0;
            playerLap++;
            
            Debug.Log("Player Lap: " + playerLap);
        }
    }

    
    public void EnableControl()
    {
        controlsEnabled = true;
    }

    public void DisableControl()
    {
        controlsEnabled = false;
    }
}