using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthProxy : MonoBehaviour
{
    [SerializeField] PlayerHealth _playerHealth;
    public PlayerHealth playerHealth => _playerHealth;
}
