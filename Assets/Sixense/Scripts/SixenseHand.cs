
//
// Copyright (C) 2013 Sixense Entertainment Inc.
// All Rights Reserved
//

using UnityEngine;
using System.Collections;

public class SixenseHand : MonoBehaviour
{
	public SixenseHands	m_hand;
	public SixenseInput.Controller m_controller = null;
    
	float 		m_fLastTriggerVal;
	Vector3		m_initialPosition;
	Quaternion 	m_initialRotation;    

	protected void Start() 
	{
        m_initialPosition = gameObject.transform.position;
        m_initialRotation = gameObject.transform.rotation;
    }


	protected void Update()
	{
		if ( m_controller == null )
		{
			m_controller = SixenseInput.GetController( m_hand );
		}

		else
		{
			UpdateHand();
		}
	}
	
	
	// Updates the object from controller input.
	protected void UpdateHand()
	{
        // Right Controller Inputs
        if (m_hand == SixenseHands.RIGHT)
        {
            if (m_controller.GetButton(SixenseButtons.START)) { }
            else if (m_controller.GetButton(SixenseButtons.ONE)) { }
            else if (m_controller.GetButton(SixenseButtons.TWO)) { }
            else if (m_controller.GetButton(SixenseButtons.THREE)) { }
            else if (m_controller.GetButton(SixenseButtons.FOUR)) { }
            else if (m_controller.GetButton(SixenseButtons.BUMPER)) { }
            else if (m_controller.GetButton(SixenseButtons.JOYSTICK)) { }
            else if (m_controller.GetButton(SixenseButtons.TRIGGER)) { }            
        }

        // Left Controller Inputs
        if (m_hand == SixenseHands.LEFT)
        {
            if (m_controller.GetButton(SixenseButtons.START)) { }
            else if (m_controller.GetButton(SixenseButtons.ONE)) { }
            else if (m_controller.GetButton(SixenseButtons.TWO)) { }
            else if (m_controller.GetButton(SixenseButtons.THREE)) { }
            else if (m_controller.GetButton(SixenseButtons.FOUR)) { }
            else if (m_controller.GetButton(SixenseButtons.BUMPER)) { }
            else if (m_controller.GetButton(SixenseButtons.JOYSTICK)) { }
            else if (m_controller.GetButton(SixenseButtons.TRIGGER)) { }
        }

    }


	public Quaternion InitialRotation
	{
		get { return m_initialRotation; }
	}
	
	public Vector3 InitialPosition
	{
		get { return m_initialPosition; }
	}
}

