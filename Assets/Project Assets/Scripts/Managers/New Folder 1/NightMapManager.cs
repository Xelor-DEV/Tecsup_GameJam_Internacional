using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class NightMapManager : NonPersistentSingleton<NightMapManager>
{
    [Header("Configuration")]
    [SerializeField] private NightMapConfig config;
    [SerializeField] private string sceneToLoad = "Day"; // String para nombre de escena
    [SerializeField] private GameObject timerContainer;

    [Header("Zones")]
    [SerializeField] private List<ZoneDefinition> dayZones;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool showRemainingTime = true;

    private Timer timer;
    private bool timerFinished = false;
    private Dictionary<string, ZoneDefinition> dayZonesDict;

    void Start()
    {
        InitializeDictionaries();
        ApplyDayZoneStates();
        InitializeTimer();
        UpdateTimerDisplay(); // Asegurar que el display esté correcto al inicio
    }

    void Update()
    {
        if (config.currentDay != 0 && timer != null && timer.isRunning)
        {
            timer.Update(Time.deltaTime);
            UpdateTimerDisplay();

            if (!timer.isRunning && !timerFinished)
            {
                timerFinished = true;
                OnTimerCompleted();
            }
        }
    }

    private void InitializeDictionaries()
    {
        dayZonesDict = new Dictionary<string, ZoneDefinition>();
        foreach (var zone in dayZones)
        {
            dayZonesDict[zone.zoneName] = zone;
        }
    }

    private void InitializeTimer()
    {
        if (config.currentDay == 0) return;

        timer = new Timer(config.timerDuration);
        timer.OnTimerEnd += OnTimerCompleted;
        timer.Start();
        timerFinished = false;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (config.currentDay == 0)
        {
            if (timerText != null)
            {
                timerText.text = ""; // Día 0 - texto en blanco
                timerContainer.SetActive(false);
            }
            return;
        }

        if (timerText != null && timer != null)
        {
            timerText.text = showRemainingTime ?
                timer.GetRemainingTimeString() :
                timer.GetTimeString();
        }
    }

    private void OnTimerCompleted()
    {
        Debug.Log("¡Timer completado!");
        StartCoroutine(TransitionToDayScene());
    }

    private IEnumerator TransitionToDayScene()
    {
        // Fade de transparente (0) a negro (1)
        yield return CameraManager.Instance.StartCoroutine(CameraManager.Instance.Fade(0f, 1f));

        // Esperar un momento en negro
        yield return new WaitForSecondsRealtime(0.5f);

        // Cargar la escena del día
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("No se ha especificado el nombre de la escena a cargar");
        }
    }

    private void ApplyDayZoneStates()
    {
        foreach (var dayConfig in config.dayZoneConfigurations)
        {
            if (dayConfig.day == config.currentDay)
            {
                foreach (var zoneState in dayConfig.zoneStates)
                {
                    if (dayZonesDict.TryGetValue(zoneState.zoneName, out ZoneDefinition zone))
                    {
                        foreach (var zoneObject in zone.zoneObjects)
                        {
                            if (zoneObject != null)
                            {
                                zoneObject.SetActive(zoneState.state);
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}
[System.Serializable]
public class Timer
{
    public float duration;
    public float currentTime;
    public bool isRunning;

    public event Action OnTimerEnd;

    public Timer(float duration)
    {
        this.duration = duration;
        this.currentTime = 0;
        this.isRunning = false;
    }

    public void Start()
    {
        currentTime = 0;
        isRunning = true;
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void Reset()
    {
        currentTime = 0;
    }

    public void Update(float deltaTime)
    {
        if (!isRunning) return;

        currentTime += deltaTime;
        if (currentTime >= duration)
        {
            currentTime = duration;
            isRunning = false;
            OnTimerEnd?.Invoke();
        }
    }

    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0, duration - currentTime);
    }

    public string GetRemainingTimeString()
    {
        float remaining = GetRemainingTime();
        int minutes = Mathf.FloorToInt(remaining / 60);
        int seconds = Mathf.FloorToInt(remaining % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}