using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public enum SelectorState { BATTLE_INTRO, CHAR1, CHAR2, CHAR3, CHAR4, CHAR5 , CHAR6 , CHAR7, CHAR8, BATTLE_END}
public enum PlayerTurn { PLAYER1, PLAYER2 }

public class MainGamePlayController : MonoBehaviour {

    public SelectorState CurrentState;
    public PlayerTurn CurrentTurn;
    public Camera ActiveCamera;
    public GameObject ToInstantiate_Character_Prefab;
    public GameObject ToInstantiate_OponentCharacter_Prefab;
    public GameObject ToInstantiate_Selector_Prefab;
    public int NumberOfCharacters = 6;
    private GameObject[] Temporal_Character_Instantiated = new GameObject[6];
    private GameObject[] Temporal_Selector_Instantiated = new GameObject[6];
    public GameObject[] CharacterGenerator = new GameObject[6];
    private Vector3 NewCharacterOrientation = new Vector3(0,0,0);
    private float CameraDistance = 0.0f;
    private Vector3 ArenaCenter = new Vector3(0, 0, 0);
    public float CameraDistanceScale;
    private CapsuleCollider[] Selector_CapCollider = new CapsuleCollider[6];
    private float Rand = 0.0f;
    private bool BeginTheGame = false;
    private Ray LaunchedRay;
    private RaycastHit RayCastHit;
    private GameObject ChoosenTarget_GameObject;
    private int NumberForSelectors;
    private bool SelectorsAlreadyShown = false;
    private Vector3 ChoosenTarget_Position;
    private bool[] AlreadyAttacked = new bool[6];
    private bool GotTarget = false;
    public GameObject[] LifeBars = new GameObject[6];
    private Slider[] LifeSlider = new Slider[6];
    private int RandAttack = 0;
    private int RandChoose = 0;
    private float[] CharacterHealth = new float[6];
    private float[,] AttacksDamage = new float[6, 3];
    private float[,] AttacksRange = new float[6, 3];
    private int CurrentCaractersInGame = 0;
    public bool OnePlayer = true;
    private bool Waiting = false;
    private Animator[] Characters_Animators = new Animator[6];
    private bool[] DeathIndex = new bool[6];
    public Text[] DeckRemainingCards = new Text[2];
    private bool CharacterDamagedThisTurn = false;
    private int turn = 0;
    private Renderer[] SelectorsRenderers = new Renderer[6];
    private int ActiveCharacter_Index = 0;
    private Collider[] Characters_Collider = new Collider[6];
    private float TimeForChoosing = 2f;
    public Text TimeLeft;
    public Text ChooseText;
    private float CurrentTime = 0f;
    private bool CurrentTimeEnabled = false;
    public float TurnLenght;
    private bool TimeOver = false;
    private int[] DeckSize = new int[2];
    private bool[] WillGenerateNewCharacter = new bool[6];
    private bool[] WillBeBoss = new bool[6];
    private bool[] IsBoss = new bool[6];
    public Image[] AttackImage = new Image[2];
    public AudioClip[] FxClips;
    private AudioSource audioS;
    private GameObject SceneManager_ObjectFound;
    private bool ILost = false;
    string text = " ";
    private float[] Value = new float[100]; 

