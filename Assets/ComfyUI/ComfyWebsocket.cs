using System.Collections.Generic;
using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;


[Serializable]
public class PRORESS_DATA_CORE
{
    public int value;
    public int max;
    public string prompt_id;
    public string node;
}

[Serializable]
public class PRORESS_DATA
{
    public string type;
    public PRORESS_DATA_CORE data;

}


public class ResponseDataWebsocket
{
    public string prompt_id;
}
public class ComfyWebsocket : MonoBehaviour
{
    private string serverAddress = "127.0.0.1:8188";
    private string clientId = Guid.NewGuid().ToString();
    private ClientWebSocket ws = new ClientWebSocket();

    int cnt = 0;


    public ComfyImageCtr comfyImageCtr;
    async void Start()
    {

        await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);
        StartListening();


    }
    public string promptID;
    private async void StartListening()
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = null;

        while (ws.State == WebSocketState.Open)
        {
            var stringBuilder = new StringBuilder();
            do
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    stringBuilder.Append(str);
                }
            }
            while (!result.EndOfMessage);

            string response = stringBuilder.ToString();
            Debug.Log("Received: " + response);

            string keywordVal = "\"value\": ";
            int val = GetNumber(response, keywordVal);
            string keywordMax = "\"max\": ";
            int max = GetNumber(response, keywordMax);
            if ( val != -1)
            {
                Debug.Log("Progress : " + val.ToString() + "/" + max.ToString());
            }



            /*
                        {


                                            PRORESS_DATA json = JsonUtility.FromJson<PRORESS_DATA>(response);
                                            if (json != null)
                                            {
                                                Debug.Log(json.data.value.ToString() + "/" + json.data.max.ToString());
                                            }

                        }
            */
            if (response.Contains("\"queue_remaining\": 0"))
            {
                comfyImageCtr.RequestFileName(promptID);
            }


        }
    }

    private int GetNumber(string response, string keyword)
    {
        
        int idx = response.IndexOf(keyword);

        if (idx != -1)
        {
            string num = response.Substring(idx + keyword.Length);

            int endIndex = num.IndexOf(',');

            // Extracting the filename value (assuming it's wrapped in quotes)
            string num2 = num.Substring(0, endIndex).Trim();

            return (int.Parse(num2));

        }
        return -1;
    }

    void OnDestroy()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}
