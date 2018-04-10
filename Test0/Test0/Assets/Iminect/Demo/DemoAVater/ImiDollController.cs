using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

using IMIForUnity;
using ImiAvaterEngine;

public class ImiDollController : MonoBehaviour {
	// Bool that has the characters (facing the player) actions become mirrored. Default true.
	public bool MirroredMovement = true;

    public bool JointSmoothFilter = true;

    public bool BindMainPlayer = true;

    // Bool that determines whether the player will move or not in space.
    private bool MovesInSpace = false;

    // Bool that determines whether the player is allowed to jump -- vertical movement
	// can cause some models to behave strangely, so use at your own discretion.
    private bool VerticalMovement = false;

    // Rate at which player will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
    private int MoveRate = 1;
	
	// Slerp smooth factor
    public float SmoothFactor = 0;

    private ImiSkeleton[] g_skeleton;

	// Public variables that will get matched to bones. If empty, the kinect will simply not track it.
	// These bones can be set within the Unity interface.
    public Transform Head;
    public Transform Neck;
    public Transform Spine;
    public Transform HipCenter;
    public Transform HipCenter1;

    public Transform LeftShoulder;
    public Transform LeftElbow;
    public Transform LeftWrist;
    public Transform LeftHand;

    public Transform RightShoulder;
    public Transform RightElbow;
    public Transform RightWrist;
    public Transform RightHand;

    public Transform LeftHip;
    public Transform LeftKnee;
    public Transform LeftAnkle;
    public Transform LeftFoot;

    public Transform RightHip;
    public Transform RightKnee;
    public Transform RightAnkle;
    public Transform RightFoot;

    public Transform Root;

    // A required variable if you want to rotate the model in space.
    public GameObject offsetNode;

    // Variable to hold all them bones. It will initialize the same size as initialRotations.
    private Transform[] bones;

    // Rotations of the bones when the Kinect tracking starts.
    private Quaternion[] initialRotations;
    protected Quaternion[] initialLocalRotations;

    // Calibration Offset Variables for Character Position.
    private bool OffsetCalibrated = false;
    private float XOffset, YOffset, ZOffset;
    
    // Initial position and rotation of the transform
    protected Vector3 originalPosition;
    private Quaternion originalRotation;

    /// add by sunsai
    private bool CustomDepthDataProcessor = true;
    private bool floorDetected = false;
    private Vector4 floorPlane = new Vector4();

    private JointPositionsFilter filterJoints;
    private BoneOrientationsConstraint boneConstraint;
    private BoneOrientationsFilter boneOrientationFilter;
    private ClippedLegsFilter clippedLegsFilter;
    private BoneOrientationsConstraint boneConstraintsFilter;
    private SelfIntersectionConstraint selfIntersectionConstraint;
    private JointTiltCorrection jointTiltCorrection;
    // Timer for controlling Filter Lerp blends.
    private float lastNuiTime;
    // The local skeleton with leg filtering applied.
    private ImiSkeleton[] filteredSkeleton;

    private ImiJointConstraint jointconstraint;

    private IManager imiManager;


    Dictionary<int, IPlayerInfo> playerInfos;

    // transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
    private Transform _transformCache;
    public new Transform transform
    {
        get
        {
            if (!_transformCache)
                _transformCache = base.transform;

            return _transformCache;
        }
    }