    void Start () {


        //Getting Drapa values from txt
        try
        {
            StreamReader SR = new StreamReader("Assets/Resources/Values.txt");
            int ind = 0;
            while (text != "99999")
            {
                text = SR.ReadLine();
                Value[ind] = float.Parse(text);
                ind++;
                Debug.Log(Value[ind - 1]);
            }
            ind = 0;
            SR.Close();

            //Gets values and atributes for every character, this will come from DB or External source
            for (int i = 0; i < NumberOfCharacters; i++)
            {
                CharacterHealth[i] = Value[0];
                AttacksDamage[i, 0] = Value[1];
                AttacksDamage[i, 1] = Value[2];
                AttacksDamage[i, 2] = Value[3];
                AttacksRange[i, 0] = Value[4];
                AttacksRange[i, 1] = Value[5];
                AttacksRange[i, 2] = Value[6];
                DeathIndex[i] = false;

            }
        }
        catch
        {
            for (int i = 0; i < NumberOfCharacters; i++)
            {
                CharacterHealth[i] = 100f;
                AttacksDamage[i, 0] =25f;
                AttacksDamage[i, 1] = 15f;
                AttacksDamage[i, 2] = 10f;
                AttacksRange[i, 0] =3f;
                AttacksRange[i, 1] = 5f;
                AttacksRange[i, 2] = 6f;
                DeathIndex[i] = false;

            }
        }

        DeckSize[0] = 7;
        DeckSize[1] = 4;
        DeckRemainingCards[0].text = "" + DeckSize[0];
        DeckRemainingCards[1].text = "" + DeckSize[1];
        AttackImage[0].enabled = false;
        AttackImage[1].enabled = false;

        //Initialize variables
        BeginTheGame = false;
        turn = 0;
        CurrentState = SelectorState.BATTLE_INTRO;
        CurrentTurn = PlayerTurn.PLAYER1;
        SelectorsAlreadyShown = false;
        for (int i = 0; i < AlreadyAttacked.Length; i++) AlreadyAttacked[i] = false;
        ChooseText.enabled = false;
        TimeLeft.enabled = false;

        //Initialize all instances  
        StartCoroutine(WaitingForCharactersEntry(0));
        audioS = GetComponent<AudioSource>();

        //Get components
        for (int i = 0; i < NumberOfCharacters; i++)
        {
            LifeSlider[i] = LifeBars[i].GetComponent<Slider>();
        }

    }

