using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System;

public class ReconocimientoVoz : MonoBehaviour
{
    public KeywordRecognizer keywordRecognizer;
    public KeywordRecognizer keywordRecognizer2;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private List<String> dic = new List<String>();
    float xOriginal = 0;
    float yOriginal = 0;
    private int x = 0;
    private int y = 0;

    

    void Start()
    {
        actions.Add("Mover", Move);
        xOriginal = transform.position.x;
        yOriginal = transform.position.y;

        for (int i = -10; i < 11; i++)
        {
            dic.Add(Convert.ToString(i));
        }
        /*
        foreach(KeyValuePair<String, Action> action in actions)
        {
            dic.Add(action.Key);
        }
        */

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;


        keywordRecognizer2 = new KeywordRecognizer(dic.ToArray());
        keywordRecognizer2.OnPhraseRecognized += RecognizedSpeechX;
        keywordRecognizer.Start();
    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        actions[speech.text].Invoke();
    }

    private void RecognizedSpeechX(PhraseRecognizedEventArgs speech)
    {
        FindObjectOfType<AudioManager>().Play("SpidermanDestinoColumna");
        Debug.Log(speech.text);
        x = Convert.ToInt32(speech.text);
        keywordRecognizer2.OnPhraseRecognized -= RecognizedSpeechX;
        keywordRecognizer2.OnPhraseRecognized += RecognizedSpeechY;
    }
    private void RecognizedSpeechY(PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        y = Convert.ToInt32(speech.text);
        keywordRecognizer2.Stop();
        Mueve(x, y);
        keywordRecognizer2.OnPhraseRecognized -= RecognizedSpeechY;
        keywordRecognizer2.OnPhraseRecognized += RecognizedSpeechX;
    }

    private void Move()
    {
        FindObjectOfType<AudioManager>().Play("SpidermanDestinoFila");
        keywordRecognizer2.Start();
    }

    private void Mueve(int x, int y)
    {
        transform.position = new Vector3(xOriginal, yOriginal, 0);
        transform.Translate(x, -y, 0);
    }

}
