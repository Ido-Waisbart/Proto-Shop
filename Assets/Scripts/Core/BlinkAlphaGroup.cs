using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BlinkAlphaGroup : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    public float startFlickerRate = 0.25f;
    private float currentFlickerRate;

    private float currentTickTime;

    public float totalFlickerTime = 1f;
    private float flickerTimeStart;
    private float flickerTimeEnd;

    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        currentFlickerRate = startFlickerRate;

        flickerTimeStart = Time.time;
        flickerTimeEnd = Time.time + totalFlickerTime;

        currentTickTime = Time.time;
    }

    void Update()
    {
        if (Time.time >= flickerTimeEnd)
            Destroy(gameObject);

        // a%b, Time.time - flickerTimeStart % currentFlickerRate 
        //float a = Time.time - flickerTimeStart;
        //float b = currentFlickerRate;
        if (Time.time - currentTickTime > currentFlickerRate)
        {
            _canvasGroup.alpha = 1 - _canvasGroup.alpha;  // 0 -> 1, 1 -> 0.
            currentTickTime = Time.time;
        }

        float t = (Time.time - flickerTimeStart) / totalFlickerTime;
        currentFlickerRate = Mathf.Lerp(startFlickerRate, 0, t);
    }
}
