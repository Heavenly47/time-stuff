//Copyright © 2020 Benjamin Robinson - Heavenly47

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region init
    [Header("Movement")]
    //Movement
    public float gravity = -25;
    public float moveSpeed = 5;
    private float groundDamp = 10;
    private float inAirDamp = 5;
    public float jumpHeight = 2;
    private float hoverAmount = .25f;

    //Prime31 2D Character Controller https://github.com/prime31/CharacterController2D
    private CharacterController2D _controller;
    private Animator _animator;
    private BoxCollider2D _collider;
    private Vector3 _velocity;
    private TimeBarManager _timeBarManager;
    private float normalHorizontalSpeed = 0;
    private float _hoverAmount = .3f;
    private int extraJumpTime = 10;
    private int extraJumpTimer = 0;
    private int dodgeMultiplier = 1;
    private bool jumped = false;
    private bool inAir = false;
    public bool canManipulate = false;

    private Transform currentCheckpoint;
    private List<TimeBound> boundTrek = new List<TimeBound>();

    [Header("Pop Text")]
    public Image dialogPanel;
    public Text dialogText;
    public Text restartText;

    public Image fadeImage;
    private int fadeSpeed = 4;
    #endregion

    InputManager iCon = new InputManager();

    public class InputManager
    {
        private class InputStates
        {
            public bool left;
            public bool right;
            public bool jump;
        }

        public bool leftDown { get { return !m_previousState.left && m_currentState.left; } }
        public bool left { get { return m_currentState.left; } }

        public bool rightDown { get { return !m_previousState.right && m_currentState.right; } }
        public bool right { get { return m_currentState.right; } }

        public bool jumpDown { get { return !m_previousState.jump && m_currentState.jump; } }
        public bool jump { get { return m_currentState.jump; } }

        private InputStates m_currentState;
        private InputStates m_previousState;

        private bool m_updated;

        public InputManager()
        {
            m_currentState = new InputStates();
            m_previousState = new InputStates();
        }

        public void OnUpdate(bool left, bool right, bool jump)
        {
            if (left)
                m_currentState.left = true;

            if (right)
                m_currentState.right = true;

            if (jump)
                m_currentState.jump = true;

            m_updated = true;
        }

        public void ResetUpdate()
        {
            if (!m_updated)
                return;

            InputStates tempInputStates = m_previousState;
            m_previousState = m_currentState;
            m_currentState = tempInputStates;

            m_currentState.left = false;
            m_currentState.right = false;
            m_currentState.jump = false;

            m_updated = false;
        }
    }

    void Start()
    {
        Application.targetFrameRate = 120;
        _controller = GetComponent<CharacterController2D>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
        _timeBarManager = GameObject.Find("Time Bar Manager").GetComponent<TimeBarManager>();

        _controller.onTriggerEnterEvent += OnTriggerEnterEvent;
        _controller.onTriggerExitEvent += OnTriggerExitEvent;
    }

    private void OnTriggerEnterEvent(Collider2D col)
    {
        if (col.CompareTag("TimeSector"))
        {
            canManipulate = true;
            TimeBound colTimeBound = col.GetComponent<TimeBound>();
            StartCoroutine(_timeBarManager.IncreaseTime(colTimeBound.sectionTime));
            //Having a higher functioning machine leads to a disadvantage as time is decreased per frame. Fixed framerate of 120fps now
            //StartCoroutine(_timeBarManager.IncreaseTime(Mathf.FloorToInt(colTimeBound.sectionTime / (Time.smoothDeltaTime * 30))));
            colTimeBound.SetBound();
            boundTrek.Add(colTimeBound);

            if (colTimeBound.usePopText)
            {
                StartCoroutine(DisplayText(colTimeBound.popText));
                colTimeBound.usePopText = false;
            }

            StartCoroutine(CameraZoom(colTimeBound.cameraSize));
        }

        if (col.CompareTag("Collectable"))
        {
            PositionLogger collectableLogger = col.GetComponent<PositionLogger>();
            if (!collectableLogger.collected)
            {
                StartCoroutine(_timeBarManager.IncreaseTime(_timeBarManager.CurrentTime + collectableLogger.collectableTime));
                collectableLogger.collected = true;
                col.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        if (col.CompareTag("Checkpoint"))
        {
            currentCheckpoint = col.transform;
            boundTrek.Clear();
            StartCoroutine(FadeRestartText());
        }
    }

    private void OnTriggerExitEvent(Collider2D col)
    {
        if (col.CompareTag("TimeSector"))
        {
            if (col.GetComponent<TimeBound>().freezeAfter)
                TimeManager.FreezeAllObjects();
            TimeManager.ClearList();
            TimeManager.timeManipulated = false;
            canManipulate = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Hazard"))
            StartCoroutine(ResetToLast());
        else if (collision.collider.CompareTag("Win"))
            WinGame();
    }

    void Update()
    {
        iCon.OnUpdate(Input.GetAxis("Horizontal") < 0, Input.GetAxis("Horizontal") > 0, Input.GetButton("Jump"));

        //Time manipulation inputs
        if (canManipulate && !OptionHolder.paused)
        {
            if (TimeManager.timeRemaining > 0)
            {
                if (Input.GetButton("Freeze"))
                    TimeManager.FreezeAllObjects();
                else if (Input.GetButtonUp("Freeze") && !Input.GetButton("Rewind"))
                    TimeManager.ResumeAllObjects();

                if (Input.GetButtonDown("Rewind"))
                    TimeManager.FreezeAllObjects();
                if (Input.GetButton("Rewind"))
                    TimeManager.RewindTime();
                if (Input.GetButtonUp("Rewind"))
                    TimeManager.ResumeAllObjects();
            }

            if (TimeManager.timeManipulated)
            {
                if (TimeManager.timeRemaining > 0)
                    _timeBarManager.DecreaseTime(1);
                else
                {
                    TimeManager.ResumeAllObjects();
                    StartCoroutine(DisplayRestartText());
                }
            }
        }

        //Reset at last checkpoint
        if (Input.GetKeyUp(KeyCode.R))
            StartCoroutine(ResetToLast());
    }

    void FixedUpdate()
    {
        if (Time.timeScale != 0 && !OptionHolder.paused)
            Movement();
        iCon.ResetUpdate();
    }

    //Movement
    void Movement()
    {
        //player_
        if (_controller.isGrounded)
        {
            _velocity.y = -0.1f;
            jumped = false;
            extraJumpTimer = extraJumpTime;
        }
        else
            extraJumpTimer--;

        if (!(_controller.collisionState.right || _controller.collisionState.left) && !_controller.isGrounded)
            inAir = false;

        if (_velocity.y < 4 && inAir)
            _velocity.y = -4;
        
        if (iCon.right)
        {
            normalHorizontalSpeed = Input.GetAxis("Horizontal");
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            dodgeMultiplier = 1;
            if (!_controller.isGrounded && inAir)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (iCon.left)
        {
            normalHorizontalSpeed = Input.GetAxis("Horizontal");
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            dodgeMultiplier = -1;
            if (!_controller.isGrounded && inAir)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            normalHorizontalSpeed = 0;
        }

        if ((_controller.isGrounded || inAir) && iCon.jumpDown)
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _hoverAmount = hoverAmount;
            jumped = true;
            if (inAir && !_controller.isGrounded)
            {
                if (transform.localScale.x < 0)
                    _velocity.x = -10;

                if (transform.localScale.x > 0)
                    _velocity.x = 10;
            }
        }

        if (!_controller.isGrounded && iCon.jump && _velocity.y > 0)
        {
            _velocity.y += _hoverAmount;
            _hoverAmount -= .005f;
        }

        float _deltaTime = Time.deltaTime;

        _velocity.y += gravity * _deltaTime;

        float smoothMove = (_controller.isGrounded) ? groundDamp : inAirDamp;

        _velocity.x = Mathf.Lerp(_velocity.x, normalHorizontalSpeed * moveSpeed, _deltaTime * smoothMove);

        _controller.move(_velocity * _deltaTime);

        _velocity = _controller.velocity;
    }

    //Called when player reaches game end
    private void WinGame()
    {
        //Score math; (Score Cap / (Resets + Time Stuff + Time)) * Minimum Score
        int score = (1000000 / ((OptionHolder.resetTotal * 10) + Mathf.FloorToInt(OptionHolder.timeStuffUsed / 100) + Mathf.FloorToInt(Time.timeSinceLevelLoad))) * 100;
        OptionHolder.score = score;
        OptionHolder.transitioning = true;
        StartCoroutine(PlayerDisappear());
    }

    //Resets the player and blocks to last checkpoint
    public IEnumerator ResetToLast()
    {
        if (currentCheckpoint != null && !OptionHolder.transitioning)
        {
            //Fade in
            OptionHolder.transitioning = true;
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = Color.clear;
            for (float c = 0; c < 255; c += fadeSpeed)
            {
                fadeImage.color = new Color(0, 0, 0, c / 255);
                yield return new WaitForEndOfFrame();
            }
            fadeImage.color = Color.black;

            restartText.gameObject.SetActive(false);
            transform.position = currentCheckpoint.position;

            if (boundTrek.Count > 0 && boundTrek[boundTrek.Count - 1].HintGateReady())
                StartCoroutine(DisplayText(boundTrek[boundTrek.Count - 1].hintText));
            foreach (TimeBound sector in boundTrek)
            {
                sector.ResetBound();
            }
            boundTrek.Clear();
            TimeManager.ClearList();
            OptionHolder.resetTotal++;

            //Fade out
            fadeImage.color = Color.black;
            for (float c = 255; c > 0; c -= fadeSpeed)
            {
                fadeImage.color = new Color(0, 0, 0, c / 255);
                yield return new WaitForEndOfFrame();
            }
            fadeImage.color = Color.clear;
            fadeImage.gameObject.SetActive(false);
            OptionHolder.transitioning = false;

            //for every object in the current time sector (if in one) place them at their initial position

            //Every time the player enters a new time section, if the section isn't already on a list, add it to a list
            //The list clears every time the player resets to the last checkpoint or a new stage is reached
            //Use the list to put every object in each time sector back to their initial position
        }
    }

    //Coroutine that displays and fades pop text
    private IEnumerator DisplayText(string text)
    {
        dialogText.text = text;
        dialogPanel.gameObject.SetActive(true);

        dialogPanel.color = Color.clear;
        dialogText.color = Color.clear;
        for (float c = 0; c < 255; c += fadeSpeed)
        {
            dialogPanel.color = new Color(0, 0, 0, c / 255);
            dialogText.color = new Color(255, 255, 255, c / 255);
            yield return new WaitForEndOfFrame();
        }

        dialogPanel.color = Color.black;
        dialogText.color = Color.white;

        for (float c = 255; c > 0; c -= 1)
        {
            dialogPanel.color = new Color(0, 0, 0, c / 255);
            dialogText.color = new Color(255, 255, 255, c / 255);
            yield return new WaitForEndOfFrame();
        }
        dialogPanel.color = Color.clear;
        dialogText.color = Color.clear;

        dialogPanel.gameObject.SetActive(false);
    }

    //Called when player runs out of time stuff, fades restart text
    private IEnumerator DisplayRestartText()
    {
        restartText.gameObject.SetActive(true);

        restartText.color = Color.clear;
        for (float c = 0; c < 255; c += fadeSpeed)
        {
            restartText.color = new Color(255, 255, 255, c / 255);
            yield return new WaitForEndOfFrame();
        }
        restartText.color = Color.white;
    }

    //Called when player reaches a checkpoint, fades restart text
    private IEnumerator FadeRestartText()
    {
        restartText.color = Color.white;
        for (float c = 255; c > 0; c -= fadeSpeed)
        {
            restartText.color = new Color(255, 255, 255, c / 255);
            yield return new WaitForEndOfFrame();
        }
        restartText.color = Color.clear;
        restartText.gameObject.SetActive(false);

    }

    //Called when game won to fade player sprite
    public IEnumerator PlayerDisappear()
    {
        StartCoroutine(CameraZoom(20));
        for (float p = gameObject.GetComponent<SpriteRenderer>().color.a * 255; p > 0; p -= 2)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(gameObject.GetComponent<SpriteRenderer>().color.r, gameObject.GetComponent<SpriteRenderer>().color.g, gameObject.GetComponent<SpriteRenderer>().color.b, p / 255);
            yield return new WaitForEndOfFrame();
        }
        gameObject.GetComponent<SpriteRenderer>().color = new Color(gameObject.GetComponent<SpriteRenderer>().color.r, gameObject.GetComponent<SpriteRenderer>().color.g, gameObject.GetComponent<SpriteRenderer>().color.b, 0);

        StartCoroutine(GameObject.Find("Pause Manager").GetComponent<PauseController>().FadeToWin());
    }

    //Called whenever the camera needs to change size, creates smooth shift
    public IEnumerator CameraZoom(int newSize)
    {
        float c = Camera.main.orthographicSize;

        int shrinkMutliplier = 1;
        if (c > newSize)
            shrinkMutliplier = -1;

        for (; (shrinkMutliplier * c) < (shrinkMutliplier * newSize); c += 0.01f * shrinkMutliplier)
        {
            Camera.main.orthographicSize = c;
            yield return new WaitForEndOfFrame();
        }

        Camera.main.orthographicSize = newSize;
    }
}
