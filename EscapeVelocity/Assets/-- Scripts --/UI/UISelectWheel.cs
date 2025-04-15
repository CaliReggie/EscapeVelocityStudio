using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UISelectWheel : MonoBehaviour
{
    [Header("Animation Settings")]
    
    [SerializeField] private float normalScale;
    
    [SerializeField] private float selectedScale;
    
    [SerializeField] private float changeDuration;
    
    [Header("Color Settings")]
    
    [SerializeField] private Sprite normalImage;
    
    [SerializeField] private Sprite selectedImage;
    
    [SerializeField] private Sprite equippedImage;
    
    [Header("Equip Icons")]
    
    [SerializeField] private Sprite grappleIcon;
    
    [SerializeField] private Sprite damageDiscIcon;
    
    [SerializeField] private Sprite utilityDiscIcon;
    
    [SerializeField] private Sprite meleeIcon;
    
    [SerializeField] private Image equipIcon;
    
    [Header("Wheel Slots")]
    
    [SerializeField] private List<EquipWheelSlot> wheelSlots;
    
    //Dynamic, or Non-Serialized Below
    
    private EquipWheelSlot _selectedSlot;
    
    private List<Transform> _animatingSlots;
    
    public InputAction EquipWheelAction { get; set; }

    private void Awake()
    {
        if (wheelSlots == null)
        {
            Debug.LogError("No wheel slots assigned to the wheel, disabling wheel.");
            
            enabled = false;
        }
        else if (wheelSlots.Count == 0)
        {
            Debug.LogError("No wheel slots assigned to the wheel, disabling wheel.");
            
            enabled = false;
        }
    }
    
    public void SetToBase()
    {
        equipIcon.sprite = grappleIcon; // Default icon
    }

    private void OnEnable()
    {
        EEquippableClass currentEquippableClass = PlayerEquipabbles.S.CurrentPrimaryEquippable.EquippableClass;
        
        foreach (EquipWheelSlot slot in wheelSlots) // Showing equipped color if something is equipped
        {
            if (slot.slotEquipClass == currentEquippableClass)
            {
                slot.slotImage.sprite = equippedImage;
            }
        }
        
        _animatingSlots = new List<Transform>();
    }

    private void OnDisable()
    {
        Vector3 resetScale = new Vector3(normalScale, normalScale, normalScale);
        
        foreach (EquipWheelSlot slot in wheelSlots) // Resetting slots
        {
            slot.Reset(resetScale);

            slot.slotImage.sprite = normalImage;

            if (_selectedSlot != null && _selectedSlot == slot) // If we have a selected slot, ensure it's equipped
            {
                PlayerEquipabbles.S.EquipByClass(slot.slotEquipClass);

                switch (slot.slotEquipClass)
                {
                    case EEquippableClass.Grapple:
                        equipIcon.sprite = grappleIcon;
                        break;
                    case EEquippableClass.CombatDisk:
                        equipIcon.sprite = damageDiscIcon;
                        break;
                    case EEquippableClass.UtilityDisk:
                        equipIcon.sprite = utilityDiscIcon;
                        break;
                    case EEquippableClass.Melee:
                        equipIcon.sprite = meleeIcon;
                        break;
                }
            }
        }
        
        _animatingSlots.Clear();
        
        _selectedSlot = null;
    }

    private void Update()
    {
        GetInput();
        
        if (SlotsAnimating())
        {
            foreach (EquipWheelSlot slot in wheelSlots)
            {
                if (_animatingSlots.Contains(slot.slotTransform))
                {
                    if (slot == _selectedSlot)
                    {
                        ChangeSlotSize(slot, selectedScale, slot.activeTimeLeft, changeDuration);
                    }
                    else
                    {
                        ChangeSlotSize(slot, normalScale, slot.activeTimeLeft, changeDuration);
                    }
                }
            }
        }
    }
    
    private void GetInput()
    {
        if (EquipWheelAction == null) return;
        
        Vector2 input = EquipWheelAction.ReadValue<Vector2>();
        
        if (input == Vector2.zero) return;
        
        //want an angle from 0 to 360 where angle is determined in clockwise distance from Vector2.up to input
        float angle = Vector2.SignedAngle(Vector2.up, input);
        
        if (angle < 0) angle += 360;
        
        angle = 360 - angle;
        
        foreach (EquipWheelSlot slot in wheelSlots)
        {
            float minAngle = slot.slotAngleRange.x;
            
            float maxAngle = slot.slotAngleRange.y;
            
            if (maxAngle > minAngle) // logic differs based on which direction the range goes
            {
                if (angle >= minAngle && angle <= maxAngle)
                {
                    if (_selectedSlot != null)
                    {
                        if (slot == _selectedSlot) return;
                        
                        DeselectSlot(_selectedSlot, normalImage, equippedImage);
                        
                        SelectSlot(slot, selectedImage);
                    }
                    else
                    {
                        SelectSlot(slot, selectedImage);
                    }
                }
            }
            else if (maxAngle < minAngle)
            {
                if (angle >= minAngle || angle <= maxAngle)
                {
                    if (_selectedSlot != null)
                    {
                        if (slot == _selectedSlot) return;
                        
                        DeselectSlot(_selectedSlot, normalImage, equippedImage);
                        
                        SelectSlot(slot, selectedImage);
                    }
                    else
                    {
                        SelectSlot(slot, selectedImage);
                    }
                }
            }
        }
    }

    private void SelectSlot(EquipWheelSlot slot, Sprite selectedImage)
    {
        _animatingSlots.Add(slot.slotTransform);
        
        _selectedSlot = slot;
        
        slot.activeTimeLeft = changeDuration;
        
        slot.slotImage.sprite = selectedImage; // Set to selected color
    }
    
    private void DeselectSlot(EquipWheelSlot slot, Sprite normalImage, Sprite equippedImage)
    {
        _selectedSlot = null;
        
        _animatingSlots.Add(slot.slotTransform);
        
        slot.activeTimeLeft = changeDuration;
        
        if (slot.slotEquipClass != PlayerEquipabbles.S.CurrentPrimaryEquippable.EquippableClass)
        {
            slot.slotImage.sprite = normalImage;
        } 
        else
        {
            slot.slotImage.sprite = equippedImage;
        }
    }

    //will be called on update for wheelSlots in need of scaling
    private void ChangeSlotSize(EquipWheelSlot slot, float endScale, float timeLeft, float duration)
    {
        Vector3 currentScale = slot.slotTransform.localScale;
        
        slot.slotTransform.localScale = 
            Vector3.Lerp(currentScale, new Vector3(endScale, endScale, endScale), timeLeft / duration);
        
        slot.activeTimeLeft -= Time.unscaledDeltaTime;
        
        if (slot.activeTimeLeft <= 0)
        {
            slot.activeTimeLeft = 0;
            
            _animatingSlots.Remove(slot.slotTransform);
        }
    }
    
    private bool SlotsAnimating()
    {
        return _animatingSlots.Count > 0;
    }
}

[Serializable]
public class EquipWheelSlot
{
    public Transform slotTransform;
    
    public Image slotImage;
    
    [Tooltip("The minimum and maximum degrees of selection starting with 0 degrees being upward, and 360 meeting" +
             " back at 0.")] //For example, 315 to 45 would be the top quarter of the wheel.
    public Vector2 slotAngleRange;
    
    public EEquippableClass slotEquipClass;
    
    [HideInInspector] public float activeTimeLeft;
    
    public void Reset(Vector3 resetScale)
    {
        slotTransform.localScale = resetScale;
        
        activeTimeLeft = 0;
    }
}
