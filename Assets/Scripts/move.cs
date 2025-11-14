using UnityEngine;

public class PlayerMovementCC : MonoBehaviour
{
    public float velocidad = 5f;
    private CharacterController controller;
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(horizontal, 0, vertical);
        if (dir.magnitude > 0.1f)
        {
            controller.Move(dir.normalized * velocidad * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.15f);
        }

        animator.SetFloat("xspeed", horizontal);
        animator.SetFloat("yspeed", vertical);
    }
}
