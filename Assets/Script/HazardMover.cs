using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire de pièges unifié (projet 2D) :
///   ── Anciennes mécaniques ──
///   • UpDown, LeftRight, Rotation, CircularOrbit, PingPongDiag, Pendulum
///   ── Nouvelles mécaniques ──
///   • JumpPad   – zone qui propulse le joueur vers le haut
///   • BouleTrap – zone qui fait apparaître une boule et la lance vers le joueur
///   • SmashTrap – piège qui tombe violemment sur le joueur puis remonte
/// </summary>
public class HazardManager : MonoBehaviour
{
    // ═══════════════════════════════════════════════
    //  ANCIENNE MÉCANIQUE – OBJETS ANIMÉS
    // ═══════════════════════════════════════════════

    [System.Serializable]
    public class Hazard
    {
        [Header("Objet")]
        public GameObject target;

        [Header("Type de mouvement")]
        public MovementType movementType = MovementType.UpDown;

        [Header("Paramètres")]
        public float speed = 3f;
        public float amplitude = 2f;
        public float phase = 0f;

        [Header("Rotation")]
        public float rotationSpeed = 90f;

        [Header("Orbite")]
        public Transform orbitCenter;
        public float orbitRadius = 3f;

        [Header("Pendule")]
        public float pendulumMaxAngle = 45f;

        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public Quaternion startRotation;
        [HideInInspector] public float timer;
        [HideInInspector] public float orbitAngle;
    }

    public enum MovementType
    {
        UpDown,
        LeftRight,
        Rotation,
        CircularOrbit,
        PingPongDiag,
        Pendulum
    }

    [Header("═ Objets animés (anciennes mécaniques) ═")]
    [SerializeField] List<Hazard> hazards = new List<Hazard>();

    // ═══════════════════════════════════════════════
    //  NOUVELLE MÉCANIQUE 1 – JUMP PAD
    // ═══════════════════════════════════════════════

    [System.Serializable]
    public class JumpPad
    {
        [Header("Zone de déclenchement (Collider2D Trigger requis)")]
        public GameObject zone;

        [Header("Paramètres de saut")]
        public float jumpForce = 25f;
        public bool resetYVelocity = true;

        [Header("Visuel (optionnel)")]
        public Animator zoneAnimator;
        public string animTriggerName = "Jump";

        [HideInInspector] public JumpPadTrigger2D triggerComponent;
    }

    [Header("═ Jump Pads ═")]
    [SerializeField] List<JumpPad> jumpPads = new List<JumpPad>();

    // ═══════════════════════════════════════════════
    //  NOUVELLE MÉCANIQUE 2 – BOULE TRAP
    // ═══════════════════════════════════════════════

    [System.Serializable]
    public class BouleTrap
    {
        [Header("Boule")]
        public GameObject boule;
        public float rollSpeed = 8f;
        public float resetDelay = 5f;

        [Header("Zone de déclenchement (Collider2D Trigger requis)")]
        public GameObject triggerZone;

        [Header("Point de départ de la boule (optionnel)")]
        public Transform spawnPoint;

        [HideInInspector] public BouleTrigger2D bouleTriggerComponent;
        [HideInInspector] public Vector3 bouleOrigin;
        [HideInInspector] public bool isRolling;
    }

    [Header("═ Boule Traps ═")]
    [SerializeField] List<BouleTrap> boulTraps = new List<BouleTrap>();

    // ═══════════════════════════════════════════════
    //  NOUVELLE MÉCANIQUE 3 – SMASH TRAP
    // ═══════════════════════════════════════════════

    [System.Serializable]
    public class SmashTrap
    {
        [Header("Piège (masse, hache, pique...)")]
        public GameObject smashObject;

        [Header("Zone de déclenchement (Collider2D Trigger requis)")]
        public GameObject triggerZone;

        [Header("Paramètres de chute")]
        [Tooltip("Distance de chute vers le bas (en unités Unity)")]
        public float smashDistance = 5f;
        [Tooltip("Vitesse de chute vers le bas")]
        public float fallSpeed = 20f;
        [Tooltip("Vitesse de remontée vers la position de départ")]
        public float returnSpeed = 5f;
        [Tooltip("Temps d'attente en bas avant de remonter (secondes)")]
        public float pauseAtBottom = 0.2f;
        [Tooltip("Délai avant de pouvoir se redéclencher après un cycle complet (secondes)")]
        public float cooldown = 2f;

