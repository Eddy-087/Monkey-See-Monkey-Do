using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void SetSpeed(float speed)
    {
        anim.SetFloat("Speed", speed);
    }
}
