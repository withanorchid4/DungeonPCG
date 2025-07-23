using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class MonsterChase : MonoBehaviour
{
    public DungeonMap dungeonMap;
    
    public Animator anim;
    
    public Transform player;  // 拖入玩家对象
    public GameObject camera;
    public Camera mainCamera;
    
    private NavMeshAgent agent;
    
    [Header("瞬移设置")]
    [SerializeField] public float chasingConditionDist = 15f; //这一段距离内认为处于追击状态
    [SerializeField] public float minTeleportDist = 8f;  //大于此距离时才能触发传送
    [SerializeField] public float lastTeleportTime = -Mathf.Infinity;  //上次传送的时间
    [SerializeField] public float teleportInterval = 5f;  //传送间隔
    
    public bool isChasing = false;
    
    

    [Header("视觉检测")] [Tooltip("检测间隔时间(秒)")]
    public float checkInternal = 0.2f;

    [Tooltip("视觉检测距离(米)")] public float maxCheckDist = 50f;

    private float checkTimer; //上一次视觉检测的时间

    private int dist;

    private int awareness;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        foreach (Transform child in player)
        {
            if (child.gameObject.name == "Main Camera")
            {
                camera = child.gameObject;
                break;
            }
        }
        mainCamera = camera.GetComponent<Camera>();

        dist = Animator.StringToHash("DistFromPlayer");
        awareness = Animator.StringToHash("Awareness");
    }

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            agent.SetDestination(player.position);
            isChasing = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && anim != null)
        {
            anim.SetFloat(dist, 0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && anim != null)
        {
            anim.SetFloat(dist, 5);
        }
    }

    private void Update()
    {   
        
        //Debug.Log("在视野中？？" + IsMonsterInPlayerSight());
        float curDistFromPlayer = Vector3.Distance(transform.position, player.position);
        if (curDistFromPlayer < chasingConditionDist)
        {
            // isChasing = true;
            //追逐靠碰撞体检测来触发为true，然后靠距离远于chaseConditionDist来判断是否在追击状态
            //放置还没开始追击时，isChasing为true
        }
        else
        {
            isChasing = false;
        }
        
        if (isChasing && curDistFromPlayer > minTeleportDist)
        {
            float timeNow = Time.time;
            if (timeNow - lastTeleportTime > teleportInterval)//大于传送间隔，可以传送
            {
                if (timeNow - checkTimer > checkInternal) //大于检测间隔，可以检测
                {
                    checkTimer = timeNow;
                    if (!IsMonsterInPlayerSight())
                    {
                        lastTeleportTime = timeNow;
                        //执行传送
                        Debug.LogError("传送！！！！！！！！！！！！！");
                        //transform.position = player.position - mainCamera.transform.forward;
                        Vector3 teleportPos = GetTeleportPositionFromDungeon(player.position);
                        transform.position = teleportPos;
                        agent.Warp(teleportPos);
                        agent.SetDestination(teleportPos);
                        isChasing = false;
                    }
                    else
                    {
                        Debug.LogError("怪物仍然在玩家视野中");
                    }
                }
            }
        }
    }

    private bool IsMonsterInPlayerSight()
    {
        Vector3 playerToMonster = transform.position - camera.transform.position;
        
        //视锥检测
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(transform.position);
        bool inFrustum = screenPoint.x >= 0 && screenPoint.x <= Screen.width && screenPoint.y >= 0 && screenPoint.y <= Screen.height && screenPoint.z > 0;

        if (!inFrustum)
        {
            //Debug.LogError("怪物不在玩家视锥中");
            return false;
        }
        
        //角度检测
        float angle = Vector3.Angle(camera.transform.forward, playerToMonster.normalized);
        if (angle > mainCamera.fieldOfView / 2)
        {
            //Debug.LogError("怪物不在玩家视野中");
            return false;
        }
        
        //射线检测
        if (Physics.Raycast(camera.transform.position, playerToMonster.normalized, out var hit, maxCheckDist))
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }
        Debug.LogError("怪物不被玩家视线打中");
        return false;
        
    }

    private Vector3 GetTeleportPositionFromDungeon(Vector3 playerPosition)
    {
        if (dungeonMap == null)
        {
            Debug.Log("地牢地图为空，找不到传送点，传到身后");
            return playerPosition - mainCamera.transform.forward;
        }
        Debug.Log("获取到地牢地图");
        var genPos = dungeonMap.GetRandomPointInRoom(playerPosition);

        return genPos;
    }

}