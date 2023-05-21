using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimcadeVehicleController : MonoBehaviour
{
        [Header("Suspension")]
        public float springForce = 30000f;
        public float springDamper = 200f;
        private float MaxSpringDistance;

        [Header("Car Stats")]
        public float MaxSpeed = 200f;
        public float Acceleration;
        public AnimationCurve AccelerationCurve;
        public float MaxTurnAngle = 30f;
        public AnimationCurve turnCurve;
        public float brakeAcceleration = 50f;
        public float RollingResistance = 2f;
        public float FrictionCoefficient = 1f;
        public AnimationCurve sideFrictionCurve;
        public AnimationCurve forwardFrictionCurve;
        public Transform CenterOfMass_air;
        private Vector3 CentreOfMass_ground;
        public bool AutoCounterSteer = false;
        public float DownForce = 5;


        [Header("Visuals")]
        public Transform VehicleBody;
        public GameObject WheelSkid;
        public GameObject SkidMarkController;
        public float wheelRadius;
        public float skidmarkWidth;
        public Transform[] HardPoints = new Transform[4];
        public Transform[] Wheels;

        [HideInInspector]
        public Vector3 carVelocity;

        [Header("Other Things")]


        private RaycastHit[] wheelHits = new RaycastHit[4];

        [HideInInspector]
        public float steerInput, accelerationInput, brakeInput, rearTrack, wheelBase, ackermennLeftAngle, ackermennRightAngle;

        private Rigidbody rb;
        [HideInInspector]
        public Vector3 localVehicleVelocity;
        private Vector3 lastVelocity;
        private int NumberOfGroundedWheels;
        [HideInInspector]
        public bool vehicleIsGrounded;

        private float[] wheelHitDistancePrev = new float[4];

        [HideInInspector]
        public bool CanAccelarate;

        private GearSystem GearSystem;


        //Skidmarks
        [HideInInspector]
        public float[] forwardSlip = new float[3], slipCoeff = new float[3], skidTotal = new float[3];
        private WheelSkid[] wheelSkids = new WheelSkid[3];


        void Awake()
        {
            //GameObject SkidMarkController_Self = Instantiate(SkidMarkController);
            //SkidMarkController_Self.GetComponent<Skidmarks>().SkidmarkWidth = skidmarkWidth;

            CanAccelarate = true;
            rb = GetComponent<Rigidbody>();
            lastVelocity = Vector3.zero;


            /*for (int i = 0; i < Wheels.Length; i++)
            {
                HardPoints[i].localPosition = new Vector3(Wheels[i].localPosition.x, 0, Wheels[i].localPosition.z);

                wheelSkids[i] = Instantiate(WheelSkid, Wheels[i].GetChild(0)).GetComponent<WheelSkid>();
                setWheelSkidvalues_Start(i, SkidMarkController_Self.GetComponent<Skidmarks>(), wheelRadius);
            }*/

            MaxSpringDistance = Mathf.Abs(Wheels[0].localPosition.y - HardPoints[0].localPosition.y) + 0.1f;

            wheelBase = Vector3.Distance(Wheels[0].position, Wheels[2].position);
            rearTrack = Vector3.Distance(Wheels[0].position, Wheels[1].position);

            GearSystem = GetComponent<GearSystem>();
        }

        private void Start()
        {
            CentreOfMass_ground = (HardPoints[0].localPosition + HardPoints[1].localPosition + HardPoints[2].localPosition + HardPoints[3].localPosition) / 4;

            rb.centerOfMass = CentreOfMass_ground;
        }

        private void Update()
        {
            if (CanAccelarate)
            {
                accelerationInput = Input.GetAxis("Vertical");
            }
            else
            {
                accelerationInput = 0;
            }
            steerInput = Input.GetAxis("Horizontal");
            brakeInput = Input.GetAxis("Jump");
        }

        void FixedUpdate()
        {
            localVehicleVelocity = transform.InverseTransformDirection(rb.velocity);

            AckermannSteering(steerInput);

            float suspensionForce = 0;
            for (int i = 0; i < Wheels.Length; i++)
            {
                bool wheelIsGrounded = false;

                AddSuspensionForce_2(HardPoints[i].position, Wheels[i], MaxSpringDistance, out wheelHits[i], out wheelIsGrounded, out suspensionForce, i);

                GroundedCheckPerWheel(wheelIsGrounded);

                tireVisual(wheelIsGrounded, Wheels[i], HardPoints[i], wheelHits[i].distance, i);
                setWheelSkidvalues_Update(i, skidTotal[i], wheelHits[i].point, wheelHits[i].normal);

            }
            vehicleIsGrounded = (NumberOfGroundedWheels > 1);
            if (vehicleIsGrounded)
            {
                AddAcceleration(accelerationInput);
                AddRollingResistance();
                brakeLogic(brakeInput);
                bodyAnimation();

                //AutoBalence
                if (rb.centerOfMass != CentreOfMass_ground)
                {
                    rb.centerOfMass = CentreOfMass_ground;
                }

                // angular drag
                rb.angularDrag = 1;

                //downforce
                rb.AddForce(-transform.up * DownForce * rb.mass);
            }
            else
            {
                if (rb.centerOfMass != CenterOfMass_air.localPosition)
                {
                    rb.centerOfMass = CenterOfMass_air.localPosition;
                }

                // angular drag
                rb.angularDrag = 0.1f;
            }

            //friction
            for (int i = 0; i < Wheels.Length; i++)
            {
                if (i < 2)
                {
                    AddLateralFriction_2(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 1, suspensionForce, i);
                }
                else
                {
                    if (brakeInput > 0.1f)
                    {
                        AddLateralFriction_2(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 0.2f, suspensionForce, i);
                    }
                    else
                    {
                        AddLateralFriction_2(HardPoints[i].position, Wheels[i], wheelHits[i], vehicleIsGrounded, 1, suspensionForce, i);
                    }

                }
            }


            NumberOfGroundedWheels = 0; //reset grounded int

        }

        void AddAcceleration(float accelerationInput)
        {
            float deltaSpeed = Acceleration * accelerationInput * Time.fixedDeltaTime;
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed) * AccelerationCurve.Evaluate(Mathf.Abs(localVehicleVelocity.z / MaxSpeed));

            if (accelerationInput > 0 && localVehicleVelocity.z < 0 || accelerationInput < 0 && localVehicleVelocity.z > 0)
            {
                deltaSpeed = (1 + Mathf.Abs(localVehicleVelocity.z / MaxSpeed)) * Acceleration * accelerationInput * Time.fixedDeltaTime;
            }
            if (brakeInput < 0.1f && localVehicleVelocity.z < MaxSpeed)
            {
                rb.velocity += transform.forward * deltaSpeed;
            }

        }

        void AddRollingResistance()
        {
            float localSpeed = Vector3.Dot(rb.velocity, transform.forward);

            float deltaSpeed = RollingResistance * Time.fixedDeltaTime * Mathf.Clamp01(Mathf.Abs(localSpeed));
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed);
            if(accelerationInput == 0)
            {
                if (localSpeed > 0)
                {
                    rb.velocity -= transform.forward * deltaSpeed;
                }
                else
                {
                    rb.velocity += transform.forward * deltaSpeed;
                }
            }
            
        }

        void brakeLogic(float brakeInput)
        {
            float localSpeed = Vector3.Dot(rb.velocity, transform.forward);

            float deltaSpeed = brakeAcceleration * brakeInput * Time.fixedDeltaTime * Mathf.Clamp01(Mathf.Abs(localSpeed));
            deltaSpeed = Mathf.Clamp(deltaSpeed, -MaxSpeed, MaxSpeed);
            if (localSpeed > 0)
            {
                rb.velocity -= transform.forward * deltaSpeed;
            }
            else
            {
                rb.velocity += transform.forward * deltaSpeed;
            }

        }

        void AddSuspensionForce(Vector3 hardPoint, Transform wheel, float MaxSpringDistance, out RaycastHit wheelHit, out bool WheelIsGrounded, out float SuspensionForce)
        {
            var direction = -transform.up;

            if (Physics.SphereCast(hardPoint, wheelRadius, direction, out wheelHit, MaxSpringDistance))
            {
                WheelIsGrounded = true;
            }
            else
            {
                WheelIsGrounded = false;
            }

            // suspension spring force
            if (WheelIsGrounded)
            {
                Vector3 springDir = transform.up;
                Vector3 wheelWorldVel = rb.GetPointVelocity(hardPoint);
                float offset = MaxSpringDistance - wheelHit.distance;

                float vel = Vector3.Dot(springDir, wheelWorldVel);
                float force = (offset * springForce) - (vel * springDamper);
                SuspensionForce = force;
                if (offset > 0)
                {
                    rb.AddForceAtPosition(springDir * force, hardPoint);
                }

            }
            else
            {
                SuspensionForce = 0;
            }

        }
        void AddSuspensionForce_2(Vector3 hardPoint, Transform wheel, float MaxSpringDistance, out RaycastHit wheelHit, out bool WheelIsGrounded, out float SuspensionForce, int WheelNum)
        {
            var direction = -transform.up;

            if (Physics.SphereCast(hardPoint, wheelRadius, direction, out wheelHit, MaxSpringDistance + 0.05f))
            {
                WheelIsGrounded = true;
            }
            else
            {
                WheelIsGrounded = false;
            }

            // suspension spring force
            if (WheelIsGrounded)
            {
                Vector3 springDir = wheelHit.normal;
                float offset = (MaxSpringDistance + 0.05f - wheelHit.distance) / MaxSpringDistance + 0.05f;

                float vel = ((wheelHit.distance - wheelHitDistancePrev[WheelNum]) / Time.fixedDeltaTime);
                wheelHitDistancePrev[WheelNum] = wheelHit.distance;

                float force = (offset * offset * springForce) - (vel * springDamper);
                SuspensionForce = force;

                rb.AddForceAtPosition(springDir * force, hardPoint);

            }
            else
            {
                SuspensionForce = 0;
            }

        }
        public void AddLateralFriction(Vector3 hardPoint, Transform wheel, RaycastHit wheelHit, bool wheelIsGrounded, float factor)
        {
            if (wheelIsGrounded)
            {
                Vector3 SurfaceNormal = wheelHit.normal;

                Vector3 contactVel = (wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).x) * wheel.right;
                //contactVel = localVehicleVelocity.x * wheel.right;
                //Debug.DrawRay(hardPoint, contactVel.normalized, Color.gray);
                Vector3 contactDesiredAccel = -Vector3.ProjectOnPlane(contactVel, SurfaceNormal) / Time.fixedDeltaTime;

                //Vector3 frictionForce = Vector3.ClampMagnitude(rb.mass/4 * contactDesiredAccel, springForce * FrictionCoefficient);
                Vector3 frictionForce = rb.mass / 4 * contactDesiredAccel * FrictionCoefficient;

                //Debug.DrawRay(hardPoint, frictionForce.normalized, Color.red);

                rb.AddForceAtPosition(frictionForce * factor, hardPoint);
            }

        }
        public void AddLateralFriction_2(Vector3 hardPoint, Transform wheel, RaycastHit wheelHit, bool wheelIsGrounded, float factor, float suspensionForce, int wheelNum)
        {
            if (wheelIsGrounded)
            {
                Vector3 SurfaceNormal = wheelHit.normal;

                Vector3 sideVelocity = (wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).x) * wheel.right;
                Vector3 forwardVelocity = (wheel.InverseTransformDirection(rb.GetPointVelocity(hardPoint)).z) * wheel.forward;

                slipCoeff[wheelNum] = sideVelocity.magnitude / (sideVelocity.magnitude + Mathf.Clamp(forwardVelocity.magnitude, 0.1f, forwardVelocity.magnitude));

                Vector3 contactDesiredAccel = -Vector3.ProjectOnPlane(sideVelocity, SurfaceNormal) / Time.fixedDeltaTime;

                Vector3 frictionForce = Vector3.ClampMagnitude(rb.mass * contactDesiredAccel * sideFrictionCurve.Evaluate(slipCoeff[wheelNum]), suspensionForce * FrictionCoefficient);// good
                frictionForce = suspensionForce * FrictionCoefficient * -sideVelocity.normalized;


                frictionForce = Vector3.ClampMagnitude(frictionForce * forwardFrictionCurve.Evaluate(forwardVelocity.magnitude / MaxSpeed), rb.mass / 4 * contactDesiredAccel.magnitude);
                rb.AddForceAtPosition(frictionForce * factor, hardPoint);
            }

        }

        void AckermannSteering(float steerInput)
        {
            float turnRadius = wheelBase / Mathf.Tan(MaxTurnAngle / Mathf.Rad2Deg);
            if (steerInput > 0) //is turning right
            {
                ackermennLeftAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
                ackermennRightAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            }
            else if (steerInput < 0) //is turning left
            {
                ackermennLeftAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
                ackermennRightAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            }
            else
            {
                ackermennLeftAngle = 0;
                ackermennRightAngle = 0;
            }

            // auto counter steering
            if (localVehicleVelocity.z > 0 && AutoCounterSteer && Mathf.Abs(localVehicleVelocity.x) > 1f)
            {
                ackermennLeftAngle += Vector3.SignedAngle(transform.forward, rb.velocity + transform.forward, transform.up);
                ackermennLeftAngle = Mathf.Clamp(ackermennLeftAngle, -70, 70);
                ackermennRightAngle += Vector3.SignedAngle(transform.forward, rb.velocity + transform.forward, transform.up);
                ackermennRightAngle = Mathf.Clamp(ackermennRightAngle, -70, 70);
            }

            Wheels[0].localRotation = Quaternion.Euler(0, ackermennLeftAngle * turnCurve.Evaluate(localVehicleVelocity.z / MaxSpeed), 0);
            Wheels[1].localRotation = Quaternion.Euler(0, ackermennRightAngle * turnCurve.Evaluate(localVehicleVelocity.z / MaxSpeed), 0);
        }

        void tireVisual(bool WheelIsGrounded, Transform wheel, Transform hardPoint, float hitDistance, int tireNum)
        {
            if (WheelIsGrounded)
            {
                wheel.localPosition = hardPoint.localPosition - Vector3.up * (hitDistance);
            }
            else
            {
                wheel.localPosition = Vector3.Lerp(new Vector3(hardPoint.localPosition.x, wheel.localPosition.y, hardPoint.localPosition.z), hardPoint.localPosition - Vector3.up * MaxSpringDistance, 0.1f);
            }

            Vector3 wheelVelocity = rb.GetPointVelocity(hardPoint.position);
            float minRotation = (Vector3.Dot(wheelVelocity, wheel.forward) / wheelRadius) * Time.fixedDeltaTime * Mathf.Rad2Deg;
            float maxRotation = (Mathf.Sign(Vector3.Dot(wheelVelocity, wheel.forward)) * MaxSpeed / wheelRadius) * Time.fixedDeltaTime * Mathf.Rad2Deg;
            float wheelRotation = 0;

            if (brakeInput > 0.1f)
            {
                wheelRotation = 0;
            }
            else if (Mathf.Abs(accelerationInput) > 0.1f)
            {
                wheel.GetChild(0).RotateAround(wheel.position, wheel.right, maxRotation);
                wheelRotation = maxRotation;
            }
            else
            {
                wheel.GetChild(0).RotateAround(wheel.position, wheel.right, minRotation);
                wheelRotation = minRotation;
            }
            wheel.GetChild(0).localPosition = Vector3.zero;
            var rot = wheel.GetChild(0).localRotation;
            rot.y = 0;
            rot.z = 0;
            wheel.GetChild(0).localRotation = rot;

            //wheel slip calculation
            forwardSlip[tireNum] = Mathf.Abs(Mathf.Clamp((wheelRotation - minRotation) / (maxRotation), -1, 1));
            if (WheelIsGrounded)
            {
                skidTotal[tireNum] = (forwardSlip[tireNum] + slipCoeff[tireNum]) / 2;
            }
            else
            {
                skidTotal[tireNum] = 0;
            }


        }

        void setWheelSkidvalues_Start(int wheelNum, Skidmarks skidmarks, float radius)
        {
            wheelSkids[wheelNum].skidmarks = skidmarks;
            wheelSkids[wheelNum].radius = wheelRadius;
        }
        void setWheelSkidvalues_Update(int wheelNum, float skidTotal, Vector3 skidPoint, Vector3 normal)
        {
            wheelSkids[wheelNum].skidTotal = skidTotal;
            wheelSkids[wheelNum].skidPoint = skidPoint;
            wheelSkids[wheelNum].normal = normal;
        }


        void bodyAnimation()
        {
            Vector3 accel = Vector3.ProjectOnPlane((rb.velocity - lastVelocity) / Time.fixedDeltaTime, transform.up);
            accel = transform.InverseTransformDirection(accel);
            lastVelocity = rb.velocity;

            VehicleBody.localRotation = Quaternion.Lerp(VehicleBody.localRotation, Quaternion.Euler(Mathf.Clamp(-accel.z / 10, -3, 3), 0, Mathf.Clamp(accel.x / 5, -4, 4)), 0.1f);
        }

        void GroundedCheckPerWheel(bool wheelIsGrounded)
        {
            if (wheelIsGrounded)
            {
                NumberOfGroundedWheels += 1;
            }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < Wheels.Length; i++)
            {
                Gizmos.DrawLine(HardPoints[i].position, Wheels[i].position);
                Gizmos.DrawWireSphere(Wheels[i].position, wheelRadius);
                Gizmos.DrawSphere(HardPoints[i].position, 0.05f);
            }

        }
}


