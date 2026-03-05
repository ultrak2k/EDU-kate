using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Manages background paralax because apparently unity2D does not have it built in lol
/// </summary>
[RequireComponent(typeof(Camera))]
public class Parallax : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> _backgroundLayers = new();
    [SerializeField] private Dictionary<SpriteRenderer, float> _bgLayerStartingPos = new();
    private Camera _camera;
    private int _furthestLayer = 0;
    private int _lengthsTravelled = 0;



    void Start()
    {
        FindFurthestLayer();
        _camera = GetComponent<Camera>();
        foreach (SpriteRenderer layer in _backgroundLayers)
        {
            _bgLayerStartingPos.Add(layer, layer.transform.position.x);
        }
    }

    void FindFurthestLayer()
    {
        foreach (SpriteRenderer layer in _backgroundLayers)
        {
            if (layer.sortingOrder > _furthestLayer)
            {
                _furthestLayer = layer.sortingOrder;
            }
        }
    }

    //move backgrounds porportional to their sort order
    void HandleParralax()
    {
        foreach (SpriteRenderer layer in _backgroundLayers)
        {
            //fractional movement with further sorting orders travelling faster. things move at a fraction of the camera's movement relative to distance
            float lengthtraversal = _camera.transform.position.x * (1 - 1 / (float)layer.sortingOrder);
            float dist = _camera.transform.position.x * (1 / (float)layer.sortingOrder);
            layer.transform.position = new(_bgLayerStartingPos[layer] + dist, layer.transform.position.y, layer.transform.position.z);

            //check if the length traversed by this layer is longer then the size of the art layer
            if (lengthtraversal > _bgLayerStartingPos[layer] + layer.bounds.size.x)
            {
                _bgLayerStartingPos[layer] += layer.bounds.size.x;
            }
            //check to see if we have done so in the other direction
            else if (lengthtraversal < _bgLayerStartingPos[layer] - layer.bounds.size.x)
            {
                _bgLayerStartingPos[layer] -= layer.bounds.size.x;
            }
        }
    }

    void Update()
    {
        HandleParralax();
    }




}
