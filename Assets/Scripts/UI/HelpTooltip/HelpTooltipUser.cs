using UnityEngine;

public class HelpTooltipUser : MonoBehaviour
{
    public string tooltipText;
    private Collider2D userCollider;

    private void Awake()
    {
        userCollider = GetComponent<Collider2D>();
    }
    public void OnMouseEnter()
    {
        Vector3 tooltipPosition = userCollider.bounds.center + 
            new Vector3(userCollider.bounds.extents.x, -userCollider.bounds.extents.y, 0);

        HelpTooltip.Instance.Show(tooltipPosition, tooltipText);
    }

    public void OnMouseExit()
    {
        HelpTooltip.Instance.Hide();
    }
}
