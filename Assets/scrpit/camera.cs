using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform playerTransform;
    private Vector3 playerPosition;
    void Start(){
        if(playerTransform!=null){
        playerPosition = playerTransform.position;
        transform.position = new Vector3(playerPosition.x,
                                        playerPosition.y,
                                        transform.position.z);
    }
    }
void Update()
{
    if(playerTransform!=null){
playerPosition = playerTransform.position;
    transform.position = new Vector3(playerPosition.x,
                                    playerPosition.y,
                                    transform.position.z);
    }
    
}
}
