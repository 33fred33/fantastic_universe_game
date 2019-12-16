using UnityEngine;
using System.Collections;

public class Drapa_Animator : MonoBehaviour {

    private GameObject MainGameController;
    private bool ImAttacking;
    private bool[] Status = new bool[3];
    private float[] AttacksDamage = new float[3];
    private bool[,] MyAttackStatusChange = new bool[3,3];
    private string DamagedUnitName;
    private string HitTag;
    private bool ImTheAttacker = false;
    private int MyAttackThisTurn = 0;
    private Rigidbody MyRigidbody;
    //private Transform MySelector;

    void Start () {
        MainGameController = GameObject.Find("MainGameController");
        if (MainGameController != null)
        {
            Debug.Log("Found MainGameController");
            MainGameController.SendMessage("SayingHi", transform.tag);
        }
        else Debug.Log("Nothing found");
        MyRigidbody = gameObject.GetComponent<Rigidbody>();
       // MySelector = transform.FindChild("Selector");
        //if (MySelector != null) Debug.Log("Found Child");

    }

	void Update () {
        //transform.Translate(Input.GetAxis("Horizontal") * 0.01f, 0, Input.GetAxis("Vertical") * 0.01f);
    }

    void OnCollisionEnter(Collision CollisionObject)
    {
        if (!ImAttacking)
        {
            if (CollisionObject.transform.tag != transform.tag && CollisionObject.transform.tag != "Scenario" && CollisionObject.transform.tag != "Selector")
            {
                string MyName = transform.name;
                Debug.Log(MyName + ": I sent my message for receiving damage");
                MyRigidbody.AddForce(CollisionObject.transform.forward * (3f - MyRigidbody.mass), ForceMode.Impulse);
                MainGameController.SendMessage("IGotDamage", MyName);
            }
        }
        else
        {
            if (CollisionObject.transform.tag != transform.tag && CollisionObject.transform.tag != "Scenario" && CollisionObject.transform.tag != "Selector")
            {
                MyRigidbody.AddForce(-transform.forward * (3f - MyRigidbody.mass), ForceMode.Impulse);
            }
        }
    }

    public void YoureAttacking()
    {
        //MyAttackThisTurn = GotMyAttackThisTurn;
        ImAttacking = true;
        //Debug.Log(transform.name + " got the turn!");
    }

    public void YoureNotAttacking()
    {
        ImAttacking = false;
    }

}
