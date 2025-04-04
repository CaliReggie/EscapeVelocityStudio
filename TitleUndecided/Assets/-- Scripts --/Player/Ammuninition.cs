using UnityEngine;
using System.Collections.Generic;
public class Ammuninition : MonoBehaviour
{
    public int ammoTotalCount = 5;
    public float ammoRestoredPerSec = 0.5f;
    public float offset = 20f;
    public GameObject AmmoPrefab;
    public Transform AmmoContainer;
    private float startPosX;
    private float _currentAmmoCount;
    private List<GameObject> UIAmmo = new List<GameObject>();
    private float height;
    private float width;
    public float currentAmmoCount
    {
        get
        {
            return _currentAmmoCount;
        }
        set
        {
            _currentAmmoCount = value;
            int index = (int)Mathf.Clamp(_currentAmmoCount, 0, ammoTotalCount - 0.1f);
            for (int i = index + 1; i < UIAmmo.Count; i++)
            {
                UIAmmo[i].transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0,-height,0);
            }
            for (int i = 0; i < index; i++)
            {
                UIAmmo[i].transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
            }
            if (_currentAmmoCount % 1 != 0)
            {
                UIAmmo[index].transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0,_currentAmmoCount % 1 * height - height,0);
            }
        }
    }
    public bool UseAmmo(float ammoRequired)
    {
        if (ammoRequired > currentAmmoCount)
        {
            return false;
        }
        else
        {
            currentAmmoCount -= ammoRequired;
            return true;
        }
    }

    public void Awake()
    {
        height = AmmoPrefab.GetComponent<RectTransform>().rect.height;
        width = AmmoPrefab.GetComponent<RectTransform>().rect.width;
        _currentAmmoCount = ammoTotalCount;
        startPosX = -(ammoTotalCount-1) * (width + offset)/2;
    }

    public void Start()
    {
        AmmoContainer = GameObject.Find("AmmoHolder").transform;
        for (int i = 0; i < ammoTotalCount; i++)
        {
            GameObject ammo = Instantiate(AmmoPrefab, AmmoContainer);
            ammo.transform.localPosition = new Vector3(startPosX, 0, 0);
            ammo.transform.localPosition += new Vector3(width + offset,0,0) * i;
            UIAmmo.Add(ammo);
        }
    }
    public void Update()
    {
        currentAmmoCount += Mathf.Clamp(ammoRestoredPerSec * Time.deltaTime, 0, ammoTotalCount - currentAmmoCount);
    }
}
