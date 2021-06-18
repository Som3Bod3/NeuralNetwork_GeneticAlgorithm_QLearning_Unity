using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    #region General variables
    private int gameMode; //gamemode ID 1-3: 1- user, 2 - gen, 3 - Q
    public int[] layers = { 5, 5, 2 }; //NN structure
    public GameObject carPrefab; //Referencja do klonu obiektu samochodu genetycznego
    public GameObject UserCarPrefab; //Referencja do klonu obiektu samochodu USER
    public GameObject QLCarPrefab; //Referencja do klonu obiektu samochodu Q-Learning
    public Text GameInfo, ModeInfo, TimeT;
    private float startTime;
    #endregion

    #region User Input variables
    private UserCarMovement U_carBrain; //Pojedynczy mózg samochodu
    private GameObject user;
    #endregion

    #region Genetic Algorithm variables
    private int G_noOfCars; //liczba samochodów w generacji
    private int G_mutateAmp; //from 1 to 10, where 1 is most power
    private int G_mutateDens; //Density of mutation

    private int G_carsKilled; //liczba dot. zabitych samochodów
    private int G_ttk;
    private float G_bestScore; //dot. najlepszy wynik
    private CarMovement G_carBrain; //Pojedynczy mózg samochodu
    private CarMovement G_bestBrain; //Rekordowy mózg samochodu
    private List<GameObject> G_pool = new List<GameObject>(); //Lista wszystkich samochodów
    private int G_genNo; //Liczba obecnej generacji
    #endregion

    #region QLearn variables
    private int QL_noOfCars; //liczba aut
    private int QL_ttk; //time to kill
    private int QL_modelID; //ID modelu sieci
    private float QL_bestScore = 0; //najlepszy wynik
    private QLCarMovement QL_carBrain; //obiekt klasy
    #endregion

    //=================================================================================================================================================

    #region Unity methods
    // Start is called before the first frame update
    void Start()
    {    
        //wybranie trybu
        chooseMode(); 
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        //Sprawdzenei funkcji update trybów
        checkModeUpdate();
        //Sprawdzanie czy uzytkownik chce wrócic do menu
        checkForExit();
        //zmiana prêdkoœci czasu gry
        changeSpeed();
        TimeT.text = "Elapsed time:\n" + Mathf.Abs(startTime - Time.time);
    }
    #endregion

    #region General methods
    void chooseMode() //Wybiera poprawny tryb i go inicjalizuje
    {
        gameMode = globalVar.globalMode;
        switch (gameMode)
        {
            case 1:
                //User
                UserModeInit();
                break;
            case 2:
                //Genetic
                GeneticModeInit();
                break;
            case 3:
                //Qlearn
                QlearnModeInit();
                break;
        }
    }

    void checkModeUpdate() //sprawdza update trybów
    {
        switch (gameMode)
        {
            case 1:
                //User
                UserUpdate();               
                break;
            case 2:
                //Genetic  
                GeneticUpdate();
                break;
            case 3:
                //Qlearn 
                QLearnUpdate();
                break;
        }
    }

    void changeSpeed() //Zmienia prêdkoœæ gry
    {
        if (Input.GetKeyDown(KeyCode.F))
            Time.timeScale = 1;
        if (Input.GetKeyDown(KeyCode.G))
            Time.timeScale = 25;
        if (Input.GetKeyDown(KeyCode.H))
            Time.timeScale = 50;
        if (Input.GetKeyDown(KeyCode.J))
            Time.timeScale = 100;
    }

    void checkForExit() 
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            SceneManager.LoadScene("Menu");
        }
    }
    #endregion

    #region User Input methods
    void UserModeInit() //Inicjalizuje tryb User
    {
        GameInfo.text = "User";
        user = Instantiate(UserCarPrefab) as GameObject;
        user.transform.position = new Vector2(21.5f, -36f);
        user.name = "Car";
        U_carBrain = user.GetComponent<UserCarMovement>();
        U_carBrain.Constructor(this);
    }

    void UserUpdate() //updateuje tryb user
    {        
        ModeInfo.text = "Score: " + U_carBrain.getScore();
    }

    public void revive() //resetuje pojazd po wypadku
    {
        Destroy(user);
        user = Instantiate(UserCarPrefab) as GameObject;
        user.transform.position = new Vector2(21.5f, -36f);
        user.name = "Car";
        U_carBrain = user.GetComponent<UserCarMovement>();
        U_carBrain.Constructor(this);
    }

    #endregion

    #region Genetic Algorithm methods
    void GeneticModeInit() //inicjalizuje tryb algorytm genetyczny
    {
        GameInfo.text = "Genetic Algorithm";
        G_noOfCars = globalVar.globalCarN;
        G_mutateDens = globalVar.globalMDen;
        G_mutateAmp = globalVar.globalMAmp;
        G_ttk = globalVar.globalTTK;
        G_carsKilled = 0;
        G_genNo = 1;

        //Spawnuje x samochodów i dodaje je do listy
        for (int i = 1; i <= G_noOfCars; i++)
        {            
            GameObject a = Instantiate(carPrefab) as GameObject;
            a.transform.position = new Vector2(21.5f, -36f);
            a.name = "Car" + i;
            G_carBrain = a.GetComponent<CarMovement>();
            G_carBrain.Constructor(this, layers, G_mutateAmp, G_mutateDens, G_ttk);
            G_pool.Add(a);
        }
        Debug.Log(G_genNo);
        ModeInfo.text = "Gen. no.: " + G_genNo + "\nCars no.: " + G_noOfCars + "\nMutate dens.: " + G_mutateDens + "\nMutate amp.: " + G_mutateAmp + "\nTime to Kill: " + G_ttk + "\nBest score: ";
    }

    void GeneticUpdate() { 

    }

    public void areDead() //sprawdza czy wszystkie pojazdy s¹ martwe, jesli tak to wybiera najlepszy i tworzy now¹ generacje
    {
        //zwiêkszenie liczby zabitych samochodów
        G_carsKilled++;

        //Sprawdzenie czy samochody s¹ martwe
        if (G_carsKilled >= G_noOfCars)
        {
            //reset
            G_carsKilled = 0;
            G_genNo++;

            //Odnalezienie najlepszego pojazdu i jego ibiektu CarMovement
            float score;
            foreach (GameObject a in G_pool)
            {
                G_carBrain = a.GetComponent<CarMovement>();
                score = G_carBrain.getScore();
                if (score > G_bestScore || G_bestScore != G_bestScore)
                {
                    G_bestScore = score;
                    G_bestBrain = G_carBrain;
                    Debug.Log(G_bestScore);
                }
            }

            //zabicie wszystkich obiektów
            foreach (GameObject a in G_pool)
            {
                Destroy(a);
            }

            //wyczyszczenie listy obiektów
            G_pool.Clear();

            //Utworzenie nowej generacji
            for (int i = 1; i <= G_noOfCars; i++)
            {
                GameObject a = Instantiate(carPrefab) as GameObject;
                a.name = "car" + i;
                a.transform.position = new Vector2(21.5f, -36f);
                G_carBrain = a.GetComponent<CarMovement>();
                if (i > 1)
                {
                    G_carBrain.Constructor(this, G_bestBrain, G_mutateAmp, G_mutateDens, G_ttk);
                    G_carBrain.mutate();
                }
                else
                {
                    G_carBrain.Constructor(this, G_bestBrain, G_mutateAmp, G_mutateDens, G_ttk);
                    G_carBrain.setLeader();
                }
                G_pool.Add(a);
            }
            Debug.Log(G_genNo);
            ModeInfo.text = "Gen. no.: " + G_genNo + "\nCars no.: " + G_noOfCars + "\nMutate dens.: " + G_mutateDens + "\nMutate amp.: " + G_mutateAmp + "\nTime to Kill: " + G_ttk +  "\nBest score: " + G_bestScore;
        }
    }
    #endregion

    #region QLearn methods
    void QlearnModeInit() //Inicjalizuje tryb QLearn
    {        
        GameInfo.text = "Q-Learning Model";
        QL_noOfCars = globalVar.globalCarN;
        QL_ttk = globalVar.globalTTK;
        QL_modelID = globalVar.globalModelID;
        
        //Spawnuje 1 pojazd
        for (int i = 1; i <= QL_noOfCars; i++) //chng ncars to 1
        {            
            GameObject a = Instantiate(QLCarPrefab) as GameObject;
            a.transform.position = new Vector2(21.5f, -36f);
            a.name = "Car" + i;
            QL_carBrain = a.GetComponent<QLCarMovement>();
            QL_carBrain.Constructor(this, QL_ttk, QL_modelID);            
        }        
        ModeInfo.text = "Best score: ";
    }

    void QLearnUpdate() { 

    }

    public void updatescore(float x) //aktualizuje najlepszy wynik
    {
        if (x > QL_bestScore) {
            QL_bestScore = x;
            Debug.Log(QL_bestScore);
            ModeInfo.text = "Best score: " + QL_bestScore;
        }
    }
    #endregion

}
