using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

public class Controller : MonoBehaviour
{
    public Camera cam;
    public GameObject cancelButton;
    public Material hg;
    public GameObject piyon;

    [Scene]
    public string MenuScene;

    public Transform[] RowTransforms;

    public float AnimSpeed = 7f;
    public AnimationCurve curve;

    private List<IleriGeri> move_list = new List<IleriGeri>();
    public float Camspeed = 20f;

    public bool isAnimPlaying = true;   // anim yok ama oyun baslama tusunua basana kadar...

    private bool sagadon, soladon;

    public GameObject StartPanel;
    public GameObject ayarlarPanel;

    public TextMeshProUGUI oynancaksayi;

    public GameObject kekran;
    public TextMeshProUGUI kalan_piyon_sayisi;
    public TextMeshProUGUI sonuc;

    string[] sonuclar = {"Sen bir dehasın", "Bilgin", "Zeki", "Kurnaz", "Başarılı", "Normal", "Tecrübesiz", "Aptal", "Gerizekalı", "Beyinsiz"};

    /*
    x\y 0 1 2 3 4 5 6
    6
    5
    4
    3
    2
    1
    0
    */
    Transform[,] holeArray = new Transform[7,7];

    Ray ray;
    Selection selection;

    /*private void printt()
    {
        int rowLength = holeArray.GetLength(0);
        int colLength = holeArray.GetLength(1);
        string arrayString = "";
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                arrayString += string.Format("{0} ", holeArray[i, j].name);
            }
            arrayString += System.Environment.NewLine + System.Environment.NewLine;
        }

        Debug.Log(arrayString);
    }*/

    public void QuitToMenu()
    {
        SceneManager.LoadScene(MenuScene);
    }

    void Start()
    {
        int ArrIndex = 0;
        foreach(Transform x in RowTransforms)
        {
            for(int i = 0; i < x.transform.childCount; i++)
            {
                Transform tr = x.transform.GetChild(i);
                string name = tr.name;
                int index = int.Parse(name.Substring(name.Length - 1));
                holeArray[ArrIndex,index] = tr;
            }

            ArrIndex++;
        }

        selection = new Selection(null);
        cancelButton.SetActive(false); // buton kapalı olmalı
        StartPanel.SetActive(true);     // baslama paneli açık olmalı
        ayarlarPanel.SetActive(false);   // ayarlarpaneli kapalı olmalı
        kekran.SetActive(false);
    }   

    void kekransetup(int kalansayi)
    {
        kekran.SetActive(true);
        kalan_piyon_sayisi.text = $"KALAN PIYON\n{kalansayi}";
        if (kalansayi > 9)
        {
            sonuc.text = "Beyinsizden Ötesini\n Düşünemiyorum";
        }
        else
        {
            sonuc.text = sonuclar[kalansayi];
        }
    }

    void Update()
    {
        if (sagadon){MoveCam(Vector3.down);}
        if (soladon){MoveCam(Vector3.up);}
        if (isAnimPlaying){return;}


        if (Input.GetMouseButtonDown(0)) //Input.GetMouseButtonDown(0) Input.touchCount > 0
        {
            ray = cam.ScreenPointToRay(Input.mousePosition); //Input.mousePosition Input.GetTouch(0).position
            RaycastHit hit;
            if (Physics.Raycast (ray, out hit, 20f)) 
            {
                if (hit.transform.TryGetComponent(out HoleObject hole)) 
                {
                    if (!hole.canTaken){return;}    // kırmızı olanlar alınamaz

                    //1 - seçilmiş yoksa ve seçtiğimizde o karede piyon varsa bakıcaz  (koyulabilcek yerler bul)
                    //2 - seçilmiş yoksa ve seçtiğimizde o karede piyon yoksa siktiret (oynıcak bişey yok)
                    //3 - seçilmiş varsa ve seçtiğimizde o karede piyon yoksa bakıcaz  (oraya yerleştir)
                    //4 - seçilmiş varsa ve seçtiğimizde o karede piyon varsa siktiret (illegal move)
                    
                    if (selection.SecilenObje == null) // seçilmiş yoksa
                    {
                        if (hole.piyon == null) // o karede piyon yoksa
                        {
                            // boşver..
                            return;
                        }
                        else  // o karede piyon da varsa
                        {
                            // gidebileceği yerleri işaretle ve piyonu seç
                            selection.SetHole(hit.collider.transform, hole, hg);
                            GidecegiYerleriGoster(hole);
                            cancelButton.SetActive(true);
                        }
                    }
                    else    // seçilmiş Varsa
                    {
                        // aynı şeyi seçemezsin öncelikle
                        if (selection.SecilenObje.position == hit.collider.transform.position){print("aynısını seçtin"); return;}
                        if (hole.piyon == null) // ve seçtiğimiz boş + alınabilir ise
                        {
                            // piyonu oraya yerleştir
                            if (GidecegiYerKontrolu(hole))
                            {
                                PiyonOynat(hit.collider.transform, hole);
                                
                            }
                            
                        }
                        else    // ve seçtiğimiz boş değilse
                        {
                            // illegal move zaten lmao
                        }
                    }
                }
            }

        }
        
    }

