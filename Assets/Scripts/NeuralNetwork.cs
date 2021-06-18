using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork 
{
    #region zmienne
    private int[] layers; //macierz warstw
    private float[][] neurons; //macierz neuronów
    private float[][][] weights; //macierz wag
    #endregion

    /// <summary>
    /// Inicjalizacja sieci neuronowej z losowymi wagami
    /// </summary>
    /// <param name="layers">warstwy dla sieci neuronowej</param>
    public NeuralNetwork(int[] layers)
    {
        //G³êboka kopia warstw sieci neuronowej
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        //generacja macierzy
        InitNeurons();
        InitWeights();
    }

    /// <summary>
    /// G³êboka kopia innego obiektu sieci neuronowej
    /// </summary>
    /// <param name="copyNetwork">Obiekt sieci do skopiowania</param>
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork.weights);
    }

    /// <summary>
    /// Metoda kopiuje wagi z argumentu
    /// </summary>
    private void CopyWeights(float[][][] copyWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    /// <summary>
    /// Utworzenie macierzy neurons
    /// </summary>
    private void InitNeurons()
    {
        //Inicjalizacja listy neuronów
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++) //przejœcie przez wszystkie warstwy
        {
            neuronsList.Add(new float[layers[i]]); //Dodanie warstwy do macierzy
        }

        neurons = neuronsList.ToArray(); //konwersja listy do macierzy
    }

    /// <summary>
    /// Utworzenie pierwszych losowych wag
    /// </summary>
    private void InitWeights()
    {

        List<float[][]> weightsList = new List<float[][]>(); //Lista wag, która zostanie zamieniona na macierz

        //przejœcie po wszystkich neuronach które maja powi¹zanie wagowe
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>(); //lista warstwy wag, która zostanie przekonwrtowana na macierz

            int neuronsInPreviousLayer = layers[i - 1]; //Odniesienie do poprzedniej warstwy dla odnalezienia wag

            //przejœcie po wszystkich neuronach w tej warstwie
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer]; //wagi neurona

                //przejœcie po wszystkich neuronach w poprzedniej warstwie po³¹czonych z ym neuronem
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //Ustawienie losowej wartoœci dla neurona od -1 do 1
                    neuronWeights[k] = Random.Range(-1f, 1f);
                    //Ustawienie losowej wartoœci dla neurona wed³ug formu³y
                    //neuronWeights[k] = Random.Range(0f, 1f) * Mathf.Sqrt(2f/neurons[i].Length); 
                }
                layerWeightsList.Add(neuronWeights); //dodanie wag tego neuorna do tej warstwy neuronów
            }
            weightsList.Add(layerWeightsList.ToArray()); //Dodanie tej warstwy wag do listy warstw wag
        }
        weights = weightsList.ToArray(); //konwersja na macierz
    }

    /// <summary>
    /// Obliczenie wartoœci wyjœciowych na podstawie argumentów dla tej sieci
    /// </summary>
    /// <param name="inputs">Wejœcia dla sieci</param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs)
    {
        //Dodac wejœcia do macierzy neuronów jak wartoœci neuronów warstwy pierwszej
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        //Przejœcie po wszystkich neuronach i obliczenie ich wartoœci
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;

                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k]; //Suma wszystkich po³¹czen neuronowych dla tej warstwy
                }
                neurons[i][j] = Sigmoid(value); //Wywo³anie funkcji aktywacji dla sumy               
            }
        }
        return neurons[neurons.Length - 1]; //Zwrócenie warstwy wyjœciowej
    }

    /// <summary>
    /// Funkcja aktywacyjna sigmoid
    /// </summary>
    private static float Sigmoid(float x) => 1f / (1f + (float)Mathf.Exp(-x));

    /// <summary>
    /// Zmutowanie wag sieci neuronowej
    /// </summary>
    public void Mutate(int amp, int dens)
    {
        //Przejœcie po ka¿dej wadze w sieci neuronowej
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    //Losowa liczba, która zadecyduje czy waga zostanie zmodyfikowana
                    float randomNumber = Random.Range(0f, (float)(11-dens)*10);

                    if (randomNumber <= 2f)
                    { //if 1-2
                      //odwrócenie znaku wagi
                        weight *= -1f*amp;
                    }
                    else if (randomNumber <= 4f)
                    { //if 3-4
                      //wybranie losowej wartoœci od -1*amp do 1*amp
                        weight = Random.Range(-1f, 1f)*amp;
                    }
                    else if (randomNumber <= 6f)
                    { //if 5-6
                      //losowe zwiêkszenie od 0 do 100% razy amp
                        float factor = Random.Range(0f, 1f) + 1f;
                        weight *= factor*amp;
                    }
                    else if (randomNumber <= 8f)
                    { //if 7-8
                      //losowe zmniejszenie od 0 do 100% razy amp
                        float factor = Random.Range(0f, 1f);
                        weight *= factor*amp;
                    }

                    weights[i][j][k] = weight;
                }
            }
        }
    }
}
