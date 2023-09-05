using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using UnityEngine;

class GameManager : MonoBehaviour
{
    List<iGameStateListener> listeners = new List<iGameStateListener>();

    bool isPaused = false;

    [SerializeField] CameraRig playerCamera;

    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] GameObject playerPrefab;
    Player player;
    
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] List<Transform> enemySpawnPoints = new List<Transform>();
    
    List<Enemy> spawnedEnemies = new List<Enemy>();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        OnGameStart += SpawnPlayer;
        OnGameStart += SpawnFirstEnemeyForEachSpawner;
        OnGameStart += StartRoundCountdownTimer;





    }


    void Start()
    {
        OnGameStart?.Invoke();
        SignalGameStarted();

    }
    void SpawnPlayer()
    {
        var go = SpawnPrefabAt(playerPrefab, playerSpawnPoint);
        player = go.GetComponent<Player>();

        playerCamera.StartFolow(player.CameraFollowTarget);
        
        var weaponHandler = player.GetComponent<WeaponHandler>();
        weaponHandler.WeaponParent = playerCamera.GetComponentInChildren<PlayerWeaponParent>().transform;

        var playerCameraController = playerCamera.GetComponentInChildren<PlayerCameraController>();
        playerCameraController.orientation = player.GetComponent<PlayerLocomotion>().orientation;

        // link players camera CameraController To Player Input
        var dispatcher = player.GetComponent<PlayerInputDispatcher>();
        dispatcher.OnLookInputRecieved += playerCameraController.OnLookInputRecieved;

    }

    void SpawnFirstEnemeyForEachSpawner() 
    {
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            var go = SpawnPrefabAt(enemyPrefab, enemySpawnPoints[i]);

            var enemy = go.GetComponent<Enemy>();
            enemy.Target = player.transform;

            //var locomotion = enemy.GetComponent<EnemyLocomotion>();
            //locomotion.SetStateFollowTarget(player.transform);

            spawnedEnemies.Add(enemy);
        }
    }

    void StartRoundCountdownTimer()
    {
        StartCoroutine(CountdownTimer(1.0f));
    }

    IEnumerator CountdownTimer(float duration)
    {
        for(float i  = 0; i < duration; i+= Time.deltaTime)
        {

            yield return null;
        }

        SignalGameStarted();
    }
    
    public Action OnGameStart;

    GameObject SpawnPrefabAt(GameObject prefab, Transform spawnPoint)
    {
        var go = Instantiate(prefab);
        go.transform.position = spawnPoint.transform.position;
        go.transform.rotation = spawnPoint.transform.rotation;
        return go;
    }


    

    public static void SignalGameStarted()
    {
        instance.listeners.ForEach(l => l?.HangleGameStarted());
    }

    public static void SignalGameOver()
    {
        instance.listeners.ForEach(l => l?.HandleGameOver());
    }

    public static void SignalGamePaused()
    {
        Time.timeScale = 0.0f;
        instance.listeners.ForEach(l => l?.HandleGamePaused());
    }


    internal static void AddGameStateListener(iGameStateListener listener)
    {
        if (instance.listeners.Contains(listener))
            return;

        instance.listeners.Add(listener);
    }

    public static void RemoveEnemyFromActvePool(Enemy enemy)
    {
        instance.spawnedEnemies.Remove(enemy);
    }
    public static void SignalGameUnaused()
    {
        Time.timeScale = 1.0f;
        instance.listeners.ForEach(l => l?.HandleGameUnpaused());
    }

    public static void SignalGameQuit()
    {
        Time.timeScale = 1.0f;
        instance.listeners.ForEach(l => l?.HandleGameUnpaused());
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    static GameManager instance;

}
