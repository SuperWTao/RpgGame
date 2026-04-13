using System.Collections;
using UnityEngine;
using Cinemachine;

public sealed class BattleOpeningDirector : MonoBehaviour
{
    private PlayerController playerController;
    private GameObject _spawnedEnemy;

    [Header("References")]
    [SerializeField] private CinemachineVirtualCameraBase playerVcam;
    [SerializeField] private CinemachineVirtualCamera openingVcam;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Enemy Spawn")]
    [SerializeField] private string enemySpawnTrigger = "Spawn";
    [SerializeField] private string enemyLookAtChildName = "pos";
    [SerializeField] private float enemySpawnDuration = 14f;

    [Header("Camera Shot")]
    [SerializeField] private int playerVcamPriority = 10;
    [SerializeField] private int openingVcamPriority = 30;
    [SerializeField] private float returnToPlayerBlendDuration = 1.5f;

    [Header("Flow")]
    [SerializeField] private bool playOnStart = true;

    private bool _isPlaying;

    private void Start()
    {
        if (playOnStart)
        {
            PlayOpening();
        }
    }

    public void PlayOpening()
    {
        if (_isPlaying) return;
        StartCoroutine(CoPlayOpening());
    }

    private IEnumerator CoPlayOpening()
    {
        _isPlaying = true;
        playerController = FindObjectOfType<PlayerController>();

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[BattleOpening] No camera found.");
            _isPlaying = false;
            yield break;
        }

        if (playerVcam == null || openingVcam == null)
        {
            Debug.LogError("[BattleOpening] Please assign playerVcam and openingVcam.");
            _isPlaying = false;
            yield break;
        }

        CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();

        playerController.SetControlLocked(true);

        if (enemyPrefab == null)
        {
            Debug.LogError("[BattleOpening] Enemy prefab is not assigned.");
            playerController.SetControlLocked(false);
            _isPlaying = false;
            yield break;
        }

        Vector3 spawnPos = enemySpawnPoint != null ? enemySpawnPoint.position : transform.position;
        Quaternion spawnRot = enemySpawnPoint != null ? enemySpawnPoint.rotation : Quaternion.identity;
        _spawnedEnemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        Transform lookTarget = _spawnedEnemy.transform;
        if (!string.IsNullOrEmpty(enemyLookAtChildName))
        {
            Transform child = _spawnedEnemy.transform.Find(enemyLookAtChildName);
            if (child != null)
            {
                lookTarget = child;
            }
            else
            {
                Debug.LogWarning("[BattleOpening] LookAt child not found: " + enemyLookAtChildName + ", fallback to enemy root.");
            }
        }

        openingVcam.LookAt = lookTarget;
        openingVcam.Follow = null;

        // 确保开场镜头优先生效。
        openingVcam.gameObject.SetActive(true);
        openingVcam.Priority = openingVcamPriority;
        playerVcam.Priority = playerVcamPriority;

        BattleEnemyActor enemyActor = _spawnedEnemy.GetComponent<BattleEnemyActor>();
        if (enemyActor != null)
        {
            enemyActor.enabled = false;
        }

        Animator enemyAnimator = _spawnedEnemy.GetComponentInChildren<Animator>();
        if (enemyAnimator != null && !string.IsNullOrEmpty(enemySpawnTrigger))
        {
            enemyAnimator.SetTrigger(enemySpawnTrigger);
        }

        // 你已在场景中设好开场敌人机位，这里只等待开场动画时长。
        if (enemySpawnDuration > 0f)
        {
            yield return new WaitForSeconds(enemySpawnDuration);
        }

        // 开场结束：隐藏敌人机位并平滑切回玩家机位。
        SetBlendTime(brain, returnToPlayerBlendDuration);
        playerVcam.Priority = openingVcamPriority;
        openingVcam.Priority = playerVcamPriority;
        openingVcam.gameObject.SetActive(false);

        if (returnToPlayerBlendDuration > 0f)
        {
            yield return new WaitForSeconds(returnToPlayerBlendDuration);
        }

        if (enemyActor != null)
        {
            enemyActor.enabled = true;
        }

        playerController.SetControlLocked(false);

        _isPlaying = false;
    }

    private static void SetBlendTime(CinemachineBrain brain, float duration)
    {
        CinemachineBlendDefinition blend = brain.m_DefaultBlend;
        blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        blend.m_Time = Mathf.Max(0f, duration);
        brain.m_DefaultBlend = blend;
    }
}
