using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor.ShaderGraph.Internal;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}
public class ComfyPromptCtr : MonoBehaviour
{

    public InputField pInput, nInput, humanImageInput, bgImageInput, cfgInput,weightInput,promptJsonInput;



    private void Start()
    {
        // QueuePrompt("pretty man","watermark");
    }

    public void QueuePrompt()
    {
        StartCoroutine(QueuePromptCoroutine(pInput.text, nInput.text, cfgInput.text, weightInput.text, humanImageInput.text, bgImageInput.text));
    }
    private IEnumerator QueuePromptCoroutine(string positivePrompt, string negativePrompt, string cfgValue, string weightValue, string humanImage, string bgImage)
    {
        string url = "http://127.0.0.1:8188/prompt";
        string dataPath = "file://D:/Unity/ComfyUI/Assets/StreamingAssets/";

        string promptText = GeneratePromptJson();

        promptText = promptText.Replace("UnityPprompt", positivePrompt);
        promptText = promptText.Replace("UnityNprompt", negativePrompt);

        promptText = promptText.Replace("UnityCfg",cfgValue);
        promptText = promptText.Replace("UnityWeight", weightValue);


        //        nblack and white, bad eyes, deformed eyes, deformed face

        promptText = promptText.Replace("UnityFace", dataPath+humanImage);
        promptText = promptText.Replace("UnityBg", dataPath+bgImage);


        Debug.Log(promptText);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            Debug.Log("Prompt ID: " + data.prompt_id);
            GetComponent<ComfyWebsocket>().promptID = data.prompt_id;
            // GetComponent<ComfyImageCtr>().RequestFileName(data.prompt_id);
        }
    }
    public string promptJson;

    private string GeneratePromptJson()
    {
        string guid = Guid.NewGuid().ToString();

        string promptJsonWithGuid = $@"
{{
    ""id"": ""{guid}"",
    ""prompt"": {promptJson}
}}";

        return promptJsonWithGuid;
    }
}
