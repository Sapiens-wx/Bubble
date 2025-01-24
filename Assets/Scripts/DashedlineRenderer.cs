using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashedlineRenderer : MonoBehaviour
{
    
    public LineRenderer lineRenderer;
    public float dashLength = 0.1f;
    public float gapLength = 0.1f;

    private void Start()
    {
        DrawDashedLine();
    }

    private void DrawDashedLine()
    {
        Vector3[] positions = new Vector3[2];
        positions[0] = transform.position;
        positions[1] = transform.position + transform.forward * 10f;

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        float totalLength = Vector3.Distance(positions[0], positions[1]);
        float currentLength = 0f;
        bool isDash = true;
        int pointIndex = 0;

        while (currentLength < totalLength)
        {
            if (isDash)
            {
                float dashEndLength = Mathf.Min(currentLength + dashLength, totalLength);
                Vector3 dashEndPoint = positions[0] + (positions[1] - positions[0]).normalized * dashEndLength;
                lineRenderer.positionCount = pointIndex + 2;
                lineRenderer.SetPosition(pointIndex, positions[0] + (positions[1] - positions[0]).normalized * currentLength);
                lineRenderer.SetPosition(pointIndex + 1, dashEndPoint);
                pointIndex += 2;
                currentLength = dashEndLength;
            }
            else
            {
                currentLength += gapLength;
            }
            isDash = !isDash;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Calculate the position of the collision point on the line segment
            Vector3 collisionPoint = collision.contacts[0].point;
            Vector3 startPoint = lineRenderer.GetPosition(0);
            Vector3 endPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            Vector3 direction = (endPoint - startPoint).normalized;
            float distanceToStart = Vector3.Dot(collisionPoint - startPoint, direction);

            // Split line segment
            int newPositionCount = lineRenderer.positionCount * 2;
            Vector3[] newPositions = new Vector3[newPositionCount];
            int leftIndex = 0;
            int rightIndex = lineRenderer.positionCount;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                Vector3 position = lineRenderer.GetPosition(i);
                float distance = Vector3.Dot(position - startPoint, direction);
                if (distance < distanceToStart)
                {
                    newPositions[leftIndex++] = position;
                }
                else
                {
                    newPositions[rightIndex++] = position;
                }
            }

            lineRenderer.positionCount = newPositionCount;
            lineRenderer.SetPositions(newPositions);
        }
    }
}

