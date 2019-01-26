using K_PathFinder.Graphs;
using System.Collections;
using System.Collections.Generic;
using Test.Patrol;
using UnityEngine;

[RequireComponent(typeof(K_PathFinder.PathFinderAgent), typeof(CharacterController))]

public class PathMovement : MonoBehaviour
{

    public SimplePatrolPath patrol;
    [Range(0f, 5f)]
    public float speed = 3;
    public float gravity = 20.0f;
    public float rotationSpeed = 240.0f;

    private K_PathFinder.PathFinderAgent agent;
    private CharacterController controler;
    private int currentPoint;
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        if (patrol == null || patrol.Count == 0)
            Debug.LogError("Not valid patrol path");

        controler = GetComponent<CharacterController>();
        agent = GetComponent<K_PathFinder.PathFinderAgent>();
        _animator = GetComponent<Animator>();

        //find nearest point
        float sqrDist = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < patrol.Count; i++)
        {
            float curSqrDist = (patrol[i] - pos).sqrMagnitude;
            if (curSqrDist < sqrDist)
            {
                sqrDist = curSqrDist;
                currentPoint = i;
            }
        }

        agent.SetRecievePathDelegate((K_PathFinder.Path path) => { Debug.Log(path.pathType); });

        //queue navmesh
        K_PathFinder.PathFinder.QueueGraph(new Bounds(transform.position, Vector3.one * 20), agent.properties);
    }

    // Update is called once per frame
    void Update()
    {
        //if patrol not valid return
        if (patrol.Count == 0)
        {
            Debug.Log("patrol.Count == 0");
            return;
        }

        _animator.SetBool("run", false);

        //if we have points left in path
        if (agent.haveNextNode)
        {
            //remove point if it is closer than agent radius. return true if removed. there is other versions of that function
            if (agent.RemoveNextNodeIfCloserThanRadiusVector2())
            {
                //before that there was point. if after it removed here no point mean we reach end of current path
                //if no points left then path no longer valid and agent get another path
                if (agent.haveNextNode == false)
                {
                    currentPoint++;//move to next point on patrol   
                    if (currentPoint >= patrol.Count)
                        currentPoint = 0;

                    agent.SetGoalMoveHere(patrol[currentPoint]); //queue new path
                }
            }

            //if next point still exist then we move towards it
            if (agent.haveNextNode)
            {
                Vector2 moveDirection = agent.nextNodeDirectionVector2.normalized;
                Vector3 moveDirection3 = Vector3.zero;

                Vector2 moveTurn = transform.InverseTransformDirection(moveDirection);

                // Get Euler angles
                float turnAmount = Mathf.Atan2(moveTurn.x, moveTurn.y);

                transform.Rotate(0, turnAmount * rotationSpeed * Time.deltaTime, 0);

                if (controler.isGrounded)
                {
                    moveDirection3 = new Vector3(moveDirection.x, 0, moveDirection.y);
                    moveDirection3 *= speed;
                    _animator.SetBool("run", true);

                }

                moveDirection3.y -= gravity * Time.deltaTime;
                controler.Move(moveDirection3 * Time.deltaTime);
            }
        }
        else
        {
            //get path to current point
            agent.SetGoalMoveHere(patrol[currentPoint]);
        }

    }
}
