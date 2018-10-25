﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Events
    public delegate void WeaponChanged(WeaponData wdata);
    public event WeaponChanged OnWeaponChanged;

    public delegate void ReloadStart();
    public delegate void ReloadStop();
    public event ReloadStart OnReloadStart;
    public event ReloadStop OnReloadStop;

    public delegate void MagChange(int inMag);
    public event MagChange OnMagChange;

    public delegate void MagEmpty();
    public event MagEmpty OnMagEmpty;

    public delegate void ExtraAmmoChange(int extraMag);
    public event ExtraAmmoChange OnExtraAmmoChange;

    public delegate void ReloadChange(float time, float percent);
    public event ReloadChange OnReloadChange;
    #endregion
    #region ShowInEditor
    [Header("Weapons:")]
    [SerializeField] List<WeaponData> data;
    [SerializeField] GameObject droppedWeapon;

    [Header("Components:")]
    [SerializeField] SpriteRenderer muzzleflashRenderer;
    [SerializeField] Transform rightHand, leftHand;
    #endregion
    #region HideInEditor
    SpriteRenderer sRenderer;
    List<WeaponData> instances;
    int dataIndex = 0;
    private int DataIndex
    {
        get
        {
            if (dataIndex < data.Count)
            {
                return dataIndex;
            }
            else
            {
                DataIndex = dataIndex;
                return dataIndex;
            }

        }
        set
        {
            if(value > data.Count - 1)
            {
                dataIndex = 0;
            }
            else if(value < 0)
            {
                dataIndex = data.Count - 1;
            }
            else
            {
                dataIndex = value;
            }
        }
    }
    private Coroutine reloadingRoutine;
    public Coroutine ReloadRoutine
    {
        get
        {
            return reloadingRoutine;
        }
        set
        {
            if (reloadingRoutine == null)
            {
                OnReloadStart?.Invoke();
            }
            if (value == null)
            {
                OnReloadStop?.Invoke();
            }
            reloadingRoutine = value;
        }
    }
    private Coroutine muzzleFlash;
    private Coroutine MuzzleFlash
    {
        get
        {
            return muzzleFlash;
        }
        set
        {
            if (value != null && muzzleFlash != null)
            {
                StopCoroutine(muzzleFlash);
            }
            else if(value == null && muzzleFlash != null)
            {
                StopCoroutine(muzzleFlash);
                muzzleflashRenderer.enabled = false;
            }
            muzzleFlash = value;
        }
    }
    public int AmmoInMag { get => instances[DataIndex].AmmoInMag; }
    public int ExtraAmmo { get => instances[DataIndex].ExtraAmmo; }
    public bool IsAuto { get => instances[DataIndex].isAuto; }
    #endregion

    #region UnityFunctions
    private void Awake()
    {
        sRenderer = GetComponent<SpriteRenderer>();

        InitializeInstances();
        RefreshData();
    }
    #endregion
    #region CustomFunctions
    void Initialize(WeaponData weaponData)
    {
        var instance = Instantiate(weaponData);
        instance.Initialize
        (
            GetComponent<Rigidbody2D>(),
            this
        );
        instances.Add(instance);

        OnWeaponChanged?.Invoke(instances[DataIndex]);
    }
    void InitializeInstances()
    {
        instances = new List<WeaponData>();
        foreach (var weaponData in data)
        {
            Initialize(weaponData);
        }
    }
    void RefreshData()
    {
        sRenderer.sprite = data[DataIndex].sprite;
        muzzleflashRenderer.color = data[DataIndex].bulletData.muzzleFlashColor;
        OnWeaponChanged?.Invoke(instances[DataIndex]);
    }

    public void Attack()
    {
        if (ReloadRoutine == null)
        {
            instances[dataIndex].Attack();
        }
        else if (ReloadRoutine != null)
        {
            StopCoroutine(ReloadRoutine);
            ReloadRoutine = null;
            instances[dataIndex].Attack();
        }
    }
    IEnumerator MuzzleFlashRoutine()
    {
        muzzleflashRenderer.enabled = true;

        foreach (Sprite sprite in data[DataIndex].muzzleFashAnimation)
        {
            muzzleflashRenderer.sprite = sprite;
            yield return new WaitForEndOfFrame();
        }

        muzzleflashRenderer.enabled = false;
        MuzzleFlash = null;
    }
    public void Reload()
    {
        if (ReloadRoutine == null)
        {
            ReloadRoutine = StartCoroutine(instances[DataIndex].ReloadRoutine());
        }
    }

    public void ChangeWeapon(bool next)
    {
        if (ReloadRoutine != null)
        {
            StopCoroutine(ReloadRoutine);
            ReloadRoutine = null;
        }
        if(next)
        {
            DataIndex++;
        }
        else
        {
            DataIndex--;
        }
        RefreshData();
    }
    public void DropWeapon()
    {
        if (instances.Count > 1)
        {
            GameObject weapon = Instantiate(droppedWeapon, transform.position, transform.rotation);
            DroppedWeapon droppedWeaponInstance = weapon.GetComponent<DroppedWeapon>();
            droppedWeaponInstance.data = instances[DataIndex];
            droppedWeaponInstance.dropDirection = transform.right.normalized;

            instances.RemoveAt(DataIndex);
            data.RemoveAt(DataIndex);
            RefreshData();
        }
    }
    public void AddWeapon(WeaponData newWeaponData)
    {
        data.Add(newWeaponData);
        Initialize(newWeaponData);
    }
    public void SetApearence(bool lookingRight)
    {
        if (lookingRight)
        {
            sRenderer.flipY = false;
            transform.localPosition = data[DataIndex].weaponPosition;
            leftHand.localPosition = data[DataIndex].leftHandPosition;
            rightHand.localPosition = data[DataIndex].rightHandPosition;
        }
        else
        {
            sRenderer.flipY = true;
            transform.localPosition = new Vector2(-data[DataIndex].weaponPosition.x, data[DataIndex].weaponPosition.y);
            leftHand.localPosition  = new Vector2(data[DataIndex].leftHandPosition.x, -data[DataIndex].leftHandPosition.y);
            rightHand.localPosition = new Vector2(data[DataIndex].rightHandPosition.x, -data[DataIndex].rightHandPosition.y);
        }
    }

    public void MuzleFlash()
    {
        if(MuzzleFlash == null)
        {
            MuzzleFlash = StartCoroutine(MuzzleFlashRoutine());
        }
    }
    public void TriggerMagChange(int inMag)
    {
        OnMagChange?.Invoke(inMag);
        if(inMag == 0)
        {
            OnMagEmpty?.Invoke();
        }
    }
    public void TriggerExtraAmmoChange(int extraMag)
    {
        OnExtraAmmoChange?.Invoke(extraMag);
    }
    public void TriggerReloadChange(float time, float percent)
    {
        OnReloadChange?.Invoke(time, percent);
    }
    #endregion
}