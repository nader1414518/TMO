using UnityEngine;
using System;

[Serializable]
public class DoctorRating : MonoBehaviour
{
    public string review;
    public int stars;

    public DoctorRating()
    {
        review = "";
        stars = 0;
    }
}
