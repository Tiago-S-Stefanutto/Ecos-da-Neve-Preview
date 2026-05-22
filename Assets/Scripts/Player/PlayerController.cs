using UnityEngine;
using System.Collections;

// ================================================================
//  CELESTE PLAYER CONTROLLER v3
//
//  ESTRUTURA DO PREFAB ESPERADA:
//  ┌─ Player (este script + Rigidbody2D + CapsuleCollider2D)
//  │   ├─ Skin (SpriteRenderer + Animator)
//  │   └─ GroundCheck (Transform vazio, posicione nos pés)
//
//  CONTROLES:
//   A / D  ou  ← →   → Mover
//   Espaço           → Pular
//   Shift            → Dash (8 direções com diagonais)
//   A/D na parede    → Agarrar (segure na direção da parede)
//   W / S            → Subir / Descer enquanto agarrado
//
//  PARÂMETROS DO ANIMATOR:
//   SpeedX        Float  → velocidade horizontal absoluta
//   SpeedY        Float  → velocidade vertical (+ sobe, - cai)
//   IsGrounded    Bool   → está no chão
//   IsDashing     Bool   → está em dash
//   IsWallGrab    Bool   → agarrado na parede
//   IsWallSlide   Bool   → deslizando sem stamina
//   IsWallDrop    Bool   → caindo rápido após soltar parede
//   IsWallJumping Bool   → durante o lock do wall jump
// ================================================================

[RequireComponent(typeof(Rigidbody2D))]
public class CelestePlayerController : MonoBehaviour
{
    // ────────────────────────────────────────────────────────
    //  REFERÊNCIAS
    // ────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;
    private Transform skin;   // filho com o SpriteRenderer
    private JuiceEffects juice;
    private GhostEffect ghost;

    [Header("Referências")]
    [Tooltip("Transform filho vazio, posicionado nos pés do personagem")]
    public Transform groundCheck;

    // ────────────────────────────────────────────────────────
    //  CHÃO
    // ────────────────────────────────────────────────────────
    [Header("Chão")]
    public float groundRadius = 0.12f;
    public LayerMask groundLayer;

    private bool isGrounded;

    // ────────────────────────────────────────────────────────
    //  MOVIMENTO HORIZONTAL
    // ────────────────────────────────────────────────────────
    [Header("Movimento")]
    [Tooltip("Velocidade máxima horizontal")]
    public float maxSpeed = 9f;
    [Tooltip("Aceleração no chão")]
    public float groundAccel = 50f;
    [Tooltip("Frenagem no chão")]
    public float groundDecel = 50f;
    [Tooltip("Aceleração no ar")]
    public float airAccel = 30f;
    [Tooltip("Frenagem no ar")]
    public float airDecel = 20f;

    // ────────────────────────────────────────────────────────
    //  PULO
    // ────────────────────────────────────────────────────────
    [Header("Pulo")]
    public float jumpForce = 18f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public float jumpCutMultiplier = 0.4f;

    // ────────────────────────────────────────────────────────
    //  GRAVIDADE
    // ────────────────────────────────────────────────────────
    [Header("Gravidade")]
    public float fallMultiplier = 3.5f;
    public float lowJumpMultiplier = 2.5f;
    public float maxFallSpeed = 22f;

    // gravityScale salva no Awake para restaurar após dash/grab
    private float defaultGravity;

