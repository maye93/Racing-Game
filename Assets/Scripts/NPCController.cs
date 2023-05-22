using UnityEngine;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public int npcLap = 0;
    public int waypointCounter = 0;

    public float maxSpeed = 10f;
    public float accelerationTime = 0.1f;
    public float rotationSpeed = 5f;
    public float detectionDistance = 5f;

    public AudioSource engineSound;
    public AudioSource accelerateSound;
    public AudioSource decelerateSound;
    public AudioSource thudSound;

    private List<Transform> waypoints = new List<Transform>();
    private bool controlsEnabled = false;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private Rigidbody rb;
    private Quaternion initialRotation;
    private float maxAngle = 80f; // Maximum angle for the car to be considered flipped
    private float flipDuration = 3f; // Duration to wait before flipping the car back
    private float timeSinceLastFlip = 0f;
    private bool isAccelerating = false;

    private void Start()
    {
        engineSound.Pause();
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

    private void FixedUpdate()
    {
        if (controlsEnabled)
        {
            // Calculate distance to the current waypoint
            float distance = Vector3.Distance(transform.position, waypoints[waypointCounter].position);

            // Move towards the current waypoint
            Vector3 targetDirection = waypoints[waypointCounter].position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Calculate the new speed
            float acceleration = maxSpeed / accelerationTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

            // Apply movement with the new speed
            rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            // Perform raycasting to detect objects in front
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance))
            {
                if (hit.collider.CompareTag("NPC") || hit.collider.CompareTag("Player"))
                {
                    DodgeObject(hit.collider.gameObject);
                }
            }

            // Play appropriate sound effects
            if (currentSpeed > 0 && currentSpeed < maxSpeed && rb.velocity.magnitude < currentSpeed)
            {
                if (!isAccelerating && !accelerateSound.isPlaying)
                {
                    isAccelerating = true;
                    accelerateSound.Play();
                    decelerateSound.Stop();
                    engineSound.Stop();
                }
            }
            else if (currentSpeed >= maxSpeed)
            {
                if (!engineSound.isPlaying && (!isAccelerating || (isAccelerating && !accelerateSound.isPlaying)))
                {
                    isAccelerating = false;
                    engineSound.Play();
                    accelerateSound.Stop();
                    decelerateSound.Stop();
                }
            }
            else
            {
                if (!decelerateSound.isPlaying && (!isAccelerating || (isAccelerating && !accelerateSound.isPlaying)))
                {
                    isAccelerating = false;
                    decelerateSound.Play();
                    accelerateSound.Stop();
                    engineSound.Stop();
                }
            }
        }
        else
        {
            currentSpeed = 0f;
            StopAllSound();
        }

        if (Vector3.Dot(transform.up, Vector3.down) > 0f)
        {
            // Increment the time since last flip
            timeSinceLastFlip += Time.fixedDeltaTime;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("NPC"))
        {
            // Check collision side
            Vector3 collisionDirection = collision.contacts[0].point - transform.position;
            float angle = Vector3.Angle(collisionDirection, transform.forward);

            if (angle < 45f) // Front collision
            {
                thudSound.Play();
            }
            else if (angle > 135f) // Back collision
            {
                thudSound.Play();
            }
            else
            {
                Vector3 relativeDirection = transform.InverseTransformPoint(collision.contacts[0].point);

                if (relativeDirection.x < 0f) // Left collision
                {
                    thudSound.Play();
                }
                else // Right collision
                {
                    thudSound.Play();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            // Get the index of the current waypoint
            int waypointIndex = int.Parse(other.gameObject.name.Split(' ')[1]) - 1;

            if (waypointCounter == waypointIndex)
            {
                waypointCounter++;

                if (waypointCounter >= waypoints.Count)
                {
                    waypointCounter = 0;
                    npcLap++;
                }
            }
            // Debug.Log("NPC Waypoints: " + waypointCounter);
        }
    }

    private void StopAllSound()
    {
        decelerateSound.Stop();
        accelerateSound.Stop();
        engineSound.Stop();
        thudSound.Stop();
    }

    private void DodgeObject(GameObject objToDodge)
    {
        // Calculate the direction to dodge
        Vector3 dodgeDirection = transform.right * (Random.value < 0.5f ? -1f : 1f);

        // Apply the dodge
        rb.AddForce(dodgeDirection * accelerationTime, ForceMode.Acceleration);
    }

    public void EnableControl()
    {
        controlsEnabled = true;
        targetSpeed = maxSpeed;
        isAccelerating = false;
    }

    public void DisableControl()
    {
        controlsEnabled = false;
        targetSpeed = 0f;
        isAccelerating = false;
    }
}
