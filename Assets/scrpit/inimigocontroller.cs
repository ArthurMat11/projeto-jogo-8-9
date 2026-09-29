using UnityEngine;

public class InimigoController : MonoBehaviour
{
    public Transform position1;
    public Transform position2;
    public float velocity;

    private bool seguindoPos1 = true;
    private Rigidbody2D rb;
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (seguindoPos1)
        {
            rb.linearVelocity = new Vector2(-velocity, rb.linearVelocity.y);

            if (transform.position.x <= position1.position.x)
            {
                seguindoPos1 = false;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(velocity, rb.linearVelocity.y);

            if (transform.position.x >= position2.position.x)
            {
                seguindoPos1 = true;
            }
        }
    }
}