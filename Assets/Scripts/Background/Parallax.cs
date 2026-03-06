using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Manages background paralax because apparently unity2D does not have it built in lol
/// </summary>
[RequireComponent(typeof(Camera))]
public class Parallax : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> _backgroundLayers = new();
    [SerializeField] private int loopBackgroundCount = 3;

    private Dictionary<SpriteRenderer, float> _bgLayerStartingPos = new();
    private Camera _camera;
    private int _furthestLayer = 0;
    private int _lengthsTravelled = 0; // remind ben to add to his



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
    void HandleParallax()
    {
        foreach (SpriteRenderer layer in _backgroundLayers)
        {
            // we want linearity
            float parallaxFormula = (float)(_furthestLayer - layer.sortingOrder + 1) / _furthestLayer;

            //fractional movement with further sorting orders travelling faster. things move at a fraction of the camera's movement relative to distance
            float lengthtraversal = _camera.transform.position.x * (1 - parallaxFormula);
            float dist = _camera.transform.position.x * parallaxFormula;
            layer.transform.position = new(_bgLayerStartingPos[layer] + dist, layer.transform.position.y, layer.transform.position.z);

            //check if the length traversed by this layer is longer then the size of the art layer
            if (lengthtraversal > _bgLayerStartingPos[layer] + layer.size.x)
            {
                _bgLayerStartingPos[layer] += layer.size.x * loopBackgroundCount;
            }
            //check to see if we have done so in the other direction
            else if (lengthtraversal < _bgLayerStartingPos[layer] - layer.size.x)
            {
                _bgLayerStartingPos[layer] -= layer.size.x * loopBackgroundCount;
            }
        }
    }

    void Update()
    {
        HandleParallax();
    }




}
