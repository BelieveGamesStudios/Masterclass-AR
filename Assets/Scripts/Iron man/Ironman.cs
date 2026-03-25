using UnityEngine;
using UnityEngine.InputSystem;

namespace Imisi3D.Sample.IronMan
{
    [RequireComponent(typeof(Rigidbody))]
    public class Ironman : MonoBehaviour
    {
        private Rigidbody bodyRb;


        [Header("Inputs")]
        [SerializeField] private InputActionProperty moveInput;
        [SerializeField] private InputActionProperty liftInput;

        [Header("Flight variables")]
        [SerializeField] private float liftForce = 400;
        [SerializeField] private float moveSpeed = 10;
        private float angularVelocity;
        private float targetRotation;


        [Header("Animations")]
        private Animator[] characterAnimators;
        private int motionHash = Animator.StringToHash("Motion");

        [SerializeField] private GameObject[] models;
        private int modelIndex = 0;
        void Start()
        {
            bodyRb = GetComponent<Rigidbody>();
            bodyRb.useGravity = false;
            moveInput.action.actionMap.Enable();
            characterAnimators = GetComponentsInChildren<Animator>();
            SwitchModel();
        }
        private void OnDestroy()
        {
            moveInput.action.actionMap.Disable();
        }

        private void FixedUpdate()
        {
            Fly();
        }
        void Fly()
        {
            Vector2 _input = moveInput.action.ReadValue<Vector2>();
            float liftValue = liftInput.action.ReadValue<float>();

            Vector3 moveDir = new Vector3(_input.x, 0, _input.y).normalized;
            if (moveDir.magnitude >= 0.1f)
            {
                targetRotation = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                float smoothTurn = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref angularVelocity, 4 * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, smoothTurn, 0);
            }
            Vector3 moveVector = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

            bodyRb.linearVelocity = _input.magnitude > 0.1f || Mathf.Abs(liftValue) >= 0.1f ? new Vector3(moveVector.x * _input.magnitude, liftValue, moveVector.z * _input.magnitude) * moveSpeed * Time.deltaTime : Vector3.Lerp(bodyRb.linearVelocity, Vector3.zero, 7 * Time.deltaTime);


            float motionRatio = moveDir.magnitude >= 0.1f ? 1 : 0;
            foreach (var animators in characterAnimators)
            {
                animators.SetFloat(motionHash, motionRatio, 0.2f, Time.deltaTime);
            }
        }
        public void SwitchModel()
        {
            modelIndex = (modelIndex + 1) % models.Length;
            for (int i = 0; i < models.Length; i++)
            {
                models[i].SetActive(false);
            }
            models[modelIndex].SetActive(true);
        }
    }
}
