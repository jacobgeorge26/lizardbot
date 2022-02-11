using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = System.Random;

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

        Random random = new Random();

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

        public void Increment()
        {
            //vector3 - each axis needs an increment
            if(currentValue.GetType() == typeof(Vector3))
            {
                Vector3 newValue = Value;
                for (int i = 0; i < 3; i++)
                {
                    newValue[i] += GetIncrement();
                    newValue[i] = Bounce(newValue[i]);
                }
                Value = newValue;
            }
            else if(currentValue.GetType() == typeof(int))//int or float - only one increment needed
            {
                var newValue = Value;
                newValue += GetIncrement();
                newValue = (int)Bounce(newValue);
                Value = newValue;
            }
            else //float
            {
                var newValue = Value;
                newValue += GetIncrement();
                newValue = Bounce(newValue);
                Value = newValue;
            }
        }

        private dynamic Bounce(dynamic value)
        {
            if (value > Max)
            {
                return Max - (value - Max);
            }
            else if(value < Min)
            {
                return Min + (Min - value);
            }
            else
            {
                return value;
            }
        }

        //get increment value - anywhere between /10 and /100 of the max-min range
        private float GetIncrement()
        {
            float increment = (Max - Min) * random.Next(1, 11) / 100;
            increment *= random.NextDouble() > 0.5 ? 1 : -1;
            return increment;
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

