using UnityEngine;
using System.Collections;

public class BoxMover : MonoBehaviour, IInteract
{
    [Header("Transforms")]
    [SerializeField] private Transform magnet;
    [SerializeField] private Transform piston;
    [SerializeField] private Transform ground;
    [SerializeField] private Transform endPoint;

    [Header("Button")]
    [SerializeField] private Light onHoverLight;
    [SerializeField] Renderer buttonRender;

    [Header("Layers")]
    [SerializeField] private LayerMask grabLayer;
    [SerializeField] private LayerMask groundLayer;

    public float grabActionSpeed = 2f;
    public float moveActionSpeed = 5f;
    private int interactCounter = 0;

    private bool hasBox;
    private bool isMoving;
    private bool isGrabbing;
    private Vector3 magnetStartingPosition;
    private Vector3 posMagnetGrabbed;

    void Start()
    {
        onHoverLight.enabled = false;
        isGrabbing = false;
        isMoving = false;
        hasBox = false;
        magnetStartingPosition = magnet.position;
    }


    public void Interact()
    {
        interactCounter++;

        if (!hasBox && !isMoving)
        {
            StartCoroutine(PickupBox());
        }
        else if (!isGrabbing && interactCounter <= 2)
        {
            StartCoroutine(MoveOnRail());
        }
        else if (hasBox && !isMoving)
        {
            StartCoroutine(PutThatBoxDown());
            interactCounter = 0;
        }
        
    }

    private IEnumerator PickupBox()
    {
        isGrabbing = true;
        onHoverLight.color = Color.orange;
        buttonRender.material.color = Color.orange;

        // this is the cube that connects to the rails
        Transform structure = magnet.GetChild(1);

        structure.SetParent(null);

        RaycastHit hit;
        if (Physics.Raycast(magnet.position, Vector3.down, out hit, 20, grabLayer))
        {

            // lower the magnet until the magnets lower face "touches" the objects upper face
            while (magnet.position.y > hit.point.y + (magnet.localScale.y / 2f))
            {
                magnet.Translate(Vector3.down * grabActionSpeed * Time.deltaTime);
                yield return null;
            }

            // parented to the piston so the structures position in the hierarchy stays the same
            hit.transform.SetParent(piston, true);
            hasBox = true;

            yield return new WaitForSeconds(0.5f);

        }

        posMagnetGrabbed = magnet.position;

        while (magnet.position.y < magnetStartingPosition.y)
        {
            magnet.Translate(Vector3.up * grabActionSpeed * Time.deltaTime);
            yield return null;
        }

        structure.SetParent(magnet, true);

        isGrabbing = false;
        onHoverLight.color = Color.green;
        buttonRender.material.color = Color.green;
    }

    private IEnumerator PutThatBoxDown()
    {
        isGrabbing = true;
        onHoverLight.color = Color.orange;
        buttonRender.material.color = Color.orange;

        // this is the cube that connects to the rails
        Transform structure = magnet.GetChild(1);
        structure.SetParent(null);

        Transform carriedProp = piston.GetChild(0);

        while (magnet.position.y > posMagnetGrabbed.y)
        {
            magnet.Translate(Vector3.down * grabActionSpeed * Time.deltaTime);
            yield return null;
        }

        carriedProp.SetParent(null, true);
        hasBox = false;


        while (magnet.position.y < magnetStartingPosition.y)
        {
            magnet.Translate(Vector3.up * grabActionSpeed * Time.deltaTime);
            yield return null;
        }

        structure.SetParent(magnet, true);

        isGrabbing = false;
        onHoverLight.color = Color.green;
        buttonRender.material.color = Color.green;
    }

    private IEnumerator MoveOnRail()
    {
        isMoving = true;

        if (magnet.position.x <= magnetStartingPosition.x)
        {
            while (magnet.position.x < endPoint.position.x && !isGrabbing)
            {
                magnet.Translate(Vector3.right * moveActionSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            while (magnet.position.x > magnetStartingPosition.x)
            {
                magnet.Translate(Vector3.left * moveActionSpeed * Time.deltaTime);
                yield return null;

            }
        }

        isMoving = false;
    }

    public void OnHoverEnter()
    {
        onHoverLight.enabled = true;
        if (!isGrabbing)
        {
            onHoverLight.color = Color.green;
        }
    }

    public void OnHoverExit()
    {
        if (!isGrabbing && !isMoving)
        {
            onHoverLight.enabled = false;
        }
    }
}
