using Hamster;
using UnityEngine;

[ExecuteInEditMode]
public class TestScript : MonoBehaviour {

    private ClientNetDevice _clientDevice = new ClientNetDevice();
    private ServerNetDevice _serverDevice = new ServerNetDevice();

    public string IP = "127.0.0.1";
    public int Port = 8080;

    private float _serverTick = 0;

    public void Awake() {
        _serverDevice.Listen(IP, Port);

        _clientDevice.RegistModule(new Hamster.SpaceWar.NetPingModule());
        _serverDevice.RegistModule(new Hamster.SpaceWar.NetPingModule());
    }


    public void Update() {
        _serverTick += Time.deltaTime;
        if (_serverTick >= 0.1) {
            _serverDevice.Update();
            _serverTick = 0;
        }
        if (_clientDevice.IsValid)
            _clientDevice.Update();
    }

    private void OnGUI() {
        GUILayout.Label(string.Format("Server Tick: {0}", _serverTick));
        if (!_clientDevice.IsValid) {
            if (GUILayout.Button("Connect")) {
                _clientDevice.Connect(IP, Port);
            }
        }
        if (GUILayout.Button("Close")) {
            _clientDevice.Close();
            _serverDevice.Close();
        }
    }
}
