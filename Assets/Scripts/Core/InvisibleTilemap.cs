using UnityEngine;
using UnityEngine.Tilemaps;

public class InvisibleTilemap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TilemapRenderer>().enabled = false;
    }
}
