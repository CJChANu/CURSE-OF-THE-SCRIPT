using UnityEngine;
using System.Globalization; // For string formatting if needed, though System.String.Format is usually fine

public class DayNightCycleController : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Duration of a full day-night cycle in real-time seconds.")]
    public float dayDurationInSeconds = 120f; // 2 minutes for a full day for testing

    [Tooltip("Current time of day, normalized (0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset, 1 = midnight)")]
    [Range(0f, 1f)]
    public float currentTimeNormalized = 0.25f; // Start at sunrise

    [Tooltip("Multiplier for time speed. >1 accelerates, <1 decelerates, 0 pauses.")]
    public float timeScaleMultiplier = 1.0f;

    [Tooltip("Set to true to reverse the flow of time.")]
    public bool isTimeReversed = false;

    [Header("Sun Settings")]
    [Tooltip("The Directional Light representing the sun.")]
    public Light sunLight;
    [Tooltip("Base intensity of the sun. The Sun Intensity Curve will modulate this value.")]
    public float baseSunIntensity = 1.0f; // Added for master intensity control


    [Header("Lighting Adjustments")]
    [Tooltip("Curve to control sun intensity MULTIPLIER over the day (X-axis: time 0-1, Y-axis: multiplier 0-max). This multiplies the Base Sun Intensity.")]
    public AnimationCurve sunIntensityCurve; // Now acts as a multiplier

    [Tooltip("Gradient to control sun color over the day (Time 0-1).")]
    public Gradient sunColorGradient;

    [Tooltip("Curve to control ambient light intensity over the day.")]
    public AnimationCurve ambientIntensityCurve;

    [Tooltip("Gradient to control ambient light color over the day.")]
    public Gradient ambientColorGradient;


    [Header("Shadow Settings")]
    [Tooltip("If checked, shadows are enabled (simulates 'Is Day' for shadow rendering). If unchecked, shadows are disabled.")]
    public bool isDay_EnableShadows = true;

    [Header("Informational Display (Read-Only)")]
    [Tooltip("Current time of day formatted as HH:MM.")]
    [SerializeField] // To make it visible in the inspector despite private set
    private string currentTimeString;
    [Tooltip("Current calculated sun intensity (Base * Curve).")]
    [SerializeField]
    private float currentSunIntensityDebug;


    void Start()
    {
        // Ensure a sun light is assigned
        if (sunLight == null)
        {
            // Updated to use FindFirstObjectByType as per Unity's recommendation
            sunLight = FindFirstObjectByType<Light>(); 
            if (sunLight != null && sunLight.type != LightType.Directional)
            {
                Debug.LogWarning("DayNightCycleController: Found a Light component, but it's not a Directional Light. Please assign a Directional Light manually.");
                sunLight = null;
            }

            if (sunLight == null)
            {
                Debug.LogError("DayNightCycleController: No Directional Light assigned or found in the scene. Please assign one for the cycle to work.");
                enabled = false; // Disable the script
                return;
            }
            else
            {
                Debug.Log("DayNightCycleController: Sun Light was not assigned in the Inspector. Automatically found and assigned: " + sunLight.name);
            }
        }
        
        // Initial update to set everything correctly at the start
        UpdateDayNightCycle(currentTimeNormalized);
    }

    void Update()
    {
        if (sunLight == null) return; // Don't run if no sun assigned

        // Calculate how much normalized time has passed this frame
        float timeDeltaThisFrame = (Time.deltaTime / dayDurationInSeconds) * timeScaleMultiplier;

        if (isTimeReversed)
        {
            currentTimeNormalized -= timeDeltaThisFrame;
        }
        else
        {
            currentTimeNormalized += timeDeltaThisFrame;
        }

        // Wrap currentTimeNormalized to keep it within the 0-1 range
        if (currentTimeNormalized >= 1f)
        {
            currentTimeNormalized -= 1f;
        }
        else if (currentTimeNormalized < 0f)
        {
            currentTimeNormalized += 1f;
        }

        // Apply all visual updates based on the new time
        UpdateDayNightCycle(currentTimeNormalized);
    }

    void UpdateDayNightCycle(float timeNormalized)
    {
        // --- Sun Rotation ---
        float sunXRotation = (timeNormalized * 360f) - 90f;
        
        if (sunLight != null)
        {
            sunLight.transform.localEulerAngles = new Vector3(sunXRotation, sunLight.transform.localEulerAngles.y, sunLight.transform.localEulerAngles.z);
        }


        // --- Sun Intensity & Color ---
        if (sunLight != null) 
        {
            float curveMultiplier = 1f; // Default to 1 if curve is not set up
            if (sunIntensityCurve != null && sunIntensityCurve.keys.Length > 0)
            {
                curveMultiplier = sunIntensityCurve.Evaluate(timeNormalized);
            }
            sunLight.intensity = baseSunIntensity * curveMultiplier;
            currentSunIntensityDebug = sunLight.intensity; // For inspector debugging

            if (sunColorGradient != null)
            {
                sunLight.color = sunColorGradient.Evaluate(timeNormalized);
            }
        }


        // --- Ambient Light ---
        if (ambientIntensityCurve != null && ambientIntensityCurve.keys.Length > 0)
        {
            RenderSettings.ambientIntensity = ambientIntensityCurve.Evaluate(timeNormalized);
        }
        if (ambientColorGradient != null)
        {
            RenderSettings.ambientLight = ambientColorGradient.Evaluate(timeNormalized);
            if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tint"))
            {
                 RenderSettings.skybox.SetColor("_Tint", ambientColorGradient.Evaluate(timeNormalized));
            }
        }


        // --- Shadows ---
        if (sunLight != null) 
        {
            sunLight.shadows = isDay_EnableShadows ? LightShadows.Soft : LightShadows.None;
        }


        // --- Update Current Time String (for Inspector display) ---
        float totalHoursInDay = timeNormalized * 24f;
        int hours = Mathf.FloorToInt(totalHoursInDay) % 24; 
        int minutes = Mathf.FloorToInt((totalHoursInDay - Mathf.Floor(totalHoursInDay)) * 60f);
        currentTimeString = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    // --- Public Methods for Control ---

    public void SetTimeScaleMultiplier(float multiplier)
    {
        timeScaleMultiplier = Mathf.Max(0, multiplier); 
    }

    public void SetTimeReversed(bool reverse)
    {
        isTimeReversed = reverse;
    }

    public void SetIsDay_EnableShadows(bool enable)
    {
        isDay_EnableShadows = enable;
        if (sunLight != null) 
        {
            sunLight.shadows = isDay_EnableShadows ? LightShadows.Soft : LightShadows.None;
        }
    }

    public void SetCurrentTimeNormalized(float time)
    {
        currentTimeNormalized = Mathf.Clamp01(time); 
        if (Application.isPlaying && sunLight != null) 
        {
            UpdateDayNightCycle(currentTimeNormalized);
        }
    }

    // Optional: Gizmos for visualizing sun direction in the Editor
    void OnDrawGizmosSelected()
    {
        if (sunLight == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(sunLight.transform.position, sunLight.transform.forward * 10f);
    }
}
