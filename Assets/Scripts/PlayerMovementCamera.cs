using UnityEngine;


//Contains commands the user wishes on the character
struct Cmd
{
    public float forwardmove;
    public float rightmove;
    public float upmove;
}

public class PlayerMovementCamera : MonoBehaviour
{
    //Player view
    public Transform playerView;                    //Camera
    public float playerViewYOffset = 0.6f;          //Height of a camera
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;

    //Frame occuring factors
    public float gravity = 20.0f;
    public float friction = 6f;                     //Ground friction

    //Movement
    public float moveSpeed = 7.0f;                  //Ground move speed
    public float runAcceleration = 14f;             //Ground acceleration
    public float runDeacceleration = 10f;           //Deacceleration when running on ground
    public float airAcceleration = 2f;              //Air acceleration
    public float airDeacceleration = 2f;            //Deacceleration when strafing opposite
    public float airControl = 0.3f;                 //Precision of air control
    public float sideStrafeAcceleration = 50f;      //How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
    public float sideStrafeSpeed = 1f;              //Max speed to generate when side strafing
    public float jumpSpeed = 8f;                    //Speed at which character's up axis gains when jumping
    public bool holdJumpToBhop = false;             //When enabled allows player to jump by holding the jump button

    //Styles
    public GUIStyle style;

    //Frames per second
    public float fpsDisplayRate = 4f;

    private int frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;

    private CharacterController controller;

    //Camera
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    /*
    //True means player fully on the ground
    private bool grounded = false;
    */

    //Queue next jump before hitting the ground
    private bool wishJump = false;

    //Display real-time friction values
    private float playerFriction = 0.0f;

    //Player wished commands (forward, back, jump etc.)
    private Cmd cmd;

    public void SetMovementDir()
    {
        cmd.forwardmove = Input.GetAxis("Vertical");
        cmd.rightmove = Input.GetAxis("Horizontal");
    }
    /*
    public void PlayerExplode()
    {
        isDead = true;
    }

    public void PlayerSpawn()
    {
        this.transform.position = playerSpawnPos;
        this.transform.rotation = playerSpawnRot;
        rotX = 0.0f;
        rotY = 0.0f;
        playerVelocity = Vector3.zero;
 
    }
    */

    //Calculates wish acceleration based on player's cmd wishes
    public void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        
        if (addspeed <= 0)
        {
            return;
        }

        accelspeed = accel * Time.deltaTime * wishspeed;

        if (accelspeed > addspeed)
        {
            accelspeed = addspeed;
        }

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    public void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;
        float vel;
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        //Apply friction only when player is on the ground
        if(controller.isGrounded)
        {
            control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        playerFriction = newspeed;

        if(newspeed < 0)
        {
            newspeed = 0;
        }
        if(speed > 0)
        {
            newspeed /= speed;
        }

        playerVelocity.x *= newspeed;
        //playerVelocity.y *= newspeed;
        playerVelocity.z *= newspeed;
    }

    public void GroundMove()
    {
        Vector3 wishdir;

        if (!wishJump)
        {
            ApplyFriction(1.0f);
        }
        else
        {
            ApplyFriction(0.0f);
        }

        SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        float wishspeed = wishdir.magnitude;    //maybe var?
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        playerVelocity.y = 0;

        if(wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }

    public void AirMove()
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;

        //var scale = CmdScale(); ?????????

        SetMovementDir();

        wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, wishvel);

        //Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    public void QueueJump()
    {
        if (holdJumpToBhop)
        {
            wishJump = Input.GetButton("Jump");
            return;
        }

        if (Input.GetButtonDown("Jump") && !wishJump)
        {
            wishJump = true;
        }

        if (Input.GetButtonUp("Jump"))
        {
            wishJump = false;
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 400, 100), "FPS: " + fps, style);
        Vector3 ups = controller.velocity;
        ups.y = 0;
        GUI.Label(new Rect(0, 15, 400, 100), "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups", style);
        GUI.Label(new Rect(0, 30, 400, 100), "Top Speed: " + Mathf.Round(playerTopVelocity * 100) / 100 + "ups", style);
    }

    void Start()
    {
        //Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //if(playerView == null)

        //Put the camera inside the capsule collider
        playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
        
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        //FPS calculation
        frameCount++;
        dt += Time.deltaTime;

        if (dt > 1.0 / fpsDisplayRate)
        {
            fps = Mathf.Round(frameCount / dt);
            frameCount = 0;
            dt -= 1.0f / fpsDisplayRate;
        }

        //Ensure the cursor is locked into the screen
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        //Camera rotation stuff, mouse control etc.
        rotX -= Input.GetAxis("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxis("Mouse X") * yMouseSensitivity * 0.02f;

        //Clamp the X rotation
        if (rotX < -90)
        {
            rotX = -90;
        }
        else if (rotX > 90)
        {
            rotX = 90;
        }

        this.transform.rotation = Quaternion.Euler(0, rotY, 0); //Rotate the collider
        playerView.rotation = Quaternion.Euler(rotX, rotY, 0);  //Rotate the camera

        //Movement
        QueueJump();
        if (controller.isGrounded)
        {
            GroundMove();
        }
        else if (!controller.isGrounded)
        {
            AirMove();
        }

        //Move the controller
        controller.Move(playerVelocity * Time.deltaTime);


        //Need to move the camera so it doesn't clip the player when going too fast
        //Set the camera's position to the transform
        playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
        

        //Calculate top velocity
        Vector3 udp = playerVelocity;
        udp.y = 0.0f;

        if(udp.magnitude > playerTopVelocity)
        {
            playerTopVelocity = udp.magnitude;
        }

        /*
        if (Input.GetKeyUp("x"))
        {
            PlayerExplode();
        }
        if (Input.GetKeyDown("Fire1") && isDead)
        {
            PlayerSpawn();
        }*/

    }
}