    // ────────────────────────────────────────────────────────
    //  DASH
    //
    //  Como funciona o dash nesta versão:
    //   1. Shift apertado → dashRequested = true
    //   2. HandleDash() verifica canDash + !isDashing
    //   3. DashRoutine() roda em 3 fases:
    //      - Fase 1 (dashDuration) : move em dashSpeed, gravidade = 0
    //      - Fase 2 (dashEndPause) : pausa breve, velocity = 0 (feel do Celeste)
    //      - Fase 3 (dashCooldown) : espera antes de liberar próximo dash
    // ────────────────────────────────────────────────────────
    [Header("Dash")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.15f;
    [Tooltip("Micro-pausa no final do dash — dá o 'snap' característico do Celeste")]
    public float dashEndPause = 0.05f;
    [Tooltip("Cooldown após o dash terminar completamente")]
    public float dashCooldown = 0.1f;

    private bool isDashing;       // true durante toda a DashRoutine
    private bool canDash = true;  // false durante cooldown e no ar sem dash
    private bool dashRequested;   // flag: Shift pressionado NESTE frame
    private Vector2 dashDir;         // direção que foi dashada

    // ────────────────────────────────────────────────────────
    //  PAREDE
    // ────────────────────────────────────────────────────────
    [Header("Parede")]
    public float wallCheckDist = 0.4f;
    [Tooltip("Copie o valor de Size Y do seu CapsuleCollider2D")]
    public float wallColliderHeight = 1.5f;
    [Tooltip("Copie o valor de Offset Y do seu CapsuleCollider2D")]
    public float wallColliderOffsetY = -0.3f;
    public float wallClimbSpeed = 4f;
    public float wallHoldSpeed = 1.5f;
    public float wallSlideSpeed = 3f;
    public float wallDropAccel = 80f;

    public float maxStamina = 110f;
    public float staminaDrain = 45f;

    private bool isTouchingWallRight;
    private bool isTouchingWallLeft;
    private float wallDir;          // +1, -1 ou 0
    private bool isWallGrabbing;
    private bool isWallSliding;
    private bool isWallDropping;
    private float stamina;

    // ────────────────────────────────────────────────────────
    //  WALL JUMP
    // ────────────────────────────────────────────────────────
    [Header("Wall Jump")]
    public float wallJumpX = 10f;
    public float wallJumpY = 16f;
    public float wallJumpLockTime = 0.18f;

    private bool isWallJumping;
    private Coroutine wallJumpRoutine;

    // ────────────────────────────────────────────────────────
    //  ESTADO INTERNO
    // ────────────────────────────────────────────────────────
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpRequested;
    private bool jumpHeld;
    private float inputX;
    private float inputY;
    private float inputXSign;   // -1, 0 ou +1 sem imprecisão de float
    private bool facingRight = true;

    // ================================================================
    //  AWAKE — inicialização antes do primeiro frame
    // ================================================================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Busca Animator e SpriteRenderer no filho (objeto Skin)
        anim = GetComponentInChildren<Animator>();
        skin = anim != null ? anim.transform : transform;
        juice = GetComponent<JuiceEffects>();
        ghost = GetComponent<GhostEffect>();

        // Salva a gravidade original do Rigidbody configurada no Inspector
        defaultGravity = rb.gravityScale;

        // Começa como se estivesse no ar sem pulos disponíveis
        // → evita pulo grátis no primeiro frame ao spawnar
        coyoteCounter = -1f;
        stamina = maxStamina;
    }

    // ================================================================
    //  UPDATE — input e lógica de estado (executa 1x por frame)
    // ================================================================
    void Update()
    {
        ReadInput();
        CheckGround();
        CheckWalls();
        UpdateWallGrab();
        UpdateTimers();
        HandleJump();
        HandleDash();
        HandleGravity();
        FlipSprite();
        UpdateAnimator();
    }

    // ────────────────────────────────────────────────────────
    //  STEP 1 — LER INPUT
    // ────────────────────────────────────────────────────────
    void ReadInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        // Converte para sinal seguro: threshold de 0.1 evita ruído de gamepad
        inputXSign = inputX > 0.1f ? 1f : (inputX < -0.1f ? -1f : 0f);

        // Captura pulo — usa flag para não perder o input entre frames
        if (Input.GetButtonDown("Jump"))
            jumpRequested = true;

        // Aceita Shift esquerdo OU direito para dash
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            dashRequested = true;

