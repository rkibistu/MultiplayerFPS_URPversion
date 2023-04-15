using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAmmo : UIBehaviour {
    // PRIVATE METHODS

    [SerializeField]
    private UIValue _magazineAmmoValue;
    [SerializeField]
    private UIValue _weaponAmmoValue;
    [SerializeField]
    private Image _weaponImage;

    [SerializeField]
    private Image _damageGrenade;
    [SerializeField]
    private Image _smokeGrenade;


    // PUBLIC EMTHODS

    public void UpdateAmmo(WeaponMagazine weaponMagazine) {

        if (weaponMagazine == null)
            return;

        _magazineAmmoValue.SetValue(weaponMagazine.MagazineAmmo);
        _weaponAmmoValue.SetValue(weaponMagazine.WeaponAmmo);
    }
    public void UpdateWeaponImage(Weapon weapon) {

        _weaponImage.sprite = weapon.Icon;
    }
    public void UpdateGrenade(WeaponMagazine damageGrenadeMagazine, WeaponMagazine smokeGrenadeMagazine) {

        if (damageGrenadeMagazine != null) {

            if (damageGrenadeMagazine.WeaponAmmo > 0)
                _damageGrenade.color = Color.green;
            else
                _damageGrenade.color = Color.red;
        }

        if (smokeGrenadeMagazine != null) {

            if (smokeGrenadeMagazine.WeaponAmmo > 0)
                _smokeGrenade.color = Color.green;
            else
                _smokeGrenade.color = Color.red;
        }
    }

}
