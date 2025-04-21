using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using Unity.MLAgents.Actuators;
using KartGame.KartSystems;
using Unity.MLAgents.Policies;
using System.Linq;
using System.IO;

public class VelocityCheckpoints : Agent, IInput
{
    public float myHor = 0;
    public bool myAccelerate, myBrake;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Observable(numStackedObservations: 16)]
    Vector3 vel;
    private float lastCheckpointTime;

    // Timer functionality
    public bool enableTimer = false; // Toggle for enabling timer (set in Inspector)
    private float episodeStartTime;
    private List<float> episodeTimes = new List<float>(); // Store times of episodes
    public int maxLapCount = 20; // Set this to the max number of laps before averaging
    private int currentLapCount = 0; // Track the number of laps completed

    // Tracking kart positions
    private List<Vector3> lapPositions = new List<Vector3>(); // Store positions during a lap
    private List<List<Vector3>> allLapPositions = new List<List<Vector3>>(); // Store positions for all laps

    private List<Vector3> lapVelocities = new List<Vector3>(); // Store velocities during a lap
    private List<List<Vector3>> allLapVelocities = new List<List<Vector3>>(); // Store velocities for all laps

    public float sphereSize = 0.5f; // The size of each sphere

    private float positionRecordInterval = 0.1f; // Time interval to record position
    private float timeSinceLastPosition = 0f; // Time tracker

    public override void Initialize()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        lastCheckpointTime = Time.time;

        // Reset the timer if enabled
        if (enableTimer)
        {
            episodeStartTime = Time.time;  // Start the episode timer
        }

        // Clear lap positions for the new episode
        lapPositions.Clear();
        allLapPositions.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        vel = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).normalized;

        if (enableTimer)
        {
            timeSinceLastPosition += Time.deltaTime;

            if (timeSinceLastPosition >= positionRecordInterval)
            {
                lapPositions.Add(transform.position); // Store position
                lapVelocities.Add(GetComponent<Rigidbody>().velocity); // Store velocity
                timeSinceLastPosition = 0f; // Reset time tracker
            }
        }

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        myHor = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1.0f, 1.0f);
        myAccelerate = Convert.ToBoolean(actionBuffers.DiscreteActions[0]);
        myBrake = Convert.ToBoolean(actionBuffers.DiscreteActions[1]);

        // Reward for speed (positive reward)
        float currentVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).magnitude;

        if (currentVelocity > 0)
        {
            AddReward(0.01f * currentVelocity);
        }
        if (currentVelocity < 0.1f)
        {
            AddReward(-0.1f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Track"))
        {
            AddReward(-2.0f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            // Calculate time since last checkpoint
            float timeSinceLastCheckpoint = Time.time - lastCheckpointTime;

            // Reward inversely proportional to time taken to reach the checkpoint
            AddReward(20.0f / timeSinceLastCheckpoint);

            // Update last checkpoint time
            lastCheckpointTime = Time.time;
        }

        if (other.CompareTag("Bumpy"))
        {
            AddReward(-0.5f);
            EndEpisode();
        }

        if (other.CompareTag("Goal") && enableTimer)
        {
            // Calculate lap time
            float lapTime = Time.time - episodeStartTime;

            Debug.Log($"Lap {currentLapCount + 1} completed in {lapTime:F2} seconds.");

            // Store lap time in episodeTimes
            episodeTimes.Add(lapTime);

            // Store lap positions for averaging later
            allLapPositions.Add(new List<Vector3>(lapPositions));
            allLapVelocities.Add(new List<Vector3>(lapVelocities));

            // Increment lap count
            currentLapCount++;

            // Check if we've reached the maximum laps
            if (currentLapCount >= maxLapCount)
            {
                // Calculate and log the average lap time
                float averageTime = CalculateAverageEpisodeTime();
                Debug.Log($"Average lap time after {maxLapCount} laps: {averageTime:F2} seconds.");

                // Calculate and draw the average path
                DrawAveragePath();

                // Optionally, clear lap positions after averaging or keep them for further analysis
                allLapPositions.Clear();

                // End the episode after calculating the average lap time
                EndEpisode();
            }
            else
            {
                // Reset the lap positions for the next lap
                lapPositions.Clear();
                lapVelocities.Clear();
                // Reset the episode start time for the next lap
                episodeStartTime = Time.time;
                EndEpisode();
            }
        }
    }

    private float CalculateAverageEpisodeTime()
    {
        if (episodeTimes.Count == 0)
            return 0;

        float totalTime = 0;
        foreach (float time in episodeTimes)
        {
            totalTime += time;
        }
        return totalTime / episodeTimes.Count;
    }

    private void DrawAveragePath()
{
    if (allLapPositions.Count == 0 || allLapVelocities.Count == 0)
        return;

    List<Vector3> averagePositions = new List<Vector3>();
    List<Vector3> averageVelocities = new List<Vector3>();

    int maxPositionCount = allLapPositions.Max(lap => lap.Count);

    for (int i = 0; i < maxPositionCount; i++)
    {
        Vector3 avgPosition = Vector3.zero;
        Vector3 avgVelocity = Vector3.zero;
        int validPositionCount = 0;
        int validVelocityCount = 0;

        foreach (var lap in allLapPositions)
        {
            if (i < lap.Count)
            {
                avgPosition += lap[i];
                validPositionCount++;
            }
        }

        foreach (var lap in allLapVelocities)
        {
            if (i < lap.Count)
            {
                avgVelocity += lap[i];
                validVelocityCount++;
            }
        }

        if (validPositionCount > 0)
        {
            avgPosition /= validPositionCount;
            averagePositions.Add(avgPosition);
        }

        if (validVelocityCount > 0)
        {
            avgVelocity /= validVelocityCount;
            averageVelocities.Add(avgVelocity);
        }
    }

    SaveAveragePathToJson(averagePositions, averageVelocities);
}


private void SaveAveragePathToJson(List<Vector3> averagePositions, List<Vector3> averageVelocities)
{
    List<Vector3Data> positionData = averagePositions.Select(pos => new Vector3Data(pos)).ToList();
    List<Vector3Data> velocityData = averageVelocities.Select(vel => new Vector3Data(vel)).ToList();

    string directoryPath = "Assets/Project/Lines";
    if (!Directory.Exists(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    string json = JsonUtility.ToJson(new PathData
    {
        positions = positionData,
        velocities = velocityData
    }, true);

    string filePath = Path.Combine(directoryPath, "path.json");
    File.WriteAllText(filePath, json);

    Debug.Log($"Average path with velocities saved to {filePath}");
}

[Serializable]
public class PathData
{
    public List<Vector3Data> positions;
    public List<Vector3Data> velocities;
}


    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }

    [Serializable]
    public class LineData
    {
        public List<Vector3Data> path;
    }

    public InputData GenerateInput()
    {
        return new InputData
        {
            Accelerate = myAccelerate,
            Brake = myBrake,
            TurnInput = myHor,
        };
    }
}
