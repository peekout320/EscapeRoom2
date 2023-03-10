using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private ItemManager itemManager;
    [SerializeField]
    private CameraManager camManager;
    [SerializeField]
    private AudioManager gunAudio;
    [SerializeField]
    private ClickManager click;

    [SerializeField]
    private PlayerController player;
    [SerializeField]
    private BoxCollider playerCollider;

    [SerializeField]
    private Animator skeleton;

    [SerializeField]
    private Image imgItemFolder;

    [SerializeField]
    private GameObject gun;

    [SerializeField]
    private Animator gunAnime;

    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform bulletTran;

    private GameObject bullet;

    [SerializeField]
    private Transform targetTran;

    [SerializeField]
    private ParticleSystem gunPartical;
    [SerializeField]
    private ParticleSystem hitPartical;


    private void Start()
    {
        CollisionEvent();

        //StartCoroutine(GunAction());
    }

    /// <summary>
    /// 銃を撃つ演出
    /// </summary>
    /// <returns></returns>
    public async UniTask GunAction()
    {
        //１度しかイベントが起きないようにコライダーを切る
        playerCollider.enabled = false;

        //各UIを非表示する
        uiManager.BtnReturn.enabled = false;
        imgItemFolder.gameObject.SetActive(false);
        click.QueriLogoTrigger.gameObject.SetActive(false);

        gun.SetActive(true);

        gunAudio.ChangeBGM(2);

        //カメラ切り替え
        camManager.VCams[11].Priority += 30;

        //銃を構えて撃つ
        gunAnime.SetTrigger("gunshoot");

        await UniTask.Delay(2000);

        gunAudio.PlaySE(23);
    }

    /// <summary>
    /// 弾丸を飛ばしてガイコツを倒す演出まで。AnimationEventで実行する
    /// </summary>
    public async UniTask ShootingBullet()
    {
        //発砲エフェクト
        gunPartical.Play();

        gunAudio.PlaySE(14);

        //弾丸生成
        bullet = Instantiate(bulletPrefab, bulletTran, false);

        //Transform型をVector3型にキャストする
        Vector3 target = targetTran.transform.position;

        //生成された弾丸が回転しながら目標に向かってまっすぐ飛ぶ
        bullet.transform.DOMove(target, 7);

        //目標目線のカメラに切り替え
        camManager.VCams[12].Priority += 50;

        await UniTask.Delay(1000);

        //カメラを切り替えつつスローモーションにする
        Time.timeScale = 0.3f;

        gunAudio.PlaySE(15);

        camManager.VCams[13].Priority += 60;

        await UniTask.Delay(1000);

        Time.timeScale = 3;

        await UniTask.Delay(3000);

        //目標にヒット
        gunAudio.PlaySE(16);

        //スローモーション解除
        Time.timeScale = 1;

        await UniTask.Delay(1000);

        Destroy(bullet);

        //ヒット時の演出
        hitPartical.Play();

        skeleton.SetTrigger("dead");

        gunAudio.PlaySE(17);

        await UniTask.Delay(4000);

        gunAudio.PlaySE(18);

        //ガイコツを削除
        Destroy(skeleton.gameObject);

        await UniTask.Delay(2000);

        //ゲームクリア演出
        await gameManager.GameClear();
        
    }

    /// <summary>
    /// Playerに接触時にイベント開始
    /// </summary>
    private void CollisionEvent()
    {
        player.OnCollisionEnterAsObservable()
            .Subscribe(_ => itemManager.UseItem());
    }
}
