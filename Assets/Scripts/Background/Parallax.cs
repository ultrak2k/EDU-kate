using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


/// <summary>
/// Manages background paralax because apparently unity2D does not have it built in lol
/// </summary>
[RequireComponent(typeof(Camera))]
public class Parallax : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> _backgroundLayers = new();
    [SerializeField] private int _loopBackgroundCount = 3;
    [SerializeField] private int _xMultiplier = 1;
    [SerializeField] private int _yMultiplier = 1;

    private Dictionary<SpriteRenderer, float> _bgLayerStartingPosX = new();
    private Dictionary<SpriteRenderer, float> _bgLayerStartingPosY = new();
    private Camera _camera;
    private int _furthestLayer = 0;
    



    void Start()
    {
        FindFurthestLayer();
        _camera = GetComponent<Camera>();
        foreach (SpriteRenderer layer in _backgroundLayers)
        {
            _bgLayerStartingPosX.Add(layer, layer.transform.position.x);
            _bgLayerStartingPosY.Add(layer, layer.transform.position.y);
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
            // we want linearity and looping
            float parallaxFormula = (float)(_furthestLayer - layer.sortingOrder) / _furthestLayer;
            float lengthtraversal = _camera.transform.position.x * (1 - parallaxFormula);

            //fractional movement with further sorting orders travelling faster. things move at a fraction of the camera's movement relative to distance
            float distX = _camera.transform.position.x * parallaxFormula * _xMultiplier;
            float distY = _camera.transform.position.y * parallaxFormula * _yMultiplier;
            layer.transform.position = new(_bgLayerStartingPosX[layer] + distX,  _bgLayerStartingPosY[layer] + distY, layer.transform.position.z);

            //check if the length traversed by this layer is longer then the size of the art layer
            if (lengthtraversal > _bgLayerStartingPosX[layer] + layer.size.x)
            {
                _bgLayerStartingPosX[layer] += layer.size.x * _loopBackgroundCount;
            }
            //check to see if we have done so in the other direction
            else if (lengthtraversal < _bgLayerStartingPosX[layer] - layer.size.x)
            {
                _bgLayerStartingPosX[layer] -= layer.size.x * _loopBackgroundCount;
            }
        }
    }

    //gets the x position of the furthest edge of the rightmost background
    public Vector3 GetFurthestBackgroundPos()
    {
        float biggestX = 0;
        float biggestY = 0;
        foreach (SpriteRenderer layer in _backgroundLayers)
        {
            if (layer.sortingOrder == 0)
            {
                float newX = layer.transform.position.x + layer.size.x / 2;
                biggestX = biggestX < newX ? newX : biggestX;
                biggestY = layer.transform.position.y;
            }
        }
        return new (biggestX, biggestY, 0);
    }

    void Update()
    {
        HandleParallax();
    }
}
