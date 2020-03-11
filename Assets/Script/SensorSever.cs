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
    Text dataText;
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
    private MqttController mqttController;
    [SerializeField]
    private MqttEulerController mqttEuler;
    NineAxis axisdata;
    Euler eulerdata;
    [SerializeField]
    Button button;

    void Start () {
        address.text = serverurl;
        warningText.text = "";
        StartCoroutine (ServerTest ());
        //express経由のデータ取得
        //StartCoroutine (GetSensorData ());

        //StartCoroutine (PostServerData ());
        mqttController.setHost ("192.168.1.6");
        button.OnClickAsObservable ().Subscribe (_ => Debug.Log ("click"));
    }

    // Update is called once per frame
    void Update () {
        deltatime += Time.deltaTime;
        if (deltatime > 1.0f) {
            //StartCoroutine (GetSensorData ());
            //setTarget (axisdata);

            deltatime -= 1.0f;
        }
        if (eulerdata != null) {
            SetTargetEuler (eulerdata);
        }

        //MqttTest();
        mqttController.OnMessageReceived.Subscribe (message => {
            //データ成型
            string json = "";
            string[] arr = message.Split ('{');
            json = string.Join ("{\"", arr);
            string[] semiArr = json.Split (':');
            json = string.Join ("\":", semiArr);
            string[] comArr = json.Split (',');
            json = string.Join (",\"", comArr);
            NineAxis list = JsonUtility.FromJson<NineAxis> (json);
            axisdata = list;
        });
        mqttEuler.OnMessageReceived.Subscribe (message => {
            //データ成型
            string json = "";
            string[] arr = message.Split ('{');
            json = string.Join ("{\"", arr);
            string[] semiArr = json.Split (':');
            json = string.Join ("\":", semiArr);
            string[] comArr = json.Split (',');
            json = string.Join (",\"", comArr);
            Euler data = JsonUtility.FromJson<Euler> (json);
            eulerdata = data;
        });
    }
    //SensorData取得通信
    IEnumerator GetSensorData () {
        UnityWebRequest req = UnityWebRequest.Get (serverurl + "sensorData");
        req.SetRequestHeader ("key", "KEY");
        yield return req.SendWebRequest ();
        if (req.isNetworkError) {
            Debug.Log (req.error);
        } else {
            if (req.responseCode == 200) {
                //OK
                string jsonText = req.downloadHandler.text;
                //Debug.Log (jsonText);
                List<NineAxis> list = new List<NineAxis> ();
                list = JsonUtility.FromJson<Serialize<NineAxis>> (jsonText).ToList ();
                //Debug.Log (list[0].datetime);
                setTarget (list[0]);
            }
        }
    }
    //9軸オブジェクトへ適用
    void setTarget (NineAxis data) {
        //ax 0-10を想定
        float AX = (data.ax * 18) - 90.0f;
        //ay 0-10を想定
        float AY = (data.ay * 18) + 90.0f;
        //az 0-10を想定
        float AZ = -((data.az * 18) - 90.0f);
        //gx -1-1を想定
        float GX = ((data.gx + 1.0f) * 180) - 90.0f;
        //gy -1-1を想定
        float GY = ((data.gy + 1.0f) * 180);
        //gz -1-1を想定
        float GZ = ((data.gz + 1.0f) * 180);

        //Debug.Log ($"{data.ax:F3}");
        //target.transform.rotation = Quaternion.Euler (90, 180, 0) * (new Quaternion (-data.gx, -data.gy, data.gz, 0));;
        string datastr = "AX:" + $"{data.ax:F3}" + " AY:" + $"{data.ay:F3}" + " AZ:" + $"{data.az:F3}" + "\n";
        datastr += "LX:" + $"{data.lx:F3}" + " LY:" + $"{data.ly:F3}" + " LZ:" + $"{data.lz:F3}" + "\n";
        datastr += "GX:" + $"{data.gx:F3}" + " GY:" + $"{data.gy:F3}" + " GZ:" + $"{data.gz:F3}" + "\n";
        dataText.text = datastr;
    }
    void SetTargetEuler (Euler euler) {
        target.transform.rotation = Quaternion.Euler (euler.pitch, euler.head, euler.roll);
        if (eulerdata.pitch < -30.0f) {
            warningText.text = "Warning!\n危険な体勢です！";
            spotLight.SetActive (true);
        } else {
            warningText.text = "";
            spotLight.SetActive (false);
        }
    }
    //Post通信テスト
    IEnumerator PostServerData () {
        WWWForm form = new WWWForm ();
        //key:data
        form.AddField ("myField", "myData");
        form.AddField ("newxt", 20);

        using (UnityWebRequest req = UnityWebRequest.Post (serverurl + "Post", form)) {
            yield return req.SendWebRequest ();

            if (req.isNetworkError) {
                Debug.Log (req.error);
            } else {
                Debug.Log ("Form upload complete!");
            }
        }
    }
    IEnumerator ServerTest () {
        consoleText.text = "No Server";
        UnityWebRequest req = UnityWebRequest.Get (serverurl + "tests");
        req.SetRequestHeader ("key", "KEY");
        req.timeout = 10;
        yield return req.SendWebRequest ();
        if (req.isNetworkError) {
            Debug.Log (req.error);
            consoleText.text = req.error;
        } else {
            if (req.responseCode == 200) {
                //OK
            }
        }
        consoleText.text = "responceCode:" + req.responseCode.ToString ();
    }
    public void OnClickSetServer () {
        serverurl = address.text;
        StartCoroutine (ServerTest ());
    }
}