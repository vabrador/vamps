using UnityEngine;
using System.Collections;
using UnityEngineInternal;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Muscle : MonoBehaviour {
    
    public Tendon proximalTendon;
    public Tendon distalTendon;

    public float springK_NoFibersContracted = 1;
    public float springK_AllFibersContracted = 80;
	public int numFibers = 20;

	[SerializeField]
    private float numFibersContracted = 0;
	public float NumFibersContracted {
		get {
			return numFibersContracted;
		}
		set {
			numFibersContracted = Mathf.Min (Mathf.Max (0, value), numFibers);
		}
	}
    
    public SpringJoint spring;
    private float defaultTendonDistance = 0f;
    
	[Header("Sense Memory Out (configured automatically)")]
	public SenseMemory contractionSense;
    public static string ContractionSenseSuffix { get { return " Contraction Sense"; } }
    public string ContractionSenseName { get { return gameObject.name + ContractionSenseSuffix; } }
	public SenseMemory lengthSense;
    public static string LengthSenseSuffix { get { return " Length Sense"; } }
	public string LengthSenseName { get { return gameObject.name + LengthSenseSuffix; } }
    private float initialTendonDistance = 0f;

    public string[] GetSenseNames() {
        string[] senseNames = new string[2];
        senseNames[0] = ContractionSenseName;
        senseNames[1] = LengthSenseName;
        return senseNames;
    }

    private Creature creature;

    // Awake() initialization is called before Start()
    public void Awake() {
        creature = GetComponentInParent<Creature>();
        if (!creature) {
            Debug.LogError("[Muscle] Not within a creature! Won't be able to register sensory data.");
        }

        // init tendons
        // Initialize the spring closer to the distal tendon so that pulling can be achieved
        // simply by modifying the spring constant.
        Vector3 initialPosition = proximalTendon.transform.position;
        proximalTendon.transform.position += ((distalTendon.transform.position - proximalTendon.transform.position) * (7/8F));
        // Initialize spring joint
        spring = proximalTendon.rigidbody.gameObject.AddComponent<SpringJoint>();
        spring.connectedBody = distalTendon.rigidbody;
        spring.autoConfigureConnectedAnchor = false;
        spring.anchor = proximalTendon.transform.localPosition;
        spring.connectedAnchor = distalTendon.transform.localPosition;
        defaultTendonDistance = (proximalTendon.transform.position - distalTendon.transform.position).magnitude;
        spring.spring = 0f; // To be set in update
        spring.maxDistance = 0;
        spring.minDistance = 0;
        spring.damper = 1f;
        // Put the Transform of the proximal tendon back where it was.
        proximalTendon.transform.position = initialPosition;
        spring.anchor = proximalTendon.transform.localPosition;

        // Used for length sensing
        initialTendonDistance = (distalTendon.transform.position - proximalTendon.transform.position).magnitude;

        // init senses
        contractionSense = creature.RegisterSense(ContractionSenseName, null);

        float minLength = initialTendonDistance * 0.4F;
        float maxLength = initialTendonDistance * 2F;
        SenseCoefficients lengthSenseCs = new SenseCoefficients()
            {
                minReal = minLength,
                maxReal = maxLength,
                senseLength = numFibers + 1,
                exponent = 1.618F
            };
        lengthSense = creature.RegisterSense(LengthSenseName, lengthSenseCs);
        creature.RegisterMuscle(this);
	}

	// Upon beginning the simulation
    void Start() {
        if (!proximalTendon || !distalTendon) {
            Debug.LogWarning("[Muscle] Muscle lacks proximal or distal tendon; it won't be able to apply forces.");
        }
    }

    public void SetNumFibersToContract(float numFibersToContract) {
        if (numFibersToContract < 0 || numFibersToContract > numFibers) {
            Debug.LogWarning("[Muscle] Got an OoB argument setting num fibers: " + numFibersToContract);
            numFibersContracted = Mathf.Max(Mathf.Min(numFibers, numFibersToContract), 0);
        }
        numFibersContracted = numFibersToContract;
    }
    public void Relax() {
        numFibersContracted = 0;
    }
    public void Relax(float nFibers) {
        numFibersContracted = Mathf.Max(0, numFibersContracted - nFibers);
    }
    public void Tighten(float nFibers) {
        numFibersContracted = Mathf.Min(numFibers, numFibersContracted + nFibers);
    }
    public float GetRandomContraction() {
        return (Random.value * numFibers);
    }
    public void SetRandomContraction() {
        numFibersContracted = (Random.value * numFibers);
    }
    public void DriftContraction() {
        DriftContraction(numFibers / 10);
    }
    public void DriftContraction(float maxDelta) {
        NumFibersContracted = numFibersContracted + ((Random.value * 2F - 1F) * maxDelta);
    }
    public void SetFibersContracted(float newContractionValue, bool normalized=false) {
        if (normalized) {
            numFibersContracted = (numFibers * newContractionValue);
        }
        else {
            numFibersContracted = newContractionValue;
        }
    }

    [Header("UI Control - not automatic")]
    public Slider linkedSliderControl = null;
    private bool controlledByUser = false;
    void FixedUpdate() {

		if (!proximalTendon && !distalTendon) {
			Debug.LogError ("No tendon attached to muscle: " + gameObject.name);
			return;
		}

        // Muscle's spring gets exponentially stronger the further extended it is from its springK-defining length.
        float tendonDistance = (proximalTendon.transform.position - distalTendon.transform.position).magnitude;
        float distanceDelta = Mathf.Abs(defaultTendonDistance - tendonDistance);

        spring.spring = SenseUtil.WorldValueFromSenseValue(
                numFibersContracted,
                springK_NoFibersContracted,
                springK_AllFibersContracted,
                numFibers,
                1.618F)
            * distanceDelta
            * (proximalTendon.rigidbody.mass + distalTendon.rigidbody.mass) / 2f;

        // Sense Memory
        if (contractionSense) {
            if (contractionSense.Length != numFibers + 1) {
                contractionSense.Length = numFibers + 1;
            }
            contractionSense.Value = numFibersContracted;
        }
        if (lengthSense) {
			if (lengthSense.Length != numFibers + 1) {
				lengthSense.Length = numFibers + 1;
			}
			float worldLength = (distalTendon.transform.position - proximalTendon.transform.position).magnitude;
            lengthSense.Value = worldLength;
        }

		UpdatePosition ();

        if (controlledByUser) {
            if (linkedSliderControl) {
                SetFibersContracted(linkedSliderControl.value, true);
            }
        }
        else {
            if (linkedSliderControl) {
                linkedSliderControl.value = Normalize(numFibersContracted);
            }
        }
    }

    public float Normalize(float numFibersToNormalizedFloat) {
        return numFibersToNormalizedFloat / (float)numFibers;
    }

	public void UpdatePosition() {
		// The position of the muscle doesn't really matter,
		// but it makes more visual sense for the muscle to rest between its tendons.
		transform.position = (proximalTendon.transform.position + distalTendon.transform.position) / 2F;
	}


    public void SliderControl_BeginDrag() {
        controlledByUser = true;
    }
    public void SliderControl_EndDrag() {
        controlledByUser = false;
    }

}
