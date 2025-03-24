using System;
using UnityEngine;
using System.Collections;
public class UIButton : MonoBehaviour
{
    private void OnMouseEnter()
    {
        StopAllCoroutines();
        
        StartCoroutine(ChangeScale(1, 1.25f, 0.1f));
        
        Debug.Log("Mouse Enter");
    }
    
    private void OnMouseExit()
    {
        StopAllCoroutines();
        
        StartCoroutine(ChangeScale(1.25f, 1, 0.1f));
    }
    
    private IEnumerator ChangeScale (float startScale, float endScale, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(new Vector3(startScale, startScale, 1), new Vector3(endScale, endScale, 1), time / duration);
            yield return null;
        }
        
        transform.localScale = new Vector3(endScale, endScale, 1);
    }
}
