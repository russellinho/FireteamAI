namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;
	using UnityEngine.Events;

	public abstract class DetectorBase<T> : KeepAliveBehaviour<T> where T: DetectorBase<T>
	{
		public event Action CheatDetected;
		public bool IsCheatDetected { get; protected set; }
		[SerializeField]
		protected UnityEvent detectionEvent;

		[SerializeField]
		protected bool detectionEventHasListener;
		protected bool started;
		protected bool isRunning;

		public bool IsStarted
		{
			get { return started; }
		}

		public bool IsRunning
		{
			get { return isRunning; }
		}

		#region unity messages

		protected override void Start()
		{
			base.Start();
		}

		private void OnEnable()
		{
			ResumeDetector();
		}

		private void OnDisable()
		{
			PauseDetector();
		}

		private void OnApplicationQuit()
		{
			DisposeInternal();
		}

		protected override void OnDestroy()
		{
			StopDetectionInternal();
			base.OnDestroy();
		}
		#endregion

		// TODO: Ban player here
		internal virtual void OnCheatingDetected() {
			IsCheatDetected = true;

			if (CheatDetected != null) {
				CheatDetected.Invoke();
			}

			if (detectionEventHasListener) {
				detectionEvent.Invoke();
			}

			StopDetectionInternal();
		}

		protected virtual bool DetectorHasCallbacks()
		{
			return CheatDetected != null || detectionEventHasListener;
		}

		protected virtual void StopDetectionInternal()
		{
			CheatDetected = null;
			started = false;
			isRunning = false;
		}

		protected virtual void PauseDetector()
		{
			if (!started)
				return;

			isRunning = false;
		}

		protected virtual bool ResumeDetector()
		{
			if (!started || !DetectorHasCallbacks())
				return false;

			isRunning = true;
			return true;
		}

	}
}