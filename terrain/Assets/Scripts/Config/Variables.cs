using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Config
{
    public class BaseVariable
    {
        protected dynamic rangedValue;

        private List<Type> compatibleTypes = new List<Type>{ typeof(int), typeof(float), typeof(bool), typeof(Vector3) };

        public dynamic Value
        {
            get => rangedValue;
            set
            {
                if(!compatibleTypes.Contains(value.GetType())){
                    throw new Exception($"BaseVariable has been assigned a variable with non-compatible type {value.GetType()}.");
                }
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
        public dynamic Min;

        public dynamic Max;

        private List<Type> compatibleTypes = new List<Type> { typeof(int), typeof(float), typeof(Vector3) };

        public new dynamic Value
        {
            get => rangedValue;
            set
            {
                try
                {
                    if (!compatibleTypes.Contains(value.GetType()))
                    {
                        throw new Exception($"RangedVariable has been assigned a variable with non-compatible type {value.GetType()}.");
                    }
                    if (rangedValue.GetType() == typeof(Vector3))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            rangedValue[i] = HandleRange(value[i]);
                        }
                    }
                    else
                    {
                        rangedValue = HandleRange(value);
                    }
                }
                catch (System.Exception)
                {
                    throw new System.Exception("There was an issue setting the value of a RangedVariable.");
                }

            }
        }

        public RangedVariable(dynamic defaultValue, dynamic minValue, dynamic maxValue) : base((object)defaultValue)
        {
            this.Min = minValue;
            this.Max = maxValue;
            Value = defaultValue;
            if(minValue > maxValue)
            {
                throw new Exception("RangedVariable has been assigned a min value that is greater than the max value.");
            }
        }
        private dynamic HandleRange(dynamic value)
        {
            return value < Min ? Min :
                value > Max ? Max :
                    value;
        }
    }
}