    public int TahtadaKalanTasSayisi()
    { 
        int num = 0;
        for (int x=0; x<7; x++)
        {
            for (int y=0; y<7; y++)
            { 
                if (holeArray[x,y].GetComponent<HoleObject>().piyon != null)
                {
                    num++;
                }
            }

        }

        return num;
    }

    public bool GidecegiYerKontrolu(HoleObject istenenYer)
    {
        foreach(IleriGeri x in move_list)
        {
            if (x.iki_ileri == istenenYer)
            {
                StartCoroutine(PiyonAnim(x.bir_ileri.piyon, (Vector3.up+Vector3.forward)*2, 7f, true, x.bir_ileri));
                return true;
            }
        }
        return false;
    }

    public void GidecegiYerleriGoster(HoleObject hole)
    {
        // verilen hole objesinin gidebüleceği yirler
        for (int x=0; x<7; x++)
        {
            for (int y=0; y<7; y++)
            { 
                if (holeArray[x,y].GetComponent<HoleObject>() == hole)
                {
                    // her yönün iki ilerisi mümkün mü ona bak
                    // iki ilerisine bakmışken orası boşmu onada bakıver
                    // o yönün bir ilerisine bak, dolu ise
                    // iki ileriyi Gidilebilir olarak göster ve gidilebilirler listesine ekle.

                    if (y+2 <= 6) // y ekseni yani YATAYDA sağa 2 blok ilerisi varmı
                    {
                        IleriGeri d0 = ikiilerisibos_birgerisidolu(x,y, new int[2] {0,0}, new int[2] {2,1});
                        if (d0.izin)
                        {
                            move_list.Add(d0);
                        }
                    }
                    if (y-2 >= 0) // y ekseni yani YATAYDA sola 2 blok gerisi varmı
                    {
                        IleriGeri d1 = ikiilerisibos_birgerisidolu(x,y, new int[2] {0,0}, new int[2] {-2,-1});
                        if (d1.izin)
                        {
                            move_list.Add(d1);
                        }
                    }
                    if (x+2 <= 6) // x ekseni yani DİKEYDE yukarı 2 blok ilerisi varmı
                    {
                        IleriGeri d2 = ikiilerisibos_birgerisidolu(x,y, new int[2] {2,1}, new int[2] {0,0});
                        if (d2.izin)
                        {
                            move_list.Add(d2);
                        }
                    }
                    if (x-2 >= 0) // x ekseni yani DİKEYDE aşağı 2 blok gerisi varmı
                    {
                        IleriGeri d3 = ikiilerisibos_birgerisidolu(x,y, new int[2] {-2,-1}, new int[2] {0,0});
                        if (d3.izin)
                        {
                            move_list.Add(d3);
                        }
                    }
                    return;
                    
                }
            }
        }
    }

    void HicOynancakVarmi()
    {
        int oynancaksayi = 0;

        for (int x=0; x<7; x++)
        {
            for (int y=0; y<7; y++)
            { 
                if (y+2 <= 6){
                IleriGeri d0 = ikiilerisibos_birgerisidolu(x,y, new int[2] {0,0}, new int[2] {2,1}, false);
                if (d0.izin){oynancaksayi++; }
                }

                if (y-2 >= 0){
                IleriGeri d1 = ikiilerisibos_birgerisidolu(x,y, new int[2] {0,0}, new int[2] {-2,-1}, false);
                if (d1.izin){oynancaksayi++; }
                }

                if (x+2 <= 6){
                IleriGeri d2 = ikiilerisibos_birgerisidolu(x,y, new int[2] {2,1}, new int[2] {0,0}, false);
                if (d2.izin){oynancaksayi++; }
                }

                if (x-2 >= 0){
                IleriGeri d3 = ikiilerisibos_birgerisidolu(x,y, new int[2] {-2,-1}, new int[2] {0,0}, false);
                if (d3.izin){oynancaksayi++; }
                }
                //yield return null;
            }
        }
        UpdateOynancakCount(oynancaksayi);
    }

    public void UpdateOynancakCount(int newsayi)
    {
        oynancaksayi.text = $"Oynanabilir: {newsayi}";
        if (newsayi <= 0)
        {
            // oyun bitti yeah motherfucker
            kekransetup(TahtadaKalanTasSayisi());
        }
    }

