using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInput input;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        Debug.Log(input.actions["Look"].ReadValue<Vector2>());
    }
}