        // ── Runtime ──
        [HideInInspector] public SmashTrigger2D triggerComponent;
        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public Vector3 targetPosition;
        [HideInInspector] public SmashState state;
        [HideInInspector] public float pauseTimer;
        [HideInInspector] public float cooldownTimer;
    }

    public enum SmashState { Idle, Falling, PausingAtBottom, Returning }

    [Header("═ Smash Traps ═")]
    [SerializeField] List<SmashTrap> smashTraps = new List<SmashTrap>();

    const string PLAYER_TAG = "Player";

    // ═══════════════════════════════════════════════
    //  INIT
    // ═══════════════════════════════════════════════

    void Awake()
    {
        // ── Anciennes mécaniques ──
        foreach (Hazard h in hazards)
        {
            if (h.target == null) continue;
            h.startPosition = h.target.transform.position;
            h.startRotation = h.target.transform.rotation;
            h.timer = h.phase * Mathf.PI * 2f;
            h.orbitAngle = 0f;
        }

        // ── Jump Pads ──
        foreach (JumpPad jp in jumpPads)
        {
            if (jp.zone == null) continue;
            EnsureTriggerCollider2D(jp.zone);
            jp.triggerComponent = jp.zone.GetComponent<JumpPadTrigger2D>();
            if (jp.triggerComponent == null)
                jp.triggerComponent = jp.zone.AddComponent<JumpPadTrigger2D>();
            jp.triggerComponent.Init(jp, this, PLAYER_TAG);
        }

        // ── Boule Traps ──
        foreach (BouleTrap bt in boulTraps)
        {
            if (bt.boule == null || bt.triggerZone == null) continue;
            bt.bouleOrigin = bt.spawnPoint != null
                ? bt.spawnPoint.position
                : bt.boule.transform.position;
            bt.boule.SetActive(false);
            EnsureTriggerCollider2D(bt.triggerZone);
            bt.bouleTriggerComponent = bt.triggerZone.GetComponent<BouleTrigger2D>();
            if (bt.bouleTriggerComponent == null)
                bt.bouleTriggerComponent = bt.triggerZone.AddComponent<BouleTrigger2D>();
            bt.bouleTriggerComponent.Init(bt, this, PLAYER_TAG);
        }

        // ── Smash Traps ──
        foreach (SmashTrap st in smashTraps)
        {
            if (st.smashObject == null || st.triggerZone == null) continue;
            st.startPosition = st.smashObject.transform.position;
            st.targetPosition = st.startPosition + Vector3.down * st.smashDistance;
            st.state = SmashState.Idle;
            st.cooldownTimer = 0f;

            EnsureTriggerCollider2D(st.triggerZone);
            st.triggerComponent = st.triggerZone.GetComponent<SmashTrigger2D>();
            if (st.triggerComponent == null)
                st.triggerComponent = st.triggerZone.AddComponent<SmashTrigger2D>();
            st.triggerComponent.Init(st, this, PLAYER_TAG);
        }
    }

    // ═══════════════════════════════════════════════
    //  UPDATE
    // ═══════════════════════════════════════════════

    void Update()
    {
        // ── Anciennes mécaniques ──
        foreach (Hazard h in hazards)
        {
            if (h.target == null) continue;
            h.timer += Time.deltaTime * h.speed;
            ProcessHazard(h);
        }

        // ── Smash Traps (gérés en Update pour le mouvement fluide) ──
        foreach (SmashTrap st in smashTraps)
        {
            if (st.smashObject == null) continue;

            // Décompte du cooldown
            if (st.cooldownTimer > 0f)
                st.cooldownTimer -= Time.deltaTime;

            ProcessSmash(st);
        }
    }

    void ProcessHazard(Hazard h)
    {
        Transform t = h.target.transform;

        switch (h.movementType)
        {
            case MovementType.UpDown:
                t.position = h.startPosition + new Vector3(0f, Mathf.Sin(h.timer) * h.amplitude, 0f);
                break;

            case MovementType.LeftRight:
                t.position = h.startPosition + new Vector3(Mathf.Sin(h.timer) * h.amplitude, 0f, 0f);
                break;

            case MovementType.Rotation:
                t.Rotate(0f, 0f, h.rotationSpeed * Time.deltaTime);
                break;

            case MovementType.CircularOrbit:
                h.orbitAngle += h.speed * Time.deltaTime;
                Vector3 center = h.orbitCenter != null ? h.orbitCenter.position : h.startPosition;
                t.position = center + new Vector3(
                    Mathf.Cos(h.orbitAngle) * h.orbitRadius,
                    Mathf.Sin(h.orbitAngle) * h.orbitRadius,
                    0f
                );
                break;

            case MovementType.PingPongDiag:
                float diag = Mathf.Sin(h.timer) * h.amplitude;
                t.position = h.startPosition + new Vector3(diag, diag, 0f);
                break;

            case MovementType.Pendulum:
                float angle = Mathf.Sin(h.timer) * h.pendulumMaxAngle;
                t.rotation = Quaternion.Euler(0f, 0f, angle);
                break;
        }
    }

