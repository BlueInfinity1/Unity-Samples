using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStats),typeof(PlayerAnimations), typeof(PlayerMovement))]
public class PlayerCollisions : MonoBehaviour
{
    PlayerStats playerStats;
    PlayerAnimations playerAnimations;
    PlayerMovement playerMovement;
    [HideInInspector] public bool isInvincible = false;
    [SerializeField] float invincibilityTime; //in seconds

    [SerializeField] LayerMask standableLayer;
    BoxCollider2D BC;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerAnimations = GetComponent<PlayerAnimations>();
        playerMovement = GetComponent<PlayerMovement>();

        BC = GetComponent<BoxCollider2D>();        
    }
    
    /*NOTE: For detecting collisions, multiple colliders can be used. For example, the player can be helped by having a smaller hitbox for registering enemy collisions, but a larger hitbox
     *for detecting collisions with pick-ups. Additionally, a different collider can also be used with collisions with the ground. Here, a single collider has been used for now.*/
    void OnTriggerStay2D(Collider2D col) //This needs to be "Stay" instead of "Enter", since otherwise we won't be damaged if we walk inside the enemy while we're invincible
    {        
        switch (col.gameObject.tag)
        {
            case "HealthPickUp":            
                HealthPickUp healthPickUp = col.gameObject.GetComponent<HealthPickUp>();
                playerStats.ChangeHp(healthPickUp.healAmount);
                Destroy(col.gameObject);
                AudioManager.instance.Play(Sound.HealthPickUp);
                //TODO: graphical effects
                break;

            case "EnemyHitBox":
                if (!isInvincible)
                {
                    //TODO: Knockback if needed
                    Enemy enemy = col.transform.parent.GetComponent<Enemy>(); //Each EnemyHitBox should have a parent that has an Enemy component
                    playerStats.ChangeHp(-enemy.baseAttackPower);                    
                    isInvincible = true;

                    if (playerStats.Hp > 0) //if we're still alive, do the normal invinvibility behaviour
                    {
                        playerAnimations.StartCoroutine(BeingInvincible());
                        playerMovement.PlayerState = PlayerMovement.State.Damaged;
                    }
                    else
                        playerMovement.PlayerState = PlayerMovement.State.Dying;
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
        Color tempPlayerColor = playerAnimations.SR.color;
        tempPlayerColor.a = 0.5f;

        playerAnimations.SR.color = tempPlayerColor; // We make the player slightly transparent when invincible

        while (Time.time - invincibilityStartTime < invincibilityTime) //check whether we've spent the necessary time in the invincibility mode
        {
            //TODO: Create flicker effects etc.
            yield return null;
        }

        playerAnimations.SR.color = originalPlayerColor;
        isInvincible = false;
    }

    /// <summary>
    /// Return whether the player's feet are touching the ground
    /// </summary>
    public bool PerformGroundCheck() 
    {
        //The idea here is that we'll build a line that is below the player's feet, and then linecast using that line to see if there are any collisions with objects of the standableLayer (currently
        //consists of only "Ground" layer)
        Vector3 playerHorizontalMiddlePos = transform.position + (Vector3)BC.offset;
        
        float groundCheckDownwardLeeway = 0.005f; //how much below the box collider bounds should we check
        float boxColliderGroundCheckDownLength = BC.size.y * 0.5f + groundCheckDownwardLeeway;
        
        //The horizontal length of the line can't quite be the full width of the box collider, since otherwise the linecast is successful if we're hugging a wall, even if we're jumping
        float boxColliderGroundCheckHorizontalLength = BC.size.x * 0.48f; 

        Vector3 lineLeftEnd = playerHorizontalMiddlePos + Vector3.down * boxColliderGroundCheckDownLength - boxColliderGroundCheckHorizontalLength * Vector3.right;
        Vector3 lineRightEnd = playerHorizontalMiddlePos + Vector3.down * boxColliderGroundCheckDownLength + boxColliderGroundCheckHorizontalLength * Vector3.right;

        RaycastHit2D groundHit = Physics2D.Linecast(lineLeftEnd, lineRightEnd, standableLayer);
        Debug.DrawLine(lineLeftEnd, lineRightEnd, Color.cyan);

        return groundHit;
    }
}
