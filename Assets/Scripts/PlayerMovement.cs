using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 move;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        anim.SetFloat("MoveX", move.x);
        anim.SetFloat("MoveY", move.y);
        anim.SetBool("IsMoving", move != Vector2.zero);
    }

    void FixedUpdate()
    {
        rb.velocity = move.normalized * speed;
    }
}