using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine {
    States currentState;
    private int maxStateNumber;

    public StateMachine() {
        currentState = States.WaitingForInput;
        maxStateNumber = Enum.GetValues(typeof(States)).Length-1;
    }

    public States NextState() {
        currentState += 1;
        if ((int)currentState > maxStateNumber) {
            currentState = States.WaitingForInput;
        }
        return currentState;
    }
}
