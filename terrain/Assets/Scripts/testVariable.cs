using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace testvariableclass
{
    public class BaseVariable : object
    {
        protected dynamic rangedValue;

        public dynamic Value
        {
            get => rangedValue;
            set
            {
                rangedValue = value;
            }
        }

        public BaseVariable(dynamic defaultValue)
        {
            Value = defaultValue;
        }
    }

    public class RangedVariable : BaseVariable
    {
        private dynamic min;

        private dynamic max;

        public new dynamic Value
        {
            get => rangedValue;
            set
            {
                try
                {
                    if (rangedValue.GetType() == typeof(Vector3))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            rangedValue[i] = HandleRange(value[i]);
                        }
                    }
                    else
                    {
                        HandleRange(value);
                    }
                }
                catch (System.Exception)
                {
                    throw new System.Exception("There was an issue setting the value of a RangedVariable. Check that the type is compatible.");
                }

            }
        }

        public RangedVariable(dynamic defaultValue, dynamic min, dynamic max) : base((object)defaultValue)
        {
            this.min = min;
            this.max = max;
            Value = defaultValue;
        }
        private dynamic HandleRange(dynamic value)
        {
            return value < min ? min :
                value > max ? max :
                    value;
        }
    }

    class testVariable : MonoBehaviour
    {
        void Start()
        {
            //boolean variable
            BaseVariable IsEnabled = new BaseVariable(true);
            IsEnabled.Value = false;
            Debug.Log(IsEnabled.Value);

            //ranged non-vector
            RangedVariable Velocity = new RangedVariable(3, 0, 5);
            Debug.Log(Velocity.Value);

            //constrained vector
            RangedVariable ConstrainedAngle = new RangedVariable(new Vector3( 45, 60, 90 ), 10, 60);
            Debug.Log(ConstrainedAngle.Value);

            //nonconstrained vectir
            BaseVariable Angle = new BaseVariable(new Vector3(45, 60, 90));
            Debug.Log(Angle.Value);

        }
    }
}

