using UnityEngine;
using System.Collections;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 2.0f; //the movement speed of the player
        private Rigidbody2D rb;
        private Vector2 movementDirection;
        private bool isOnStairs = false; //when on stairs, the player moves in a different angle.
        public bool isCrouching = false; //when crouching, the player moves slowe
        private SpriteRenderer spriteRenderer;
        private float lastAngle;  // Store the last calculated angle

        //Archer speficics:A
        public bool isRanged; //If the character is an archer OR caster character
        public bool isStealth; //If true, Makes the player transparent when crouched
        public GameObject projectilePrefab; //prefab to the projectile
        public GameObject AoEPrefab;

        public float projectileSpeed = 10.0f; // Speed at which the projectile travels
        public float shootDelay = 0.5f; // Delay in seconds before the projectile is fired


        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        }
        void Update()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized;

            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            lastAngle = SnapAngleToEightDirections(angle);  // Update lastAngle here

            movementDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));

            HandleMovement();

            if (Input.GetKeyDown(KeyCode.C))
            {
                HandleCrouching();
            }

            if (isRanged)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Invoke(nameof(DelayedShoot), shootDelay);
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    StartCoroutine(Quickshot());
                }
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    StartCoroutine(CircleShot());
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    StartCoroutine(DeployAoEDelayed());
                }
            }
        }



        void FixedUpdate()
        {
            if (movementDirection != Vector2.zero)
            {
                rb.MovePosition(rb.position + movementDirection * speed * Time.fixedDeltaTime);
            }
        }

        float SnapAngleToEightDirections(float angle)
        {
            angle = (angle + 360) % 360;

            if (isOnStairs)
            {
                // Angle adjustments when on stairs
                if (angle < 30 || angle >= 330)
                    return 0;
                else if (angle >= 30 && angle < 75)
                    return 60;
                else if (angle >= 75 && angle < 105)
                    return 90;
                else if (angle >= 105 && angle < 150)
                    return 120;
                else if (angle >= 150 && angle < 210)
                    return 180;
                else if (angle >= 210 && angle < 255)
                    return 240;
                else if (angle >= 255 && angle < 285)
                    return 270;
                else if (angle >= 285 && angle < 330)
                    return 300;
            }
            else
            {
                // Normal angle adjustments
                if (angle < 15 || angle >= 345)
                    return 0; // East (isEast)
                else if (angle >= 15 && angle < 60)
                    return 35; // Northeast (isNorthEast)
                else if (angle >= 60 && angle < 120)
                    return 90; // North (isNorth)
                else if (angle >= 120 && angle < 165)
                    return 145; // Northwest (isNorthWest)
                else if (angle >= 165 && angle < 195)
                    return 180; // West (isWest)
                else if (angle >= 195 && angle < 240)
                    return 215; // Southwest (isSouthWest)
                else if (angle >= 240 && angle < 300)
                    return 270; // South (isSouth)
                else if (angle >= 300 && angle < 345)
                    return 330; // Southeast (isSouthEast)

            }

            return 0;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = false;
            }
        }

        float GetPerpendicularAngle(float angle, bool isLeft)
        {
            // Calculate the base perpendicular angle (90 degrees offset)
            float perpendicularAngle = isLeft ? angle - 90 : angle + 90;
            perpendicularAngle = (perpendicularAngle + 360) % 360; // Normalize the angle

            // Use your SnapAngleToEightDirections function to snap to the nearest valid angle
            return SnapAngleToEightDirections(perpendicularAngle);
        }

        void HandleMovement()
        {
            if (Input.GetKey(KeyCode.W))
            {
                return;
            }
            else if (!isCrouching) // Allow strafing only when not crouching, if desired
            {
                if (Input.GetKey(KeyCode.S))
                {
                    movementDirection = -movementDirection; // Move backwards
                }

                else if (Input.GetKey(KeyCode.A))
                {
                    float leftAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, true);
                    movementDirection = new Vector2(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad));

                }
                else if (Input.GetKey(KeyCode.D))
                {

                    float rightAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, false);
                    movementDirection = new Vector2(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad));
                }
                else
                {
                    movementDirection = Vector2.zero; // No movement input
                }
            }
            else
            {
                movementDirection = Vector2.zero; // No movement input
            }
        }

        void HandleCrouching()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching; // Toggle crouching
                speed = isCrouching ? 1.0f : 2.0f; // Adjust speed based on crouch state

                if (isCrouching && isStealth)
                {
                    // Set the color to dark gray and reduce opacity to 50%
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                else
                {
                    // Reset the color to white and opacity to 100%
                    spriteRenderer.color = Color.white;
                }
            }
        }


        //Ranged character specific methods:

        public void SetArcherStatus(bool status)
        {
            isRanged = status;
        }


        void DelayedShoot()
        {
            Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
            ShootProjectile(fireDirection);
        }

        void ShootProjectile(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            Rigidbody2D rbProjectile = projectileInstance.GetComponent<Rigidbody2D>();
            if (rbProjectile != null)
            {
                rbProjectile.linearVelocity = direction * projectileSpeed;
            }
        }

        IEnumerator Quickshot()
        {
            // Initial small delay before starting the quickshot sequence
            yield return new WaitForSeconds(0.1f);

            // Loop to fire five projectiles in the facing direction
            for (int i = 0; i < 5; i++)
            {
                Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                ShootProjectile(fireDirection);

                // Wait for 0.18 seconds before firing the next projectile
                yield return new WaitForSeconds(0.18f);
            }
        }


        IEnumerator CircleShot()
        {
            float initialDelay = 0.1f;
            float timeBetweenShots = 0.9f / 8;  // Total time divided by the number of shots

            yield return new WaitForSeconds(initialDelay);

            // Use the lastAngle as the start angle and generate projectiles in 8 directions
            for (int i = 0; i < 8; i++)
            {
                float angle = lastAngle + i * 45;  // Increment by 45 degrees for each direction
                angle = Mathf.Deg2Rad * angle;  // Convert to radians for direction calculation
                Vector2 fireDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ShootProjectile(fireDirection);

                yield return new WaitForSeconds(timeBetweenShots);
            }
        }

        IEnumerator DeployAoEDelayed()
        {
            if (AoEPrefab != null)
            {
                // Wait for 0.3 seconds before instantiating the AoEPrefab
                yield return new WaitForSeconds(0.3f);

                // Instantiate the AoE prefab at the player's position
                GameObject aoeInstance = Instantiate(AoEPrefab, transform.position, Quaternion.identity);

                // Destroy the instantiated prefab after another 0.5 seconds
                Destroy(aoeInstance, 0.5f);
            }
        }



    }
}