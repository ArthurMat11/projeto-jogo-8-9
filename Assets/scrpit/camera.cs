using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform playerTransform;
    private Vector3 playerPosition;
    void Start(){
        playerPosition = playerTransform.position;
        transform.position = new Vector3(playerPosition.x,
                                        playerPosition.y,
                                        transform.position.z);
    }

void Update()
{
    playerPosition = playerTransform.position;
    transform.position = new Vector3(playerPosition.x,
                                    playerPosition.y,
                                    transform.position.z);
}
}
