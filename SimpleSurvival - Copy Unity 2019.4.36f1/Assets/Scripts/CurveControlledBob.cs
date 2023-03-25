using System;
using UnityEngine;
using Cinemachine;


[Serializable]
public class CurveControlledBob
{
    public float HorizontalBobRange = 0.33f;
    public float VerticalBobRange = 0.33f;
    public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                        new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                        new Keyframe(2f, 0f)); // sin curve for head bob

    public AnimationCurve HorizBobcurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0f),
                                                    new Keyframe(1f, -1f), new Keyframe(1.5f, 0f),
                                                    new Keyframe(2f, 1f)); // cos curve for horiz head bob

    public AnimationCurve stepCurve = new AnimationCurve();
    public float stepBuffer = 0.001f;

    public float VerticaltoHorizontalRatio = 1f;

    public HeadBob headBob;

    public float step1Time;
    public float step2Time;

    private float m_CyclePositionX;
    private float m_CyclePositionY;
    private float m_BobBaseInterval;
    private Vector3 m_OriginalCameraPosition;
    private float m_Time;


    //public void Setup(Camera camera, float bobBaseInterval)
    public void Setup(CinemachineVirtualCamera camera, float bobBaseInterval)
    {
        m_BobBaseInterval = bobBaseInterval;
        m_OriginalCameraPosition = camera.transform.localPosition;

        // get the length of the curve in time
        m_Time = Bobcurve[Bobcurve.length - 1].time;
    }


    public Vector3 DoHeadBob(float speed)
    {
        //float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX)*HorizontalBobRange);
        float xPos = m_OriginalCameraPosition.x + (HorizBobcurve.Evaluate(m_CyclePositionX)*HorizontalBobRange);
        float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY)*VerticalBobRange);

        if((1 - stepCurve.Evaluate(m_CyclePositionX)) <= stepBuffer)
        {
            headBob.Step();
        }

        m_CyclePositionX += (speed*Time.deltaTime)/m_BobBaseInterval;
        m_CyclePositionY += ((speed*Time.deltaTime)/m_BobBaseInterval)*VerticaltoHorizontalRatio;

        if (m_CyclePositionX > m_Time)
        {
            m_CyclePositionX = m_CyclePositionX - m_Time;
        }
        if (m_CyclePositionY > m_Time)
        {
            m_CyclePositionY = m_CyclePositionY - m_Time;
        }

        return new Vector3(xPos, yPos, 0f);
    }

    public void ResetCyclePosition()
    {
        m_CyclePositionX = m_CyclePositionY = 0f;
    }

    public void SetBobBaseInterval(float bobBaseInterval)
    {
        m_BobBaseInterval = bobBaseInterval;
    }
}