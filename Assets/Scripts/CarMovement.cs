using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarMovement : MonoBehaviour
{
    #region zmienne
    //Fizyka
    private Rigidbody2D rb;

    //Zmienne sterowania
    public float accelerationPower = 200f;
    public float steeringPower = 0.15f;

    //punkty i sensory
    private bool isdead = false;
    private float sensorLength = 30f;
    private float distanceTraveled = 0;
    private float sensorDist = 0;
    private float score = 0;

    //Zmienne mutacji algorytmu
    private int mutateamp;
    private int mutatedens;

    //Wektor punktów
    private Vector2 lastPos;
    private Vector2 fwd;

    //Tekstury
    public Sprite dead;
    public Sprite first;
    public SpriteRenderer spriteRenderer;

    //obiekt klasy gamecontroller oraz modelu sieci
    private GameController manager;
    private NeuralNetwork brain;

    //Zmienne pomocnicze
    private float newV;
    private float startTime;
    private int ttk;
    #endregion

    public void Constructor(GameController gc, int[] layers, int x, int y, int timetk) {
        this.manager = gc;
        this.brain = new NeuralNetwork(layers);
        this.mutateamp = x;
        this.mutatedens = y;
        this.ttk = timetk;
    }

    public void Constructor(GameController gc, CarMovement template, int x, int y, int timetk) {
        this.manager = gc;
        this.brain = new NeuralNetwork(template.brain);
        this.mutateamp = x;
        this.mutatedens = y;
        this.ttk = timetk;
    }

    public void mutate() {
        brain.Mutate(mutateamp, mutatedens);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPos = transform.position;
        fwd = -transform.up;
        startTime = Time.time;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isdead) {
            steering();
            addScore();
            checkIfSlow();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        kill();
    }

    //Ustawienie samochodu prowadz¹cego
    public void setLeader() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = first;
    }

    //Sprawdzenie czy pojazd jest powolny
    private void checkIfSlow() {        
        if (Mathf.Abs(startTime - Time.time) > 2) {
            newV = rb.velocity.magnitude;
            //Debug.Log(newV);
            if (newV < 2 || Input.GetKeyDown(KeyCode.R) || Mathf.Abs(startTime - Time.time) > ttk)
            {
                kill();
            }
        }
    }

    private void kill() {
        isdead = true;
        rb.velocity = Vector2.zero;
        spriteRenderer.sprite = dead;
        manager.areDead();
    }

    //Metoda zwracaj¹ca punkty
    public float getScore() {        
        return score; 
    }


    //Metoda zwiêksza punkty jeœli pojazd porusza siê do przodu
    private void addScore() {
        if (!isdead) {
            Vector2 newPos = transform.position;
            Vector2 movement = newPos - lastPos;
            if (Vector2.Dot(fwd, movement) < 0)
            {
                distanceTraveled = Vector2.Distance(transform.position, lastPos);
            }
            fwd = -transform.up;
            lastPos = transform.position;

            score += distanceTraveled;  // - sensorDist / 200;
        }
    }

    //Metoda tworzy promienie i zwraca d³ugoœæ ka¿dego z nich
    private float[] sensor() {
        float[] readings = new float[5];
        RaycastHit2D front = Physics2D.Raycast(transform.position, transform.up, sensorLength);
        RaycastHit2D frontr = Physics2D.Raycast(transform.position, transform.up+transform.right, sensorLength);
        RaycastHit2D frontl = Physics2D.Raycast(transform.position, transform.up-transform.right, sensorLength);
        RaycastHit2D right = Physics2D.Raycast(transform.position, transform.right, sensorLength);
        RaycastHit2D left = Physics2D.Raycast(transform.position, -transform.right, sensorLength);

        if (front.collider != null)
        {
            Debug.DrawLine(transform.position, front.point);
            readings[2] = front.distance;
        }
        else {
            readings[2] = sensorLength;
        }

        if (right.collider != null) {
            Debug.DrawLine(transform.position, right.point);
            readings[4] = right.distance;
        }
        else {
            readings[4] = sensorLength;
        }

        if (left.collider != null)
        {
            Debug.DrawLine(transform.position, left.point);
            readings[0] = left.distance;
        }
        else {
            readings[0] = sensorLength;
        }

        if (frontr.collider != null)
        {
            Debug.DrawLine(transform.position, frontr.point);
            readings[3] = frontr.distance;
        }
        else {
            readings[3] = sensorLength;
        }

        if (frontl.collider != null)
        {
            Debug.DrawLine(transform.position, frontl.point);
            readings[1] = frontl.distance;
        }
        else {
            readings[1] = sensorLength;
        }

        sensorDist = Mathf.Sqrt(Mathf.Abs((readings[0] - readings[4]) + (readings[1] - readings[3])));
        return readings;
        //Debug.Log(readings[0]+";"+ readings[1] + ";"+ readings[2] + ";" + readings[3] + ";"+ readings[4]);
    }

    //Metoda odczytuje input i steruje pojazdem
    private void steering()
    {
        if (!isdead)
        {
            float[] input = brain.FeedForward(sensor());          
            
            float steeringAmount, speed, direction;
            steeringAmount = -(input[0] * 2 - 1);
                //steeringAmount = -Input.GetAxis("Horizontal");
            speed = (input[1] * 2 - 1) * accelerationPower;
                //speed = Input.GetAxis("Vertical") * accelerationPower;
            direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));
            rb.rotation += steeringAmount * steeringPower * rb.velocity.magnitude * direction;

            rb.AddRelativeForce(Vector2.up * speed);
            rb.AddRelativeForce(-Vector2.right * rb.velocity.magnitude * steeringAmount / 2);
        }
    }
}
