using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = System.Random;

namespace Config
{
    //public class BaseVariable
    //{
    //    protected dynamic currentValue;

    //    private List<Type> compatibleTypes = new List<Type>{typeof(float)};

    //    public Variable Type;

    //    protected bool IsBool = false;

    //    public dynamic Value
    //    {
    //        get => currentValue;
    //        set
    //        {
    //            currentValue = value;
    //            CheckType();
    //        }
    //    }

    //    public BaseVariable(dynamic defaultValue, Variable type)
    //    {
    //        Value = defaultValue;
    //        Type = type;
    //    }

    //    protected virtual void CheckType()
    //    {
    //        if (!compatibleTypes.Contains(currentValue.GetType()))
    //        {
    //            throw new Exception($"BaseVariable has been assigned a variable with non-compatible type {currentValue.GetType()}.");
    //        }
    //    }
    //}



    //--------------------------------------------------------------------------------------





    public class GeneVariable
    {
        private dynamic currentValue;

        private dynamic Min;

        private dynamic Max;

        public Variable Type;

        private bool IsBool = false;

        private List<Type> compatibleTypes = new List<Type> { typeof(int), typeof(float), typeof(Vector3) };

        private Random random = new Random();

        public GeneVariable(dynamic defaultValue, dynamic minValue, dynamic maxValue, Variable type)
        {
            this.Min = minValue;
            this.Max = maxValue;
            currentValue = defaultValue;
            Value = defaultValue;
            if (minValue > maxValue)
            {
                throw new Exception("RangedVariable has been assigned a min value that is greater than the max value.");
            }
        }

        public GeneVariable(bool boolValue, Variable type)
        {
            this.Min = 0f;
            this.Max = 1f;
            currentValue = boolValue;
            Value = boolValue;
        }

        public dynamic Value
        {
            get => IsBool ? GetBool() : currentValue;
            set
            {
                try
                {
                    value = SetBool(value);
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
                    throw new System.Exception("There was an issue setting the value of a GeneVariable.");
                }

            }
        }

        public void Increment()
        {
            //vector3 - each axis needs an increment
            if (currentValue.GetType() == typeof(Vector3))
            {
                Vector3 newValue = Value;
                for (int i = 0; i < 3; i++)
                {
                    newValue[i] += GetIncrement();
                    newValue[i] = Bounce(newValue[i]);
                }
                Value = newValue;
            }
            else if (currentValue.GetType() == typeof(int))//int or float - only one increment needed
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
            else if (value < Min)
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

        private void CheckType()
        {
            if (!compatibleTypes.Contains(currentValue.GetType()))
            {
                throw new Exception($"GeneVariable has been assigned a variable with non-compatible type {currentValue.GetType()}.");
            }
        }

        private dynamic SetBool(dynamic value)
        {
            if (value.GetType() == typeof(bool))
            {
                if (!IsBool)
                {
                    IsBool = true;
                    //new variable, set between 0 - 0.5, or 0.5 - 1
                    Min = 0f;
                    Max = 1f;
                    return value ? 0.75f : 0.25f;
                }
                else
                {
                    //existing variable, return current float value
                    return currentValue;
                }
            }
            //not bool, return value
            return value;
        }

        private bool GetBool()
        {
            //return false if less than 0.5, true if > 0.5
            return currentValue < 0.5f ? false : true;
        }
    }
}

