using UnityEngine;

public class ChantEnd : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.chantRecovery = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.chantRecovery = false;
        }
    }
}