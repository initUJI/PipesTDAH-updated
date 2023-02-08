﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PipesGameManager : MonoBehaviour
{
    public string gameDifficulty = "Easy";

    public int numberOfPoints;
    int actualPointsInScene;

    public UI_Manager_PipeGame UI_Manager;

    [SerializeField] List<GameObject> pipesObjects;
    List<GameObject> dirtyPointsSelected;

    GameObject pipeInActiveGame;

    public float totalTimeToPlay;
    float gameplayTime = 0;

    public GameObject gameplayUI;
    public GameObject finishingPanel;
    private PartidaJugada partidaActual;

    public bool hasStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        //Seleccion de la tubería que se vaya a utilizar
        numberOfPoints = UnityEngine.Random.Range(0, pipesObjects.Count);
        pipeInActiveGame = pipesObjects[numberOfPoints];
        pipeInActiveGame.SetActive(true);

        //Determinando cuantos puntos sucios van a aparecer en base a la dificultad

        if (SessionManager.instance == null || (SessionManager.instance != null && SessionManager.instance.playingSession == false))
        {
            Debug.Log("No se encuentra en sesion");

            //El jugador no se encuentra en una sesion
            totalTimeToPlay = 60;
            numberOfPoints = 14;
        }
        else
        {
            MinijuegoLevel thisLevel = SessionManager.instance.sucesionDeJuegos[0];
            partidaActual = new PartidaJugada(thisLevel.difficulty);
            DataManager.instancia.añadirPartidaGuardada(partidaActual);
            DataManager.instancia.Guardar();

            this.gameObject.GetComponent<UI_InGame_Manager>().setPauseScreenModeToPractise();

            //El jugador se encuentra en una sesion
            gameDifficulty = thisLevel.difficulty;

            //totalTimeToPlay = 15;
            switch (gameDifficulty)
            {
                case "Easy":
                    totalTimeToPlay = 60;
                    numberOfPoints = 20;
                    break;
                case "Medium":
                    totalTimeToPlay = 60;
                    numberOfPoints = 17;
                    break;
                case "Hard":
                    totalTimeToPlay = 60;
                    numberOfPoints = 12;
                    break;
            }
        }

        UI_Manager = GameObject.Find("/GameManager").GetComponent<UI_Manager_PipeGame>();
        UI_Manager.setTotalDirtyPoints(numberOfPoints);

        CreateDirtyPointsInScene();

        StartCoroutine(StartCountdown());
    }

    void CreateDirtyPointsInScene()
    {
        //Determinando qué puntos sucios van a ser activados
        int i = 0;
        int totalDirtyPoints = pipeInActiveGame.transform.Find("DirtyPoints").childCount;
        List<int> positionsToBeActivated = new List<int>();

        while (i < numberOfPoints)
        {
            bool valido = false;
            int aux = 0;
            while (valido == false)
            {
                aux = UnityEngine.Random.Range(0, totalDirtyPoints);

                valido = !positionsToBeActivated.Contains(aux); //Si no lo contiene devovlerá true
                if (!valido)
                    Debug.Log("repetido" + aux);
            }

            positionsToBeActivated.Add(aux);
            i++;
            Debug.Log(aux);
        }

        //Activar las particulas sucias que resultaron del proceso anterior
        List<Transform> dirtyPoints = GetChildren(pipeInActiveGame.transform.Find("DirtyPoints"));
        foreach (int element in positionsToBeActivated)
        {
            dirtyPoints[element].gameObject.SetActive(true);
            dirtyPoints[element].gameObject.GetComponent<DirtyPointObject>().SetDifficultMode(gameDifficulty);
        }

        actualPointsInScene = positionsToBeActivated.Count;
    }

    IEnumerator StartCountdown()
    {
        Transform countdownObject = GameObject.Find("Canvas/Countdown").transform;

        foreach (Transform child in countdownObject)
        {
            LeanTween.scale(child.gameObject, new Vector3(1, 1, 1), 0.75f);
            yield return new WaitForSeconds(0.75f);
            LeanTween.scale(child.gameObject, new Vector3(0, 0, 0), 0.25f);
            yield return new WaitForSeconds(0.25f);
        }
        hasStarted = true;
        gameplayUI.SetActive(true);

        yield return null;
    }

    List<Transform> GetChildren (Transform parent)
    {
        if (parent.childCount == 0)
            return null;
        else
        {
            List<Transform> result = new List<Transform>();

            for (int i = 0; i < parent.childCount; i++)
            {
                result.Add(parent.transform.GetChild(i));
            }

            return result;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(hasStarted)
        {
            ActualizarTiempo();
            ComprobarFinalDeJuego();
        }
    }

    private void ComprobarFinalDeJuego()
    {
        if(gameplayTime >= totalTimeToPlay/* || UI_Manager.actualDirtyPoints <= 0*/)
        {
            //Termina la partida
            hasStarted = false;

            GetComponent<UI_InGame_Manager>().setPauseButtonState(false);
            finishingPanel.SetActive(true);

            LeanTween.scale(finishingPanel, new Vector3(1, 1, 1), 1);

            //Fill the panel with info
            //float percentageErased = 100 - Mathf.Round(UI_Manager.actualDirtyPoints / UI_Manager.totalDirtyPoints * 100);

            //Make buttons functional
            //finishingPanel.transform.Find("Panel/Buttons/MenuButton").gameObject.GetComponent<Button>().onClick.AddListener(() => gameObject.GetComponent<UI_InGame_Manager>().exitButtonPressed());

            if (SessionManager.instance == null || (SessionManager.instance != null && SessionManager.instance.playingSession == false))
            {
                finishingPanel.transform.Find("Panel/Title").gameObject.GetComponent<Text>().text = "¡Práctica terminada!";

                //Editando texto de resultado
                finishingPanel.transform.Find("Panel/Information/PointsText").gameObject.GetComponent<Text>().text = "";
                finishingPanel.transform.Find("Panel/Information/PointsText/ResolutionText").gameObject.GetComponent<Text>().text = "";

                //Editando texto de resultado adicional
                finishingPanel.transform.Find("Panel/Information/AdditionalText").gameObject.GetComponent<Text>().text = "Virus eliminados: " + UI_Manager.destroyedPoints;

                //Editando texto de puntos
                finishingPanel.transform.Find("Panel/Information/TimeText").gameObject.GetComponent<Text>().text = "Errores cometidos: un montón";


                //Editando las funciones de los botones
                finishingPanel.transform.Find("Panel/Buttons/NextButton").gameObject.GetComponent<Button>().onClick.AddListener(() => gameObject.GetComponent<UI_InGame_Manager>().exitButtonPressed());
                finishingPanel.transform.Find("Panel/Buttons/NextButton/Information2").GetComponent<Text>().text = "Menu";
            }
            else
            {
                SessionManager.instance.SumarTiempoAlTotal((int)gameplayTime);
                partidaActual.aciertos = UI_Manager.destroyedPoints;
                partidaActual.fallos = UI_Manager.playerErrors;
                partidaActual.juegoTerminado = true;
                DataManager.instancia.sustituirPartidaGuardada(partidaActual);
                DataManager.instancia.Guardar();

                string resultadoPrueba = "";
                SessionManager.instance.SumarPuntosAlTotal(UI_Manager.destroyedPoints * 10);
                if (UI_Manager.destroyedPoints >= 10)
                {
                    resultadoPrueba = "¡Los estás haciendo excelente!";
                }
                else if (UI_Manager.destroyedPoints >= 7)
                {
                    resultadoPrueba = "¡Vas mejorando!";
                }
                else if (UI_Manager.destroyedPoints >= 1)
                {
                    resultadoPrueba = "¡Ánimo! ¡Sigue así!";
                }
                else
                {
                    resultadoPrueba = "¡Vuelve a intentarlo! ¡Tu puedes!";
                }

                //Se prepara la Interfaz
                SessionManager.instance.sucesionDeJuegos.RemoveAt(0);

                //Editando titulo de la pantalla final
                int completed = SessionManager.instance.totalGamesInSession - SessionManager.instance.sucesionDeJuegos.Count;
                finishingPanel.transform.Find("Panel/Title").gameObject.GetComponent<Text>().text = "¡Terminado! " + completed + " / " + SessionManager.instance.totalGamesInSession;

                //Editando texto de resultado
                //finishingPanel.transform.Find("Panel/Information/PointsText").gameObject.GetComponent<Text>().text = "Resultado: ";
                finishingPanel.transform.Find("Panel/Information/PointsText/ResolutionText").gameObject.GetComponent<Text>().text = resultadoPrueba;

                //Editando texto de resultado adicional
                finishingPanel.transform.Find("Panel/Information/AdditionalText").gameObject.GetComponent<Text>().text = "Virus eliminados: " + UI_Manager.destroyedPoints;

                //Editando texto de puntos
                //finishingPanel.transform.Find("Panel/Information/TimeText").gameObject.GetComponent<Text>().text = "Errores cometidos: " + UI_Manager.playerErrors;
                finishingPanel.transform.Find("Panel/Information/TimeText").gameObject.GetComponent<Text>().text = "Puntos totales: "
                    + SessionManager.instance.puntosTotales; // + "/" + SessionManager.instance.puntosAConseguir;

                //Editando las funciones de los botones y su texto correspondiente
                finishingPanel.transform.Find("Panel/Buttons/NextButton").gameObject.GetComponent<Button>().onClick.AddListener(() => SessionManager.instance.chargeNextScene());
                finishingPanel.transform.Find("Panel/Buttons/NextButton/Information2").GetComponent<Text>().text = "Siguiente";
            }
        }
    }

    public void dirtyPointErased()
    {
        actualPointsInScene--;
        if(actualPointsInScene <= 0)
        {
            CreateDirtyPointsInScene();
        }
    }

    void replayMatch()
    {
        SceneManager.LoadScene("Pipes");
    }

    private void ActualizarTiempo()
    {
        gameplayTime += Time.deltaTime;
        float timeToShow = totalTimeToPlay - gameplayTime;
        if (timeToShow <= 0)
        {
            timeToShow = 0;
        }
        UI_Manager.ActualizarTiempo(timeToShow);
    }
}
