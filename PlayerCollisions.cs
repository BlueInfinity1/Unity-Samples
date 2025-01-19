using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerStats), typeof(PlayerAnimations), typeof(PlayerMovement))]
    public class PlayerCollisions : MonoBehaviour
    {
        PlayerStats playerStats;
        PlayerAnimations playerAnimations;
        PlayerMovement playerMovement;

        [HideInInspector] public bool isInvincible = false;
        [SerializeField] float invincibilityTime; // In seconds

        [SerializeField] LayerMask standableLayer;
        BoxCollider2D BC;

        void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            playerAnimations = GetComponent<PlayerAnimations>();
            playerMovement = GetComponent<PlayerMovement>();

            BC = GetComponent<BoxCollider2D>();
        }

        /* 
         NOTE: For detecting collisions, multiple colliders can be used. For example, the player can be helped by having a smaller hitbox for registering enemy collisions, 
         but a larger hitbox for detecting collisions with pick-ups. Additionally, a different collider can also be used for collisions with the ground. 
         Here, a single collider has been used for now.
        */
        void OnTriggerStay2D(Collider2D col) // This needs to be "OnTriggerStay" instead of "OnTriggerEnter" to handle walking into an enemy while invincible
        {
            switch (col.gameObject.tag)
            {
                case "HealthPickUp":
                    HealthPickUp healthPickUp = col.gameObject.GetComponent<HealthPickUp>();
                    playerStats.ChangeHp(healthPickUp.healAmount);
                    Destroy(col.gameObject);
                    AudioManager.Instance.Play(Sound.HealthPickUp);
                    // TODO: Add graphical effects
                    break;

                case "EnemyHitBox":
                    if (!isInvincible)
                    {
                        // TODO: Add knockback if needed
                        Enemy enemy = col.transform.parent.GetComponent<Enemy>(); // Each EnemyHitBox should have a parent with an Enemy component
                        playerStats.ChangeHp(-enemy.baseAttackPower);
                        isInvincible = true;

                        if (playerStats.Hp > 0) // If still alive, handle invincibility behavior
                        {
                            playerAnimations.StartCoroutine(BeingInvincible());
                            playerMovement.PlayerState = PlayerMovement.State.Damaged;
                        }
                        else
                        {
                            playerMovement.PlayerState = PlayerMovement.State.Dying;
                        }
                    }
                    break;

                case "CameraTrigger":
                    CameraTrigger camTrigger = col.gameObject.GetComponent<CameraTrigger>();
                    camTrigger.SetNewCameraAttributes();
                    break;

                case "LevelClearTrigger":
                    LevelClearTrigger levelClearTrigger = col.gameObject.GetComponent<LevelClearTrigger>();
                    levelClearTrigger.OnTouch();
                    break;
            }
        }

        IEnumerator BeingInvincible()
        {
            float invincibilityStartTime = Time.time;
            Color originalPlayerColor = playerAnimations.SR.color;
        
            bool isTransparent = true; // State to toggle transparency
        
            while (Time.time - invincibilityStartTime < invincibilityTime) // Check invincibility duration
            {
                if (isTransparent)
                {
                    playerAnimations.SR.color = new Color(originalPlayerColor.r, originalPlayerColor.g, originalPlayerColor.b, 0.5f); // Semi-transparent
                }
                else
                {
                    playerAnimations.SR.color = new Color(originalPlayerColor.r, originalPlayerColor.g, originalPlayerColor.b, 1f); // Fully opaque
                }
        
                isTransparent = !isTransparent; // Toggle transparency state
        
                yield return new WaitForSeconds(0.2f); // Flicker interval
            }
        
            playerAnimations.SR.color = originalPlayerColor; // Reset to original color
            isInvincible = false;
        }


        /// <summary>
        /// Returns whether the player's feet are touching the ground.
        /// </summary>
        public bool PerformGroundCheck()
        {
            // Build a line below the player's feet and use a linecast to check collisions with the standableLayer
            Vector3 playerHorizontalMiddlePos = transform.position + (Vector3)BC.offset;

            float groundCheckDownwardLeeway = 0.005f; // How far below the box collider bounds to check
            float boxColliderGroundCheckDownLength = BC.size.y * 0.5f + groundCheckDownwardLeeway;

            // Horizontal line can't span full collider width to avoid registering as grounded when hugging walls
            float boxColliderGroundCheckHorizontalLength = BC.size.x * 0.48f;

            Vector3 lineLeftEnd = playerHorizontalMiddlePos + Vector3.down * boxColliderGroundCheckDownLength - boxColliderGroundCheckHorizontalLength * Vector3.right;
            Vector3 lineRightEnd = playerHorizontalMiddlePos + Vector3.down * boxColliderGroundCheckDownLength + boxColliderGroundCheckHorizontalLength * Vector3.right;

            RaycastHit2D groundHit = Physics2D.Linecast(lineLeftEnd, lineRightEnd, standableLayer);
            Debug.DrawLine(lineLeftEnd, lineRightEnd, Color.cyan);

            return groundHit;
        }
    }
}
