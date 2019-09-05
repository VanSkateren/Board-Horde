using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    public string axisKey = "New Ability";
    public KeyCode abilityButton;
    
    public abstract void CheckInput();
    public abstract void DoAbility();

}
