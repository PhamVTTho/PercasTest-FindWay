using UnityEngine;
using UnityEngine.UI;

public class FloorControl : MonoBehaviour
{
    public Image img;
    public Text txtPosArray;
    public Color colorDefault;
    public Color colorWall;
    public Color colorNPC;
    public Color colorTarget;
    public Color colorPath;

    public void SetPosFloor(int x, int y)
    {
        txtPosArray.text = x + "," + y;
    }

    public void SetTypeFloor(ETypeFloor eTypeFloor)
    {
        switch (eTypeFloor)
        {
            case ETypeFloor.Default:
                img.color = colorDefault;
                break;
            case ETypeFloor.Wall:
                img.color = colorWall;
                break;
            case ETypeFloor.NPC:
                img.color = colorNPC;
                break;
            case ETypeFloor.Target:
                img.color = colorTarget;
                break;
            case ETypeFloor.Path:
                img.color = colorPath;
                break;
        }
    }

}
public enum ETypeFloor
{
    Default,
    Wall,
    NPC,
    Target,
    Path
}