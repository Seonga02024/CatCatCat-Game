using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_manager : MonoBehaviour
{
    public GameObject ui4;
    public GameObject result;
    public Text resultnum;
    public Text resultment;
    public int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Offui4Image()
    {
        ui4.SetActive(false);
    }

    public void Onui4Image()
    {
        ui4.SetActive(true);
    }

    public void OffresultImage()
    {
        result.SetActive(false);
        SceneManager.LoadScene(0);
    }

    public void OnresultImage()
    {
        resultnum.text = score.ToString();
        if(score > 1000)
        {
            resultment.text = "상위 10%";
        }
        else if(score > 700)
        {
            resultment.text = "상위 30%";
        }
        else if (score > 500)
        {
            resultment.text = "상위 50%";
        }
        else if (score > 300)
        {
            resultment.text = "상위 70%";
        }
        else
        {
            resultment.text = "상위 100%";
        }
        result.SetActive(true);
    }
}
