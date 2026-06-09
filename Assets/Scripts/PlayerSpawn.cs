using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {
        GameObject spawn =
            GameObject.Find(GameManager.spawnID);

        if (spawn != null)
        {
            transform.position = spawn.transform.position;
        }
    }
}