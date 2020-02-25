using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    public GameObject throwObject;
    public Vector2 throwPower;

    private Vector2 inputDirection;
    void Update()
    {
        inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetButtonDown("Fire"))
        {
            GameObject obj = Instantiate(throwObject, this.transform.position, Quaternion.identity);
            BombBehaviour bomb = obj.GetComponent<BombBehaviour>();
            bomb.tossDirection = inputDirection * throwPower;
        }
    }
}