    void ProcessSmash(SmashTrap st)
    {
        Transform t = st.smashObject.transform;

        switch (st.state)
        {
            case SmashState.Falling:
                // Chute rapide vers la position cible
                t.position = Vector3.MoveTowards(
                    t.position,
                    st.targetPosition,
                    st.fallSpeed * Time.deltaTime
                );
                // Arrivé en bas → pause
                if (Vector3.Distance(t.position, st.targetPosition) < 0.02f)
                {
                    t.position = st.targetPosition;
                    st.state = SmashState.PausingAtBottom;
                    st.pauseTimer = st.pauseAtBottom;
                }
                break;

            case SmashState.PausingAtBottom:
                // Courte pause avant de remonter
                st.pauseTimer -= Time.deltaTime;
                if (st.pauseTimer <= 0f)
                    st.state = SmashState.Returning;
                break;

            case SmashState.Returning:
                // Remontée lente vers la position de départ
                t.position = Vector3.MoveTowards(
                    t.position,
                    st.startPosition,
                    st.returnSpeed * Time.deltaTime
                );
                // Retour complet → Idle + cooldown
                if (Vector3.Distance(t.position, st.startPosition) < 0.02f)
                {
                    t.position = st.startPosition;
                    st.state = SmashState.Idle;
                    st.cooldownTimer = st.cooldown;
                }
                break;

                // SmashState.Idle : rien à faire, on attend le trigger
        }
    }

    // ═══════════════════════════════════════════════
    //  API PUBLIQUE – appelée par les helpers trigger
    // ═══════════════════════════════════════════════

    public void ActivateJumpPad(JumpPad jp, Collider2D playerCollider)
    {
        Rigidbody2D rb = playerCollider.attachedRigidbody;
        if (rb == null) return;

        if (jp.resetYVelocity)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(Vector2.up * jp.jumpForce, ForceMode2D.Impulse);

        if (jp.zoneAnimator != null && !string.IsNullOrEmpty(jp.animTriggerName))
            jp.zoneAnimator.SetTrigger(jp.animTriggerName);
    }

