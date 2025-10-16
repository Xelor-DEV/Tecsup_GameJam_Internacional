using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GlobalSceneManager : NonPersistentSingleton<GlobalSceneManager>
{
    public void InitializeLoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    public IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    public AsyncOperation LoadSceneAsyncWithoutActivation(string sceneName, bool isAdditive)
    {
        LoadSceneMode mode;

        if(isAdditive == true)
        {
            mode = LoadSceneMode.Additive;
        }
        else
        {
            mode = LoadSceneMode.Single;
        }

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
        asyncOp.allowSceneActivation = false;
        return asyncOp;
    }

    public IEnumerator WaitForAllOperations(List<AsyncOperation> operations)
    {
        foreach (AsyncOperation op in operations)
        {
            while (!op.isDone)
            {
                yield return null;
            }
        }
    }

    public IEnumerator WaitUntilAllOperationsReady(AsyncOperation mainOp, List<AsyncOperation> additiveOps)
    {
        bool allReady = false;
        while (allReady == false)
        {
            allReady = true;

            if (mainOp.progress < 0.9f)
            {
                allReady = false;
            }

            for (int i = 0; i < additiveOps.Count; ++i)
            {
                AsyncOperation op = additiveOps[i];
                if (op.progress < 0.9f)
                {
                    allReady = false;
                    break;
                }
            }
            yield return null;
        }
    }

    public void SetActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() == true && scene.isLoaded == true)
        {
            SceneManager.SetActiveScene(scene);
        } 
    }
}