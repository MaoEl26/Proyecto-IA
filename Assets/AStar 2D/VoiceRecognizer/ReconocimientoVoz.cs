using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System;
[System.Serializable]
public class ReconocimientoVoz
{

    public Dictionary<string, Action> actions = new Dictionary<string, Action>();
    public List<String> dic = new List<String>();    

}
