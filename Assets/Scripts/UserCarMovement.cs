using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCarMovement : MonoBehaviour
{
    #region zmienne
    //Fizyka
    private Rigidbody2D rb;

    //Zmienne sterowania
    public float accelerationPower = 200f;
    public float steeringPower = 0.15f;

    //Punkty, sensory
    private bool isdead = false;
    private float sensorLength = 30f;
    private float distanceTraveled = 0;
    private float score = 0;

    //Wektor od punktów
    private Vector2 lastPos;
    private Vector2 fwd;

    //Tekstury
    public Sprite dead;
    public Sprite first;
    public SpriteRenderer spriteRenderer;

    private GameController manager;
    #endregion

    public void Constructor(GameController gc)
    {
        this.manager = gc;     
    }

    // Start is called before the first frame update
    void Start()
    {
        //Inicjalizacja komponentów i zmiennych
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = first;
        lastPos = transform.position;
        fwd = -transform.up;      
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //sprawdzenie czy samochód jest rozbity
        if (!isdead)
        {            
            steering();            
            addScore();
            sensor();            
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //zabicie samochodu po rozbiciu sie
        kill();
    }

    private void kill()
    {
        isdead = true;
        rb.velocity = Vector2.zero;
        spriteRenderer.sprite = dead;
        Invoke("revive", 3);
    }

    //Funkcja zwraca punkty
    public float getScore()
    {  
        return score; 
    }

    void revive() {
        manager.revive();
    }


    //Metoda zwiêksza punkty jeœli pojazd sie porusza do przodu
    private void addScore()
    {
        Vector2 newPos = transform.position;
        Vector2 movement = newPos - lastPos;
        if (Vector2.Dot(fwd, movement) < 0)
        {
            distanceTraveled = Vector2.Distance(transform.position, lastPos);
            score += distanceTraveled;
            Debug.Log(distanceTraveled);
        }
        fwd = -transform.up;
        lastPos = transform.position;
        
    }

    //Metoda tworzy promienie i zwraca d³ugoœæ ka¿dego z nich
    private float[] sensor()
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

    //Metoda odczytuje input uzytkownika i steruje pojazdem
    private void steering()
    {
        float steeringAmount, speed, direction;

        steeringAmount = -Input.GetAxis("Horizontal");
        speed = Input.GetAxis("Vertical") * accelerationPower;
        direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));
        rb.rotation += steeringAmount * steeringPower * rb.velocity.magnitude * direction;

        rb.AddRelativeForce(Vector2.up * speed);
        rb.AddRelativeForce(-Vector2.right * rb.velocity.magnitude * steeringAmount / 2);
    }
}