    public void Start() 
    {

        // check for double start
        if (bones != null)
            return;

        jointconstraint = new ImiJointConstraint();

        /// add by sunsai£¬bone orientation constraint and filter
        /// 
        g_skeleton = new ImiSkeleton[(int)ImiSkeleton.Index.COUNT];
        this.filteredSkeleton = new ImiSkeleton[(int)ImiSkeleton.Index.COUNT];
        this.filterJoints = new JointPositionsFilter();
        this.filterJoints.Init();
        jointTiltCorrection = new JointTiltCorrection();
        boneConstraint = new BoneOrientationsConstraint();
        boneConstraint.AddDefaultConstraints();

        // init the bone orientation filter
        boneOrientationFilter = new BoneOrientationsFilter();
        boneOrientationFilter.Init();

        // init the clipped legs filter
        clippedLegsFilter = new ClippedLegsFilter();
        // init the bone orientation constraints
        boneConstraintsFilter = new BoneOrientationsConstraint();
        boneConstraintsFilter.AddDefaultConstraints();
        // init the self intersection constraints
        selfIntersectionConstraint = new SelfIntersectionConstraint();
        ///

        // Holds our bones for later.
        bones = new Transform[(int)ImiSkeleton.Index.COUNT];
		
		// Initial rotations of said bones.
		initialRotations = new Quaternion[(int)ImiSkeleton.Index.COUNT];
        initialLocalRotations = new Quaternion[(int)ImiSkeleton.Index.COUNT];

		// Map bones to the points the Kinect tracks.
		MapBones();

		// Get initial rotations to return to later.
		GetInitialRotations();
    }

    public void Update() 
    {
        if (imiManager == null)
        {
            imiManager = IManagerFactory.GetInstance(); ;// ImiManager.GetInstance();
        }

        if (imiManager.IsDeviceWorking())
        {
            ProcessSkeleton();
        }
	}

    // Process the skeleton data
    private void ProcessSkeleton()
    {
        //ImiPlayerInfo mInfo = null;

        // calculate the time since last update
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - lastNuiTime;

        //Dictionary<int, IPlayerInfo> playerInfos = imiManager.IEUpdatePlayerInfos();

        playerInfos = imiManager.IEUpdatePlayerInfos();

        if (playerInfos == null || playerInfos.Count <= 0)
        {
            return;
        }


        foreach (int key in playerInfos.Keys)
        {
            // Display main player skeleton
            if (key == imiManager.IEGetMainUserId())
            {
                ImiSkeleton[] skeletons = playerInfos[key].GetSkeletons(); ;

                //mInfo = BindMainPlayer ? imiManager.GetMainPlayerInfo() : imiManager.GetSubPlayerInfo();

                if (skeletons[(int)ImiSkeleton.Index.HIP_CENTER].isTracked
                    && skeletons[(int)ImiSkeleton.Index.HIP_RIGHT].isTracked
                    && skeletons[(int)ImiSkeleton.Index.HIP_LEFT].isTracked
                    && skeletons[(int)ImiSkeleton.Index.HEAD].position.y > skeletons[(int)ImiSkeleton.Index.HIP_CENTER].position.y
                    )
                {
                    ImiHelper.CopySkeleton(ref skeletons, ref g_skeleton);
                    filterJoints.UpdateFilter(ref g_skeleton);
                    //g_skeleton = mInfo.skeletons;

                    // add by sunsai:bone orientation constraint and filter
                    // note: matrix orientation must be caculated before calling below funcs
                    //   matrix orientation of the player who bind with the contorller will be constrainted,
                    //  but quaternion orientation has not been constraint yet,and this will be done in UpdatePlayer(TransformBone) func.

                    // legs constraint and self intersection constraint
                    clippedLegsFilter.FilterSkeleton(ref g_skeleton, deltaNuiTime);
                    selfIntersectionConstraint.Constrain(ref g_skeleton);

                    //if (imiManager.GetFloorDetectedFlag())
                    //{
                    //    //jointTiltCorrection.CorrectSensorTilt2(ref g_skeleton, imiManager.GetFloorPlane());
                    //    Debug.LogError("Floor correction");
                    //}

                    // joint position filter 
                    filterJoints.UpdateFilter(ref g_skeleton);

                    // calculate bone orientation

                    //todo
                    ImiHelper.CalcJointsOri(ref g_skeleton);

                    // orientation constraint and filter
                    boneConstraint.Constrain(ref g_skeleton);
                    boneOrientationFilter.UpdateFilter(ref g_skeleton);

                    if ((g_skeleton[(int)ImiSkeleton.Index.FOOT_LEFT].isTracked || g_skeleton[(int)ImiSkeleton.Index.KNEE_LEFT].isTracked)
                        && (g_skeleton[(int)ImiSkeleton.Index.FOOT_RIGHT].isTracked || g_skeleton[(int)ImiSkeleton.Index.KNEE_RIGHT].isTracked))
                    {
                        UpdatePlayer(ref g_skeleton, false);
                    }
                    else
                    {
                        UpdatePlayer(ref g_skeleton, true);
                    }
                }
                else
                {
                    RotateToInitialPosition1();
                    // RotateToCalibrationPose(true);
                    clippedLegsFilter.Reset();
                    boneOrientationFilter.Reset();
                    filterJoints.Reset();
                }



            }
        }
   

        // update the nui-timer
        lastNuiTime = currentNuiTime;
    }

