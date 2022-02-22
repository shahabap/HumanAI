using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CivillianAI : MonoBehaviour {
	//THE CHARACTER MODEL THAT YOU ARE USING
	public Transform CharacterToAnimate;
	//THE HEAD CONTROLLER PREFAB
	public Transform HeadController;
	//THE PLAYER
	public Transform Player;
	//PLAYERS HEAD SO THEY LOOK AT HIS HEAD RATHER THAN THIS FEET
	public Transform PlayerHead;
	//THE CHANCE OF LOOKING... (EXAMPLE:  IF ITS SET TO 3, ITS A 1 IN 3 CHANCE HE WILL LOOK AT SOMEONE HE RUNS INTO)
	public int ChanceOfLooking=2;
	//THE FRONT FACING POSITION WHEN THERE IS NOTHING TO LOOK AT
	public Transform FrontFace;
	//ANIMATIONS
	public AnimationClip Walk;
	public AnimationClip Idle;
	public AnimationClip IdleSit;
	public AnimationClip Throw;
	//PROCCEDS TO TARGETS AND EXPLORES RANDOMLY..NOTE: THERE MUST BE A STARTING TARGET
	public bool ProceedToTargets;
	//ENABLE THIS TO MAKE THE AI FOLLOW ANOTHER CHARACTER
	public bool FollowParent;
	//ENABLE THIS TO MAKE FOLLOW THE TOUR GUIDE
	public bool FollowTourGuide;
	//ENABLE THIS TO MAKE AI FOLLOW A SPECIFIED PATH
	public bool FollowSpecifiedPath;
	//THE GUIDE TO FOLLOW WHEN "FollowTourGuide" IS ENABLED
	public Transform GuideToFollow;
	//THE PARENT TO FOLLOW WHEN "FollowParent" IS ENABLED
	public Transform ParentToFollow;
	
	private int CurPath;
	private bool turnaround;
	public List<Transform> PathNodes;
	public float FollowDistance=3;
	
	private bool ObserveTarget;
	public Transform FirstTarget;
	private Transform CurrentTarget;
	private Transform CurrentChair;
	public Transform CurrentObservation;
	
	
	public bool Idlestate;
	public bool Walkstate;
	public bool SitState;
	private bool ThrowState;
	public float IdleAnimSpeed=10;
	public float IdleSitAnimSpeed=10;
	public float WalkAnimSpeed=1.4f;
	public float WalkSpeed=4;
	public float TurnSpeed=4;
	private int targcount;
	public float SitTime=5;
	public float ObserveTime=4;
	private float stimer;
	private float obtimer;
	private float throwtime;
	public List<Transform> PreviousTargets;
	// Use this for initialization
	void Start () {
	
		if(FollowTourGuide&GuideToFollow){
			TourGuide TG=(TourGuide)GuideToFollow.GetComponent("TourGuide");
			if(TG){
			PathNodes.Clear();
			for (int i = 0; i < TG.PathNodes.Count; i++){
				PathNodes.Add(TG.PathNodes[i]);
					
				}
				FollowSpecifiedPath=true;
			}
			
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
		
		
		
		if(FollowParent&ParentToFollow){
			
			float dist=Vector3.Distance(ParentToFollow.position, transform.position);
			
			if(dist<FollowDistance){
			Idlestate=true;
				Walkstate=false;
				CivillianAI head=(CivillianAI)ParentToFollow.GetComponent("CivillianAI");
				if(head){
					if(head.HeadController)CurrentObservation=head.HeadController;
				}
				else if(FollowTourGuide){}
		else CurrentObservation=ParentToFollow;
					
			}
			else{
			Idlestate=false;
		Walkstate=true;
		CivillianAI head=(CivillianAI)ParentToFollow.GetComponent("CivillianAI");
				if(head){
					if(head.HeadController)CurrentObservation=head.HeadController;
				}
				else {
					if(FollowTourGuide){}
		else CurrentObservation=ParentToFollow;
					}
				
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(ParentToFollow.position -  transform.position), TurnSpeed * Time.deltaTime);	
			}
		}
		
		
		
		
		//IDLE STATE
	if(Idlestate){
		
		CharacterToAnimate.GetComponent<Animation>()[Idle.name].speed = CharacterToAnimate.GetComponent<Animation>()[Idle.name].length / IdleAnimSpeed;
			CharacterToAnimate.GetComponent<Animation>().CrossFade(Idle.name, .2f);	
		}
		
		
		
		//THE SITTING STATE
		
		if(SitState){
			if(CurrentTarget){
				//START SIT TIMER
				stimer+=Time.deltaTime;
			}
			
			//IF SIT TIMER IS GREATER THAN SIT TIME END
			if(stimer>SitTime){
				if(CurrentChair){
				TargetPoint Tc=(TargetPoint)CurrentChair.GetComponent("TargetPoint");
					Tc.chairtaken=false;
				}
				
				SitState=false;
				
				stimer=0;
			}
			
			//ROTATE OPOSITE DIRECTION OF CHAIR TO SIT
		if(CurrentChair){
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - CurrentChair.position), TurnSpeed * Time.deltaTime);		
			}
			
		CharacterToAnimate.GetComponent<Animation>()[IdleSit.name].speed = CharacterToAnimate.GetComponent<Animation>()[IdleSit.name].length / IdleSitAnimSpeed;
			CharacterToAnimate.GetComponent<Animation>().CrossFade(IdleSit.name, .1f);	
		}
		
		
		
		
		//THE WALKING STATE
		if(Walkstate){
		CharacterToAnimate.GetComponent<Animation>().wrapMode=WrapMode.Loop;
		CharacterToAnimate.GetComponent<Animation>()[Walk.name].speed = CharacterToAnimate.GetComponent<Animation>()[Walk.name].length / WalkAnimSpeed;
			CharacterToAnimate.GetComponent<Animation>().CrossFade(Walk.name, .04f);	
		transform.position += transform.forward * WalkSpeed * Time.deltaTime;
		}
		
		
		//THROW IN POOL
		
		if(ThrowState){
		throwtime+=Time.deltaTime;
		if(throwtime<1){
			Idlestate=true;
				
			}
			else{
				Idlestate=false;
			CharacterToAnimate.GetComponent<Animation>()[Throw.name].speed = CharacterToAnimate.GetComponent<Animation>()[Throw.name].length / 1;
			CharacterToAnimate.GetComponent<Animation>().CrossFade(Throw.name, .1f);	
			
			if(throwtime>=1.9){
				ThrowState=false;
					ProceedToTargets=true;
					throwtime=0;
				}
				
			}
			
			
		}
		
		
		//TARGET TO OBSERVE(lOOK AT)
		if(ObserveTarget){
			Idlestate=true;
		
			if(CurrentTarget){
		obtimer+=Time.deltaTime;
			}
			
			
			
			
			if(obtimer>ObserveTime){
				ObserveTarget=false;
				Idlestate=false;
				
				obtimer=0;
			}
			
			//IF THERE IS A CURRENT OBSERVATION, TURN TOWARDS
			if(CurrentObservation){
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CurrentObservation.position - transform.position), TurnSpeed * Time.deltaTime);	
			}
		}
		
		//FOLLOW A SPECIFIED PATH OR TOUR GUIDE
		if(FollowSpecifiedPath){
			CurrentTarget=PathNodes[CurPath];
			
			//FOLLOW TOUR GUIDE
			if(FollowTourGuide&GuideToFollow){
				
				CivillianAI guide=(CivillianAI)GuideToFollow.GetComponent("CivillianAI");
				
				if(guide){
				if(guide.HeadController){
					CurrentObservation=guide.HeadController;	
					}
					else CurrentObservation=GuideToFollow;
				}
				
					float dist=Vector3.Distance(transform.position, GuideToFollow.position);
					if(dist>FollowDistance){
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CurrentTarget.position - transform.position), TurnSpeed * Time.deltaTime);
						Walkstate=true;
						Idlestate=false;
						}
						else{
						Walkstate=false;
						Idlestate=true;	
						}
						
					}
			else{
			if(SitState|Idlestate|ObserveTarget){}
			else{
			if(Walkstate){
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CurrentTarget.position - transform.position), TurnSpeed * Time.deltaTime);
				}
			
		
			Walkstate=true;
					
				}
			}
		}
		
		
		//PROCEEDING TO TARGETS
		if(ProceedToTargets&FirstTarget){
			
			
		if(CurrentTarget){}
			else{
				CurrentTarget=FirstTarget;
			}
			
			
			

				if(SitState|Idlestate|ObserveTarget){}
			else{
				
				//WALK IF IS NOT IDLING OR SITTING OR OBSERVING
				if(Walkstate){
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(CurrentTarget.position - transform.position), TurnSpeed * Time.deltaTime);
				}
			else{
					
			Walkstate=true;
				}
				}	
			
		}
		
		//THIS MAKES SURE THE CHARACTER STAYS UPRIGHT
		transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
		
		
		//MAKE THE CURRENT OBSERVATION OBJECT THE FRONT FACEING OBJECT ONCE OBSERVATION IS DONE
		if(CurrentObservation){
			
			
			if(CurrentTarget){
		TargetPoint TP=(TargetPoint)CurrentTarget.GetComponent("TargetPoint");
				if(TP){
					if(TP.painting){}
					else{
						
					//if(FrontFace) CurrentObservation=FrontFace;	
						
					}
				}
			}
			
		}
		else{
			if(FrontFace){ if(FollowTourGuide){}
					else CurrentObservation=FrontFace;
			}
		}
		
	}
	
	
	
	//WHEN A TRIGGER COLLISION HAPPENS WITH A TARGET
	void OnTriggerEnter(Collider other){
		
		//RANDOM CHANCE TO LOOK AT ANOTHER NEAR BY CHARACTER
		int RAN=(int)((Random.value)*ChanceOfLooking);
		
		if(RAN==0){
			if(FrontFace){
				
				if(other.transform==Player){
			if(PlayerHead){
			if(FollowTourGuide){}
					else CurrentObservation=PlayerHead;		
					}
					}
				else{
					
					
			if(FollowTourGuide){}
					else if(CurrentObservation==FrontFace){
			CivillianAI CHAR=(CivillianAI)other.GetComponent("CivillianAI");
			
					
					
			if(CHAR){
						
			if(CHAR.HeadController)CurrentObservation=CHAR.HeadController;
						else{
								if(FollowTourGuide){}
					else CurrentObservation=other.transform;
							}
				
			
				
				}
				}
				}
			}
		}
		
		
		//ASK IF ITS THE CURRENT TARGET
		if(other.transform==CurrentTarget){
  //GET COMPONENT OF TARGET	
	TargetPoint TP=(TargetPoint)other.GetComponent("TargetPoint");
			
			
		
			
			
		
			
			
		if(TP){
				
				
				//IF THE TARGET IS A CHAIR
			if(TP.chair){
				TP.chairtaken=true;
					SitState=true;
					Walkstate=false;
				CurrentChair=other.transform;		
					
				}
				
				//IF THE TARGET IS A PAINTING
				if(TP.painting|TP.Pool){
					Walkstate=false;
					ObserveTarget=true;
					if(FollowTourGuide){}
					else CurrentObservation=other.transform;
				}
				
				
				//THROW IN POOL STATE
				if(TP.Pool){
					ThrowState=true;
					Walkstate=false;
					ObserveTarget=other.transform;
					ObserveTarget=true;
				}
				
				
				if(FollowSpecifiedPath){
					if(CurPath==0)turnaround=false;
					
					if(CurPath>=PathNodes.Count-1){
						turnaround=true;
					}
					
					if(turnaround){
					CurPath=CurPath-1;	
					}
					else{
					CurPath=CurPath+1;	
					}
					
				}
				else{
				int CN=(int)((Random.value)*TP.ConnectingTargetPoints.Count);
				int lt=PreviousTargets.Count;
				
				
				if(TP.ConnectingTargetPoints.Count>1){
					for (int i = 0; i < lt; i++){
						
				
					
						
					
							
							
					if(TP.ConnectingTargetPoints[CN]==PreviousTargets[i]){
							
					if(CN>=TP.ConnectingTargetPoints.Count-1){
									
							CN=CN-1;
									
										//Debug.Log("Next target is same as last, changing..");
									
						}
						else{
							CN=CN+1;
									
										//Debug.Log("Next target is same as last, changing..");
						
						}
							}
							
							}
						
				}
							
					for (int i = 0; i < 10; i++){
			TargetPoint TP2=(TargetPoint)TP.ConnectingTargetPoints[CN].GetComponent("TargetPoint");
						
						if(TP2.chairtaken){
							if(TP2.chairtaken)Debug.Log("Chair Taken");
							CN=(int)((Random.value)*TP.ConnectingTargetPoints.Count);
							}
					
					
					
					}
					
			
			if(TP.ConnectingTargetPoints.Count>0){
				PreviousTargets[targcount]=CurrentTarget;
					
						targcount=targcount+1;
						if(targcount<PreviousTargets.Count){}
						else targcount=0;
					
					
				if(TP.ConnectingTargetPoints.Count<=1){
						
			
						
					CurrentTarget=TP.ConnectingTargetPoints[0];
						
					
					}
					else{

					
		CurrentTarget=TP.ConnectingTargetPoints[CN];
						
			TargetPoint TP3=(TargetPoint)CurrentTarget.GetComponent("TargetPoint");	
			if(TP3){
					if(TP3.chair)TP3.chairtaken=true;		
						}
						
			}
				}
				}
			}
		}
	}
}
