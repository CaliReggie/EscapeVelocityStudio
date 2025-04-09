using System.Collections.Generic;
using UnityEngine;

public enum EScreenPos
    {
        TopLeft,
        TopCenter,
        TopRight,
        RightCenter,
        BottomRight,
        BottomCenter,
        BottomLeft,
        LeftCenter,
        Center
    }
public class Utils : MonoBehaviour
{
    // This method is used to determine the placement of a button based on given bounds and a button rect
    public static Vector2 DeterminePlacement(Vector2 min, Vector2 max, Rect buttonRect, EScreenPos placement)
    {
        Vector2 targetPos = Vector2.zero;
        
        switch (placement)
        {
            case EScreenPos.TopLeft:
                targetPos = new Vector2(min.x + buttonRect.width / 2, max.y - buttonRect.height / 2);
                break;
            case EScreenPos.TopCenter:
                targetPos = new Vector2((min.x + max.x) / 2, max.y - buttonRect.height / 2);
                break;
            case EScreenPos.TopRight:
                targetPos = new Vector2(max.x - buttonRect.width / 2, max.y - buttonRect.height / 2);
                break;
            case EScreenPos.RightCenter:
                targetPos = new Vector2(max.x - buttonRect.width / 2, (min.y + max.y) / 2);
                break;
            case EScreenPos.BottomRight:
                targetPos = new Vector2(max.x - buttonRect.width / 2, min.y + buttonRect.height / 2);
                break;
            case EScreenPos.BottomCenter:
                targetPos = new Vector2((min.x + max.x) / 2, min.y + buttonRect.height / 2);
                break;
            case EScreenPos.BottomLeft:
                targetPos = new Vector2(min.x + buttonRect.width / 2, min.y + buttonRect.height / 2);
                break;
            case EScreenPos.LeftCenter:
                targetPos = new Vector2(min.x + buttonRect.width / 2, (min.y + max.y) / 2);
                break;
            case EScreenPos.Center:
                targetPos = new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2);
                break;
        }
        
        return targetPos;
    }
    public static bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }
    
    //Shoutout JB
    static public Vector3 Bezier( float u, List<Vector3> vList, int i0=0, int i1=-1 ) {
        // Set i1 to the last element in vList
        if (i1 == -1) i1 = vList.Count-1;
        // If we are only looking at one element of vList, return it
        if (i0 == i1) {
            return( vList[i0] );
        }
        // Otherwise, call Bezier again with all but the leftmost used element of vList
        Vector3 l = Bezier(u, vList, i0, i1-1);
        // And call Bezier again with all but the rightmost used element of vList
        Vector3 r = Bezier(u, vList, i0+1, i1);
        // The result is the Lerp of these two recursive calls to Bezier
        Vector3 res = Vector3.LerpUnclamped( l, r, u );
        return( res );
    }
    
    // This version allows an Array or a series of Vector3s as input
	static public Vector3 Bezier( float u, params Vector3[] vecs ) {
		return( Bezier( u, new List<Vector3>(vecs) ) );
	}

}
