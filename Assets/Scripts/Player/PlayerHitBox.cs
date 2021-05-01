using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [SerializeField] PlayerHealth _playerHealth;
    public PlayerHealth playerHealth => _playerHealth;
}
