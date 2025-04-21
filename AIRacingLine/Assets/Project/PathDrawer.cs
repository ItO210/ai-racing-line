using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Velocity
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Data
{
    public List<Position> positions;
    public List<Velocity> velocities;
}

public class PathDrawer : MonoBehaviour
{
    public TextAsset jsonFile; // Drag the JSON file here in the Inspector
    public GameObject kart; // Reference to the kart GameObject

    private Data data;
    private List<GameObject> spheres = new List<GameObject>();

    void Start()
    {
        LoadJSON();
        CreateSpheres();
    }

    void Update()
    {
        if (data != null && data.velocities != null)
        {
            UpdateSphereColors();
        }
    }

    void LoadJSON()
    {
        if (jsonFile != null)
        {
            data = JsonUtility.FromJson<Data>(jsonFile.text);
            Debug.Log("JSON Loaded Successfully");
        }
        else
        {
            Debug.LogError("JSON file is not assigned.");
        }
    }

    void CreateSpheres()
    {
        if (data != null && data.positions != null)
        {
            for (int i = 0; i < data.positions.Count; i++)
            {
                Vector3 pos = new Vector3(data.positions[i].x, data.positions[i].y, data.positions[i].z);

                // Create a sphere at the specified position
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = pos; // Set the position of the sphere

                // Optionally, adjust the size of the sphere
                sphere.transform.localScale = new Vector3(1f, 1f, 1f); // You can change the scale here if needed

                Collider sphereCollider = sphere.GetComponent<Collider>();
                if (sphereCollider != null)
                {
                    sphereCollider.enabled = false; // Disable the collider
                }

                // Add the sphere to the list
                spheres.Add(sphere);
            }
        }
        else
        {
            Debug.LogError("No data available to create spheres.");
        }
    }

    void UpdateSphereColors()
    {
        Rigidbody kartRigidbody = kart.GetComponent<Rigidbody>();
        if (kartRigidbody == null)
        {
            Debug.LogError("Kart GameObject does not have a Rigidbody component.");
            return;
        }

        Vector3 kartVelocity = kartRigidbody.velocity;

        // Calculate the magnitude of the kart's velocity
        float kartSpeed = kartVelocity.magnitude;

        for (int i = 0; i < spheres.Count && i < data.velocities.Count; i++)
        {
            Vector3 sphereVelocity = new Vector3(
                data.velocities[i].x,
                data.velocities[i].y,
                data.velocities[i].z
            );

            // Calculate the magnitude of the sphere's velocity
            float sphereSpeed = sphereVelocity.magnitude;

            // Compare the magnitudes to determine color
            Renderer sphereRenderer = spheres[i].GetComponent<Renderer>();
            if (sphereRenderer != null)
            {
                // Define thresholds for color changes based on velocity magnitude
                float speedDifference = kartSpeed - sphereSpeed;

                if (speedDifference <= 1.0f) // Kart and sphere have similar speeds
                {
                    sphereRenderer.material.color = Color.green; // Green for similar speeds
                }
                else if (speedDifference <= 3.0f) // Kart is a little faster than the sphere
                {
                    sphereRenderer.material.color = Color.yellow; // Yellow for small difference
                }
                else // Kart is much faster than the sphere
                {
                    sphereRenderer.material.color = Color.red; // Red for significant difference
                }
            }
        }
    }
}
