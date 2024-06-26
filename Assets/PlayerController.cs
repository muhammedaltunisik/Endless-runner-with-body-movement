using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.IO;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameObject Platform;

    public UnityEngine.UI.Button pauseClickButton;   //Pause butonu i�in
    public UnityEngine.UI.Button resumeClickButton;  //Resume butonu i�in
    public UnityEngine.UI.Button restartClickButton;

    private float platformGenisligi;

    public float karakterHizi; // Karakterin hareket h�z�
    public float ziplamaYuksekligi = 10f;
    public float dususHizi = 20f; // Karakterin d���� h�z�
    public float seritGenisligi; // �eritler aras� mesafe

    public float[] hareketKoordinatlari = new float[3];

    private int mevcutSerit = 1; // Ba�lang��ta orta �eritte ba�lar
    private bool isMoving = false;
    private Rigidbody rb; // Rigidbody de�i�keni

    private bool isJumping = false; // Z�plama i�lemi kontrol�
    public UDPReceive UdpVeriAlma; //Udp transferi i�in gerekli nesne

    private string yollananVeri;
    private float yonBilgisi;
    private int bilekBilgisi;

    public bool oyunDurumu = true;
    public bool tuslarlaOyna = true;



    void Start()
    {
        platformGenisligi = Platform.transform.localScale.x;
        seritGenisligi = platformGenisligi / 3;
        hareketKoordinatlari[0] = seritGenisligi * (-1);
        hareketKoordinatlari[1] = 0;
        hareketKoordinatlari[2] = seritGenisligi * (1);

        rb = gameObject.AddComponent<Rigidbody>(); // Rigidbody bile�enini ekleyin  


        //Setting araylar�n� yukleme
        string json = File.ReadAllText(Application.dataPath + "/settings.json");
        SettingsData data = JsonUtility.FromJson<SettingsData>(json);

        
        karakterHizi = data.oyunHizi;
        tuslarlaOyna = data.tuslarlaOyna;

        oyunDurumu = true;

        Time.timeScale = 1;


        GameObject buttonObject = GameObject.FindWithTag("PauseButtonTag");
        pauseClickButton = buttonObject.GetComponent<UnityEngine.UI.Button>();
        //pauseClickButton.onClick.Invoke();

        buttonObject = GameObject.FindWithTag("ResumeButtonTag");
        resumeClickButton = buttonObject.GetComponent<UnityEngine.UI.Button>();
        //resumeClickButton.onClick.Invoke();

        buttonObject = GameObject.FindWithTag("RestartButtonTag");
        restartClickButton = buttonObject.GetComponent<UnityEngine.UI.Button>();

    }

    void Update()
    {
        if(oyunDurumu)
        {
            //Karekterin s�rekli olarak ileri hareket etmesi

            transform.Translate(Vector3.forward * karakterHizi * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);


            //Yon tuslariyla oynamak istendigi zaman;
            if(tuslarlaOyna)
            {
                TuslaOynamaFonk();
            }

            //Beden hareketleriyle oynanmak istendigi zaman
            else if (!tuslarlaOyna)
            {
                VucutOynamaFonk();
            }
        }
        else
        {
            Durdur();
        }
    }

    void ChangeLaneTus(int direction)
    {
        int targetLane = mevcutSerit + direction;

        // Hedef �erit s�n�rlar�n�n kontrol�
        if (targetLane < 0 || targetLane > 2)
        {
            return; // Hedef �erit s�n�rlar�n d���nda ise i�lemi sonland�r
        }

        // Hedef konumun belirlenmesi
        float targetX = hareketKoordinatlari[targetLane];

        // Karakterin hedef konuma hareket etmesi
        transform.DOMoveX(targetX, 0.2f).SetEase(Ease.OutQuad).OnStart(() =>
        {
            //isMoving = true;
        }).OnComplete(() =>
        {
            //isMoving = false;
            mevcutSerit = targetLane; // �erit g�ncellemesi
        });
    }

    void ChangeLaneVucut(int direction)
    {
        int targetLane = direction;

        // Hedef �erit s�n�rlar�n�n kontrol�
        if (targetLane < 0 || targetLane > 2)
        {
            return; // Hedef �erit s�n�rlar�n d���nda ise i�lemi sonland�r
        }

        // Hedef konumun belirlenmesi
        float targetX = hareketKoordinatlari[targetLane];

        // Karakterin hedef konuma hareket etmesi
        transform.DOMoveX(targetX, 0.2f).SetEase(Ease.OutQuad).OnStart(() =>
        {
            //isMoving = true;
        }).OnComplete(() =>
        {
            //isMoving = false;
            mevcutSerit = targetLane; // �erit g�ncellemesi
        });
    }

    void Jump()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Yalpalanmay� engelle

        //Z�plama y�ksekligi
        rb.AddForce(Vector3.up * ziplamaYuksekligi, ForceMode.Impulse);

        isJumping = true; // Z�plama i�lemi ba�lad�
    }

    public void Durdur()
    {
        karakterHizi = 0;

        // Karakterin t�m y�nlerdeki h�zlar�n� s�f�rla
        rb.velocity = Vector3.zero;

        // Karakterin t�m y�nlerde hareketini engellemek i�in RigidbodyConstraints.FreezeAll flag'i aktif edilir.
        rb.constraints = RigidbodyConstraints.FreezeAll;

    }

    public void tusOrVucut(bool deger)
    {
        tuslarlaOyna = deger;
    }

    public void VucutOynamaFonk()
    {
        UdpVeriAlma.startRecieving = true;

        //Udp ile yollanan verileri alma
        try
        {
            yollananVeri = UdpVeriAlma.data;
            string[] splitData = yollananVeri.Split(',');



            yonBilgisi = float.Parse(splitData[0]);
            bilekBilgisi = int.Parse(splitData[1]);

            Debug.Log(yonBilgisi);
            //Debug.Log(bilekBilgisi);
        }
        catch (Exception err)
        {
            Debug.Log("Python verisi bulunamadi");
            Debug.Log(err.ToString());
            yonBilgisi = -1;
        }

        if (yonBilgisi == 3)
        {
            ChangeLaneVucut(2);
        }
        else if (yonBilgisi == 2)
        {
            ChangeLaneVucut(1);
        }
        else if (yonBilgisi == 1)
        {
            ChangeLaneVucut(0);
        }


        if (bilekBilgisi == 0)
        {
            resumeClickButton.onClick.Invoke();
            //resumeClickButton.onClick.AddListener(a);
        }
        else if (bilekBilgisi == 1)
        {
            pauseClickButton.onClick.Invoke();
            //pauseClickButton.onClick.AddListener(a);
        }
        else if (bilekBilgisi == 2)
        {
            Debug.Log("Restart Button");
            restartClickButton.onClick.Invoke();
        }

    }

    public void TuslaOynamaFonk()
    {
        UdpVeriAlma.startRecieving = false;

        // Karakterin hareketi sadece �u an hareket etmiyorsa ger�ekle�ir
        if (!isMoving)
        {
            //UDP haberle�mesini kapat�yor
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeLaneTus(1); // Sa�a git
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeLaneTus(-1); // Sola git
            }
            else if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.y == 0f) // Yere de�di�inde ve z�plamaya haz�r oldu�unda z�plama i�lemini yap
            {
                Jump();
            }

            // Karakter z�pl�yorsa
            if (isJumping)
            {
                // D���� h�z�n� kontrol et ve d���� h�z�n� sabit tut
                if (rb.velocity.y < 0)
                {
                    rb.velocity += Vector3.down * dususHizi * Time.deltaTime;
                    isJumping = false;
                }
            }
        }
    }

    public void a()
    {

    }
}
