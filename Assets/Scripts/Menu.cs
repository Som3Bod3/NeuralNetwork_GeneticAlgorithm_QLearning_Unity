using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    #region zmienne
    public GameObject genTgl, usrTgl, qTgl; //obeitk sliderów
    private bool gen, usr, q; //zmienne tymczasowe
    private int muteD, muteA, nCars, ttk, modelID; //zmienne globalne
    public Text percent1, percent2, percent3, percent4, percent5; //Obiekt pól tekstowych
    #endregion

    void Start()
    {
        //przypisanie pocz¹tkowych wartoœci toggle
        gen = genTgl.GetComponent<Toggle>().isOn;
        usr = usrTgl.GetComponent<Toggle>().isOn;
        q = qTgl.GetComponent<Toggle>().isOn;
        //przypisanie pocz¹tkowych wartoœci sliderów
        nCars = 50;
        muteA = 1;
        muteD = 1;
        ttk = 60;
        modelID = 1;
    }

    #region metody onAction sliderów
    public void muteDslider(float x)
    {
        muteD = Mathf.RoundToInt(x);
        percent1.text = muteD.ToString();
    }

    public void muteAslider(float x)
    {
        muteA = Mathf.RoundToInt(x);
        percent2.text = muteA.ToString();
    }

    public void nCarsf(float x)
    {
        nCars = Mathf.RoundToInt(x);
        percent3.text = nCars.ToString();
    }

    public void ttkf(float x)
    {
        ttk = Mathf.RoundToInt(x);
        percent4.text = ttk.ToString();
    }

    public void idf(float x)
    {
        modelID = Mathf.RoundToInt(x);
        percent5.text = modelID.ToString();
    }
    #endregion

    #region metody onAction toggli
    public void genToggle(bool tog)
    {
        if (usr || q && !gen)
        {
            genTgl.GetComponent<Toggle>().isOn = true;
            usrTgl.GetComponent<Toggle>().isOn = false;
            qTgl.GetComponent<Toggle>().isOn = false;
            gen = true;
            usr = false;
            q = false;
        }
        else
        {
            gen = tog;
        }
    }

    public void UserToggle(bool tog)
    {
        if (gen || q && !usr)
        {
            genTgl.GetComponent<Toggle>().isOn = false;
            usrTgl.GetComponent<Toggle>().isOn = true;
            qTgl.GetComponent<Toggle>().isOn = false;
            gen = false;
            usr = true;
            q = false;
        }
        else
        {
            usr = tog;
        }
    }

    public void QlearnToggle(bool tog)
    {
        if (usr || gen && !q)
        {
            genTgl.GetComponent<Toggle>().isOn = false;
            usrTgl.GetComponent<Toggle>().isOn = false;
            qTgl.GetComponent<Toggle>().isOn = true;
            gen = false;
            usr = false;
            q = true;
        }
        else
        {
            q = tog;
        }
    }
    #endregion

    /// <summary>
    /// Metoda wywo³ywana w momencie wciœniêcia przycisku start
    /// </summary>
    public void onActionPlayBtn() {
        if (gen)
        {
            globalVar.globalMode = 2;
            globalVar.globalMAmp = muteA;
            globalVar.globalMDen = muteD;
            globalVar.globalCarN = nCars;
            globalVar.globalTTK = ttk;
            SceneManager.LoadScene("OnTrack");
        }
        else if (usr)
        {
            globalVar.globalMode = 1;
            SceneManager.LoadScene("OnTrack");
        }
        else if (q) 
        {
            globalVar.globalCarN = nCars; //todelete
            globalVar.globalMode = 3;            
            globalVar.globalTTK = ttk;
            globalVar.globalModelID = modelID;
            SceneManager.LoadScene("OnTrack");
        }
        
    }
}
