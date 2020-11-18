using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//01 Add Unity.MLAgents namespace
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

//02 Inherit from the Agent class
public class CubeAgent : Agent
{
    //03 Create regions
    #region Private fields
    //04 Create the variables of the program
    
    //04.1 Variables to store the boundaries and target game objects
    GameObject _boundaries;
    GameObject _target;

    //04.2 Variable to store the rigidbody of the agent
    Rigidbody _rigidbody;

    //04.3 Variable to store the multiplier that will drive the movement of the agent
    float _moveMultiplier = 10f;

    //04.4 Variables to store the materials that will be applied to the environment
    Material _successMat;
    Material _failMat;
    Material _regularMat;

    #endregion

    #region Unity standard methods

    private void Start()
    {
        //05 Get the boundaries and target elements
        _boundaries = transform.parent.Find("Boundaries").gameObject;
        _target = transform.parent.Find("Target").gameObject;

        //06 Get the rigidbody associated with the agent
        _rigidbody = transform.GetComponent<Rigidbody>();

        //07 Get the Materials to change the environment with
        _regularMat = Resources.Load<Material>("Materials/EnvironmentMaterial");
        _successMat = Resources.Load<Material>("Materials/SuccessMaterial");
        _failMat = Resources.Load<Material>("Materials/FailMaterial");
    }

    #endregion

    #region MLAgents Methods

    //08 Create the MLAgents Methods that will be utilised in the program

    //08.1 Override OnEpisodeBegin
    public override void OnEpisodeBegin()
    {
        //09 Reset the movement momentum of the agent in the begining of each episode
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.velocity = Vector3.zero;

        //10 Reset the position of the agent in the begining of each episode, in the middle of the environment
        transform.localPosition = new Vector3(0, 0.5f, 0);
        transform.localRotation = Quaternion.identity;

        //11 Randomize the position of the target in the begining of each episode
        float x = (Random.value * 28) - 14f;
        float y = (Random.value * 28) - 14f;
        _target.transform.localPosition = new Vector3(x, 0.5f, y);
    }

    //08.2 Override CollectObservations
    public override void CollectObservations(VectorSensor sensor)
    {
        //13 Add the current position of the target as an observation [3 Observations]
        sensor.AddObservation(_target.transform.localPosition);

        //12 Add the current local position of the agent as an observation [3 Observations]
        sensor.AddObservation(transform.localPosition);

        

        //14 Add the current velocity of the agent as an observation [2 Observations]
        sensor.AddObservation(_rigidbody.velocity.x);
        sensor.AddObservation(_rigidbody.velocity.z);
    }

    //08.3 Override OnActionReceived
    public override void OnActionReceived(float[] vectorAction)
    {
        //ACTIONS [2 Actions]
        
        //15 Create a vector to control the movement of the agent
        Vector3 movementSignal = Vector3.zero;
        
        //16 Read the signal from the action vector
        movementSignal.x = vectorAction[0];
        movementSignal.z = vectorAction[1];

        //17 Apply the vector as a force to the agent
        _rigidbody.AddForce(movementSignal * _moveMultiplier);

        //REWARDS
        //18 Check if distance to target is less than 1.25
        if (Vector3.Distance(transform.localPosition, _target.transform.localPosition) <= 1.25f)
        {
            //19 Assign maximum reward to agent
            SetReward(1.0f);

            //28 Add the material animation after the win reward
            StartCoroutine(ChangeEnvironmentMaterial(_successMat));
            
            //20 End the episode
            EndEpisode();
        }
    }

    //08.4 Override Heuristic
    public override void Heuristic(float[] actionsOut)
    {
        //29 Set the movement on the X axis from Horizontal
        actionsOut[0] = Input.GetAxis("Horizontal");
        
        //30 Set the movement on the Z axis from Vertical
        actionsOut[1] = Input.GetAxis("Vertical");

    }

    #endregion

    #region Regular private methods

    //31 Create the OnCollisionEnter method
    private void OnCollisionEnter(Collision collision)
    {
        //32 Check if the collided element is a Wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            //33 Apply penalty to the agent
            SetReward(-0.5f);

            //34 Initiate the material animation with the fail material
            StartCoroutine(ChangeEnvironmentMaterial(_failMat));
            
            //35 End the episode
            EndEpisode();
        }
    }

    //21 Create the Ienumerator to change the environment material
/// <summary>
/// Temporarily sets the material of the enviroment according to the success of the episode
/// </summary>
/// <param name="material">The material to be set</param>
/// <returns></returns>
    private IEnumerator ChangeEnvironmentMaterial(Material material)
    {
        //22 Get the boundaries parent game object
        var boundaries = transform.parent.Find("Boundaries");
        
        //23 Iterate through each of the boundaries elements
        for (int i = 0; i < boundaries.childCount; i++)
        {
            //24 Get the boundary element and assign the material
            var bound = boundaries.GetChild(i);
            bound.GetComponent<MeshRenderer>().material = material;
        }

        //25 Wait for half a second
        yield return new WaitForSeconds(0.5f);

        //26 Iterate again through the boudary elements
        for (int i = 0; i < boundaries.childCount; i++)
        {
            //27 Set the material back to the regular one
            var bound = boundaries.GetChild(i);
            bound.GetComponent<MeshRenderer>().material = _regularMat;
        }
    }

    #endregion
}