    public IleriGeri ikiilerisibos_birgerisidolu(int x, int y, int[] x_m, int[] y_m, bool aydinlat = true)
    {
        HoleObject firstO =  holeArray[x+x_m[0],  y+y_m[0]].GetComponent<HoleObject>();  // iki ilerisi
        HoleObject secondO = holeArray[x+x_m[1],  y+y_m[1]].GetComponent<HoleObject>(); // bir ilerisi
        HoleObject bakilan = holeArray[x,y].GetComponent<HoleObject>();
        if (!bakilan.canTaken || !(bakilan.piyon != null) ) 
        {
            return new IleriGeri(null, null, false);
        }
        
        if (firstO.canTaken == false){print("bos");return new IleriGeri(null, null, false);}    // iki ilerdeki alınamaz ise boşver
        if (secondO.canTaken == false){print("bos");return new IleriGeri(null, null, false);}
        bool first  = firstO.piyon == null  ; // iki ilerisi boşsa
        bool second = secondO.piyon != null ; // bir ilerisi doluysa
        if (first && second)
        {
            if (aydinlat){
            firstO.ChangeMesh(true);
            }

            return new IleriGeri(firstO, secondO, first && second);
        }
        return new IleriGeri(null, null, false);
       
    }

    IEnumerator PiyonAnim(Transform obje, Vector3 hedef, float hiz = 1f, bool kill = false, HoleObject killTo = null, bool temizle = false)
    {
        Vector3 ilkPos = obje.position;
        float time = 0f;
        isAnimPlaying = true;
        while (Vector3.Distance(obje.position, hedef) > 0.001)
        {
            time += Time.deltaTime;
            Vector3 pos = Vector3.Lerp(ilkPos, hedef, time);
            pos.y += curve.Evaluate(time);
            obje.position = pos;
            yield return null;
        }
        if (kill)
        {
            Destroy(killTo.piyon.gameObject);
            killTo.piyon = null;
        }
        if (temizle)
        {
            Temizleme();    
        }
        isAnimPlaying = false;
        HicOynancakVarmi();
    }

    public void PiyonOynat(Transform newPos, HoleObject gidenYer)
    {
        // secilen piyonun grafik pozisyonunu yeni yere at
        StartCoroutine(PiyonAnim(selection.holeKomponent.piyon, newPos.position, AnimSpeed));
        
        // gidicek yerin piyon transformuna şu an seçili olanı koy
        gidenYer.piyon = selection.holeKomponent.piyon;   

        // piyonu aldığımız yerin piyon komponentini nulla
        selection.holeKomponent.piyon = null; 

        Temizleme();
    }

    public void Temizleme()
    {
        // temizle
        selection.Clear();

        // seçimleri temizle ve renkleri kaldır
        foreach(IleriGeri x in move_list)
        {
            x.iki_ileri.ChangeMesh(false);
        }
        move_list.Clear();

        // seçimi iptal butonu kapat
        cancelButton.SetActive(false);
    }

    public void CancelSelection()
    {
        if (selection.SecilenObje != null)
        {
            Temizleme();
        }
    }

    public void GUIoyunubaslat()
    {
        StartGame();
        isAnimPlaying = false;
        StartPanel.SetActive(false);
    }

    public void GUIoyunukapat()
    {
        Application.Quit();
    }

    public void GUIayarlarmenuac()
    {
        ayarlarPanel.SetActive(true);
    }

    public void GUIayarlarmenukapat()
    {
        ayarlarPanel.SetActive(false);
    }

    public void anamenuyecik()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()
    {
        for (int x=0; x<7; x++)
        {
            for (int y=0; y<7; y++)
            { 
                Vector3 pos = holeArray[x,y].position;
                HoleObject hl = holeArray[x,y].GetComponent<HoleObject>();
                if (hl.canTaken)
                {
                    hl.piyon = Instantiate(piyon, pos, Quaternion.identity).transform;
                }
            }
        }

        HoleObject orta = holeArray[3,3].GetComponent<HoleObject>();
        Destroy(orta.piyon.gameObject);
        orta.piyon = null;
    }

    public void MoveCam(Vector3 mov)
    {
        cam.transform.RotateAround (new Vector3(0f,0f,0.6f), mov, Camspeed * Time.deltaTime);
    }

    public void SagaDonBasla()
    {
        sagadon = true;
    }

    public void SagaDonDur()
    {
        sagadon = false;
    }

    public void SolaDonBasla()
    {
        soladon = true;
    }

    public void SolaDonDur()
    {
        soladon = false;
    }

    public class Selection
    {
        public Transform SecilenObje;
        public Material lastMaterial;
        public HoleObject holeKomponent;

        public Selection(Transform t)
        {
            SecilenObje = t; 
        }

        public void Clear()
        {
            // objeye eski materyalini ver
            SecilenObje.GetComponent<MeshRenderer>().material = lastMaterial;

            SecilenObje = null;
            holeKomponent = null;
            lastMaterial = null;
        }

        public void SetHole(Transform t, HoleObject p , Material h)
        {
            MeshRenderer x = t.gameObject.GetComponent<MeshRenderer>(); // get the material
            lastMaterial = x.material;  // store the material
            x.material = h; // give it a new material
            SecilenObje = t;
            holeKomponent = p;
        }
    }

    public struct IleriGeri
    {
        public HoleObject iki_ileri;
        public HoleObject bir_ileri;
        public bool izin;

        public IleriGeri(HoleObject iki, HoleObject bir, bool _izin)
        {
            this.iki_ileri = iki;
            this.bir_ileri = bir;
            this.izin = _izin;
        }
    }
}
