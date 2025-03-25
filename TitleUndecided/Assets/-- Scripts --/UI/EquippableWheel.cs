using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquippableWheel : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Here so Unity serlializes the field, but not used in code. Click away.")]
    [SerializeField] private bool placeholder;
    
    [field: SerializeField] public Image GrappleSelect { get; private set; }
    
    [field: SerializeField] public Image CombatDiscSelect { get; private set; }
    
    [field: SerializeField] public Image UtilDiscSelect { get; private set; }
    
    [field: SerializeField] public Image MeleeSelect { get; private set; }
    
    
    [Header("Selection Visual Settings")]
    
    [SerializeField] private float normalScale;
    
    [SerializeField] private float selectedScale;
    
    [SerializeField] private float changeDuration;
    
    [SerializeField] private Color selectedColor = Color.red;
    
    [Header("Equipped Visual Settings")]
    
    [SerializeField] private Color normalColor = Color.white;
    
    [SerializeField] private Color equippedColor = Color.green;
    
    [Header("Wheel Slots")]
    
    [SerializeField] private List<EquipWheelSlot> wheelSlots;
    
    [Header("Dynamic")]
    
    [Tooltip("Also here to serialize")]
    [SerializeField] private bool placeholder2;
    
    [field: SerializeField] public InputAction EquipWheelAction { get; set; }

    [field: SerializeField] public Equippables CurrentEquippable { get; set; } = Equippables.None;
    
    private List<Transform> _changingSlots;

    private void OnEnable()
    {
        if (wheelSlots != null && wheelSlots.Count <= 0)
        {
            Debug.LogError("No wheel slots assigned to the wheel, disabling wheel.");
            
            enabled = false;
            
            return;
        }
        
        _changingSlots = new List<Transform>();
    }

    private void OnDisable()
    {
        Vector3 resetScale = new Vector3(normalScale, normalScale, normalScale);
        
        foreach (EquipWheelSlot slot in wheelSlots) // Resetting slots
        {
            slot.Reset(resetScale);

            if (SelectedSlot != null )
            {
                if (slot == SelectedSlot)
                {
                    CorrespondingImage(slot.slotEquipType).color = equippedColor; // Set selected slot to equipped color
                    
                    CurrentEquippable = slot.slotEquipType; // Set current equippable to selected slot
                }
                else
                {
                    CorrespondingImage(slot.slotEquipType).color = normalColor; // Set other slots to normal color
                }
            }
        }
        
        _changingSlots.Clear();
        
        SelectedSlot = null;
    }

    private void Update()
    {
        GetInput();
        
        if (SlotsActive())
        {
            foreach (EquipWheelSlot slot in wheelSlots)
            {
                if (_changingSlots.Contains(slot.slotTransform))
                {
                    if (slot == SelectedSlot)
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
                    if (SelectedSlot != null)
                    {
                        if (slot == SelectedSlot) return;
                        
                        DeselectSlot(SelectedSlot);
                        
                        SelectSlot(slot);
                    }
                    else
                    {
                        SelectSlot(slot);
                    }
                }
            }
            else if (maxAngle < minAngle)
            {
                if (angle >= minAngle || angle <= maxAngle)
                {
                    if (SelectedSlot != null)
                    {
                        if (slot == SelectedSlot) return;
                        
                        DeselectSlot(SelectedSlot);
                        
                        SelectSlot(slot);
                    }
                    else
                    {
                        SelectSlot(slot);
                    }
                }
            }
        }
    }

    private void SelectSlot(EquipWheelSlot slot)
    {
        _changingSlots.Add(slot.slotTransform);
        
        SelectedSlot = slot;
        
        slot.activeTimeLeft = changeDuration;
        
        CorrespondingImage(slot.slotEquipType).color = selectedColor;
    }
    
    private void DeselectSlot(EquipWheelSlot slot)
    {
        SelectedSlot = null;
        
        _changingSlots.Add(slot.slotTransform);
        
        slot.activeTimeLeft = changeDuration;
        
        if (slot.slotEquipType != CurrentEquippable)
        {
            CorrespondingImage(slot.slotEquipType).color = normalColor; // Set to normal color if not equipped
        } 
        else
        {
            CorrespondingImage(slot.slotEquipType).color = equippedColor; // Keep equipped color if it is
        }
    }
    
    private Image CorrespondingImage(Equippables type)
    {
        switch (type)
        {
            case Equippables.Grapple:
                
                return GrappleSelect;
            
            case Equippables.Disk:
                
                return CombatDiscSelect;
            
            case Equippables.TeleportDisk:
            case Equippables.StickyDisk:
                
                return UtilDiscSelect;
            
            case Equippables.Melee:
                
                return MeleeSelect;
            
            default:
                return null;
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
            
            _changingSlots.Remove(slot.slotTransform);
        }
    }
    
    private bool SlotsActive()
    {
        return _changingSlots.Count > 0;
    }
    
    private EquipWheelSlot SelectedSlot { get; set; }
}

[Serializable]
public class EquipWheelSlot
{
    public Transform slotTransform;
    
    [Tooltip("The minimum and maximum degrees of selection starting with 0 degrees being upward, and 360 meeting" +
             " back at 0.")] //For example, 315 to 45 would be the top quarter of the wheel.
    public Vector2 slotAngleRange;
    
    public Equippables slotEquipType;
    
    [HideInInspector] public float activeTimeLeft;
    
    public void Reset(Vector3 resetScale)
    {
        slotTransform.localScale = resetScale;
        
        activeTimeLeft = 0;
    }
}