    public void ActivateBouleTrap(BouleTrap bt, Transform playerTransform)
    {
        if (bt.isRolling) return;

        bt.isRolling = true;
        bt.boule.transform.position = bt.bouleOrigin;
        bt.boule.SetActive(true);

        Vector2 dir = playerTransform.position - bt.boule.transform.position;
        dir.y = 0f;
        dir.Normalize();

        Rigidbody2D rb = bt.boule.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dir * bt.rollSpeed, ForceMode2D.Impulse);
        }

        StartCoroutine(ResetBoule(bt));
    }

    /// <summary>Déclenche la chute du SmashTrap si pas déjà actif et hors cooldown.</summary>
    public void ActivateSmashTrap(SmashTrap st)
    {
        if (st.state != SmashState.Idle) return;
        if (st.cooldownTimer > 0f) return;

        st.state = SmashState.Falling;
    }

    // ═══════════════════════════════════════════════
    //  COROUTINES
    // ═══════════════════════════════════════════════

    IEnumerator ResetBoule(BouleTrap bt)
    {
        yield return new WaitForSeconds(bt.resetDelay);
        bt.boule.SetActive(false);
        bt.boule.transform.position = bt.bouleOrigin;
        Rigidbody2D rb = bt.boule.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        bt.isRolling = false;
    }

    // ═══════════════════════════════════════════════
    //  UTILITAIRES
    // ═══════════════════════════════════════════════

    static void EnsureTriggerCollider2D(GameObject go)
    {
        Collider2D col = go.GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            Debug.LogWarning($"[HazardManager] Aucun Collider2D sur '{go.name}', BoxCollider2D Trigger ajouté automatiquement.");
        }
        else
        {
            col.isTrigger = true;
        }
    }

    // ═══════════════════════════════════════════════
    //  GIZMOS
    // ═══════════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        // ── Anciennes mécaniques ──
        foreach (Hazard h in hazards)
        {
            if (h.target == null) continue;
            Gizmos.color = Color.red;
            Vector3 origin = Application.isPlaying ? h.startPosition : h.target.transform.position;

            switch (h.movementType)
            {
                case MovementType.UpDown:
                    Gizmos.DrawLine(origin + Vector3.up * h.amplitude, origin + Vector3.down * h.amplitude);
                    break;
                case MovementType.LeftRight:
                    Gizmos.DrawLine(origin + Vector3.left * h.amplitude, origin + Vector3.right * h.amplitude);
                    break;
                case MovementType.CircularOrbit:
                    Vector3 c = h.orbitCenter != null ? h.orbitCenter.position : origin;
                    Gizmos.DrawWireSphere(c, h.orbitRadius);
                    break;
                case MovementType.PingPongDiag:
                    Gizmos.DrawLine(origin - new Vector3(h.amplitude, h.amplitude, 0f),
                                    origin + new Vector3(h.amplitude, h.amplitude, 0f));
                    break;
                case MovementType.Pendulum:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(origin, 0.2f);
                    break;
            }
        }

        // ── Jump Pads ──
        foreach (JumpPad jp in jumpPads)
        {
            if (jp.zone == null) continue;
            Gizmos.color = new Color(0f, 1f, 0.2f, 0.35f);
            Collider2D col = jp.zone.GetComponent<Collider2D>();
            if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.green;
            Vector3 p = jp.zone.transform.position;
            Gizmos.DrawLine(p, p + Vector3.up * 2f);
        }

        // ── Boule Traps ──
        foreach (BouleTrap bt in boulTraps)
        {
            if (bt.triggerZone != null)
            {
                Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
                Collider2D col = bt.triggerZone.GetComponent<Collider2D>();
                if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
            if (bt.boule != null)
            {
                Gizmos.color = Color.red;
                Vector3 origin = bt.spawnPoint != null
                    ? bt.spawnPoint.position
                    : bt.boule.transform.position;
                Gizmos.DrawWireSphere(origin, 0.5f);
            }
        }

        // ── Smash Traps ──
        foreach (SmashTrap st in smashTraps)
        {
            if (st.smashObject != null)
            {
                Vector3 origin = Application.isPlaying ? st.startPosition : st.smashObject.transform.position;
                Vector3 target = origin + Vector3.down * st.smashDistance;

                // Ligne de chute
                Gizmos.color = new Color(0.8f, 0f, 1f, 0.8f);
                Gizmos.DrawLine(origin, target);

                // Position de départ
                Gizmos.color = new Color(0.8f, 0f, 1f, 0.5f);
                Gizmos.DrawWireCube(origin, Vector3.one * 0.4f);

                // Position d'arrivée
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawWireCube(target, Vector3.one * 0.4f);

                // Flèche vers le bas
                Gizmos.color = new Color(0.8f, 0f, 1f, 0.9f);
                Vector3 mid = (origin + target) / 2f;
                Gizmos.DrawLine(mid, mid + Vector3.down * 0.3f + Vector3.left * 0.2f);
                Gizmos.DrawLine(mid, mid + Vector3.down * 0.3f + Vector3.right * 0.2f);
            }

            if (st.triggerZone != null)
            {
                Gizmos.color = new Color(0.8f, 0f, 1f, 0.2f);
                Collider2D col = st.triggerZone.GetComponent<Collider2D>();
                if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}

// ═══════════════════════════════════════════════════════
//  HELPERS 2D – composants internes ajoutés dynamiquement
// ═══════════════════════════════════════════════════════

public class JumpPadTrigger2D : MonoBehaviour
{
    HazardManager.JumpPad _jp;
    HazardManager _manager;
    string _playerTag;

    public void Init(HazardManager.JumpPad jp, HazardManager manager, string playerTag)
    {
        _jp = jp;
        _manager = manager;
        _playerTag = playerTag;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _manager?.ActivateJumpPad(_jp, other);
    }
}

public class BouleTrigger2D : MonoBehaviour
{
    HazardManager.BouleTrap _bt;
    HazardManager _manager;
    string _playerTag;

    public void Init(HazardManager.BouleTrap bt, HazardManager manager, string playerTag)
    {
        _bt = bt;
        _manager = manager;
        _playerTag = playerTag;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _manager?.ActivateBouleTrap(_bt, other.transform);
    }
}

public class SmashTrigger2D : MonoBehaviour
{
    HazardManager.SmashTrap _st;
    HazardManager _manager;
    string _playerTag;

    public void Init(HazardManager.SmashTrap st, HazardManager manager, string playerTag)
    {
        _st = st;
        _manager = manager;
        _playerTag = playerTag;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;
        _manager?.ActivateSmashTrap(_st);
    }
}