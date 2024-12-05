using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class AssignAimTarget : MonoBehaviour
{
    [SerializeField]
    private MultiAimConstraint headAimConstraint;

    [SerializeField]
    private MultiAimConstraint chestAimConstraint;

    private void Start()
    {
        StartCoroutine(AssignAimConstraintSources());
    }

    private IEnumerator AssignAimConstraintSources()
    {
        while (Camera.main == null)
        {
            yield return null; // Wait for the next frame
        }
        
        Transform cameraTransform = Camera.main.transform;
        
        // Assign camera to HeadAim
        if (headAimConstraint != null)
        {
            AssignSourceToConstraint(headAimConstraint, cameraTransform);
            Debug.Log("Main Camera assigned - HeadAim");
        }
        else
        {
            Debug.LogError("HeadAimConstraint is not assigned in the inspector.");
        }
        
        // Assign camera to ChestAim
        if (chestAimConstraint != null)
        {
            AssignSourceToConstraint(chestAimConstraint, cameraTransform);
            Debug.Log("ChestAimConstraint: Assigned source to " + cameraTransform.name);
        }
        else
        {
            Debug.LogError("Main Camera assigned - ChestAim");
        }
    }

    private void AssignSourceToConstraint(MultiAimConstraint constraint, Transform sourceTransform)
    {
        // Get the current list of sources
        var sourceObjects = constraint.data.sourceObjects;
        
        sourceObjects.Clear();

        // Add the new source
        WeightedTransform weightedSource = new WeightedTransform(sourceTransform, 1f);
        sourceObjects.Add(weightedSource);

        // Apply the changes
        constraint.data.sourceObjects = sourceObjects;
    }
}
