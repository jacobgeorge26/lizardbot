using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = System.Random;

namespace Config
{
    public class GeneVariable
    {
        private dynamic currentValue;

        public dynamic Min;

        public dynamic Max;

        public Variable Type;

        private bool IsBool = false;

        private List<Type> compatibleTypes = new List<Type> { typeof(int), typeof(float), typeof(Vector3) };

        private Random random = new Random();

        public GeneVariable(dynamic defaultValue, dynamic minValue, dynamic maxValue, Variable type)
        {
            this.Min = minValue;
            this.Max = maxValue;
            this.Type = type;
            currentValue = defaultValue;
            //allow this to run before random init because there is some bool setup in here, and type checking
            Value = defaultValue;
            if (minValue > maxValue)
            {
                throw new Exception("GeneVariable has been assigned a min value that is greater than the max value.");
            }
            //now we know if there are any issues, we can generate a random value if necessary
            if (AIConfig.RandomInitValues) Value = GenerateValue();
        }

        public GeneVariable(bool boolValue, Variable type)
        {
            this.Min = 0f;
            this.Max = 1f;
            this.Type = type;
            //allow this to run before random init because there is some bool setup in here, and type checking
            currentValue = boolValue;
            Value = boolValue;
            //now we know if there are any issues, we can generate a random value if necessary
            if (AIConfig.RandomInitValues) Value = GenerateValue();
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

        //used when doing maths on bool / int values elsewhere - don't use to get values to be applied directly
        public dynamic Real
        {
            get => currentValue;
        }

        private dynamic GenerateValue()
        {
            //vector3 - each axis needs a value
            if (currentValue.GetType() == typeof(Vector3))
            {
                Vector3 newValue = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    newValue[i] = (float)random.NextDouble() * (Max - Min) + Min;
                }
                return newValue;
            }
            else if (currentValue.GetType() == typeof(int))//int or float - only one value needed
            {
                return random.Next(Min, Max);

            }
            else //float
            {
                return (float)random.NextDouble() * (Max - Min) + Min;
            }
        }

        public void Increment()
        {
            //vector3 - each axis needs an increment
            if (currentValue.GetType() == typeof(Vector3))
            {
                Vector3 newValue = currentValue;
                for (int i = 0; i < 3; i++)
                {
                    newValue[i] += GetIncrement();
                    newValue[i] = Bounce(newValue[i]);
                }
                Value = newValue;
            }
            else if (currentValue.GetType() == typeof(int))//int or float - only one increment needed
            {
                var newValue = currentValue;
                newValue += GetIncrement();
                newValue = (int)Bounce(newValue);
                Value = newValue;
            }
            else //float
            {
                var newValue = currentValue;
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
                if(currentValue.GetType() == typeof(bool)){
                    //new variable - needs setup
                    IsBool = true;
                    Min = 0f;
                    Max = 1f;
                    return currentValue = value ? 0.75f : 0.25f;
                }
                //existing value, check if value needs to be changed as bool has changed
                else if (value && currentValue < 0.5f) return 0.75f; //set true
                else if (!value && currentValue >= 0.5f) return 0.25f; //set false
                else return currentValue;
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

