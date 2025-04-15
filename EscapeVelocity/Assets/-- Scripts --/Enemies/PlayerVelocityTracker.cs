using System.Collections.Generic;
using UnityEngine;

public class PlayerVelocityTracker : MonoBehaviour
{
    // Ref to player controller
    private PlayerMovement characterController;
    //private Movement playerMovement;
    [SerializeField]
    [Range(0.1f, 5f)]
    [Tooltip("Stores historical position of player")] private float histPosDur = 1f;  //Stores the historical position of player
    [SerializeField]
    [Range(0.001f, 1f)]
    [Tooltip("How often to check for the players' velocity")] private float histPosInterval = 0.1f;    //How often to check for player position

    public static Vector3 averageVel
    {
        get
        {
            Vector3 average = Vector3.zero;
            foreach (Vector3 vel in histVelocities)
            {
                average += vel;
            }
            average.y = 0;

            return average / histVelocities.Count;
        }
    }


    private static Queue<Vector3> histVelocities; //stores list of velocities
    private float lastPosTime;
    private int maxQueueSize;
    void Start()
    {
        characterController = GetComponent<PlayerMovement>();
        //playerMovement = GetComponent<Movement>();
        maxQueueSize = Mathf.CeilToInt(1f / histPosInterval * histPosDur);
        histVelocities = new Queue<Vector3>(maxQueueSize);
    }

    void Update()
    {
        if (lastPosTime + histPosInterval <= Time.time)
        {
            if (histVelocities.Count == maxQueueSize)
            {
                histVelocities.Dequeue();
            }

            histVelocities.Enqueue(characterController.GetComponent<Rigidbody>().linearVelocity);
            lastPosTime = Time.time;
        }
    }

}