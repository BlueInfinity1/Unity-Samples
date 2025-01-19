using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(PlayerCollisions))]
    public class PlayerMovement : MonoBehaviour
    {
        public Vector2 inputVector { private set; get; }
        [SerializeField] float runningSpeed;
        [SerializeField] float runningDrag;
    
        bool activateJump = false, jumpButtonHeld = false; // jumpButtonHeld is here if we want to make the jump height control with the button holds
        public bool touchingGround { private set; get; } = false;
        [SerializeField] float initialJumpSpeed; // The velocity at the beginning of the player jump
    
        bool canShoot = true, shootButtonHeld = false;
        public bool isShooting { private set; get; } = false;
        [SerializeField] float shotReloadTime; // determines the spawn rate of the bullets when you hold down the shoot key
        [SerializeField] GameObject playerBullet;
        [SerializeField] Vector2 bulletSpawnOffset; // NOTE: This is used to get the bullet to spawn from the tip of the gun. We're probably going to need different offsets for different animations, but for now, just this one is used
        // This is the offset when the player character is facing right. If he's facing left, we'll mirror the x coordinate of this offset.
    
        Dictionary<string, KeyCode> keys = new();
        Dictionary<State, Coroutine> stateCoroutines = new();
    
        Rigidbody2D RB;
        PlayerCollisions playerCollisions;
    
        // We use an enum variable that other classes can access, but we also ensure smooth transition to another state by stopping all the coroutines associated with a certain state
        public State PlayerState
        {
            get
            {
                return playerState;
            }
            set
            {
                playerState = value;
                // StopAllCoroutines(); 
                // It would be cleaner to just stop all the coroutines here and restart the one we need, but the problem is that StopAllCoroutines won't take effect instantly,
                // so it would override the effect of any StartCoroutines we perform here.
                switch (value)
                {
                    case State.Controllable:
                        if (stateCoroutines[State.Controllable] != null)
                            StopCoroutine(stateCoroutines[State.Controllable]);
                        stateCoroutines[State.Controllable] = StartCoroutine(ControllingPlayer());
                        break;
    
                    case State.Uncontrollable: // NOTE: We don't currently transition to this state anywhere, but we start in this state when a level begins
                        break;
    
                    case State.GettingDamaged:
                        if (stateCoroutines[State.Controllable] != null)
                            StopCoroutine(stateCoroutines[State.Controllable]);
                        stateCoroutines[State.GettingDamaged] = StartCoroutine(GettingDamaged());
                        break;
    
                    case State.Dying:
                        if (stateCoroutines[State.Controllable] != null)
                            StopCoroutine(stateCoroutines[State.Controllable]);
                        stateCoroutines[State.Dying] = StartCoroutine(Dying());
                        break;
                }
            }
        }
        public enum State { Controllable, Uncontrollable, GettingDamaged, Dying };
        [HideInInspector] State playerState = State.Uncontrollable;
    
        void Start()
        {        
            RB = GetComponent<Rigidbody2D>();
    
            playerCollisions = GetComponent<PlayerCollisions>();
    
            // NOTE: We're currently handling input detection in this class, since it's tied so closely to the player actions. In case this class starts to get big, we could put the InitializeKeys()
            // and HandlingInputs() to a separate InputManager class, that we would then access from this class.
            
            InitializeKeys();
            InitializeStateCoroutines();
            StartCoroutine(HandlingInputs());
        }
     
        void InitializeStateCoroutines()
        {
            foreach (int i in Enum.GetValues(typeof(State)))        
                stateCoroutines[(State) i] = null;            
            
            //NOTE: Uncontrollable has no state coroutine
        }
    
        void InitializeKeys() 
        {
            // These can be modified
            keys["Jump"] = KeyCode.Space;
            keys["Shoot"] = KeyCode.Z;
            keys["Left"] = KeyCode.LeftArrow;
            keys["Right"] = KeyCode.RightArrow;        
        }
        
        IEnumerator ControllingPlayer()
        {
            Vector2 tempVelocity;
            float lastShotTime = 0;        
    
            while (playerState == State.Controllable)
            {
                tempVelocity = RB.velocity; //gather the new velocity to a temp variable since it's easier to modify than RB.velocity
    
                if (inputVector.x != 0) //change our facing direction whenever the input vector direction changes
                    transform.localScale = new Vector3(inputVector.x, transform.localScale.y, 1);
    
                // MOVING            
                RB.AddForce(runningSpeed * inputVector);
                touchingGround = playerCollisions.PerformGroundCheck();
    
                // JUMPING
                if (touchingGround && activateJump)
                {
                    tempVelocity.y = initialJumpSpeed; // Making the upward motion this way                
                    activateJump = false;
                    AudioManager.Instance.Play(Sound.PlayerJump);
                }
                // POSSIBLE TODO: Platform games usually implement a "late jump leeway" / "coyote time", which allows the player to jump even if he's left a platform already, e.g. jump can be performed even
                // if we touched the ground 0.1 seconds ago
    
                // SHOOTING
                if (canShoot && shootButtonHeld)
                {
                    Vector3 bulletSpawnPosition = transform.position + new Vector3(bulletSpawnOffset.x * transform.localScale.x, bulletSpawnOffset.y);
                    GameObject bulletObject = Instantiate(playerBullet, bulletSpawnPosition, Quaternion.identity);
                    PlayerBullet bullet = bulletObject.GetComponent<PlayerBullet>();
                    // NOTE: We're assuming the player sprite will always face either left or right, so localScale.x determines the shot direction. If this is not the case, then we need to change this
                    bullet.velocity = bullet.speed * transform.localScale.x * Vector2.right; 
                    canShoot = false;
                    lastShotTime = Time.time;
                    AudioManager.Instance.Play(Sound.PlayerShoot);
                }
                else if (!canShoot && Time.time - lastShotTime >= shotReloadTime) // Check if we can fire again                
                    canShoot = true;
    
                isShooting = shootButtonHeld;
    
                tempVelocity.x *= runningDrag;
                RB.velocity = tempVelocity;
                
                yield return new WaitForFixedUpdate();
            }
        }
        
        IEnumerator GettingDamaged()
        {        
            float timeToRegainControl = 0.5f;
            RB.velocity = Vector2.zero;
            float knockbackVelocity = 200f;
    
            AudioManager.Instance.Play(Sound.PlayerHurt);
    
            RB.AddForce(knockbackVelocity * transform.localScale.x * Vector2.left); // Push the player to the opposite of his facing direction
            //TODO: Other effects if required
            // Wait for the animation to finish, then allow the player to be controlled again
            yield return new WaitForSeconds(timeToRegainControl);
            PlayerState = State.Controllable;   
        }
    
        IEnumerator Dying()
        {        
            // PlayerAnimations.SetPlayerAnimation will handle the death animation. Here, we'll take the player out of the physics simulation and wait a moment before restarting the level
            RB.velocity = Vector2.zero;
            RB.isKinematic = true;
            float waitTimeBeforeRestart = 3.0f;
            AudioManager.Instance.Play(Sound.PlayerHurt); //TODO: Dying sound
    
            yield return new WaitForSeconds(waitTimeBeforeRestart);
            LevelController.Instance.RestartLevel();        
        }
    
        IEnumerator HandlingInputs()
        {
            while (true)
            {
                // Create an input vector based on our horizontal key presses. It could be enough to use only the x component of the vector for movement, but I decided to make this Vector2 in case there are
                // features like swimming or climbing up ladders
                inputVector = new Vector2((Input.GetKey(keys["Left"]) ? -1 : 0) + (Input.GetKey(keys["Right"]) ? 1 : 0), 0);
    
                if (Input.GetKeyDown(keys["Jump"]))
                    activateJump = true;
                
                if (Input.GetKeyUp(keys["Jump"]))
                    activateJump = false;
    
                jumpButtonHeld = Input.GetKey(keys["Jump"]);
                shootButtonHeld = Input.GetKey(keys["Shoot"]);
    
                yield return null;
            }
        }
    }
}
