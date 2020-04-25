using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera = null;

    [SerializeField]
    private Transform _target = null;

    [SerializeField]
    private float _maxRange = 4.0f;
	private void Awake()
	{
	    
	}
	
    private void Start()
    {
        
    }

    private void Update()
    {
        if (_target != null)
        {
            Vector2 mousePosNormalized = new Vector2(2.0f * Input.mousePosition.x / Screen.width - 1.0f,
                2.0f * Input.mousePosition.y / Screen.height - 1.0f);
            mousePosNormalized = mousePosNormalized.normalized * Mathf.SmoothStep(0.0f, 1.0f,
                Vector3.ClampMagnitude(mousePosNormalized, 1.0f).magnitude);

            Vector2 targetPosition = _target.position.ToVector2();
            Vector2 newPosition = targetPosition + mousePosNormalized * _maxRange;

            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }

    private void ShakeCamera(float duration, float positionStrength,
        int positionVibrato = 10, float positionRandomness = 90.0f)
    {
        Sequence shake = DOTween.Sequence();
        shake.Append(_camera.DOShakePosition(duration, positionStrength, positionVibrato, positionRandomness, false));
        shake.Append(_camera.transform.DOLocalMove(Vector3.zero, 0.5f, true));
        shake.Append(_camera.transform.DORotate(Vector3.zero, 0.5f));
        shake.Play();
    }

    private void SetCameraPosition(Vector2 position)
    {
        Vector3 oldPos = transform.position;
        transform.position = new Vector3(position.x, position.y, oldPos.z);
    }
}
