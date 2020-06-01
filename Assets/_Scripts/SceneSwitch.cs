using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public void SwitchScene(string scene)
    {
        if (VideoStuff.isClient != InitSettings.isClient || (VideoStuff.isClient))
            VideoStuff.shutdownJobs();

        //        SceneManager.UnloadSceneAsync("Video Player");
        SceneManager.LoadScene(scene, LoadSceneMode.Single);

        //      while( SceneManager.SetActiveScene(SceneManager.GetSceneByName("Video Player")));
    }
}