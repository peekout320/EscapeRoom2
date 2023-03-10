using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cinemachine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// UIにまつわる機能を管理するクラス
/// Hierarchy:"UIManager"にアタッチ
/// </summary>
public class UIManager : MonoBehaviour
{
    private Tween tween;
    public Tween Tween { get => tween; }

    private GameManager gameManager;
    private AudioManager audioManager;
    private ItemManager itemManager;
    private ClickManager click;

    [SerializeField]
    private CameraManager camManager;

    public IntReactiveProperty[] displayNumbers;
    public StringReactiveProperty[] displayTelops;

    public ReactiveProperty<Sprite> displayItemSprite;

    public ReactiveProperty<string> txtQueriSwitch = new ReactiveProperty<string>();

    [SerializeField]
    private bool queriswitch;

    [SerializeField]
    private QueriChanController queriController;

    [SerializeField]
    private Fade fade;

    [SerializeField]
    private FadeImage fadeImg;

    [SerializeField]
    private Text txtTelop;
    public Text TxtTelop { get => txtTelop; }

    [SerializeField]
    private Image imgCenter;
    public Image ImagCenter { get => imgCenter; }

    [SerializeField]
    private Button btnReturn;
    public　Button BtnReturn { get => btnReturn; }

    [SerializeField]
    private Slider[] sliders;
    public Slider[] Sliders { get => sliders; }

    [SerializeField]
    private GameObject player;
    public GameObject Player { get => player; }

    /// <summary>
    /// テロップ表示で使用する為にItemManagerクラスを事前に取得する
    /// </summary>
    /// <param name="itemManager"></param>
    public void SetupUIManager1(ItemManager itemManager)
    {
        this.itemManager = itemManager;
    }

    /// <summary>
    /// Presenterクラスを取得する
    /// </summary>
    public void SetupUIManager2(ClickManager click)
    {
        this.click = click;
    }

    /// <summary>
    /// NumberBoardの数値の変化を設定
    /// </summary>
    /// <param name="index"></param>
    public async UniTask ChangeNumberModel(int index)
    {
        displayNumbers[index].Value += 1;

        if(displayNumbers[index].Value > 9)
        {
            displayNumbers[index].Value = 0;
        }

        audioManager.PlaySE(3);

        await gameManager.JudgeNumberBoard();
        await gameManager.JudgeNumberBoard_2();
    }

    /// <summary>
    /// スライダーをクリックした時の設定
    /// </summary>
    /// <param name="index"></param>
    public async UniTask ChangeSliderValueModel(int index)
    {
        sliders[index].value += 1;

        if(sliders[index].value > 5)
        {
            sliders[index].value = 0;
        }

        audioManager.PlaySE(4);

        await gameManager.JudgeSliderBoard();
        await gameManager.JudgeSliderBoard_2();
    }

    /// <summary>
    /// テロップを表示する
    /// </summary>
    /// <param name="index"></param>
    public void DisplayTelopModel(int index,int seconds)
    {
        txtTelop.text = displayTelops[index].Value;

        DOVirtual.DelayedCall(seconds, () =>
        {
            txtTelop.text = null;
        });
    }

    /// <summary>
    /// 表示されたアイテムイメージを消す
    /// </summary>
    public void DisCenterImage()
    {
        imgCenter.enabled = false;
        displayItemSprite.Value = null;
    }

    /// <summary>
    /// 操作クエリちゃんの操作を切り替えるON/OFF
    /// </summary>
    /// <param name="on_off"></param>
    public void SwitchQueriController()
    {
        if (!queriswitch)
        {
            //クエリちゃんを移動不可に
            queriController.RigidQueri.isKinematic = true;

            //テキストをONに変更
            txtQueriSwitch.Value = "ON";

            //クリック機能をオンにする
            for(int i = 1; i < click.telopTriggerList.Count; i++)
            {
                click.telopTriggerList[i].enabled = true;
            }
            for (int i = 0; i < click.ChangeCameraTriggers.Length; i++)
            {
                click.ChangeCameraTriggers[i].enabled = true;

            }
            //カメラを中央に戻すボタンを使用可に
            btnReturn.gameObject.SetActive(true);

            //プレイヤーを非表示
            player.SetActive(false);

            //カメラを中央へ戻す
            camManager.VCams[9].Priority -= 10;

            //BGM切り替え
            audioManager.ChangeBGM(0);

            queriswitch = true;
        }

        else
        {
            //クエリちゃんを移動可に
            queriController.RigidQueri.isKinematic = false;

            //テキストをONに変更
            txtQueriSwitch.Value = "OFF";

            //クリック機能をオフにする
            for (int i = 0; i < click.telopTriggerList.Count; i++)
            {
                click.telopTriggerList[i].enabled = false;
            }
            for (int i = 0; i < click.ChangeCameraTriggers.Length; i++)
            { 
                click.ChangeCameraTriggers[i].enabled = false;
            }

            //カメラを中央に戻すボタンを使用可に
            btnReturn.gameObject.SetActive(false);

            //プレイヤーを表示
            player.SetActive(true);

            //カメラをクエリちゃんPOVカメラへ戻す
            camManager.VCams[9].Priority += 10;

            //BGM切り替え
            audioManager.ChangeBGM(1);

            queriswitch = false;
        }
    }

    /// <summary>
    /// UI(ボタン)を点滅させる(gameManagerとAudioManagerコンポーネントをついでに取得する)
    /// </summary>
    /// <param name="btn"></param>
    public void FlashButton(Button btn,GameManager gameManager,AudioManager audioManager)
    {
        this.gameManager = gameManager;
        this.audioManager = audioManager;

        tween = btn.image.DOFade(1, 0.3f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(btn.gameObject);
        tween.Pause();
    }

    /// <summary>
    /// UI(テキスト)を点滅させる
    /// </summary>
    /// <param name="txt"></param>
    public void FlashText(Text txt)
    {
        tween = txt.DOFade(1, 0.3f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetLink(txt.gameObject);
    }

    /// <summary>
    /// シーン遷移　フェードアウト
    /// </summary>
    /// <param name="texture"></param>
    public void FadeOutScreen(Texture texture)
    {
        fadeImg.UpdateMaskTexture(texture);

        fade.FadeOut(4);
    }
}