        // Segurar jump = pulo mais alto
        jumpHeld = Input.GetButton("Jump");
    }

    // ────────────────────────────────────────────────────────
    //  STEP 2 — CHECAR CHÃO
    // ────────────────────────────────────────────────────────
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundRadius, groundLayer);

        if (isGrounded)
        {
            canDash = true;       // recarrega dash ao pousar
            stamina = maxStamina; // recarrega stamina ao pousar
            isWallDropping = false;
        }
    }

    // ────────────────────────────────────────────────────────
    //  STEP 3 — CHECAR PAREDES
    //
    //  3 raycasts por lado (cima/centro/baixo do collider)
    //  Resolve o problema quando transform.position não coincide
    //  com o centro real do collider por causa do Offset Y.
    //
    //  IMPORTANTE: ajuste wallColliderHeight e wallColliderOffsetY
    //  no Inspector para bater com seu CapsuleCollider2D:
    //   Size Y     → wallColliderHeight  (padrão 1.5)
    //   Offset Y   → wallColliderOffsetY (padrão -0.3)
    // ────────────────────────────────────────────────────────
    void CheckWalls()
    {
        // Centro real do collider levando em conta o offset
        float centerY = transform.position.y + wallColliderOffsetY;
        float halfH = wallColliderHeight * 0.4f;

        Vector2 originMid = new Vector2(transform.position.x, centerY);
        Vector2 originHigh = new Vector2(transform.position.x, centerY + halfH);
        Vector2 originLow = new Vector2(transform.position.x, centerY - halfH);

        // Detecta parede à direita em qualquer uma das 3 alturas
        isTouchingWallRight =
            Physics2D.Raycast(originMid, Vector2.right, wallCheckDist, groundLayer) ||
            Physics2D.Raycast(originHigh, Vector2.right, wallCheckDist, groundLayer) ||
            Physics2D.Raycast(originLow, Vector2.right, wallCheckDist, groundLayer);

        // Detecta parede à esquerda em qualquer uma das 3 alturas
        isTouchingWallLeft =
            Physics2D.Raycast(originMid, Vector2.left, wallCheckDist, groundLayer) ||
            Physics2D.Raycast(originHigh, Vector2.left, wallCheckDist, groundLayer) ||
            Physics2D.Raycast(originLow, Vector2.left, wallCheckDist, groundLayer);

        if (isTouchingWallRight) wallDir = 1f;
        else if (isTouchingWallLeft) wallDir = -1f;
        else wallDir = 0f;
    }

    // ────────────────────────────────────────────────────────
    //  STEP 4 — WALL GRAB / SLIDE / DROP
    //
    //  UNITY 6: rb.velocity foi removido — usar rb.linearVelocity
    //  Além disso, lemos a velocidade UMA vez no início para evitar
    //  dessincronização entre Update e FixedUpdate
    // ────────────────────────────────────────────────────────
    void UpdateWallGrab()
    {
        bool touchingWall = wallDir != 0f;

        // Lê a velocidade atual uma única vez para consistência
        // No Unity 6, rb.linearVelocity é a API correta
        float velY = rb.linearVelocity.y;

        // Pressiona em direção à parede?
        // Compara inputXSign (que é -1, 0 ou +1) com wallDir
        bool pressingToWall = touchingWall && !isGrounded
                              && inputXSign == wallDir;

        // GRAB: pressionando na direção da parede + tem stamina
        isWallGrabbing = pressingToWall && stamina > 0f;

        // SLIDE SUAVE: pressionando mas sem stamina → cai devagar
        isWallSliding = pressingToWall && stamina <= 0f && velY < 0f;

        // DROP RÁPIDO: encostado mas NÃO pressionando → cai rápido
        isWallDropping = touchingWall && !isGrounded
                         && !isWallGrabbing && !isWallSliding
                         && velY < 0f;

        if (isWallGrabbing)
        {
            stamina -= staminaDrain * Time.deltaTime;
            stamina = Mathf.Max(stamina, 0f);
            canDash = true; // agarrar parede recarrega o dash
        }
    }

    // ────────────────────────────────────────────────────────
    //  STEP 5 — ATUALIZAR CONTADORES (coyote + buffer)
    // ────────────────────────────────────────────────────────
    void UpdateTimers()
    {
        // Coyote time: positivo quando no chão, decai no ar
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Jump buffer: positivo ao apertar espaço, decai sozinho
        if (jumpRequested)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    // ────────────────────────────────────────────────────────
    //  STEP 6 — PULO
    // ────────────────────────────────────────────────────────
    void HandleJump()
    {
        // Só processa se há buffer ativo e não está dashando
        if (jumpBufferCounter <= 0f || isDashing) return;

        bool touchingWall = wallDir != 0f;

        if (touchingWall && !isGrounded)
        {
            // WALL JUMP
            DoWallJump();
        }
        else if (coyoteCounter > 0f)
        {
            // PULO NORMAL (ou coyote)
            DoJump();
        }

        // Corte do pulo: soltar espaço na subida = pulo mais curto
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpRequested = false;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSound);
        if (juice != null) juice.ApplyStretch();
        if (ParticleManager.Instance != null) ParticleManager.Instance.PlayJump(groundCheck.position);
    }

    void DoWallJump()
    {
        float dirX = -wallDir; // empurra para longe da parede
        rb.linearVelocity = new Vector2(dirX * wallJumpX, wallJumpY);

        SetFacing(dirX > 0f);
        canDash = true;

        if (wallJumpRoutine != null) StopCoroutine(wallJumpRoutine);
        wallJumpRoutine = StartCoroutine(WallJumpLock());

        jumpBufferCounter = 0f;
        jumpRequested = false;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSound);
    }

    IEnumerator WallJumpLock()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpLockTime);
        isWallJumping = false;
    }

    // ────────────────────────────────────────────────────────
    //  STEP 7 — DASH
    // ────────────────────────────────────────────────────────
    void HandleDash()
    {
        if (!dashRequested) return;
        dashRequested = false; // sempre consome o request

        if (!canDash || isDashing) return; // não pode agora

        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        // ── Direção ──────────────────────────────────────
        Vector2 dir = new Vector2(inputX, inputY);
        if (dir.sqrMagnitude < 0.01f)
            dir = new Vector2(facingRight ? 1f : -1f, 0f);
        dir = dir.normalized;
        dashDir = dir;

        // ── Inicia ───────────────────────────────────────
        isDashing = true;
        canDash = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = dir * dashSpeed;

        if (dir.x > 0.1f) SetFacing(true);
        else if (dir.x < -0.1f) SetFacing(false);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.dashSound);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.8f);
        if (juice != null) juice.TriggerDashFeedback();
        if (ghost != null) ghost.makeGhost = true;
        if (ParticleManager.Instance != null) ParticleManager.Instance.PlayDash(transform.position);

        // ── FASE 1: dash ativo ────────────────────────────
        yield return new WaitForSeconds(dashDuration);

        // ── FASE 2: snap de parada ────────────────────────
        // Zera a velocidade brevemente — dá a sensação de "clamp" do Celeste
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        yield return new WaitForSeconds(dashEndPause);

        // ── Restaura ──────────────────────────────────────
        rb.gravityScale = defaultGravity;
        isDashing = false;
        if (ghost != null) ghost.makeGhost = false;

        // Se dashrou para baixo e pousou: cancela Y para não prender no chão
        if (isGrounded && dashDir.y < 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // ── FASE 3: cooldown ──────────────────────────────
        yield return new WaitForSeconds(dashCooldown);

        // Recarrega dash se estiver no chão
        // Se estiver no ar, CheckGround() recarrega ao pousar
        if (isGrounded) canDash = true;
    }

    // ────────────────────────────────────────────────────────
    //  STEP 8 — GRAVIDADE CUSTOMIZADA
    // ────────────────────────────────────────────────────────
    void HandleGravity()
    {
        if (isDashing) return; // dash controla a gravidade

        if (isWallGrabbing)
        {
            // Grab: desliga gravidade (FixedUpdate controla Y)
            rb.gravityScale = 0f;
            return;
        }

        // Garante que a gravidade está restaurada
        rb.gravityScale = defaultGravity;

        if (rb.linearVelocity.y < 0f)
        {
            // Caindo: gravidade extra para queda rápida
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (fallMultiplier - 1f)
                * Time.deltaTime;

            // Clamp terminal velocity
            if (rb.linearVelocity.y < -maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            // Subindo sem segurar pulo: pulo mais curto
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (lowJumpMultiplier - 1f)
                * Time.deltaTime;
        }
    }

    // ================================================================
    //  FIXED UPDATE — física de movimento (executa no physics step)
    // ================================================================
    void FixedUpdate()
    {
        if (isDashing) return;

        // ── WALL GRAB: controla Y ─────────────────────────
        if (isWallGrabbing)
        {
            float climbY = 0f;
            if (inputY > 0.1f) climbY = wallClimbSpeed;
            else if (inputY < -0.1f) climbY = -wallHoldSpeed;

            rb.linearVelocity = new Vector2(0f, climbY);
            return;
        }

        // ── WALL SLIDE ────────────────────────────────────
        if (isWallSliding && rb.linearVelocity.y < -wallSlideSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

        // ── WALL DROP ─────────────────────────────────────
        if (isWallDropping)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.MoveTowards(rb.linearVelocity.y, -maxFallSpeed,
                    wallDropAccel * Time.fixedDeltaTime)
            );
        }

        // ── MOVIMENTO HORIZONTAL ──────────────────────────
        float h = isWallJumping ? inputX * 0.4f : inputX;
        float target = h * maxSpeed;
        float diff = target - rb.linearVelocity.x;
        bool moving = Mathf.Abs(target) > 0.1f;

        float accel = isGrounded
            ? (moving ? groundAccel : groundDecel)
            : (moving ? airAccel : airDecel);

        rb.AddForce(Vector2.right * diff * accel, ForceMode2D.Force);

        // Clamp velocidade horizontal
        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
            rb.linearVelocity = new Vector2(
                Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
    }

    // ────────────────────────────────────────────────────────
    //  FLIP DO SPRITE
    // ────────────────────────────────────────────────────────
    void FlipSprite()
    {
        if (isWallJumping || isDashing) return;

        if (inputXSign > 0f) SetFacing(true);
        else if (inputXSign < 0f) SetFacing(false);
    }

    void SetFacing(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;

        Vector3 s = skin.localScale;
        s.x = Mathf.Abs(s.x) * (right ? 1f : -1f);
        skin.localScale = s;
    }

    // ────────────────────────────────────────────────────────
    //  ANIMATOR
    // ────────────────────────────────────────────────────────
    void UpdateAnimator()
    {
        if (anim == null) return;

        anim.SetFloat("SpeedX", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("SpeedY", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsDashing", isDashing);
        anim.SetBool("IsWallGrab", isWallGrabbing);
        anim.SetBool("IsWallSlide", isWallSliding);
        anim.SetBool("IsWallDrop", isWallDropping);
        anim.SetBool("IsWallJumping", isWallJumping);
    }

    // ────────────────────────────────────────────────────────
    //  GIZMOS (visível na viewport do Editor)
    // ────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        // Visualiza os 3 raycasts de parede (amarelo)
        Gizmos.color = Color.yellow;
        float cy = transform.position.y + wallColliderOffsetY;
        float halfH = wallColliderHeight * 0.4f;
        Vector3 mid = new Vector3(transform.position.x, cy, 0f);
        Vector3 high = new Vector3(transform.position.x, cy + halfH, 0f);
        Vector3 low = new Vector3(transform.position.x, cy - halfH, 0f);
        // Direita
        Gizmos.DrawLine(mid, mid + Vector3.right * wallCheckDist);
        Gizmos.DrawLine(high, high + Vector3.right * wallCheckDist);
        Gizmos.DrawLine(low, low + Vector3.right * wallCheckDist);
        // Esquerda
        Gizmos.DrawLine(mid, mid + Vector3.left * wallCheckDist);
        Gizmos.DrawLine(high, high + Vector3.left * wallCheckDist);
        Gizmos.DrawLine(low, low + Vector3.left * wallCheckDist);
    }
}