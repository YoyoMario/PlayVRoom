using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    Animator m_Animator;

    [Range(0.1f,2f)]
    public float m_MovingTargetSpeed;

    void Start () {
        //Get animator reference from the game object
        m_Animator = gameObject.GetComponent<Animator>();
    }

    public void Update () {
        //Set animator speed based on m_MovingTargetSpeed value
        m_Animator.speed = m_MovingTargetSpeed;
    }
}