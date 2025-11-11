using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AltarCheck : MonoBehaviour
{
    [Header("Senjata yang dibutuhkan")]
    public List<string> requiredWeaponIDs = new List<string> { "kujang", "kris", "topeng" };

    private HashSet<string> placedWeapons = new HashSet<string>();

    private void OnTriggerEnter(Collider other)
    {
        WeaponItem weapon = other.GetComponent<WeaponItem>();
        if (weapon != null)
        {
            string id = weapon.weaponID.ToLower();

            // Tambah senjata jika memang termasuk required
            if (requiredWeaponIDs.Contains(id) && !placedWeapons.Contains(id))
            {
                placedWeapons.Add(id);
                Debug.Log("Senjata diletakkan: " + id);

                // Cek apakah semua senjata sudah lengkap
                if (IsAllWeaponsPlaced())
                {
                    Debug.Log("Semua pusaka sudah lengkap di altar!");
                    GameFinished();
                }
            }
        }
    }

    private bool IsAllWeaponsPlaced()
    {
        return requiredWeaponIDs.TrueForAll(id => placedWeapons.Contains(id.ToLower()));
    }

    private void GameFinished()
    {
        Debug.Log("GAME SELESAI!");
        // Contoh: pindah ke UI Menu
        SceneManager.LoadScene("UImenu");

        // Atau aktifkan canvas kemenangan:
        // victoryUI.SetActive(true);
    }
}
