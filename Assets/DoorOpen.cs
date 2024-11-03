using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Animator left;
    public Animator right;

    public bool closed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (closed)
            {
                left.SetTrigger("open");
                right.SetTrigger("open");
            } else
            {
                left.SetTrigger("close");
                right.SetTrigger("close");
            }
            
        }
    }
}
