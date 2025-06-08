// Imported from Night Mood.
// Provides basic functionality for the lerping of transform.position, given several transforms in a 2D scene.
// New addition: Lerp method.

using System;
using System.Collections;
using UnityEngine;
// using UnityEngine.Events;

public class SimplePositionLerper : MonoBehaviour
{
    public void LerpForHalfSecond(Transform target)
        => GlobalMasterManager.Main.StartCoroutine(ILerp(transform, transform.position, target.position, 0.5f));
    public void LerpFor1Second(Transform target)
        => GlobalMasterManager.Main.StartCoroutine(ILerp(transform, transform.position, target.position, 1));
    // Use the script's currently assigned props.
    public void LerpWithAssignedProps()
        => GlobalMasterManager.Main.StartCoroutine(ILerp(transform, transform.position, transform.position + LerpOnStart_deltaPosition, LerpOnStart_deltaTime, LerpOnStart_LerpMethod, rb));
    public void Lerp(Transform target, float deltaTime = 1, LerpMethod lerpMethod = LerpMethod.Linear)
        => GlobalMasterManager.Main.StartCoroutine(ILerp(transform, transform.position, target.position, deltaTime, lerpMethod));
    public void Lerp(Vector3 targetPosition, float deltaTime = 1, LerpMethod lerpMethod = LerpMethod.Linear)
        => GlobalMasterManager.Main.StartCoroutine(ILerp(transform, transform.position, targetPosition, deltaTime, lerpMethod));
    public static void Lerp(Transform lerpedObject, Vector3 sourcePos, Vector3 targetPos, float deltaTime = 1)
        => GlobalMasterManager.Main.StartCoroutine(ILerp(lerpedObject, sourcePos, targetPos, deltaTime));

    [SerializeField] protected bool LerpOnStart = false;
    // [SerializeField] protected Transform LerpOnStart_source;
    [SerializeField] protected Vector3 LerpOnStart_deltaPosition;  // Useful for this specific case
    // [SerializeField] protected Vector3 LerpOnStart_target;  // Useful for this specific case
    // [SerializeField] protected Transform LerpOnStart_shouldOrphanizeTarget;  // Useful for this specific case
    [SerializeField] protected float LerpOnStart_deltaTime;
    // [SerializeField] protected UnityEvent LerpOnStart_onFinish;  // A nifty idea. A bit too extra.
    [SerializeField] protected LerpMethod LerpOnStart_LerpMethod;
    [SerializeField, Tooltip("Nullable.")] protected Rigidbody rb;  // May be useful if physics and collisions are an important part of the equation. Like in this case, when a basket lerps away with items in it, it might cause items to slip through the basket's floor.

    public enum LerpMethod{
        Linear,
        EaseInOut
    }

    void Start(){
        if (LerpOnStart)
        {
            // Lerp(LerpOnStart_target, LerpOnStart_deltaTime, LerpOnStart_LerpMethod);
            LerpWithAssignedProps();
        }
    }

    private IEnumerator ILerp(Vector3 startPosition, Vector3 targetPosition, float deltaTime = 1, LerpMethod lerpMethodToUse = LerpMethod.Linear, Rigidbody rb = null)
        => ILerp(transform, startPosition, targetPosition, deltaTime, lerpMethodToUse, rb);

    private static IEnumerator ILerp(Transform lerpedObject, Vector3 startPosition, Vector3 targetPosition, float deltaTime = 1, LerpMethod lerpMethodToUse = LerpMethod.Linear, Rigidbody rb = null)
    {
        if (startPosition == null
            || targetPosition == null
            || deltaTime <= 0)
        {
            Debug.LogWarning("Improper use of Lerp().");
            yield break;
        }

        float elapsedTime = 0;
        float normalizedElapsedTime = 0;
        float methodAppliedElapsedTime = 0;  // Linear? Ease-in-out?
        float startTime = Time.time;
        Vector3 newPos;

        while (StillHasTime(elapsedTime, deltaTime))
        {
            elapsedTime = Time.time - startTime;
            normalizedElapsedTime = elapsedTime / deltaTime;
            switch(lerpMethodToUse){
                case LerpMethod.Linear:
                    methodAppliedElapsedTime = normalizedElapsedTime;
                    break;
                case LerpMethod.EaseInOut:
                    methodAppliedElapsedTime = (float)-(Math.Cos(Math.PI * normalizedElapsedTime) - 1) / 2;
                    break;
                default:
                    throw new ArgumentException();
            }
            
            newPos = new Vector3(Mathf.SmoothStep(startPosition.x, targetPosition.x, methodAppliedElapsedTime),
                Mathf.SmoothStep(startPosition.y, targetPosition.y, methodAppliedElapsedTime),
                startPosition.z  // For a 2D game.
                );
            if(rb == null) lerpedObject.position = newPos;
            else rb.MovePosition(newPos);

            yield return null;
        }
    }

    private static bool StillHasTime(float elapsedTime, float deltaTimeToPass)
    {
        return elapsedTime < deltaTimeToPass;
    }
}
