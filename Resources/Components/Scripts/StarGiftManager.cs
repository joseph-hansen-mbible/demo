
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Turnroot.Demos
{
    [System.Serializable]
    public struct StarGift
    {
        public int index;
        public string name;
        public string nameColor;
        public string descriptionText;
        public int strength;
        public int strengthGrowth;

        public int skill;
        public int skillGrowth;
        public int defense;
        public int defenseGrowth;
        public int magic;
        public int magicGrowth;
        public int resistance;
        public int resistanceGrowth;
        public int speed;
        public int speedGrowth;
        public int luck;
        public int luckGrowth;
        public int dexterity;
        public int dexterityGrowth;
    }

    [System.Serializable]
    public class StarGiftSelectedEvent : UnityEvent<StarGift> { }

    public class StarGiftManager : MonoBehaviour
    {
        public List<StarGift> StarGifts = new();

        public TextMeshProUGUI NameText;
        public TextMeshProUGUI DescriptionText;
        public GameObject Disk;

        public float RotationAmountPer = 30f;
        public float RotationSpeed = 1f;

        public StarGiftSelectedEvent OnStarGiftSelected = new();

        private int currentIndex = 0;
        private bool isRotating = false;

        public AudioClip NavigateClip;
        public AudioSource UiFx;

        private void Start()
        {
            if (StarGifts.Count > 0)
            {
                UpdateUI();
            }
        }

        public void HandleInput(string action)
        {
            // Don't allow input while rotating
            if (isRotating || StarGifts.Count == 0)
            {
                return;
            }

            if (action == "NavigateUp" || action == "NavigateLeft")
            {
                UiFx.PlayOneShot(NavigateClip);
                NavigatePrevious();
            }
            else if (action == "NavigateDown" || action == "NavigateRight")
            {
                UiFx.PlayOneShot(NavigateClip);
                NavigateNext();
            }
            else if (action == "Select" || action == "Start")
            {
                SelectCurrentStarGift();
            }
        }

        private void NavigatePrevious()
        {
            currentIndex++;
            if (currentIndex >= StarGifts.Count)
            {
                currentIndex = 0;
            }

            StartCoroutine(RotateDisk(RotationAmountPer));
        }

        private void NavigateNext()
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = StarGifts.Count - 1;
            }

            StartCoroutine(RotateDisk(-RotationAmountPer));
        }

        private IEnumerator RotateDisk(float angle)
        {
            isRotating = true;

            var startRot = Disk.transform.rotation;
            var endRot = Quaternion.Euler(0f, 0f, angle) * startRot;

            float elapsed = 0f;
            float duration = 1f / RotationSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                
                // Apply easing
                var easedT = EaseInOutSine(t);
                
                var newRot = Quaternion.Slerp(startRot, endRot, easedT);
                Disk.transform.rotation = newRot;

                yield return null;
            }

            // Ensure final rotation is exact
            Disk.transform.rotation = endRot;

            // Update UI after rotation completes
            UpdateUI();

            isRotating = false;
        }

        private void UpdateUI()
        {
            if (currentIndex < 0 || currentIndex >= StarGifts.Count)
            {
                return;
            }

            var currentGift = StarGifts[currentIndex];
            
            // Format: "The <color={color}>{NAME}</color>'s Gift"
            NameText.text = $"The <color={currentGift.nameColor}>{currentGift.name}</color>'s Gift";
            
            DescriptionText.text = currentGift.descriptionText;
        }

        private void SelectCurrentStarGift()
        {
            if (currentIndex >= 0 && currentIndex < StarGifts.Count)
            {
                var selectedGift = StarGifts[currentIndex];
                OnStarGiftSelected?.Invoke(selectedGift);
                Debug.Log($"Selected Star Gift: {selectedGift.name}");
            }
        }

        private float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
    }
}