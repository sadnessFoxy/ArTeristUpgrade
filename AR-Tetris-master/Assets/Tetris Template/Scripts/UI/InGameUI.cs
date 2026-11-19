
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour {

	public Text score;
    public Text highScore;
    public Text scoreLabel;
    public Text highScoreLabel;
    public Text nextLabel;
    
    public GameObject gameOverPopUp;

    public void UpdateScoreUI()
	{
        score.text = Managers.Score.currentScore.ToString();
        highScore.text = Managers.Score.highScore.ToString();
    }

    public void InGameUIStartAnimation()
    {
        SimpleTween.AnchorPosY(scoreLabel.rectTransform, -334, 1, true);
        SimpleTween.AnchorPosY(highScoreLabel.rectTransform, -334, 1, true);
        SimpleTween.AnchorPosY(score.rectTransform, -375, 1, true);
        SimpleTween.AnchorPosY(highScore.rectTransform, -375, 1, true);
        SimpleTween.AnchorPosY(nextLabel.rectTransform, 900, 1, true);
    }

    public void InGameUIEndAnimation()
    {
        SimpleTween.AnchorPosY(scoreLabel.rectTransform, -334+650, 0.3f, true);
        SimpleTween.AnchorPosY(highScoreLabel.rectTransform, -334 + 650, 0.3f, true);
        SimpleTween.AnchorPosY(score.rectTransform, -375 + 650, 0.3f, true);
        SimpleTween.AnchorPosY(highScore.rectTransform, -375 + 650, 0.3f, true);
        SimpleTween.AnchorPosY(nextLabel.rectTransform, 900 + 650, 0.3f, true);
    }


}
