using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Managing Interactions for a single FBBIK effector.
	/// </summary>
	[System.Serializable]
	public class InteractionEffector {

		// The type of the effector
		public FullBodyBipedEffector effectorType { get; private set; }

		// Has the interaction been paused?
		public bool isPaused { get; private set; }
		// The current InteractionObject (null if there is no interaction going on)
		public InteractionObject interactionObject { get; private set; }
		// Is this InteractionEffector currently in the middle of an interaction?
		public bool inInteraction { get { return interactionObject != null; }}

		// Internal values
		private Poser poser;
		private IKEffector effector;
		private float timer, length, weight, fadeInSpeed, defaultPositionWeight, defaultRotationWeight, defaultPull, defaultReach, defaultPush, defaultPushParent, defaultBendGoalWeight, defaultPoserWeight, resetTimer;
		private float prevPositionWeight, prevRotationWeight, prevRotateBoneWeight, prevPull, prevReach, prevPush, prevPushParent, prevBendGoalWeight, prevPoserWeight, switchTimer;
		private bool positionWeightUsed, rotationWeightUsed, pullUsed, reachUsed, pushUsed, pushParentUsed, bendGoalWeightUsed, poserUsed;
		private bool prevPositionWeightUsed, prevRotationWeightUsed, prevRotateBoneWeightUsed, prevPullUsed, prevReachUsed, prevPushUsed, prevPushParentUsed, prevBendGoalWeightUsed, prevPoserUsed;
		private bool pickedUp, defaults, pickUpOnPostFBBIK;
		private Vector3 pickUpPosition, pausePositionRelative, prevTargetPosition;
		private Quaternion pickUpRotation, pauseRotationRelative, prevTargetRotation = Quaternion.identity, prevRotateBoneValue = Quaternion.identity;
		private InteractionTarget interactionTarget;
		private Transform target;
		private List<bool> triggered = new List<bool>();
		private InteractionSystem interactionSystem;
		private bool started;
		private bool isSwitching;

		// The custom constructor
		public InteractionEffector (FullBodyBipedEffector effectorType) {
			this.effectorType = effectorType;
		}

		// Initiate this, get the default values
		public void Initiate(InteractionSystem interactionSystem) {
			this.interactionSystem = interactionSystem;

			// Find the effector if we haven't already
			effector = interactionSystem.ik.solver.GetEffector(effectorType);
			poser = effector.bone.GetComponent<Poser>();

			StoreDefaults();
		}

        /// <summary>
        /// Store the default values to which the effector will be reset to after an interaction has ended.
        /// </summary>
		public void StoreDefaults() {
            if (interactionSystem == null) return;

			defaultPositionWeight = interactionSystem.ik.solver.GetEffector(effectorType).positionWeight;
			defaultRotationWeight = interactionSystem.ik.solver.GetEffector(effectorType).rotationWeight;
			defaultPoserWeight = poser != null? poser.weight: 0f;
			defaultPull = interactionSystem.ik.solver.GetChain(effectorType).pull;
			defaultReach = interactionSystem.ik.solver.GetChain(effectorType).reach;
			defaultPush = interactionSystem.ik.solver.GetChain(effectorType).push;
			defaultPushParent = interactionSystem.ik.solver.GetChain(effectorType).pushParent;
            defaultBendGoalWeight = interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight;
		}

		/// <summary>
		/// Store the current interaction values from which to interpolate to the next interaction
		/// </summary>
		public void StorePrevious(InteractionObject interactionObject)
		{
			if (interactionSystem == null) return;

			// See which InteractionObject.WeightCurve.Types are used
			prevTargetPosition = effector.position;
			prevTargetRotation = effector.rotation;

			prevPositionWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PositionWeight);
			prevRotationWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotationWeight);
			prevRotateBoneWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotateBoneWeight);
			prevPullUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Pull);
			prevReachUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Reach);
			prevPushUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Push);
			prevPushParentUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PushParent);
			prevBendGoalWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.BendGoalWeight);
			prevPoserUsed = poser != null && interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PoserWeight);

			prevPositionWeight = interactionSystem.ik.solver.GetEffector(effectorType).positionWeight;
			prevRotationWeight = interactionSystem.ik.solver.GetEffector(effectorType).rotationWeight;
			prevPoserWeight = poser != null ? poser.weight : 0f;
			prevPull = interactionSystem.ik.solver.GetChain(effectorType).pull;
			prevReach = interactionSystem.ik.solver.GetChain(effectorType).reach;
			prevPush = interactionSystem.ik.solver.GetChain(effectorType).push;
			prevPushParent = interactionSystem.ik.solver.GetChain(effectorType).pushParent;
			prevBendGoalWeight = interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight;

			switchTimer = 0f;
			isSwitching = true;
		}

		// Interpolate from previous to current interaction
		public bool Switch(float speed, float deltaTime)
		{
			//if (!inInteraction) return false;
			//if (isPaused) return false;
			if (!isSwitching) return false;

			switchTimer = Mathf.MoveTowards(switchTimer, 1f, deltaTime * speed);
			
			// Pull and Reach
			if (effector.isEndEffector)
			{
				if (prevPullUsed) interactionSystem.ik.solver.GetChain(effectorType).pull = Mathf.Lerp(prevPull, interactionSystem.ik.solver.GetChain(effectorType).pull, switchTimer);
				if (prevReachUsed) interactionSystem.ik.solver.GetChain(effectorType).reach = Mathf.Lerp(prevReach, interactionSystem.ik.solver.GetChain(effectorType).reach, switchTimer);
				if (prevPushUsed) interactionSystem.ik.solver.GetChain(effectorType).push = Mathf.Lerp(prevPush, interactionSystem.ik.solver.GetChain(effectorType).push, switchTimer);
				if (prevPushParentUsed) interactionSystem.ik.solver.GetChain(effectorType).pushParent = Mathf.Lerp(prevPushParent, interactionSystem.ik.solver.GetChain(effectorType).pushParent, switchTimer);
				if (prevBendGoalWeightUsed) interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight = Mathf.Lerp(prevBendGoalWeight, interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight, switchTimer);
			}

			// Effector weights
			if (prevPositionWeightUsed)
			{
				effector.positionWeight = Mathf.Lerp(prevPositionWeight, effector.positionWeight, switchTimer);
				effector.position = Vector3.Lerp(prevTargetPosition, effector.position, switchTimer);
			}
			if (prevRotationWeightUsed)
			{
				effector.rotationWeight = Mathf.Lerp(prevRotationWeight, effector.rotationWeight, switchTimer);
				effector.rotation = Quaternion.Lerp(prevTargetRotation, effector.rotation, switchTimer);
			}

			if (switchTimer >= 1f)
			{
				prevPullUsed = false;
				prevReachUsed = false;
				prevPushUsed = false;
				prevPushParentUsed = false;
				prevPositionWeightUsed = false;
				prevRotationWeightUsed = false;
				prevBendGoalWeightUsed = false;
				prevPoserUsed = false;

				isSwitching = false;
			}

			return true;
		}

		// Interpolate to default values when currently not in interaction
		public bool ResetToDefaults(float speed, float deltaTime) {
			if (inInteraction) return false;
			if (isPaused) return false;
			if (defaults) return false; 

			resetTimer = Mathf.MoveTowards(resetTimer, 0f, deltaTime * speed);

			// Pull and Reach
			if (effector.isEndEffector) {
				if (pullUsed) interactionSystem.ik.solver.GetChain(effectorType).pull = Mathf.Lerp(defaultPull, interactionSystem.ik.solver.GetChain(effectorType).pull, resetTimer);
				if (reachUsed) interactionSystem.ik.solver.GetChain(effectorType).reach = Mathf.Lerp(defaultReach, interactionSystem.ik.solver.GetChain(effectorType).reach, resetTimer);
				if (pushUsed) interactionSystem.ik.solver.GetChain(effectorType).push = Mathf.Lerp(defaultPush, interactionSystem.ik.solver.GetChain(effectorType).push, resetTimer);
				if (pushParentUsed) interactionSystem.ik.solver.GetChain(effectorType).pushParent = Mathf.Lerp(defaultPushParent, interactionSystem.ik.solver.GetChain(effectorType).pushParent, resetTimer);
                if (bendGoalWeightUsed) interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight = Mathf.Lerp(defaultBendGoalWeight, interactionSystem.ik.solver.GetChain(effectorType).bendConstraint.weight, resetTimer);
            }

			// Effector weights
			if (positionWeightUsed) effector.positionWeight = Mathf.Lerp(defaultPositionWeight, effector.positionWeight, resetTimer);
			if (rotationWeightUsed) effector.rotationWeight = Mathf.Lerp(defaultRotationWeight, effector.rotationWeight, resetTimer);

			if (resetTimer <= 0f) {
				pullUsed = false;
				reachUsed = false;
				pushUsed = false;
				pushParentUsed = false;
				positionWeightUsed = false;
				rotationWeightUsed = false;
                bendGoalWeightUsed = false;
				poserUsed = false;

				defaults = true;
			}

			return true;
		}

		// Pause this interaction
		public bool Pause() {
			if (!inInteraction) return false;
			isPaused = true;

			pausePositionRelative = target.InverseTransformPoint(effector.position);
			pauseRotationRelative = Quaternion.Inverse(target.rotation) * effector.rotation;

			if (interactionSystem.OnInteractionPause != null) {
				interactionSystem.OnInteractionPause(effectorType, interactionObject);
			}

			return true;
		}

		// Resume a paused interaction
		public bool Resume() {
			if (!inInteraction) return false;

			isPaused = false;
			if (interactionSystem.OnInteractionResume != null) interactionSystem.OnInteractionResume(effectorType, interactionObject);

			return true;
		}

        // Start interaction
        public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt)
        {
			if (inInteraction && !interrupt) return false;

            // Get the InteractionTarget
            InteractionTarget interactionTarget = null;
            target = interactionObject.GetTarget(effectorType, tag);
            if (target != null) interactionTarget = target.GetComponent<InteractionTarget>();
            
            return Start(interactionObject, interactionTarget, fadeInTime, interrupt);
        }

        public bool Start(InteractionObject interactionObject, InteractionTarget interactionTarget, float fadeInTime, bool interrupt)
        {
            // If not in interaction, set effector positions to their bones
            if (!inInteraction)
            {
                effector.position = effector.bone.position;
                effector.rotation = effector.bone.rotation;
            }
            else
            {
				if (!interrupt)
				{
					return false;
				}
				else
				{
					defaults = false;
					StorePrevious(interactionObject);
				}
            }

			this.interactionTarget = interactionTarget;

			target = interactionTarget != null? interactionTarget.transform: interactionObject.transform;

			// Start the interaction
			this.interactionObject = interactionObject;
            if (interactionSystem.OnInteractionStart != null) interactionSystem.OnInteractionStart(effectorType, interactionObject);
            interactionObject.OnStartInteraction(interactionSystem);

            // Cleared triggered events
            triggered.Clear();

            for (int i = 0; i < interactionObject.events.Length; i++)
            {
                triggered.Add(false);
            }

            // See which InteractionObject.WeightCurve.Types are used
            positionWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PositionWeight);
            rotationWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotationWeight);
            pullUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Pull);
            reachUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Reach);
            pushUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Push);
            pushParentUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PushParent);
            bendGoalWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.BendGoalWeight);
            poserUsed = poser != null && interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PoserWeight);

            // Posing the hand/foot
            if (poser != null && poserUsed)
            {
                if (poser.poseRoot == null) poser.weight = 0f;

                if (interactionTarget != null)
                {
					if (interactionTarget.usePoser)
					{
						poser.SetPoseRoot(target.transform, interactionTarget.bones, interactionSystem.switchInteractionSpeed);
					}
                } else
                {
					poser.SetPoseRoot(null, null, interactionSystem.switchInteractionSpeed);
                }

                poser.AutoMapping();
            }

            if (defaults) StoreDefaults();

            // Reset internal values
            timer = 0f;
            weight = 0f;
            fadeInSpeed = fadeInTime > 0f ? 1f / fadeInTime : 1000f;
            length = interactionObject.length;

            isPaused = false;
            pickedUp = false;
            pickUpPosition = Vector3.zero;
            pickUpRotation = Quaternion.identity;

            if (interactionTarget != null) interactionTarget.RotateTo(effector.bone);

            started = true;

            return true;
        }

		// Update the (possibly) ongoing interaction
		public void Update(Transform root, float speed, float deltaTime) {
			if (!inInteraction) {
				// If the InteractionObject has been destroyed, reset to defaults
				if (started) {
					isPaused = false;
					pickedUp = false;
					defaults = false;
					resetTimer = 1f;
					started = false;
				}
				return;
			}

			// Rotate target
			if (interactionTarget != null && !interactionTarget.rotateOnce) interactionTarget.RotateTo(effector.bone);

			if (isPaused) {
                if (!pickedUp)
                {
                    effector.position = target.TransformPoint(pausePositionRelative);
                    effector.rotation = target.rotation * pauseRotationRelative;
                }
				// Apply the current interaction state to the solver
				interactionObject.Apply(interactionSystem.ik.solver, effectorType, interactionTarget, timer, weight, true);

				return;
			}

			// Advance the interaction timer and weight
			timer += deltaTime * speed * (interactionTarget != null? interactionTarget.interactionSpeedMlp: 1f);
			weight = Mathf.Clamp(weight + deltaTime * fadeInSpeed * speed, 0f, 1f);

			// Interaction events
			bool pickUp = false;
			bool pause = false;
			TriggerUntriggeredEvents(true, out pickUp, out pause);

			// Effector target positions and rotations
			Vector3 targetPosition = pickedUp? interactionSystem.transform.TransformPoint(pickUpPosition): target.position;
			Quaternion targetRotation = pickedUp? interactionSystem.transform.rotation * pickUpRotation: target.rotation;

			// Interpolate effector position and rotation
			effector.position = Vector3.Lerp(effector.bone.position, targetPosition, weight);
			effector.rotation = Quaternion.Lerp(effector.bone.rotation, targetRotation, weight);

			// Apply the current interaction state to the solver
			interactionObject.Apply(interactionSystem.ik.solver, effectorType, interactionTarget, timer, weight, false);

            if (pickUp) PickUp(root);
            if (pause) Pause();

            // Hand poser weight
            float poserWeight = interactionObject.GetValue (InteractionObject.WeightCurve.Type.PoserWeight, interactionTarget, timer);

			if (poser != null && poserUsed) {
				poser.weight = Mathf.Lerp (poser.weight, poserWeight, weight);
			} else {
				if (poserWeight > 0f) {
					Warning.Log("InteractionObject " + interactionObject.name + " has a curve/multipler for Poser Weight, but the bone of effector " + effectorType.ToString() + " has no HandPoser/GenericPoser attached.", effector.bone);
				}
			}

			if (timer >= length) Stop();
		}

		// Get the normalized progress of the interaction
		public float progress {
			get {
				if (!inInteraction) return 0f;
				if (length == 0f) return 0f;
				return timer / length;
			}
		}

		// Go through all the InteractionObject events to trigger the ones that have not yet triggered
		private void TriggerUntriggeredEvents(bool checkTime, out bool pickUp, out bool pause) {
			pickUp = false;
			pause = false;

			for (int i = 0; i < triggered.Count; i++) {
				// If this event has not been triggered by this effector
				if (!triggered[i]) {
					
					// If time has passed...
					if (!checkTime || interactionObject.events[i].time < timer) {
						
						// Activate the event
						interactionObject.events[i].Activate(effector.bone);
						
						// Picking up
						if (interactionObject.events[i].pickUp) {
							if (timer >= interactionObject.events[i].time) timer = interactionObject.events[i].time;

							pickUp = true;
						}
						
						// Pausing
						if (interactionObject.events[i].pause) {
							if (timer >= interactionObject.events[i].time) timer = interactionObject.events[i].time;

							pause = true;
						}
						
						if (interactionSystem.OnInteractionEvent != null) interactionSystem.OnInteractionEvent(effectorType, interactionObject, interactionObject.events[i]);
						
						triggered[i] = true;
					}
				}
			}
		}

		// Trigger the interaction object
		private void PickUp(Transform root) {
			// Picking up the object
			pickUpPosition = root.InverseTransformPoint(effector.position);
			pickUpRotation = Quaternion.Inverse(interactionSystem.transform.rotation) * effector.rotation;
				
			pickUpOnPostFBBIK = true;

			pickedUp = true;

			var rigidbody = interactionObject.targetsRoot.GetComponent<Rigidbody>();

			if (rigidbody != null) {
				if (!rigidbody.isKinematic) {
					rigidbody.isKinematic = true;
				}

                // Ignore collisions between the character and the colliders of the interaction object
                var rootCollider = root.GetComponent<Collider>();

				if (rootCollider != null) {
					var colliders = interactionObject.targetsRoot.GetComponentsInChildren<Collider>();

					foreach (Collider collider in colliders) {
						if (!collider.isTrigger && collider.enabled) Physics.IgnoreCollision(rootCollider, collider);
					}
				}
			}
				
			if (interactionSystem.OnInteractionPickUp != null) interactionSystem.OnInteractionPickUp(effectorType, interactionObject);
		}

		// Stop the interaction
		public bool Stop() {
			if (!inInteraction) return false;

			bool pickUp = false;
			bool pause = false;
			TriggerUntriggeredEvents(false, out pickUp, out pause);

			if (interactionSystem.OnInteractionStop != null) interactionSystem.OnInteractionStop(effectorType, interactionObject);

			// Reset the interaction target
			if (interactionTarget != null) interactionTarget.ResetRotation();

			// Reset the internal values
			interactionObject = null;
			weight = 0f;
			timer = 0f;

			isPaused = false;
			target = null;
			defaults = false;
			resetTimer = 1f;
			//if (poser != null && !pickedUp) poser.weight = 0f;
			pickedUp = false;
			started = false;

			return true;
		}

		// Called after FBBIK update
		public void OnPostFBBIK() {
			if (!inInteraction) return;

			// Rotate the hands/feet to the RotateBoneWeight curve
			float rotateBoneWeight = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, interactionTarget, timer) * weight;
			
			if (isSwitching)
            {
				rotateBoneWeight = Mathf.Lerp(prevRotateBoneWeight, rotateBoneWeight, switchTimer);
            } else
            {
				prevRotateBoneWeight = rotateBoneWeight;
            }

			if (rotateBoneWeight > 0f) {
				Quaternion r = pickedUp? interactionSystem.transform.rotation * pickUpRotation: effector.rotation;

				Quaternion targetRotation = Quaternion.Slerp(effector.bone.rotation, r, rotateBoneWeight * rotateBoneWeight);
				effector.bone.localRotation = Quaternion.Inverse(effector.bone.parent.rotation) * targetRotation;
			}

			if (isSwitching)
			{
				effector.bone.localRotation = Quaternion.Slerp(prevRotateBoneValue, effector.bone.localRotation, switchTimer);
			}
			else
			{
				prevRotateBoneValue = effector.bone.localRotation;
			}

			// Positioning the interaction object to the effector (not the bone, because it is still at its animated translation)
			if (pickUpOnPostFBBIK) {
				Vector3 bonePosition = effector.bone.position;
				effector.bone.position = interactionSystem.transform.TransformPoint(pickUpPosition);

				interactionObject.targetsRoot.parent = effector.bone;
				
				effector.bone.position = bonePosition;

				pickUpOnPostFBBIK = false;
			}
		}
	}
}
