using System.Collections;
using System.Collections.Generic;
using MQTTnet;
using MQTTnet.Client;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SensorSever : MonoBehaviour {
    private float deltatime = 0;
    [SerializeField]
    GameObject target;
    [SerializeField]
    Text infoText;
    [SerializeField]
    InputField address; //url
    [SerializeField]
    Text consoleText;
    [SerializeField]
    Text warningText;
    [SerializeField]
    GameObject spotLight;
    private string serverurl = "http://localhost:3000/";
    //IMqttClient mqttClient;
    [SerializeField]
    private MqttEulerController mqttEuler;
    private EulerBlanding euler;
    private EulerBlanding calibrateEuler;
    [SerializeField]
    Button button;

    void Start () {
        address.text = serverurl;
        warningText.text = "";
        infoText.text = "device:null type:null";
        consoleText.text = "console:";
        //StartCoroutine (ServerTest ());
        //express経由のデータ取得
        //StartCoroutine (GetSensorData ());

        //StartCoroutine (PostServerData ());
        //mqttController.setHost ("192.168.1.6");
        button.OnClickAsObservable ().Subscribe (_ => Debug.Log ("click"));
        //MqttTest();
        mqttEuler.OnMessageReceived.Subscribe (message => {
            bool contain = message.Contains ("EH");
            if (!contain) return;
            //Debug.Log (message);
            EulerBlanding data = JsonUtility.FromJson<EulerBlanding> (message);
            euler = data;
        });
    }

    // Update is called once per frame
    void Update () {
        deltatime += Time.deltaTime;

        if (euler != null) {
            infoText.text = "device:" + euler.devices + " type:" + euler.type;
            SetTargetEuler (euler);
        }

    }
    //9軸オブジェクトへ適用

    void SetTargetEuler (EulerBlanding eulerdata) {
        float targetPitch = eulerdata.EP; // - calibrateEuler.pitch + 90.0f;
        float targetHead = eulerdata.EH; // - calibrateEuler.head;
        float targetRoll = eulerdata.ER; // - calibrateEuler.roll;
        target.transform.rotation = Quaternion.Euler (
            targetPitch,
            targetHead,
            targetRoll);
        if (eulerdata.EP < -30.0f) {
            warningText.text = "Warning!\n危険な体勢です！";
            spotLight.SetActive (true);
        } else {
            warningText.text = "";
            spotLight.SetActive (false);
        }
    }

    public void OnClickSetServer () {
        serverurl = address.text;
    }

    public void SetConsole (string log) {
        consoleText.text = "console:" + log;
    }
}