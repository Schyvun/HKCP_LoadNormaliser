//Made in Dnspy but changes are:

//In SceneLoad:
private IEnumerator BeginRoutine()
	{
    //in case the variable from Gamemanager is borked (variable from Gamemanager includes the whole unloading process, 
    // (usually takes 20ms) which is less important than the actual Loading process)
		float loadTime = Time.realtimeSinceStartup; 
		SceneAdditiveLoadConditional.loadInSequence = true;
		yield return this.runner.StartCoroutine(ScenePreloader.FinishPendingOperations());
		this.RecordBeginTime(SceneLoad.Phases.FetchBlocked);
		while (!this.IsFetchAllowed)
		{
			yield return null;
		}
		this.RecordEndTime(SceneLoad.Phases.FetchBlocked);
		this.RecordBeginTime(SceneLoad.Phases.Fetch);
		AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(this.targetSceneName, LoadSceneMode.Additive);
		loadOperation.allowSceneActivation = false;
		while (loadOperation.progress < 0.9f)
		{
			yield return null;
		}
		this.RecordEndTime(SceneLoad.Phases.Fetch);
		if (this.FetchComplete != null)
		{
			try
			{
				this.FetchComplete();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception in responders to SceneLoad.FetchComplete. Attempting to continue load regardless.");
				Debug.LogException(exception);
			}
		}
		this.RecordBeginTime(SceneLoad.Phases.ActivationBlocked);
		while (!this.IsActivationAllowed)
		{
			yield return null;
		}
		this.RecordEndTime(SceneLoad.Phases.ActivationBlocked);
		this.RecordBeginTime(SceneLoad.Phases.Activation);
		if (this.WillActivate != null)
		{
			try
			{
				this.WillActivate();
			}
			catch (Exception exception2)
			{
				Debug.LogError("Exception in responders to SceneLoad.WillActivate. Attempting to continue load regardless.");
				Debug.LogException(exception2);
			}
		}
    // Wait before telling the game that it can activate the Scene
		if (GameManager.loadTime != 0f)
		{
			loadTime = GameManager.loadTime;
		}
		float num = Time.realtimeSinceStartup - loadTime;
		yield return new WaitForSecondsRealtime(Mathf.Clamp(2f - num, 0f, 2f));
		Debug.Log(string.Concat(new object[]
		{
			"Time for loading was: ",
			num
		}));
		loadOperation.allowSceneActivation = true;
		yield return loadOperation;
		this.RecordEndTime(SceneLoad.Phases.Activation);
		if (this.ActivationComplete != null)
		{
			try
			{
				this.ActivationComplete();
			}
			catch (Exception exception3)
			{
				Debug.LogError("Exception in responders to SceneLoad.ActivationComplete. Attempting to continue load regardless.");
				Debug.LogException(exception3);
			}
		}
		this.RecordBeginTime(SceneLoad.Phases.UnloadUnusedAssets);
		if (this.IsUnloadAssetsRequired)
		{
			AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
			yield return asyncOperation;
		}
		this.RecordEndTime(SceneLoad.Phases.UnloadUnusedAssets);
		this.RecordBeginTime(SceneLoad.Phases.GarbageCollect);
		if (this.IsGarbageCollectRequired)
		{
			GCManager.Collect();
		}
		this.RecordEndTime(SceneLoad.Phases.GarbageCollect);
		if (this.Complete != null)
		{
			try
			{
				this.Complete();
			}
			catch (Exception exception4)
			{
				Debug.LogError("Exception in responders to SceneLoad.Complete. Attempting to continue load regardless.");
				Debug.LogException(exception4);
			}
		}
		this.RecordBeginTime(SceneLoad.Phases.StartCall);
		yield return null;
		this.RecordEndTime(SceneLoad.Phases.StartCall);
		if (this.StartCalled != null)
		{
			try
			{
				this.StartCalled();
			}
			catch (Exception exception5)
			{
				Debug.LogError("Exception in responders to SceneLoad.StartCalled. Attempting to continue load regardless.");
				Debug.LogException(exception5);
			}
		}
		if (SceneAdditiveLoadConditional.ShouldLoadBoss)
		{
			this.RecordBeginTime(SceneLoad.Phases.LoadBoss);
			yield return this.runner.StartCoroutine(SceneAdditiveLoadConditional.LoadAll());
			this.RecordEndTime(SceneLoad.Phases.LoadBoss);
			try
			{
				if (this.BossLoaded != null)
				{
					this.BossLoaded();
				}
				if (GameManager.instance)
				{
					GameManager.instance.LoadedBoss();
				}
			}
			catch (Exception exception6)
			{
				Debug.LogError("Exception in responders to SceneLoad.BossLoaded. Attempting to continue load regardless.");
				Debug.LogException(exception6);
			}
		}
		try
		{
			ScenePreloader.Cleanup();
		}
		catch (Exception exception7)
		{
			Debug.LogError("Exception in responders to ScenePreloader.Cleanup. Attempting to continue load regardless.");
			Debug.LogException(exception7);
		}
		this.IsFinished = true;
		if (this.Finish != null)
		{
			try
			{
				this.Finish();
				yield break;
			}
			catch (Exception exception8)
			{
				Debug.LogError("Exception in responders to SceneLoad.Finish. Attempting to continue load regardless.");
				Debug.LogException(exception8);
				yield break;
			}
		}
		yield break;
}

//Changes in Gamemanager:
public static float loadTime;
private IEnumerator BeginSceneTransitionRoutine(GameManager.SceneLoadInfo info)
	{
		GameManager.loadTime = Time.realtimeSinceStartup;
		if (this.sceneLoad != null)
    ...
    // rest isn't changed
  }
  
  
// Display "Xs LoadNormaliser" in the Main Menu
private void OnGUI()
	{
		if (this.GetSceneNameString() == "Menu_Title")
		{
			Color backgroundColor = GUI.backgroundColor;
			Color contentColor = GUI.contentColor;
			Color color = GUI.color;
			Matrix4x4 matrix = GUI.matrix;
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
			GUI.color = Color.white;
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 1920f, (float)Screen.height / 1080f, 1f));
			GUI.Label(new Rect(20f, 20f, 200f, 200f), "2s LoadNormaliser", new GUIStyle
			{
				fontSize = 30,
				normal = new GUIStyleState
				{
					textColor = Color.white
				}
			});
			GUI.backgroundColor = backgroundColor;
			GUI.contentColor = contentColor;
			GUI.color = color;
			GUI.matrix = matrix;
		}
	}
