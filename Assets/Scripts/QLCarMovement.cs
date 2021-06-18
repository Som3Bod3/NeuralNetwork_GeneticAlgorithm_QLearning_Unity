using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Barracuda;

public class QLCarMovement : Agent
{
    #region zmienne
    //Fizyka
    private Rigidbody2D rb;

    //Zmienne sterowania
    public float accelerationPower = 200f;
    public float steeringPower = 0.15f;

    //Punkty i sensory
    private bool isdead = false;
    private float sensorLength = 30f;
    private float distanceTraveled = 0;
    private float score = 0;

    //wektor dodawania punktów
    private Vector2 lastPos;
    private Vector2 fwd;

    //tekstury
    public Sprite dead;
    public Sprite first;
    public Sprite normal;
    public SpriteRenderer spriteRenderer;

    //obiekt managera
    private GameController manager;

    //zmienne pomocnicze
    private float startTime;
    private float reverseTime;
    private int ttk;

    //Obiekty wczeœniej wytrenowanych modeli SN
    public NNModel brain1, brain2, brain3, brain4, brain5, brain6, brain7, brain8, brain9, brain10;
    #endregion

    public void Constructor(GameController gc, int x, int id)
    {        
        this.manager = gc;
        this.ttk = x;
        switch (0) { //change to id
            case 1:
                Debug.Log("HERE");
                SetModel("brain", brain1, 0);
                break;
            case 2:
                SetModel("brain", brain2, 0);
                break;
            case 3:
                SetModel("brain", brain3, 0);
                break;
            case 4:
                SetModel("brain", brain4, 0);
                break;
            case 5:
                SetModel("brain", brain5, 0);
                break;
            case 6:
                SetModel("brain", brain6, 0);
                break;
            case 7:
                SetModel("brain", brain7, 0);
                break;
            case 8:
                SetModel("brain", brain8, 0);
                break;
            case 9:
                SetModel("brain", brain9, 0);
                break;
            case 10:
                SetModel("brain", brain10, 0);
                break;
        }
    }

    public override void OnEpisodeBegin() //Metoda start
    {
        score = 0;
        startTime = Time.time;
        reverseTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normal;
        transform.position = new Vector2(21.5f, -36f);
        transform.rotation = Quaternion.Euler(Vector3.forward * -90);
        lastPos = transform.position;
        fwd = -transform.up;
    }

    public override void CollectObservations(VectorSensor sensor) //ML-Agents zbiera inputy
    {
        float[] input = sensorF();
        foreach (float i in input) {
            sensor.AddObservation(i);
        }
    }

    public override void OnActionReceived(ActionBuffers actions) //ML-Agents wykonuje akcje
    {
        steering(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        addScore();
        if (Mathf.Abs(startTime - Time.time) > ttk) { kill(); }
    }


    //Metoda zwiêksza punkty gdy pojazd sie porusza
    private void addScore()
    {
        Vector2 newPos = transform.position;
        Vector2 movement = newPos - lastPos;
        if (Vector2.Dot(fwd, movement) < 0 && Mathf.Abs(reverseTime - Time.time) > 2)
        {            
            distanceTraveled = Vector2.Distance(transform.position, lastPos);
            score += distanceTraveled;
            AddReward(distanceTraveled);
        }
        else if(Mathf.Abs(reverseTime - Time.time) > 2)
        {
            reverseTime = Time.time;
            SetReward(-10f);
        }
        fwd = -transform.up;
        lastPos = transform.position;      
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SetReward(-20f);        
        kill();
    }


    private void kill()
    {        
        rb.velocity = Vector2.zero;        
        spriteRenderer.sprite = dead;        
        manager.updatescore(score);        
        EndEpisode();
        
    }

    //Metoda zwraca punkty
    public float getScore()
    {
        return score;
    }

    #region useless
    // Start is called before the first frame update
    void Start()
    {
 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
   
    }
    #endregion

    //Metoda tworzy promienie i zwraca d³ugoœæ ka¿dego z nich
    private float[] sensorF()
    {
        float[] readings = new float[5];
        RaycastHit2D front = Physics2D.Raycast(transform.position, transform.up, sensorLength);
        RaycastHit2D frontr = Physics2D.Raycast(transform.position, transform.up + transform.right, sensorLength);
        RaycastHit2D frontl = Physics2D.Raycast(transform.position, transform.up - transform.right, sensorLength);
        RaycastHit2D right = Physics2D.Raycast(transform.position, transform.right, sensorLength);
        RaycastHit2D left = Physics2D.Raycast(transform.position, -transform.right, sensorLength);

        if (front.collider != null)
        {
            Debug.DrawLine(transform.position, front.point);
            readings[2] = front.distance;
        }
        else
        {
            readings[2] = sensorLength;
        }

        if (right.collider != null)
        {
            Debug.DrawLine(transform.position, right.point);
            readings[4] = right.distance;
        }
        else
        {
            readings[4] = sensorLength;
        }

        if (left.collider != null)
        {
            Debug.DrawLine(transform.position, left.point);
            readings[0] = left.distance;
        }
        else
        {
            readings[0] = sensorLength;
        }

        if (frontr.collider != null)
        {
            Debug.DrawLine(transform.position, frontr.point);
            readings[3] = frontr.distance;
        }
        else
        {
            readings[3] = sensorLength;
        }

        if (frontl.collider != null)
        {
            Debug.DrawLine(transform.position, frontl.point);
            readings[1] = frontl.distance;
        }
        else
        {
            readings[1] = sensorLength;
        }

        return readings;
    }

    //Metoda odczytuje input i steruje pojazdem
    private void steering(float gas, float steer)
    {
        float steeringAmount, speed, direction;

        steeringAmount = -steer;
        speed = gas * accelerationPower;
        direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));
        rb.rotation += steeringAmount * steeringPower * rb.velocity.magnitude * direction;

        rb.AddRelativeForce(Vector2.up * speed);
        rb.AddRelativeForce(-Vector2.right * rb.velocity.magnitude * steeringAmount / 2);
    }
}