    // Update the player each frame.
    private void UpdatePlayer(ref ImiSkeleton[] skeletonJoints,bool IsNearMode) 
    {
        Utils.isMirror = MirroredMovement;
        jointconstraint.isJointSmoothFilter = JointSmoothFilter;

        // Update Head, Neck, Spine, and Hips normally.
        TransformBone(skeletonJoints,ImiSkeleton.Index.HIP_CENTER, 0);
        TransformBone(skeletonJoints, ImiSkeleton.Index.SPINE, 1);
        TransformBone(skeletonJoints, ImiSkeleton.Index.SHOULDER_CENTER, 2);
        TransformBone(skeletonJoints, ImiSkeleton.Index.HEAD, 3);

        TransformBone(skeletonJoints, ImiSkeleton.Index.SHOULDER_LEFT, MirroredMovement ? 8 : 4);
        TransformBone(skeletonJoints, ImiSkeleton.Index.ELBOW_LEFT, MirroredMovement ? 9 : 5);
        TransformBone(skeletonJoints, ImiSkeleton.Index.WRIST_LEFT, MirroredMovement ? 10 : 6);
        TransformBone(skeletonJoints, ImiSkeleton.Index.HAND_LEFT, MirroredMovement ? 11 : 7);

        TransformBone(skeletonJoints, ImiSkeleton.Index.SHOULDER_RIGHT, MirroredMovement ? 4 : 8);
        TransformBone(skeletonJoints, ImiSkeleton.Index.ELBOW_RIGHT, MirroredMovement ? 5 : 9);
        TransformBone(skeletonJoints, ImiSkeleton.Index.WRIST_RIGHT, MirroredMovement ? 6 : 10);
        TransformBone(skeletonJoints, ImiSkeleton.Index.HAND_RIGHT, MirroredMovement ? 7 : 11);

        if (!IsNearMode)
        {
            TransformBone(skeletonJoints, ImiSkeleton.Index.HIP_LEFT, MirroredMovement ? 16 : 12);
            TransformBone(skeletonJoints, ImiSkeleton.Index.KNEE_LEFT, MirroredMovement ? 17 : 13);
            TransformBone(skeletonJoints, ImiSkeleton.Index.ANKLE_LEFT, MirroredMovement ? 18 : 14);
            TransformBone(skeletonJoints, ImiSkeleton.Index.FOOT_LEFT, MirroredMovement ? 19 : 15);

            TransformBone(skeletonJoints, ImiSkeleton.Index.HIP_RIGHT, MirroredMovement ? 12 : 16);
            TransformBone(skeletonJoints, ImiSkeleton.Index.KNEE_RIGHT, MirroredMovement ? 13 : 17);
            TransformBone(skeletonJoints, ImiSkeleton.Index.ANKLE_RIGHT, MirroredMovement ? 14 : 18);
            TransformBone(skeletonJoints, ImiSkeleton.Index.FOOT_RIGHT, MirroredMovement ? 15 : 19);
        }
        else
        { // add by sunsai
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.HIP_LEFT, MirroredMovement ? 16 : 12);
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.KNEE_LEFT, MirroredMovement ? 17 : 13);
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.ANKLE_LEFT, MirroredMovement ? 18 : 14);
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.FOOT_LEFT, MirroredMovement ? 19 : 15);

            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.HIP_RIGHT, MirroredMovement ? 12 : 16);
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.KNEE_RIGHT, MirroredMovement ? 13 : 17);
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.ANKLE_RIGHT, MirroredMovement ? 14 : 18);
            RotateSpecialJointToInitialPosition(ImiSkeleton.Index.FOOT_RIGHT, MirroredMovement ? 15 : 19);
        }

        // add by sunsai
        ConstrainBothHands(skeletonJoints);
        
        // If the player is supposed to move in the space, move it.
		if (MovesInSpace) 
        {
			MovePlayer();
		}
    }

    // Apply the rotations tracked by the kinect to the joints.
    private void TransformBone(ImiSkeleton[] skeletons,ImiSkeleton.Index joint, int boneIndex)
    {
        if (boneIndex < 0) return;

        if (!bones[boneIndex]) return;

        if (!skeletons[(int)joint].isTracked) return;

        Transform boneTransform = bones[boneIndex];

        //Matrix4x4 jointOrientationMat;
        Quaternion jointRotation;

        //jointOrientationMat = skeletons[(int)joint].HierarchicalRotationMatrix;
        //jointRotation = Utils.GetQuaternion(jointOrientationMat);
        //jointRotation =  skeletons[(int)joint].orientation;
        jointRotation = skeletons[(int)joint].boneOrientation.hierarchicalRotation.rotationQuaternion;
        if (jointRotation == Quaternion.identity) return;

        // Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];

        //If an offset node is specified, combine the transform with its
        //orientation to essentially make the skeleton relative to the node
        if (offsetNode != null)
        {
            // Grab the total rotation by adding the Euler and offset's Euler.
            Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
            // Grab our new rotation.
            newRotation = Quaternion.Euler(totalRotation);
        }

        // Smoothly transition to our new rotation.
        // modified by sunsai
        if (SmoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, SmoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation;

        // Smoothly transition to our new rotation.
        //boneTransform.rotation = jointconstraint.updateJointOrientation(boneTransform.rotation, newRotation, SmoothFactor,ref  g_skeleton, joint);
    }

    // Moves the player in 3D space - pulls the tracked position of the spine and applies it to root.
    // Only pulls positional, not rotational.
    private void MovePlayer()
    {
        if (Root == null || Root.parent == null) return;

        // Get the position of the body and store it.
        Vector3 playerPosition = playerInfos[imiManager.IEGetMainUserId()].GetPlayerPosition();

        //playerPosition = BindMainPlayer ? imiManager.GetMainPlayerInfo().GetPlayerPosition() : imiManager.GetSubPlayerInfo().GetPlayerPosition();

        // If this is the first time we're moving the player, set the offset. Otherwise ignore it.
        if (!OffsetCalibrated)
        {
            OffsetCalibrated = true;

            XOffset = !MirroredMovement ? playerPosition.x * MoveRate : -playerPosition.x * MoveRate;
            YOffset = playerPosition.y * MoveRate;
            ZOffset = -playerPosition.z * MoveRate;
        }

        float xPos, yPos, zPos;

        // If movement is mirrored, reverse it.
        if (!MirroredMovement)
        {
            xPos = playerPosition.x * MoveRate - XOffset;
        }
        else
        {
            xPos = -playerPosition.x * MoveRate - XOffset;
        }

        yPos = playerPosition.y * MoveRate - YOffset;
        zPos = -playerPosition.z * MoveRate - ZOffset;

        // If we are tracking vertical movement, update the y. Otherwise leave it alone.
        Vector3 targetPos = new Vector3(xPos, VerticalMovement ? yPos : 0f, zPos);
        Root.parent.localPosition = Vector3.Lerp(Root.parent.localPosition, targetPos, 3 * Time.deltaTime);
    }


    // Set bones to initial position.
    private void RotateToInitialPosition()
    {
        // For each bone that was defined, reset to initial position.
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].rotation = initialRotations[i];
            }
        }
    }

    // Calibration pose is simply initial position with hands raised up. Rotation must be 0,0,0 to calibrate.
    private void RotateToCalibrationPose(bool needCalibration)
    {
        if (needCalibration)
        {
            // Reset the rest of the model to the original position.
            RotateToInitialPosition();

            //// Right Elbow
            //if (RightElbow != null)
            //    RightElbow.rotation = Quaternion.Euler(0, -90, 90) *
            //           initialRotations[(int)ImiSkeleton.Index.ELBOW_RIGHT];


            //// Left Elbow
            //if (LeftElbow != null)
            //    LeftElbow.rotation = Quaternion.Euler(0, 90, -90) *
            //        initialRotations[(int)ImiSkeleton.Index.ELBOW_LEFT];
        }
    }



	// Set bones to initial position.
    private void RotateToInitialPosition1() 
    {
        if (bones == null)
            return;

        if (offsetNode != null)
        {
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // For each bone that was defined, reset to initial position.


        //initialRotations[0].w = (float)0.7071067;
        //initialRotations[0].x = 0;
        //initialRotations[0].y= -(float)0.7071067;
        //initialRotations[0].z = 0;
        //

       
        for (int i = 0; i < bones.Length; i++) 
        {
			if (bones[i] != null) 
            {
                bones[i].rotation = initialRotations[i];
			}
		}

        //bones[0] = HipCenter1;
        //bones[0].rotation = initialRotations[1];


        if (Root != null)
        {
            Root.localPosition = Vector3.zero;
            Root.localRotation = Quaternion.identity;
        }

        // Restore the offset's position and rotation
        if (offsetNode != null)
        {
            offsetNode.transform.position = originalPosition;
            offsetNode.transform.rotation = originalRotation;
        }
        else
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
       // bones[0] = null;
       // bones[0].rotation = Quaternion.identity;
    }

    // Set special bones to initial position. add by sunsai
    private void RotateSpecialJointToInitialPosition(ImiSkeleton.Index joint, int boneIndex)
    {
        if (boneIndex < 0) return;

        if (!bones[boneIndex]) return;

        Transform boneTransform = bones[boneIndex];

        Quaternion newRotation = initialRotations[boneIndex] * Quaternion.Euler(0, 180, 0);

        //boneTransform.rotation = initialRotations[boneIndex] * Quaternion.Euler(0, -90, 90);
        boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, SmoothFactor * Time.deltaTime);
    }
	
	// Calibration pose is simply initial position with hands raised up. Rotation must be 0,0,0 to calibrate.
    private void RotateToCalibrationPose1(bool needCalibration) 
    {
		if(needCalibration) 
        {
            // Reset the rest of the model to the original position.
            RotateToInitialPosition();

			// Right Elbow
			if(RightElbow != null)
                RightElbow.rotation = Quaternion.Euler(0, -90, 90) *
                       initialRotations[(int)ImiSkeleton.Index.ELBOW_RIGHT];

            // Left Elbow
            if (LeftElbow != null)
                LeftElbow.rotation = Quaternion.Euler(0, 90, -90) *
                    initialRotations[(int)ImiSkeleton.Index.ELBOW_LEFT];
		}
    }
	
	// If the bones to be mapped have been declared, map that bone to the model.
	private void MapBones() 
    {
        //if (HipCenter != null)
            bones[0] = HipCenter;
        //if (Spine != null)
            bones[1] = Spine;
        //if (Neck != null)
            bones[2] = Neck;
        //if (Head != null)
			bones[3] = Head;
		
		//if(LeftShoulder != null)
			bones[4] = LeftShoulder;
		//if(LeftElbow != null)
			bones[5] = LeftElbow;
		//if(LeftWrist != null)
			bones[6] = LeftWrist;
		//if(LeftHand != null)
			bones[7] = LeftHand;

		//if(RightShoulder != null)
			bones[8] = RightShoulder;
		//if(RightElbow != null)
			bones[9] = RightElbow;
		//if(RightWrist != null)
			bones[10] = RightWrist;
		//if(RightHand != null)
			bones[11] = RightHand;

		//if(LeftHip != null)
			bones[12] = LeftHip;
		//if(LeftKnee != null)
			bones[13] = LeftKnee;
		//if(LeftAnkle != null)
			bones[14] = LeftAnkle;
		//if(LeftFoot != null)
			bones[15] = LeftFoot;
		
		//if(RightHip != null)
			bones[16] = RightHip;
		//if(RightKnee != null)
			bones[17] = RightKnee;
		//if(RightAnkle != null)
			bones[18] = RightAnkle;
		//if(RightFoot!= null)
			bones[19] = RightFoot;
	}
	
	// Capture the initial rotations of the model.
	private void GetInitialRotations() 
    {
		if(offsetNode != null) {
			// Store the original offset's position and rotation.
            originalPosition = offsetNode.transform.position;
			originalRotation = offsetNode.transform.rotation;
			
            // Set the offset's rotation to 0.
			//offsetNode.transform.rotation = Quaternion.Euler(Vector3.zero);
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            transform.rotation = Quaternion.identity;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation;
                initialLocalRotations[i] = bones[i].localRotation;
            }
        }

        if (offsetNode != null) {
			// Restore the offset's rotation
			offsetNode.transform.rotation = originalRotation;
		}
        else
        {
            transform.rotation = originalRotation;
        }
	}


    // constrain hands
    private void ConstrainBothHands(ImiSkeleton[] skeletonJoints)
    {
        int leftHandIndex, rightHandIndex;
        int leftElbowIndex, rightElbowIndex;

        leftHandIndex = MirroredMovement ? 11 : 7;
        rightHandIndex = MirroredMovement ? 7 : 11;
        leftElbowIndex = MirroredMovement ? 9 : 5;
        rightElbowIndex = MirroredMovement ? 5 : 9;

        Transform leftHandTrans, rightHandTrans;
        Transform leftElbowTrans, rightElbowTrans;

        leftHandTrans = bones[leftHandIndex];
        rightHandTrans = bones[rightHandIndex];
        leftElbowTrans = bones[leftElbowIndex];
        rightElbowTrans = bones[rightElbowIndex];

        if (!leftHandTrans && !rightHandTrans) return;

        Quaternion newLeftHandRotation, newRightHandRotation;

        // left hand constraint
        if (skeletonJoints[(int)ImiSkeleton.Index.HAND_LEFT].isTracked )
        {
            if( skeletonJoints[(int)ImiSkeleton.Index.HAND_LEFT].position.y < skeletonJoints[(int)ImiSkeleton.Index.ELBOW_LEFT].position.y
            && skeletonJoints[(int)ImiSkeleton.Index.HAND_LEFT].position.z < skeletonJoints[(int)ImiSkeleton.Index.ELBOW_LEFT].position.z)
            {
                newLeftHandRotation = leftHandTrans.localRotation * Quaternion.Euler(0, 150, 0);
                leftHandTrans.localRotation = Quaternion.Slerp(leftHandTrans.localRotation, newLeftHandRotation, SmoothFactor * Time.deltaTime);
            }
        }

        // right hand constraint
        if (skeletonJoints[(int)ImiSkeleton.Index.HAND_RIGHT].isTracked 
            && skeletonJoints[(int)ImiSkeleton.Index.HAND_RIGHT].position.y < skeletonJoints[(int)ImiSkeleton.Index.ELBOW_RIGHT].position.y
            && skeletonJoints[(int)ImiSkeleton.Index.HAND_RIGHT].position.z < skeletonJoints[(int)ImiSkeleton.Index.ELBOW_RIGHT].position.z)
        {
            newRightHandRotation = rightHandTrans.localRotation * Quaternion.Euler(0, 150, 0);
            rightHandTrans.localRotation = Quaternion.Slerp(rightHandTrans.localRotation, newRightHandRotation, SmoothFactor * Time.deltaTime);
        }
    }

   
}