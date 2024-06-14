using UnityEngine;

public class ShiftObject : MonoBehaviour
{
    public ShiftData sData;

    public bool canBeSelected;

    public bool isSelected;

    Material sMaterial;

    PlayerMovement pMovement;
    Transform pTransform;

    bool isHovering = false;

    void Start()
    {
        pMovement = FindObjectOfType<PlayerMovement>();
        pTransform = pMovement.transform;

        InitializeObject(GetComponent<MeshRenderer>());
    }

    void InitializeObject(MeshRenderer renderer)
    {
        renderer.material = sData.entityMaterial;
        sMaterial = renderer.material;

        gameObject.name = sData.name;
    }

    void Update()
    {
        if (isHovering)
        {
            if (Vector3.Distance(transform.position, pTransform.position) < sData.shiftDistance)
            {
                if (!isSelected)
                {
                    Select();
                }
            }
            else
            {
                if (isSelected)
                {
                    Unselect();
                }
            }
        }
    }
    
    void OnMouseEnter()
    {
        if (canBeSelected) { isHovering = true; }
    }
    
    void OnMouseExit()
    {
        Unselect();
        isHovering = false;
    }


    void Select()
    {
        isSelected = true;

        pMovement.selectedObject = sData;

        sMaterial.color = Color.red;
    }

    void Unselect()
    {
        isSelected = false;

        pMovement.selectedObject = null;

        sMaterial.color = Color.white;
    }
}