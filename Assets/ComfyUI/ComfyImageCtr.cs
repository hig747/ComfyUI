using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;

[System.Serializable]
public class ImageData
{
    public string filename;
    public string subfolder;
    public string type;
}

[System.Serializable]
public class OutputData
{
    public ImageData[] images;
}

[System.Serializable]
public class PromptData
{
    public OutputData outputs;
}

public class ComfyImageCtr: MonoBehaviour
{

public void RequestFileName(string id){
    StartCoroutine(RequestFileNameRoutine(id));
}

 IEnumerator RequestFileNameRoutine(string promptID)
    {
        string url = "http://127.0.0.1:8188/history/" + promptID;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    string imageURL = "http://127.0.0.1:8188/view?filename=" +ExtractFilename(webRequest.downloadHandler.text);
                    StartCoroutine(DownloadImage(imageURL));
                    break;
            }
        }
    }
    
    string ExtractFilename(string jsonString)
    {
        // Step 1: Identify the part of the string that contains the "filename" key
        string OutputKey = "UnityExport_";
        string keyToLookFor = "\"filename\":";


//        int startIndex = jsonString.LastIndexOf(keyToLookFor);
        int startIndex = jsonString.IndexOf(OutputKey);

        string jsonString2 = jsonString.Substring(startIndex);


        /*
                if (startIndex == -1)
                {
                    return "filename key not found";
                }

                // Adjusting startIndex to get the position right after the keyToLookFor
                startIndex += OutputKey.Length;

                string jsonString2 = jsonString.Substring(startIndex);

                startIndex = jsonString2.IndexOf(keyToLookFor);

                if (startIndex == -1)
                {
                    return "filename key not found";
                }
                startIndex += keyToLookFor.Length;


                // Step 2: Extract the substring starting from the "filename" key
                string fromFileName = jsonString2.Substring(startIndex);
        */

        string fromFileName = jsonString.Substring(startIndex);
        // Assuming that filename value is followed by a comma (,)
        int endIndex = fromFileName.IndexOf(',');

        // Extracting the filename value (assuming it's wrapped in quotes)
        string filenameWithQuotes = fromFileName.Substring(0, endIndex).Trim();

        // Removing leading and trailing quotes from the extracted value
        string filename = filenameWithQuotes.Trim('"');
        Debug.Log(filename);
        return filename;
    }

    public Image outputImage;
    
     IEnumerator DownloadImage(string imageUrl)
    {
         yield return new WaitForSeconds(0.5f);
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

                outputImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    
            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
            }
        }
    }
}
