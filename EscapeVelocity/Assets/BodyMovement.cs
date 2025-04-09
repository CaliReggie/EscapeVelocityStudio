using System;
using System.Collections;
using UnityEngine;

public class BodyMovement : MonoBehaviour
{
    [Header("Leg References")]
    
    [SerializeField] private LegMovement[] legs;
    
    //Private, or non serialized below

    private void OnEnable()
    {
        StartCoroutine(InitLegs());
    }

    private IEnumerator InitLegs()
    {
        yield return new WaitForSeconds(0.1f);
        
        foreach (LegMovement leg in legs)
        {
            leg.Initialize();
        }
    }
}
