using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Config
{
    public class BaseVariable
    {
        protected dynamic currentValue;

        private List<Type> compatibleTypes = new List<Type>{typeof(bool)};

        public Variable Type;

        public dynamic Value
        {
            get => currentValue;
            set
            {
                currentValue = value;
                CheckType();
            }
        }

        public BaseVariable(dynamic defaultValue, Variable type)
        {
            Value = defaultValue;
            Type = type;
        }

        protected virtual void CheckType()
        {
            if (!compatibleTypes.Contains(currentValue.GetType()))
            {
                throw new Exception($"BaseVariable has been assigned a variable with non-compatible type {currentValue.GetType()}.");
            }
        }
    }



    //--------------------------------------------------------------------------------------



    public class RangedVariable : BaseVariable
    {
        public dynamic Min;

        public dynamic Max;

        private List<Type> compatibleTypes = new List<Type> { typeof(int), typeof(float), typeof(Vector3) };

        public new dynamic Value
        {
            get => currentValue;
            set
            {
                try
                {                   
                    if (currentValue.GetType() == typeof(Vector3))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            currentValue[i] = HandleRange(value[i]);
                        }
                    }
                    else
                    {
                        currentValue = HandleRange(value);
                    }
                    CheckType();
                }
                catch (System.Exception)
                {
                    throw new System.Exception("There was an issue setting the value of a RangedVariable.");
                }

            }
        }

        public RangedVariable(dynamic defaultValue, dynamic minValue, dynamic maxValue, Variable type) : base((object)defaultValue, type)
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

        protected override void CheckType()
        {
            if (!compatibleTypes.Contains(currentValue.GetType()))
            {
                throw new Exception($"BaseVariable has been assigned a variable with non-compatible type {currentValue.GetType()}.");
            }
        }
    }
}

