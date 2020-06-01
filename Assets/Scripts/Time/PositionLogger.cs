//Copyright (c) 2020 Benjamin Robinson - Heavenly47

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionLogger : MonoBehaviour
{
    public List<TimeWaypoint> loggedPositions = new List<TimeWaypoint>();
    private bool logActive = false;
    int _maxLog = 500;
    private TimeWaypoint initialPosition;

    public Vector2 startVelocity = Vector2.zero;
    private Vector2 lastVelocity;

    public bool isCollectable = false;
    public int collectableTime;
    [HideInInspector]
    public bool collected = false;

    private bool onCD = false;
    private Transform _player;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = new TimeWaypoint(transform.position, transform.rotation, startVelocity);
        lastVelocity = startVelocity;
        _player = GameObject.Find("Player").transform;
    }

    public void SetActive(int maxLogs)
    {
        TimeManager.AddToList(this);
        _maxLog = maxLogs;
        loggedPositions.Capacity = _maxLog;
        InvokeRepeating("LogPosition", 0, 0.1f);
        PostRewind();
        rb.velocity = startVelocity;

    }

    public void RemoveActive()
    {
        logActive = false;
        CancelInvoke("LogPosition");
        loggedPositions.Clear();
    }

    public void ResetActive()
    {
        RemoveActive();
        rb.bodyType = RigidbodyType2D.Static;
        transform.position = initialPosition.position;
        transform.rotation = initialPosition.rotation;
        if (isCollectable)
        {
            GetComponent<MeshRenderer>().enabled = true;
            collected = false;
        }
    }

    private void OnDisable()
    {
        TimeManager.RemoveFromList(this);
    }

    void LogPosition()
    {
        if (logActive)
        {
            if (loggedPositions.Count == _maxLog)
                loggedPositions.RemoveAt(0);
            loggedPositions.Add(new TimeWaypoint(transform.position, transform.rotation, rb.velocity));
        }
    }

    public void PreRewind()
    {
        if (logActive)
        {
            logActive = false;
            lastVelocity = rb.velocity;
        }
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void PostRewind()
    {
        logActive = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = lastVelocity;
    }

    public void Rewind()
    {
        if (!logActive && !onCD && !isCollectable)
        {
            StartCoroutine(RewindCoolDown());
            int logCount = loggedPositions.Count - 1;
            if (logCount > 0)
            {
                if ((_player.position.y > loggedPositions[logCount].position.y - transform.localScale.y && _player.position.y < loggedPositions[logCount].position.y + transform.localScale.y) && (_player.position.x > loggedPositions[logCount].position.x - transform.localScale.x / 2 && _player.position.x < loggedPositions[logCount].position.x + transform.localScale.x / 2))
                    _player.Translate(new Vector2(0, ((loggedPositions[logCount].position.y + (transform.localScale.y / 2)) - (_player.position.y - (_player.localScale.y / 2)))));
                transform.position = loggedPositions[logCount].position;
                transform.rotation = loggedPositions[logCount].rotation;
                lastVelocity = loggedPositions[logCount].velocity;
                loggedPositions.RemoveAt(logCount);
            }
            else
            {
                transform.position = initialPosition.position;
                transform.rotation = initialPosition.rotation;
                lastVelocity = initialPosition.velocity;
            }
        }
    }

    private IEnumerator RewindCoolDown()
    {
        onCD = true;
        yield return new WaitForFixedUpdate();
        onCD = false;
    }
}
