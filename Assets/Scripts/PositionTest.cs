using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTest : MonoBehaviour
{
    [SerializeField] GameObject markerPrefab;
    [SerializeField] int numMarker= 102;
    public GameObject[] markers;
    // Start is called before the first frame update
    void Awake()
    {
        markers = new GameObject[numMarker];
        for (int i = 0; i < numMarker; i++)
        {
            markers[i] = Instantiate(markerPrefab);
        }
    }

}
