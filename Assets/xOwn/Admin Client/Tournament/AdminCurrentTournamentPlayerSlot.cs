﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdminUI { 

    public class AdminPreTournamentPlayerSlot : MonoBehaviour {

        public Text playerName;

        public void init(string name) {
            playerName.text = name;
        }

    }
}