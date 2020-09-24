namespace Koobando.AntiCheat
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using HttpsCallableReference = Firebase.Functions.HttpsCallableReference;

	[DisallowMultipleComponent]
	public class EncryptedCheatingDetector : DetectorBase<EncryptedCheatingDetector>
	{

		/// <summary>
		/// Max allowed difference between encrypted and fake values in encrypted types. Increase in case of false positives.
		/// </summary>
		public double doubleEpsilon = 0.0001d;
		public float floatEpsilon = 0.0001f;
		public float vector2Epsilon = 0.1f;
		public float vector3Epsilon = 0.1f;
		public float quaternionEpsilon = 0.1f;

		protected override void Start() {
			base.Start();
			StartDetection(BanPlayerOnEncryptedCheating);
		}

		protected void BanPlayerOnEncryptedCheating() {
			if (!started) {
				return;
			}
			if (PlayerData.playerdata == null) {
				Debug.Log("2");
            	Application.Quit();
			} else {
				// Ban player here for modifying memory
				Dictionary<string, object> inputData = new Dictionary<string, object>();
				inputData["callHash"] = DAOScript.functionsCallHash;
				inputData["uid"] = AuthScript.authHandler.user.UserId;
				inputData["duration"] = "-1";
				inputData["reason"] = "Illegal modification of memory.";

				HttpsCallableReference func = DAOScript.dao.functions.GetHttpsCallable("banPlayer");
				func.CallAsync(inputData).ContinueWith((task) => {
					PlayerData.playerdata.TriggerEmergencyExit("You've been banned for the following reason:\nIllegal modification of game data.\nIf you feel this was done in error, you can dispute it by opening a ticket at \"www.koobando.com/support\".");
				});
			}
		}

		public static EncryptedCheatingDetector AddToSceneOrGetExisting()
		{
			return GetOrCreateInstance;
		}

		public static EncryptedCheatingDetector StartDetection()
		{
			if (Instance != null)
			{
				return Instance.StartDetectionInternal(null);
			}

			return null;
		}

		public static EncryptedCheatingDetector StartDetection(Action callback)
		{
			return GetOrCreateInstance.StartDetectionInternal(callback);
		}

		public static void StopDetection()
		{
			if (Instance != null) Instance.StopDetectionInternal();
		}

		public static void Dispose()
		{
			if (Instance != null) Instance.DisposeInternal();
		}

		private EncryptedCheatingDetector StartDetectionInternal(Action callback) {
			if (isRunning)
			{
				return this;
			}

			if (!enabled)
			{
				return this;
			}

			if (callback == null && !detectionEventHasListener)
			{
				enabled = false;
				return this;
			}

			CheatDetected += callback;
			started = true;
			isRunning = true;

			return this;
		}

		internal static bool ExistsAndIsRunning
		{
			get
			{
                return (object)Instance != null && Instance.IsRunning;
			}
		}


		private EncryptedCheatingDetector() {} // prevents direct instantiation
    }
}