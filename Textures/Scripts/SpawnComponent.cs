using Asteroids.Game;
using Asteroids.Message;
using UnityEngine;

namespace Asteroids.Component
{
    [RequireComponent(typeof(PoolComponent))]
    public class SpawnComponent : MonoBehaviour
    {
        [SerializeField]
        private float _spawnRate;

        [SerializeField]
        private bool _manualTrigger;

        private float _spawnTimer;
        private PoolComponent _poolComponent;

        private void Start()
        {
            _poolComponent = GetComponent<PoolComponent>();
            _spawnTimer = Time.time + _spawnRate;
        }

        private void Update()
        {
            if(_manualTrigger)
            {
                return;
            }

            if(Time.time > _spawnTimer)
            {
                SpawnObject();
            }
        }

        private GameObject GetItemFromPool()
        {
            _spawnTimer = Time.time + _spawnRate;
            return _poolComponent.GetItem();
        }
        
        private void SpawnObject()
        {
            GameObject item = GetItemFromPool();
            if (item != null)
            {
                item.GetComponent<IGameObject>().Initialize();
            }
        }

        protected void SpawnObject(BaseSpawnMessage message)
        {
            GameObject item = GetItemFromPool();
            if (item != null)
            {
                item.GetComponent<IGameObject>().Initialize(message);
            }
        }
    }
}
