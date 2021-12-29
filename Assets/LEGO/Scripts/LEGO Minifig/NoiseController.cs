using UnityEngine;
using UnityEngine.UI;

// Скрипт для управлением и отображения шума
namespace Unity.LEGO.Minifig
{
    public class NoiseController : MonoBehaviour
    {
        public static NoiseController Instance { get; private set; }

        public Transform NoiseOwner { get; private set; } // Источник шума
        public float NoiseValue => _noiseScaleSlider.value; // Значение шума

        [SerializeField] private Slider _noiseScaleSlider; // Слайдер для отображения уровня шума

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);

            Instance = this;
        }

        private void Update()
        {
            if (_noiseScaleSlider.value > 0)
                _noiseScaleSlider.value -= Time.deltaTime * 0.2f; // Если значения шума больше нуля то постепенно уменьшаем
        }

        public void MakeNoise(Transform noiseOwner, float duration = 0.1f) // Метод для вызова шума
        {
            NoiseOwner = noiseOwner; 
            _noiseScaleSlider.value += duration; 
        }
    }
}
