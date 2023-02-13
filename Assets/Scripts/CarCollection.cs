using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    [CreateAssetMenu(fileName = "Car Selection", menuName = "Car Selection")]
    public class CarCollection : ScriptableObject
    {
        private static CarCollection _instance;
        public static CarCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CarCollection>("Car Collection");
                }
                return _instance;
            }
        }


        [Serializable]
        public class Selection
        {
            public CarController Car;
            public Sprite Screenshot;
        }

        public List<Selection> PossibleCars = new List<Selection>();
    }
}
