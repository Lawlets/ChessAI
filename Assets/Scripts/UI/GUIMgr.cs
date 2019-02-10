using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Simple GUI display : scores and team turn
 */

public class GUIMgr : MonoBehaviour {

    #region singleton
    static GUIMgr instance = null;
    public static GUIMgr Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GUIMgr>();
            return instance;
        }
    }
    #endregion

    Transform whiteToMoveTr = null;
    Transform blackToMoveTr = null;
    Text whiteScoreText = null;
    Text blackScoreText = null;

    // Use this for initialization
    void Awake()
    {
        whiteToMoveTr = transform.Find("WhiteTurnText");
        blackToMoveTr = transform.Find("BlackTurnText");

        whiteToMoveTr.gameObject.SetActive(false);
        blackToMoveTr.gameObject.SetActive(false);

        whiteScoreText = transform.Find("WhiteScoreText").GetComponent<Text>();
        blackScoreText = transform.Find("BlackScoreText").GetComponent<Text>();

        ChessGameMgr.Instance.OnPlayerTurn += DisplayTurn;
        ChessGameMgr.Instance.OnScoreUpdated += UpdateScore;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void DisplayTurn(bool isWhiteMove)
    {
        whiteToMoveTr.gameObject.SetActive(isWhiteMove);
        blackToMoveTr.gameObject.SetActive(!isWhiteMove);
    }

    void UpdateScore(uint whiteScore, uint blackScore)
    {
        whiteScoreText.text = string.Format("White : {0}", whiteScore);
        blackScoreText.text = string.Format("Black : {0}", blackScore);
    }
}
