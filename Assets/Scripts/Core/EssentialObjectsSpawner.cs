using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();

        if (existingObjects.Length == 0)
        {
            var spawnPosition = new Vector3(0, 0, 0);
            var grid = FindObjectOfType<Grid>();
            
            // IF there is a grid, spawn at the center of the grid
            if (grid != null)
            {
                spawnPosition = grid.transform.position;
            }
            
            Instantiate(essentialObjectsPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
