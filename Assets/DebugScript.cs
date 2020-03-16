using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public Grid _mapPrefab;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Delete)){
            BombBehaviour[] bombs = (BombBehaviour[]) GameObject.FindObjectsOfType(typeof(BombBehaviour));
            foreach(BombBehaviour bomb in bombs){
                Destroy(bomb.gameObject);
            }
        }
        if(Input.GetKeyDown(KeyCode.R)){
            Grid map = (Grid) GameObject.FindObjectOfType(typeof(Grid));
            Destroy(map.gameObject);
            Instantiate(_mapPrefab);
        }
    }
}
