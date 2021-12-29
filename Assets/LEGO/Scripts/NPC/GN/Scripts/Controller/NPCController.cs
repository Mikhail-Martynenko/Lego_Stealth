using UnityEngine;
using UnityEngine.AI;

namespace Unity.LEGO.Minifig
{
    public sealed class NPCController : MonoBehaviour
    {
        #region Variables
        public Transform LookAtPoint;

        [SerializeField] private GameObject[] patrolPoints = null;

        [Header("Количество лучей")] 
        [SerializeField] private int rays = 6;
        [Header("Дальность видения")]
        [SerializeField] private int distance = 15;
        [Header("Дистанция убийства монстром")]
        [SerializeField] private int killDistance = 5;
        [Header("Угол обзора")]
        [SerializeField] private float angle = 20;
        [Header("Уровень шума для обнаружения")]
        [SerializeField] private float _needNoiseValue = 0.4f;
        [Header("Позиция выхода лучей")] 
        [SerializeField] private Vector3 offset;

        [SerializeField] private Transform _rayPoint;
        [SerializeField] private NavMeshAgent agent;

        [SerializeField] private bool _dynamic = true;

        private GameObject previousPatrolPoint = null;
        private bool getNewDistination = false, playerDetected = false;
        private const float _shotTime = 0.9f;
        private float oldSpeed = 0f;
        private float _shotTimer;
        #endregion
        #region MonoBehaviour
        private void Start()
        {
            if (_dynamic)
                oldSpeed = agent.speed;
        }
        private void SmoothLookAt(Vector3 newDirection) // Метод для плавного поворота к указаной позиции
        {
            Vector3 targetPostition = new Vector3(newDirection.x, transform.parent.position.y, newDirection.z);
            transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, Quaternion.LookRotation(targetPostition), Time.deltaTime);
        }
        private void Update()
        {
            if (RayToScan())
                playerDetected = true;
            else
            {
                if (_dynamic) 
                {
                    if (!agent.pathPending) // Если путь завершён
                    {
                        if (agent.remainingDistance <= agent.stoppingDistance) // Если оставшеяся дистанция меньше дистанции до остановки
                        {
                            if (!agent.hasPath) // Если агент не имеет назначеного пути
                            {
                                if (!getNewDistination && !playerDetected) // Если пункт назначения не задан и игрок не обнаружен
                                {
                                    StartCoroutine(SetNewDestination(2)); 
                                    getNewDistination = true; 
                                }
                            }
                        }
                    }
                }
            }
            if (playerDetected && !_dynamic) 
            {
                if (_shotTimer > 0)
                    _shotTimer -= Time.deltaTime;
                else
                {
                    _shotTimer = _shotTime; 
                    GameObject gm = Instantiate(Resources.Load<GameObject>("Bullet"), _rayPoint.transform.position, Quaternion.identity);
                    gm.GetComponent<Rigidbody>().AddForce(transform.forward*500); 
                }
            }
        }
        #endregion
        #region Methods
        private bool GetRaycast(Vector3 dir) // Метод для обнаружения объектов лучами
        {
            bool result = false; 
            RaycastHit hit = new RaycastHit();
            Vector3 pos = _rayPoint.position + offset;
            if (Physics.Raycast(pos, dir, out hit, distance))
            {
                if (hit.transform.tag == "Player" || NoiseController.Instance.NoiseValue >= _needNoiseValue)
                {
                    var player = hit.transform;

                    if (NoiseController.Instance.NoiseValue >= _needNoiseValue) // Если причиной был шум то переменной указываем источник шума
                        player = NoiseController.Instance.NoiseOwner;

                    result = true; 
                    playerDetected = true;

                    if (!_dynamic) 
                    {
                        Vector3 targetPostition = new Vector3(player.position.x, transform.position.y, player.position.z); 
                        transform.LookAt(targetPostition);
                    }

                    if (_dynamic)
                    {
                        if (Vector3.Distance(_rayPoint.position, player.transform.position) >= killDistance)
                        {
                            agent.SetDestination(player.transform.position); 
                            agent.speed = oldSpeed + 0.5f; 
                        }
                    }

                    Debug.DrawLine(pos, hit.point, Color.green); 
                }
                else
                {
                    Debug.DrawLine(pos, hit.point, Color.blue);
                    playerDetected = false;
                }
            }
            else
            {
                Debug.DrawRay(pos, dir * distance, Color.red);
                playerDetected = false;
            }
            return result;
        }
        private bool RayToScan()
        {
            bool result = false, a = false, b = false;
            float j = 0;
            for (int i = 0; i < rays; i++)
            {
                var x = Mathf.Sin(j);
                var y = Mathf.Cos(j);

                j += angle * Mathf.Deg2Rad / rays;

                Vector3 dir = _rayPoint.TransformDirection(new Vector3(x, 0, y));
                if (GetRaycast(dir)) a = true;

                if (x != 0)
                {
                    dir = _rayPoint.TransformDirection(new Vector3(-x, 0, y));
                    if (GetRaycast(dir)) b = true;
                }
            }

            if (a || b) result = true;
            return result;
        }

        private GameObject GetNextPatrolPoint() // Метод для получения следующего поинта для патрулирования
        {
            GameObject newPoint = patrolPoints[Random.Range(0, patrolPoints.Length)];
            if (previousPatrolPoint != newPoint)
            {
                previousPatrolPoint = newPoint; 
                return previousPatrolPoint; 
            }
            else return GetNextPatrolPoint(); 
        }
        #endregion
        #region IEnumarators
        [SerializeField] Transform newPatrolPoint;
        private System.Collections.IEnumerator SetNewDestination(float delay) // назначение нового поинта
        {
            yield return new WaitForSeconds(delay); 
            agent.speed = oldSpeed; 
            newPatrolPoint = GetNextPatrolPoint().transform; 
            agent.SetDestination(newPatrolPoint.position); 
            getNewDistination = false; 
        }
        #endregion
    }
}