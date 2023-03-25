using UnityEngine;
using UnityEngine.AI;

public class SpawnEnemies : MonoBehaviour
{
    public GameObject prefab;
    public int spawnCount = 10;
    private int m_enemiesLeft = 0;
    public int enemiesLeft { get => m_enemiesLeft; }

    /// <summary>
    /// Should a new Enemy be spawned when one dies?
    /// </summary>
    public bool constantSpawn;
    public bool enemiesConstantPursuit;

    public Transform player;
    public PlayerInput playerPlayerInput; //Set each spawned enemy's reference with this!!!!!!!! (just like currently doing for player var.)
    public Vector2 animSpeedRange = Vector2.one;

    public Vector3 halfExtents = Vector3.one;
    public Vector3 origin;


    private void Start()
    {
        //Transform t = Instantiate(prefab).transform;
        //Enemy e;
        //NavMeshAgent agent;
        //Vector3 randPos = transform.position;

        //for(int i = 0; i < spawnCount; i++)
        //{
        //    t = Instantiate(prefab).transform;
        //    e = t.GetComponent<Enemy>();
        //    agent = t.GetComponent<NavMeshAgent>();
        //    randPos = transform.position;

        //    randPos.x += Random.Range(-halfExtents.x, halfExtents.x);
        //    randPos.y += Random.Range(-halfExtents.y, halfExtents.y);
        //    randPos.z += Random.Range(-halfExtents.z, halfExtents.z);

        //    //if(NavMesh.SamplePosition(randPos, out hit, halfExtents.y * 2f, NavMesh.AllAreas))
        //    //{
        //    //    randPos = hit.position;
        //    //}
        //    //else
        //    //{
        //    //    Debug.LogError("Couldn't find random position for NavMeshAgent (fix this part to keep trying again until found position).");
        //    //}

        //    if(agent)
        //    {
        //        agent.Warp(randPos);
        //    }

        //    if(e)
        //    {
        //        e.player = player;
        //        e.animSpeed = Random.Range(animSpeedRange.x, animSpeedRange.y); //Make each Enemy's Animator speed slightly different so they don't all move exactly the same (adds a little variance between Enemies)
        //        e.GetComponent<NavMeshAgent>().Warp(randPos);
        //    }
        //}

        //Spawn(spawnCount);
    }

    public void Spawn(int count, System.Action<GameObject> perSpawnedEnemyAction = null)
    {
        Transform t = Instantiate(prefab).transform;
        Enemy e;
        NavMeshAgent agent;
        Vector3 randPos = transform.position;

        for(int i = 0; i < count; i++)
        {
            t = Instantiate(prefab).transform;
            e = t.GetComponent<Enemy>();
            agent = t.GetComponent<NavMeshAgent>();

            if(agent)
            {
                bool warped;
                int tries = 15;
                do
                {
                    randPos = transform.position + origin;

                    randPos.x += Random.Range(-halfExtents.x, halfExtents.x);
                    randPos.y += Random.Range(-halfExtents.y, halfExtents.y);
                    randPos.z += Random.Range(-halfExtents.z, halfExtents.z);

                    warped = agent.Warp(randPos);
                    Debug.Log("Tried warp");
                    tries--;
                }
                while(!warped && tries >= 0);

            }

            if(e)
            {
                e.player = player;
                e.constantPursuit = enemiesConstantPursuit;
                e.notifyOnDie = gameObject;
                e.animSpeed = Random.Range(animSpeedRange.x, animSpeedRange.y); //Make each Enemy's Animator speed slightly different so they don't all move exactly the same (adds a little variance between Enemies)
                e.GetComponent<NavMeshAgent>().Warp(randPos);
            }

            perSpawnedEnemyAction?.Invoke(t.gameObject);

            m_enemiesLeft++;
        }
    }

    //Called by a spawned Enemy when it dies so that another Enemy can be spawned to replace it and keep the spawnCount constant
    public void NotifyOnDie()
    {
        if(constantSpawn)
        {
            Spawn(1);
        }

        m_enemiesLeft--;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + origin, halfExtents * 2f);
    }
}
