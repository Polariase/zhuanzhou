using UnityEngine;

public class ChantStart : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.chantStart = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.chantStart = false;
        }
    }
}