    void Update()
    {
        if (BeginTheGame)
        {
            //Initialize variables on every update}
            
            
            if (!Waiting)
            {
                
                if (CurrentTurn == PlayerTurn.PLAYER1)
                {
                    //Show time
                    if (CurrentTimeEnabled && BeginTheGame)
                    {
                        CurrentTime = Time.time;
                        CurrentTimeEnabled = false;
                    }
                    else
                    {
                        TimeLeft.text = "" + (TurnLenght - (Time.time - CurrentTime));
                        if (TurnLenght < (Time.time - CurrentTime)) TimeOver = true;
                    }
                    

                    LaunchedRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(LaunchedRay, out RayCastHit))
                    {
                        if (RayCastHit.transform.tag == "Player2_Character" || RayCastHit.transform.tag == "Selector")
                        {
                            //Characters_Collider[ActiveCharacter_Index].enabled = false; //bad idea
                            //Temporal_Character_Instantiated[ActiveCharacter_Index].transform.LookAt(new Vector3(RayCastHit.point.x, 0, RayCastHit.point.z));
                            Characters_Animators[ActiveCharacter_Index].SetTrigger("DamageReceived");
                        }
                    }
                }
                //Two players option
                if (CurrentTurn == PlayerTurn.PLAYER2 && !OnePlayer)
                {
                    //Show time
                    if (CurrentTimeEnabled && BeginTheGame)
                    {
                        CurrentTime = Time.time;
                        CurrentTimeEnabled = false;
                    }
                    else
                    {

                        TimeLeft.text = "" + (2 - (Time.time - CurrentTime));
                        if (TurnLenght < (Time.time - CurrentTime)) TimeOver = true;
                    }

                    LaunchedRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(LaunchedRay, out RayCastHit))
                    {
                        if (RayCastHit.transform.tag == "Player1_Character" || RayCastHit.transform.tag == "Selector")
                        {
                            //Characters_Collider[ActiveCharacter_Index].enabled = false; //bad idea
                            //Temporal_Character_Instantiated[ActiveCharacter_Index].transform.LookAt(new Vector3(RayCastHit.point.x, 0, RayCastHit.point.z));
                            Characters_Animators[ActiveCharacter_Index].SetTrigger("DamageReceived");
                        }
                    }
                }

                //If mouse button is clicked
                if (CurrentTurn == PlayerTurn.PLAYER1 && TimeOver)
                {
                    RandChoose = Random.Range(3, 6);
                    if (DeathIndex[RandChoose]) RandChoose = Random.Range(3, 6);
                    ChoosenTarget_GameObject = Temporal_Character_Instantiated[RandChoose];
                    ChoosenTarget_Position = Temporal_Character_Instantiated[RandChoose].transform.position;
                    HideSelectors();
                    GotTarget = true;
                    //RandAttack = Random.Range(0, 2);
                    if(TimeOver)
                    {
                        ChooseText.enabled = false;
                        TimeLeft.text = "Time over!";
                    }
                    TimeOver = false;
                }
                if (OnePlayer && CurrentTurn == PlayerTurn.PLAYER2 || (!OnePlayer && TimeOver))
                {
                    
                    RandChoose = Random.Range(0, 3);
                    while (DeathIndex[RandChoose]) RandChoose = Random.Range(0, 3);
                    ChoosenTarget_GameObject = Temporal_Character_Instantiated[RandChoose];
                    ChoosenTarget_Position = Temporal_Character_Instantiated[RandChoose].transform.position;
                    HideSelectors();
                    GotTarget = true;
                    RandAttack = Random.Range(0, 1);
                    if (TimeOver)
                    {
                        ChooseText.enabled = false;
                        TimeLeft.text = "Time over!";
                    }
                    TimeOver = false;
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        LaunchedRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(LaunchedRay, out RayCastHit))
                        {
                            switch (CurrentTurn)
                            {
                                case PlayerTurn.PLAYER1:
                                    if (RayCastHit.transform.tag == "Player2_Character" || RayCastHit.transform.tag == "Selector")
                                    {
                                        ChoosenTarget_GameObject = RayCastHit.transform.gameObject;
                                        ChoosenTarget_Position = RayCastHit.transform.position;
                                        HideSelectors();
                                        
                                        GotTarget = true;
                                        //RandAttack = Random.Range(0, 1);
                                        //CurrentTurn = PlayerTurn.PLAYER2;
                                        //Debug.Log("Correct hit on player2 character. Tag: " + RayCastHit.transform.tag);
                                    }
                                    break;
                                case PlayerTurn.PLAYER2:
                                    if (RayCastHit.transform.tag == "Player1_Character" || RayCastHit.transform.tag == "Selector")
                                    {
                                        ChoosenTarget_GameObject = RayCastHit.transform.gameObject;
                                        ChoosenTarget_Position = RayCastHit.transform.position;
                                        HideSelectors();
                                        
                                        GotTarget = true;
                                        //RandAttack = Random.Range(0, 1);
                                        //CurrentTurn = PlayerTurn.PLAYER1;
                                        //Debug.Log("Correct hit on player1 character. Tag: " + RayCastHit.transform.tag);
                                    }
                                    break;
                                default: break;
                            }

                        }
                    }
                }

                //State machine
                switch (CurrentState)
                {
                    case SelectorState.BATTLE_INTRO:
                        CurrentState = SelectorState.CHAR1;
                        CurrentTurn = PlayerTurn.PLAYER1;
                        Temporal_Character_Instantiated[0].SendMessage("YoureAttacking");
                        break;
                    case SelectorState.CHAR1:
                        ActiveCharacter_Index = 0;
                        if (DeathIndex[ActiveCharacter_Index])
                        {
                            if (WillGenerateNewCharacter[ActiveCharacter_Index])
                            {
                                GotTarget = false;
                                GenerateNewCharacter(ActiveCharacter_Index);
                            }
                            else
                            {
                                GotTarget = false;
                                if (ActiveCharacter_Index == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
                                else AlreadyAttacked[ActiveCharacter_Index - 1] = false;
                                AlreadyAttacked[ActiveCharacter_Index] = true;
                                CurrentState = SelectorState.CHAR4;
                                TurnMachine();
                            }
                        }
                        if (GotTarget && !AlreadyAttacked[0])
                        {
                            AttackNow(0);
                            CurrentState = SelectorState.CHAR4;
                        }
                        break;
                    case SelectorState.CHAR2:
                        ActiveCharacter_Index = 1;

                        if (DeathIndex[ActiveCharacter_Index])
                        {
                            if (WillGenerateNewCharacter[ActiveCharacter_Index])
                            {
                                GotTarget = false;
                                GenerateNewCharacter(ActiveCharacter_Index);
                            }
                            else
                            {
                                GotTarget = false;
                                if (ActiveCharacter_Index == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
                                else AlreadyAttacked[ActiveCharacter_Index - 1] = false;
                                AlreadyAttacked[ActiveCharacter_Index] = true;
                                CurrentState = SelectorState.CHAR5;
                                TurnMachine();
                            }
                        }
                            if (GotTarget && !AlreadyAttacked[1])
                        {
                            AttackNow(1);
                            CurrentState = SelectorState.CHAR5;
                        }
                        break;
                    case SelectorState.CHAR3:
                        ActiveCharacter_Index = 2;
                        if (DeathIndex[ActiveCharacter_Index])
                        {
                            if (WillGenerateNewCharacter[ActiveCharacter_Index])
                            {
                                GotTarget = false;
                                GenerateNewCharacter(ActiveCharacter_Index);
                            }
                            else
                            {
                                GotTarget = false;
                                if (ActiveCharacter_Index == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
                                else AlreadyAttacked[ActiveCharacter_Index - 1] = false;
                                AlreadyAttacked[ActiveCharacter_Index] = true;
                                CurrentState = SelectorState.CHAR6;
                                TurnMachine();
                            }
                        }
                            if (GotTarget && !AlreadyAttacked[2])
                        {
                            AttackNow(2);
                            CurrentState = SelectorState.CHAR6;
                        }
                        break;
                    case SelectorState.CHAR4:
                        ActiveCharacter_Index = 3;
                        if (DeathIndex[ActiveCharacter_Index])
                        {
                            if (WillGenerateNewCharacter[ActiveCharacter_Index])
                            {
                                GotTarget = false;
                                GenerateNewCharacter(ActiveCharacter_Index);
                            }
                            else
                            {
                                GotTarget = false;
                                if (ActiveCharacter_Index == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
                                else AlreadyAttacked[ActiveCharacter_Index - 1] = false;
                                AlreadyAttacked[ActiveCharacter_Index] = true;
                                CurrentState = SelectorState.CHAR2;
                                TurnMachine();
                            }
                        }
                            if (GotTarget && !AlreadyAttacked[3])
                        {
                            AttackNow(3);
                            CurrentState = SelectorState.CHAR2;
                        }
                        break;
                    case SelectorState.CHAR5:
                        ActiveCharacter_Index = 4;
                        if (DeathIndex[ActiveCharacter_Index])
                        {
                            if (WillGenerateNewCharacter[ActiveCharacter_Index])
                            {
                                GotTarget = false;
                                GenerateNewCharacter(ActiveCharacter_Index);
                            }
                            else
                            {
                                GotTarget = false;
                                if (ActiveCharacter_Index == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
                                else AlreadyAttacked[ActiveCharacter_Index - 1] = false;
                                AlreadyAttacked[ActiveCharacter_Index] = true;
                                CurrentState = SelectorState.CHAR3;
                                TurnMachine();
                            }
                        }
                            if (GotTarget && !AlreadyAttacked[4])
                        {
                            AttackNow(4);
                            CurrentState = SelectorState.CHAR3;
                        }
                        break;
                    case SelectorState.CHAR6:
                        ActiveCharacter_Index = 5;
                        if (DeathIndex[ActiveCharacter_Index])
                        {
                            if (WillGenerateNewCharacter[ActiveCharacter_Index])
                            {
                                GotTarget = false;
                                GenerateNewCharacter(ActiveCharacter_Index);
                            }
                            else
                            {
                                GotTarget = false;
                                if (ActiveCharacter_Index == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
                                else AlreadyAttacked[ActiveCharacter_Index - 1] = false;
                                AlreadyAttacked[ActiveCharacter_Index] = true;
                                CurrentState = SelectorState.CHAR1;
                                TurnMachine();
                            }
                        }
                            if (GotTarget && !AlreadyAttacked[5])
                        {
                            AttackNow(5);
                            CurrentState = SelectorState.CHAR1;
                        }
                        break;
                    case SelectorState.BATTLE_END:
                        break;
                    default: break;
                }
            }
        }
    }

    public void AttackNow(int CurrentCharacter)
    {
        turn++;
        audioS.PlayOneShot(FxClips[RandAttack]);
        CharacterDamagedThisTurn = false;
        Temporal_Character_Instantiated[CurrentCharacter].transform.LookAt(ChoosenTarget_Position);
        switch(RandAttack)
            {
            case 0:
        Characters_Animators[CurrentCharacter].SetTrigger("AttackClose");
                break;
            case 1:
                Characters_Animators[CurrentCharacter].SetTrigger("AttackNormal");
                break;
            case 2:
                Characters_Animators[CurrentCharacter].SetTrigger("AttackNormal"); //Distance attack to be added
                break;
            default: break;
        }
        //Characters_Collider[ActiveCharacter_Index].enabled = true; //bad idea
        Temporal_Character_Instantiated[CurrentCharacter].GetComponent<Rigidbody>().AddForce(Temporal_Character_Instantiated[CurrentCharacter].transform.forward * AttacksRange[CurrentCharacter,RandAttack], ForceMode.Impulse);
            Debug.Log("(Turn"+ turn + ") Character " + CurrentCharacter + " attacked!");

            GotTarget = false;
            if (CurrentCharacter == 0) AlreadyAttacked[NumberOfCharacters - 1] = false;
            else AlreadyAttacked[CurrentCharacter - 1] = false;
            AlreadyAttacked[CurrentCharacter] = true;

            StartCoroutine(WaitForAttack(CurrentCharacter));
    }

    public void ShowSelectors()
    {
        Debug.Log("(Turn" + turn + ") " + CurrentTurn + " is choosing");
        switch(CurrentTurn)
        {
            case PlayerTurn.PLAYER1:
                NumberForSelectors = 3;
                break;
            case PlayerTurn.PLAYER2:
                NumberForSelectors = 0;
                break;
            default: break;
        }
        if (CurrentTurn == PlayerTurn.PLAYER2 && OnePlayer)
        {
            SelectorsAlreadyShown = true;
            return;
        }
        for (int i = NumberForSelectors; i < NumberOfCharacters/2 + NumberForSelectors; i++)
        {
            Selector_CapCollider[i] = Temporal_Selector_Instantiated[i].GetComponent<CapsuleCollider>();
            Selector_CapCollider[i].gameObject.SetActive(false);

            //Set position
            Temporal_Selector_Instantiated[i].transform.position = Temporal_Character_Instantiated[i].transform.position;

            //Set orientation
            Temporal_Selector_Instantiated[i].transform.up = -(ActiveCamera.transform.position - Temporal_Selector_Instantiated[i].transform.position);
            //Debug.Log("Orientation " + i + " " + -(ActiveCamera.transform.position - Temporal_Selector_Instantiated[i].transform.position));
            //Temporal_Selector_Instantiated[i].transform.Translate(Temporal_Selector_Instantiated[i].transform.right * (-1f));

            //Scale according to distance
            CameraDistance = (ActiveCamera.transform.position - Temporal_Selector_Instantiated[i].transform.position).magnitude;
            Temporal_Selector_Instantiated[i].transform.localScale = new Vector3(CameraDistanceScale * CameraDistance, 0, CameraDistanceScale * CameraDistance);
            //Debug.Log("Selector " + i + " Camera Distance * Camera Scale: " + (CameraDistance * CameraDistanceScale));

            //Initialize
            Temporal_Selector_Instantiated[i].gameObject.SetActive(true);
            Selector_CapCollider[i].gameObject.SetActive(true);
            Temporal_Selector_Instantiated[i].transform.parent = Temporal_Character_Instantiated[i].transform;


        }

        SelectorsAlreadyShown = true;
    }

    public void HideSelectors()
    {
        for (int i = 0; i < NumberOfCharacters; i++)
        {
            Selector_CapCollider[i] = Temporal_Selector_Instantiated[i].GetComponent<CapsuleCollider>();
            Selector_CapCollider[i].gameObject.SetActive(false);
            Temporal_Selector_Instantiated[i].gameObject.SetActive(false);
        }

        SelectorsAlreadyShown = false;
    }

    void SayingHi(string HiMyNameIs)
    {
        Debug.Log(HiMyNameIs + " said hi");
    }

    void IGotDamage(string WhoGotDamage)
    {
        if (BeginTheGame)
        {
            for (int i = 0; i < NumberOfCharacters; i++)
            {
                if (WhoGotDamage == Temporal_Character_Instantiated[i].transform.name)
                {
                    if (!CharacterDamagedThisTurn)
                    {
                        CharacterDamagedThisTurn = true;
                        AmIDead(AttacksDamage[i, RandAttack], CharacterHealth[i], i);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < CurrentCaractersInGame; i++)
            {
                if (WhoGotDamage == Temporal_Character_Instantiated[i].transform.name)
                {
                    AmIDead(8f, CharacterHealth[i], i); //Deals main basic entry damage
                }
            }
        }

    }

    public void AmIDead (float Damage, float MaxHealth, int CharacterIndex)
    {

        audioS.PlayOneShot(FxClips[2]);
        //maps for 0 as full life, 1 as dead
        float ConvertedDamage = 0f;
            ConvertedDamage = Damage / MaxHealth;
            Debug.Log("(Turn" + turn + ") " + Temporal_Character_Instantiated[CharacterIndex].transform.name + " Got Damage: " + ConvertedDamage);


        if (LifeSlider[CharacterIndex].value + ConvertedDamage < 1)
            {
                LifeSlider[CharacterIndex].value = LifeSlider[CharacterIndex].value + ConvertedDamage;
            Characters_Animators[CharacterIndex].SetTrigger("DamageReceived");
        }
            else //this part defines what happens if the character dies
            {
            Debug.Log(Temporal_Character_Instantiated[CharacterIndex].transform.name + " , " + Temporal_Character_Instantiated[CharacterIndex].transform.tag + " is dead");
                switch( Temporal_Character_Instantiated[CharacterIndex].transform.tag)
            {
                case "Player1_Character":
                    CharacterDied(0, CharacterIndex);
                    break;
                case "Player2_Character":
                    CharacterDied(1, CharacterIndex);
                    break;
                default: break;
            }
        }
        
    }

    void CharacterDied(int Owner_Player, int CharacterIndex)
    {

        if(IsBoss[CharacterIndex])
        {
            SceneManager_ObjectFound = GameObject.Find("SceneManager_Object");
            if(SceneManager_ObjectFound != null)
            {
                Debug.Log("SceneManager found");
                audioS.PlayOneShot(FxClips[3]);
                SceneManager_ObjectFound.SendMessage("BattleWon");
            }
            else Debug.Log("SceneManager NOT found");
        }

        LifeSlider[CharacterIndex].value = 1;
        if (DeckSize[Owner_Player] > 0)
        {
            Temporal_Character_Instantiated[CharacterIndex].GetComponent<Collider>().enabled = false;
            Temporal_Character_Instantiated[CharacterIndex].GetComponent<Rigidbody>().useGravity = false;
            Temporal_Character_Instantiated[CharacterIndex].transform.position = new Vector3(0, -50, 0);

            DeathIndex[CharacterIndex] = true;
            WillGenerateNewCharacter[CharacterIndex] = true;
            DeckSize[Owner_Player]--;
            if (DeckSize[Owner_Player] == 0 && Owner_Player != 0)
            {
                WillBeBoss[CharacterIndex] = true;
            }
        }
        else
        {
            //Temporal_Character_Instantiated[CharacterIndex].GetComponent<MeshRenderer>().enabled = false;
            Temporal_Character_Instantiated[CharacterIndex].GetComponent<Collider>().enabled = false;
            Temporal_Character_Instantiated[CharacterIndex].GetComponent<Rigidbody>().useGravity = false;
            Temporal_Character_Instantiated[CharacterIndex].transform.position = new Vector3(0, -50, 0);
            DeathIndex[CharacterIndex] = true;
            /*
            ILost = true;
            for (int i = 0; i < NumberOfCharacters / 2; i++)
            {
                if (!DeathIndex[CharacterIndex]) ILost = false;
            }
            if(ILost)
            {
                SceneManager_ObjectFound = GameObject.Find("SceneManager_Object");
                SceneManager_ObjectFound.SendMessage("BattleLost");
            }
            */
        }
    }

    void GenerateNewCharacter(int CharacterIndex)
    {
        Waiting = true;
        Destroy(Temporal_Selector_Instantiated[CharacterIndex]);
        Destroy(Temporal_Character_Instantiated[CharacterIndex]);

        if (CharacterIndex < NumberOfCharacters / 2)
        {
            Temporal_Character_Instantiated[CharacterIndex] = (GameObject)Instantiate(ToInstantiate_Character_Prefab,
                                                         CharacterGenerator[CharacterIndex].transform.position,
                                                         Quaternion.identity);
            Temporal_Character_Instantiated[CharacterIndex].tag = "Player1_Character";
            DeckRemainingCards[0].text = "" + DeckSize[0];
        }
        else
        {
            Temporal_Character_Instantiated[CharacterIndex] = (GameObject)Instantiate(ToInstantiate_OponentCharacter_Prefab,
                                                         CharacterGenerator[CharacterIndex].transform.position,
                                                         Quaternion.identity);
            Temporal_Character_Instantiated[CharacterIndex].tag = "Player2_Character";
            DeckRemainingCards[1].text = "" + DeckSize[1];
        }
        Temporal_Character_Instantiated[CharacterIndex].transform.name = "Character" + CharacterIndex;
        NewCharacterOrientation = -(CharacterGenerator[CharacterIndex].transform.position - ArenaCenter);
        //Debug.Log("Orientation" + i +" : " + NewCharacterOrientation.x + " en x, " + NewCharacterOrientation.y + " en y, " + NewCharacterOrientation.z + " en z");
        Temporal_Character_Instantiated[CharacterIndex].transform.forward = new Vector3(NewCharacterOrientation.x,
                                                                        0,
                                                                        NewCharacterOrientation.z);


        //Instantiate selectors
        Temporal_Selector_Instantiated[CharacterIndex] = (GameObject)Instantiate(ToInstantiate_Selector_Prefab,
                                                                    new Vector3(0, 0, 0),
                                                                    Quaternion.identity);
        Temporal_Selector_Instantiated[CharacterIndex].gameObject.SetActive(false);

        //Getting components
        Characters_Animators[CharacterIndex] = Temporal_Character_Instantiated[CharacterIndex].GetComponent<Animator>();
        Characters_Collider[CharacterIndex] = Temporal_Character_Instantiated[CharacterIndex].GetComponent<Collider>();

        //Actions after instances
        Rand = Random.Range(1.5f, 2.9f);
        Temporal_Character_Instantiated[CharacterIndex].GetComponent<Rigidbody>().AddForce(NewCharacterOrientation * Rand, ForceMode.Impulse);
        //CurrentCaractersInGame++;

        if (WillBeBoss[CharacterIndex])
        {
            Temporal_Character_Instantiated[CharacterIndex].transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            CharacterHealth[CharacterIndex] = CharacterHealth[CharacterIndex] * 2f;
            IsBoss[CharacterIndex] = true;
        }

        LifeSlider[CharacterIndex].value = 0;
        StartCoroutine(WaitNewChar());
        Debug.Log("(Turn" + turn + ") Generating new char: " + CharacterIndex);
        DeathIndex[CharacterIndex] = false;
        WillBeBoss[CharacterIndex] = false;
        WillGenerateNewCharacter[CharacterIndex] = false;
    }

    IEnumerator WaitingForCharactersEntry(int i)
    {
        yield return new WaitForSeconds(0.7f);

            // Instantiate character
            if (i < NumberOfCharacters / 2)
            {
                Temporal_Character_Instantiated[i] = (GameObject)Instantiate(ToInstantiate_Character_Prefab,
                                                             CharacterGenerator[i].transform.position,
                                                             Quaternion.identity);
            Temporal_Character_Instantiated[i].tag = "Player1_Character";
            DeckSize[0]--;
            DeckRemainingCards[0].text = "" + DeckSize[0];
        }
            else
            {
                Temporal_Character_Instantiated[i] = (GameObject)Instantiate(ToInstantiate_OponentCharacter_Prefab,
                                                             CharacterGenerator[i].transform.position,
                                                             Quaternion.identity);
            Temporal_Character_Instantiated[i].tag = "Player2_Character";
            DeckSize[1]--;
            DeckRemainingCards[1].text = "" + DeckSize[1];
        }
            Temporal_Character_Instantiated[i].transform.name = "Character" + i;
            NewCharacterOrientation = -(CharacterGenerator[i].transform.position - ArenaCenter);
            //Debug.Log("Orientation" + i +" : " + NewCharacterOrientation.x + " en x, " + NewCharacterOrientation.y + " en y, " + NewCharacterOrientation.z + " en z");
            Temporal_Character_Instantiated[i].transform.forward = new Vector3(NewCharacterOrientation.x,
                                                                            0,
                                                                            NewCharacterOrientation.z);


            //Instantiate selectors
            Temporal_Selector_Instantiated[i] = (GameObject)Instantiate(ToInstantiate_Selector_Prefab,
                                                                        new Vector3(0, 0, 0),
                                                                        Quaternion.identity);
            Temporal_Selector_Instantiated[i].gameObject.SetActive(false);

        //Getting components
        Characters_Animators[i] = Temporal_Character_Instantiated[i].GetComponent<Animator>();
        Characters_Collider[i] = Temporal_Character_Instantiated[i].GetComponent<Collider>();

            //Actions after instances
            Rand = Random.Range(1.5f,2.9f);
            Temporal_Character_Instantiated[i].GetComponent<Rigidbody>().AddForce(NewCharacterOrientation * Rand, ForceMode.Impulse);
        CurrentCaractersInGame++;
        if (i + 1 < NumberOfCharacters)
        {
            StartCoroutine(WaitingForCharactersEntry(i + 1));
        }
        else
        {
            StartCoroutine(WaitForEntryAnimation());
        }
    }

    IEnumerator WaitForEntryAnimation()
    {
        yield return new WaitForSeconds(1.2f);
        BeginTheGame = true;
        CurrentTurn = PlayerTurn.PLAYER1;
        CurrentState = SelectorState.CHAR1;
        ShowSelectors();
    }

    IEnumerator WaitForAttack(int CurrentCharacter)
    {
        Waiting = true; 
        yield return new WaitForSeconds(1);
        TurnMachine();
        Waiting = false;
    }

    IEnumerator WaitIATurn()
    {
        yield return new WaitForSeconds(1f);
    }

    IEnumerator WaitNewChar()
    {
        Waiting = true;
        yield return new WaitForSeconds(1.3f);
        Waiting = false;
    }

    void TurnMachine()
    {
        if (CurrentTurn != PlayerTurn.PLAYER2)
        {
            Temporal_Character_Instantiated[0].SendMessage("YoureNotAttacking");
            Temporal_Character_Instantiated[1].SendMessage("YoureNotAttacking");
            Temporal_Character_Instantiated[2].SendMessage("YoureNotAttacking");
            Temporal_Character_Instantiated[3].SendMessage("YoureAttacking");
            Temporal_Character_Instantiated[4].SendMessage("YoureAttacking");
            Temporal_Character_Instantiated[5].SendMessage("YoureAttacking");
            CurrentTurn = PlayerTurn.PLAYER2;
            if (!OnePlayer)
            {
                ChooseText.enabled = true;
            }
            else
            {
                ChooseText.enabled = false;
                TimeLeft.enabled = false;
            }

        }
        else
        {
            Temporal_Character_Instantiated[0].SendMessage("YoureAttacking");
            Temporal_Character_Instantiated[1].SendMessage("YoureAttacking");
            Temporal_Character_Instantiated[2].SendMessage("YoureAttacking");
            Temporal_Character_Instantiated[3].SendMessage("YoureNotAttacking");
            Temporal_Character_Instantiated[4].SendMessage("YoureNotAttacking");
            Temporal_Character_Instantiated[5].SendMessage("YoureNotAttacking");
            CurrentTurn = PlayerTurn.PLAYER1;
            //Temporal_Character_Instantiated[4].GetComponent<Renderer>().material.color = Color.red;
            if (turn > 0)
            {
                CurrentTimeEnabled = true;
                ChooseText.enabled = true;
                TimeLeft.enabled = true;
            }
        }
        RandAttack = Random.Range(0, 2);
        Debug.Log("(Turn" + turn + ") This turn is for character " + CurrentTurn + ", with Attack: " + RandAttack);
        for (int i = 0; i < 2; i++)
        {
            if (i == RandAttack) AttackImage[i].enabled = true;
            else AttackImage[i].enabled = false;
        }
        if (!SelectorsAlreadyShown) ShowSelectors();
    }
}
